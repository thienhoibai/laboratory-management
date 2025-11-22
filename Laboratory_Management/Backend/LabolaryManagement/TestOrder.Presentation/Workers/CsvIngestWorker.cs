using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestOrder.Application.DTOs.Bookings;
using TestOrder.Infrastructure.Data;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Presentation.Workers
{
    public class CsvIngestOptions
    {
        public string DropFolder { get; set; } = "C\\Drop\\TestOrder";
        public string ArchiveOk { get; set; } = "C\\Drop\\TestOrder\\archive\\ok";
        public string ArchiveError { get; set; } = "C\\Drop\\TestOrder\\archive\\error";
        public int ScanIntervalSeconds { get; set; } = 3;
    }

    public class CsvIngestWorker : BackgroundService
    {
        private readonly ILogger<CsvIngestWorker> _logger;
        private readonly IDbContextFactory<TestOrderDBContext> _dbFactory;
        private readonly CsvIngestOptions _opt;

        private static readonly string[] RequiredHeaders = new[]
        {
            "BookingId","TestBookingNo","ParameterId","ParameterName","Value","Unit","RefMin","RefMax","InstrumentCode","MeasuredAt"
        };

        public CsvIngestWorker(ILogger<CsvIngestWorker> logger,
            IDbContextFactory<TestOrderDBContext> dbFactory,
            IOptions<CsvIngestOptions> opt)
        {
            _logger = logger; _dbFactory = dbFactory; _opt = opt.Value;
            Directory.CreateDirectory(_opt.DropFolder);
            Directory.CreateDirectory(_opt.ArchiveOk);
            Directory.CreateDirectory(_opt.ArchiveError);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CsvIngestWorker started. Watching folder: {folder}", _opt.DropFolder);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var files = Directory.GetFiles(_opt.DropFolder, "*.csv", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        // Skip archive subfolders
                        if (file.StartsWith(_opt.ArchiveOk, StringComparison.OrdinalIgnoreCase) ||
                            file.StartsWith(_opt.ArchiveError, StringComparison.OrdinalIgnoreCase))
                            continue;

                        await ProcessFileAsync(file, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scanning drop folder");
                }

                try { await Task.Delay(TimeSpan.FromSeconds(_opt.ScanIntervalSeconds), stoppingToken); } catch { }
            }
        }

        private async Task ProcessFileAsync(string path, CancellationToken ct)
        {
            _logger.LogInformation("Processing CSV: {file}", path);
            string destOk = Path.Combine(_opt.ArchiveOk, Path.GetFileName(path));
            string destErr = Path.Combine(_opt.ArchiveError, Path.GetFileName(path));

            try
            {
                var lines = await File.ReadAllLinesAsync(path, Encoding.UTF8, ct);
                if (lines.Length == 0) throw new InvalidOperationException("Empty CSV");

                var headers = SplitCsvLine(lines[0]);
                if (!ValidateHeaders(headers, out var colIndex, out var headerErr))
                    throw new InvalidOperationException("CSV header invalid: " + headerErr);

                // Group results by booking for final status update checks
                var resultsByBooking = new Dictionary<Guid, List<(long TestBookingNo, int ParameterId, string Value)>>();

                await using var db = await _dbFactory.CreateDbContextAsync(ct);
                for (int i = 1; i < lines.Length; i++)
                {
                    var row = SplitCsvLine(lines[i]);
                    if (row.Length == 1 && string.IsNullOrWhiteSpace(row[0])) continue; // skip blank
                    if (row.Length < headers.Length)
                        throw new InvalidOperationException($"Line {i + 1}: column count mismatch");

                    var bookingId = Guid.Parse(row[colIndex["BookingId"]]);
                    var testBookingNo = long.Parse(row[colIndex["TestBookingNo"]]);
                    var parameterId = int.Parse(row[colIndex["ParameterId"]]);
                    var parameterName = row[colIndex["ParameterName"]];
                    var valueStr = row[colIndex["Value"]];
                    var unit = row[colIndex["Unit"]];
                    var refMinStr = row[colIndex["RefMin"]];
                    var refMaxStr = row[colIndex["RefMax"]];
                    var instrumentCode = row[colIndex["InstrumentCode"]];
                    var measuredAtStr = row[colIndex["MeasuredAt"]];

                    // validate parse-ability
                    _ = DateTimeOffset.Parse(measuredAtStr, CultureInfo.InvariantCulture);
                    _ = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
                    _ = string.IsNullOrWhiteSpace(refMinStr) ? (decimal?)null : decimal.Parse(refMinStr, CultureInfo.InvariantCulture);
                    _ = string.IsNullOrWhiteSpace(refMaxStr) ? (decimal?)null : decimal.Parse(refMaxStr, CultureInfo.InvariantCulture);

                    // Validate TestBookingNo in booking
                    var exists = await db.Set<BookingTest>()
                        .AnyAsync(x => x.BookingId == bookingId && x.TestBookingNo == testBookingNo, ct);
                    if (!exists)
                        throw new InvalidOperationException($"Line {i + 1}: TestBookingNo {testBookingNo} not in booking {bookingId}");

                    // Upsert TestResult by (TestBookingNo, ParameterId)
                    var tr = await db.Set<TestResult>()
                        .SingleOrDefaultAsync(x => x.TestBookingNo == testBookingNo && x.ParameterId == parameterId, ct);
                    if (tr == null)
                    {
                        await db.Set<TestResult>().AddAsync(new TestResult
                        {
                            TestBookingNo = testBookingNo,
                            ParameterId = parameterId,
                            ResultValue = valueStr
                        }, ct);
                    }
                    else
                    {
                        tr.ResultValue = valueStr;
                    }

                    if (!resultsByBooking.TryGetValue(bookingId, out var list))
                    {
                        list = new(); resultsByBooking[bookingId] = list;
                    }
                    list.Add((testBookingNo, parameterId, valueStr));
                }

                await db.SaveChangesAsync(ct);

                // For each booking in this file, check completion
                foreach (var kv in resultsByBooking)
                {
                    var bookingId = kv.Key;
                    var expected = await db.Set<BookingTest>()
                        .Where(bt => bt.BookingId == bookingId)
                        .Join(db.Set<TestCatalog>(), bt => bt.CatalogId, c => c.CatalogId, (bt, c) => new { bt, c })
                        .SelectMany(x => x.c.Parameters.Select(p => new { x.bt.TestBookingNo, p.ParameterId }))
                        .CountAsync(ct);

                    var actual = await db.Set<TestResult>()
                        .Join(db.Set<BookingTest>(), r => r.TestBookingNo, bt => bt.TestBookingNo, (r, bt) => new { r, bt })
                        .Where(x => x.bt.BookingId == bookingId)
                        .Select(x => new { x.r.TestBookingNo, x.r.ParameterId })
                        .Distinct()
                        .CountAsync(ct);

                    if (expected > 0 && actual >= expected)
                    {
                        var b = await db.Set<Booking>().SingleAsync(x => x.BookingId == bookingId, ct);
                        b.Status = (byte)BookingStatusEnum.Completed; // 6
                        await db.SaveChangesAsync(ct);
                    }
                }

                // archive OK
                File.Move(path, destOk, true);
                _logger.LogInformation("CSV processed OK: {file}", path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CSV error for file {file}", path);
                try { File.Move(path, destErr, true); } catch { }
            }
        }

        private static string[] SplitCsvLine(string line)
        {
            // Simple split that handles quoted fields with commas
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    { sb.Append('"'); i++; }
                    else { inQuotes = !inQuotes; }
                }
                else if (ch == ',' && !inQuotes)
                { result.Add(sb.ToString()); sb.Clear(); }
                else
                { sb.Append(ch); }
            }
            result.Add(sb.ToString());
            return result.ToArray();
        }

        private static bool ValidateHeaders(string[] headers, out Dictionary<string, int> colIndex, out string error)
        {
            colIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            error = string.Empty;
            for (int i = 0; i < headers.Length; i++)
            {
                var h = headers[i].Trim();
                if (!colIndex.ContainsKey(h)) colIndex[h] = i;
            }
            foreach (var req in RequiredHeaders)
            {
                if (!colIndex.ContainsKey(req))
                {
                    error = $"Missing column: {req}";
                    return false;
                }
            }
            return true;
        }
    }
}
