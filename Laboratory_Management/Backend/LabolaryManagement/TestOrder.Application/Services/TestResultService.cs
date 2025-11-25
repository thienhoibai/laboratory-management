using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Repository;
using TestOrder.Application.Services.Booking;

namespace TestOrder.Application.Services
{
    public class TestResultService
    {
        private readonly TestResultRepository _testResultRepository;
        private readonly TestParameterService testParameterService;
        private readonly BookingTestService bookingTestService;

        public TestResultService(
            TestResultRepository testResultRepository,
            TestParameterService testParameterService,
            BookingTestService bookingTestService)
        {
            _testResultRepository = testResultRepository;
            this.testParameterService = testParameterService;
            this.bookingTestService = bookingTestService;
        }


        //public async Task<IEnumerable<Infrastructure.Models.TestResult>> GetTestResultsWithParametersByBookingIdAsync(Guid bookingId)
        //{
        //    var bookingTests = await bookingTestService.GetBookingTestsByBookingIdAsync(bookingId);
        //    var allTestResults = new List<Infrastructure.Models.TestResult>();
        //}


    }
}
