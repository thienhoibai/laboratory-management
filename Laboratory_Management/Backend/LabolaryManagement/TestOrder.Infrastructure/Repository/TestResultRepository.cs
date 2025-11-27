using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Models.Result;
using TestOrder.Infrastructure.Data;

namespace TestOrder.Infrastructure.Repository
{
    public class TestResultRepository : GenericRepository<TestResult>
    {
        public TestResultRepository(TestOrderDBContext context) : base(context)
        {

        }

        public async Task<ResultDetails?> GetResultByBookingId(Guid BookingId)
        {
            var booking = await _context.Bookings.FindAsync(BookingId);
            if (booking == null)
            {
                return null;
            }

            var ResultDetails = new ResultDetails
            {
                BookingCode = booking.BookingCode ?? string.Empty,
                Catalogs = new List<CatalogDetails>()
            };
            var catalogs = _context.BookingTests
                .Where(bt => bt.BookingId == BookingId)
                .Select(bt => bt.Catalog)
                .Distinct().ToList();

            foreach (var catalog in catalogs)
            {
                var catalogDetails = new CatalogDetails
                {
                    CatalogId = catalog.CatalogId.ToString(),
                    CatalogName = catalog.TestName,
                    CatalogDescription = catalog.Description ?? string.Empty,
                    Parameters = new List<ParameterDetails>()
                };

                var results = (from bt in _context.BookingTests
                              join tr in _context.TestResults on bt.TestBookingNo equals tr.TestBookingNo
                              join p in _context.TestParameters on tr.ParameterId equals p.ParameterId
                              where bt.BookingId == BookingId && bt.CatalogId == catalog.CatalogId
                              select new { p.ParameterName, tr.ResultValue, p.Unit, p.ReferenceRange, tr.IsNormal}).ToList();
                foreach (var result in results)
                {
                    var parameterDetails = new ParameterDetails
                    {
                        Name = result.ParameterName,
                        Value = result.ResultValue,
                        Unit = result.Unit,
                        ReferenceRange = result.ReferenceRange,
                        IsNormal = result.IsNormal,
                    };
                    catalogDetails.Parameters.Add(parameterDetails);
                }
                ResultDetails.Catalogs.Add(catalogDetails);
            }
            return ResultDetails;

        }

    }
}
