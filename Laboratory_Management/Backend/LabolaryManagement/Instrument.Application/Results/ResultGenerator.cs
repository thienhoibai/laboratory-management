using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace Instrument.Application.Results;

public class ResultGenerator : IResultGenerator
{
    private readonly double _pNorm;
    private readonly decimal _marginF;
    private readonly decimal _clampBelow;
    private readonly decimal _clampAbove;

    public ResultGenerator(IConfiguration cfg)
    {
        var s = cfg.GetSection("ResultSim");
        _pNorm      = s.GetValue("NormalProbability", 0.88);
        _marginF    = s.GetValue("AbnormalMarginFactor", 0.15m);
        _clampBelow = s.GetValue("ClampBelowWidth", 0.5m);
        _clampAbove = s.GetValue("ClampAboveWidth", 1.0m);
    }

    public Dictionary<int, decimal> GenerateValues(IEnumerable<ForInstrumentItemDto> items, string? patientSex)
    {
        var map = new Dictionary<int, decimal>();

        foreach (var g in items.GroupBy(i => i.ParameterId))
        {
            var sample = g.First();
            var (min, max) = ResolveRangeForPatient(sample.ReferenceRange, patientSex);

            var width = Math.Max(0.0001m, max - min);
            var clampMin = min - _clampBelow * width;
            var clampMax = max + _clampAbove * width;

            decimal v;
            if (Random.Shared.NextDouble() <= _pNorm)
            {
                v = Rand(min, max);
            }
            else
            {
                var m = _marginF * width;
                if (Random.Shared.NextDouble() < 0.5)
                    v = Rand(min - m, min - 0.0001m);
                else
                    v = Rand(max + 0.0001m, max + m);
                v = Math.Clamp(v, clampMin, clampMax);
            }

            map[g.Key] = Math.Round(v, 2);
        }

        return map;
    }

    // ===== helpers (sex-aware) =====
    private static (decimal min, decimal max) ResolveRangeForPatient(string? rr, string? sex)
    {
        var bySex = TryParseSexRanges(rr);
        if (bySex != null && NormalizeSex(sex) is string sx && bySex.TryGetValue(sx, out var r))
            return r;

        var (ok, lo, hi) = TryParseFirstPair(rr);
        if (ok) return (lo, hi);

        var (loAll, hiAll) = ParseMinMaxAll(rr);
        if (loAll.HasValue && hiAll.HasValue) return (loAll.Value, hiAll.Value);

        return (1m, 10m);
    }

    private static string? NormalizeSex(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var t = s.Trim().ToLowerInvariant();
        if (t.StartsWith("m") || t.StartsWith("na")) return "M";
        if (t.StartsWith("f") || t.StartsWith("nữ") || t.StartsWith("nu")) return "F";
        return null;
    }

    private static Dictionary<string,(decimal,decimal)>? TryParseSexRanges(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var text = s.Replace(',', '.').ToLowerInvariant();

        var map = new Dictionary<string,(decimal,decimal)>();
        foreach (var token in new[] { ("nam","M"), ("nữ","F"), ("nu","F"), ("male","M"), ("female","F"), ("m:","M"), ("f:","F") })
        {
            var idx = text.IndexOf(token.Item1);
            if (idx < 0) continue;

            var sub = text[idx..];
            var nums = Regex.Matches(sub, @"[-+]?\d*\.?\d+");
            if (nums.Count >= 2 &&
                decimal.TryParse(nums[0].Value, out var a) &&
                decimal.TryParse(nums[1].Value, out var b))
            {
                map[token.Item2] = (Math.Min(a,b), Math.Max(a,b));
            }
        }
        return map.Count > 0 ? map : null;
    }

    private static (bool ok, decimal min, decimal max) TryParseFirstPair(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return (false,0,0);
        var text = s.Replace(',', '.');
        var m = Regex.Matches(text, @"[-+]?\d*\.?\d+");
        if (m.Count >= 2 &&
            decimal.TryParse(m[0].Value, out var a) &&
            decimal.TryParse(m[1].Value, out var b))
            return (true, Math.Min(a,b), Math.Max(a,b));
        return (false,0,0);
    }

    private static (decimal? lo, decimal? hi) ParseMinMaxAll(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return (null,null);
        var text = s.Replace(',', '.');
        var m = Regex.Matches(text, @"[-+]?\d*\.?\d+");
        if (m.Count == 0) return (null,null);
        var nums = m.Select(x => decimal.Parse(x.Value, System.Globalization.CultureInfo.InvariantCulture)).ToList();
        return (nums.Min(), nums.Max());
    }

    private static decimal Rand(decimal lo, decimal hi)
    {
        var r = (decimal)Random.Shared.NextDouble();
        return lo + r * (hi - lo);
    }
}
