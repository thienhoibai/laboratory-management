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
        public TestResultService(TestResultRepository testResultRepository)
        {
            _testResultRepository = testResultRepository;
        }

        public async Task<Infrastructure.Models.Result.ResultDetails?> GetTestResultByBookingId(Guid bookingId)
        {
            return await _testResultRepository.GetResultByBookingId(bookingId);
        }
    }
}
