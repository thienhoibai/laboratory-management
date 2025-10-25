using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Repository;
using TestOrder.Infrastructure.Models;
using TestOrder.Application.DTOs.AppointmentSlots;
namespace TestOrder.Application.Services
{
    public class AppointmentSlotService
    {
        private readonly AppointmentSlotRepository _repository;
        private readonly TimeBlockRepository _timeBlockRepository;

        public AppointmentSlotService(AppointmentSlotRepository repository)
        {
            _repository = repository;
            _timeBlockRepository = new TimeBlockRepository();
        }

        public async Task<IEnumerable<AppointmentSlot>> GetAllAppointmentSlot(int pageNumber)
        {
            return await _repository.GetAllPagedAsync(pageNumber);
        }

        public async Task<IEnumerable<AppointmentSlot>> GetAppointmentSlotsByDateAsync(DateOnly appointmentDate)
        {
            return await _repository.GetByDateAsync(appointmentDate);
        }

        public async Task AddAppointmentSlotAsync(AppointmentSlotDTO appointmentSlot)
        {
            var timeBlock = await _timeBlockRepository.GetByTime(appointmentSlot.TimeBlock);
            if (timeBlock == null)
            {
                throw new Exception("Invalid Time Block");
            }

            var existingSlot = await _repository.GetByDateAndTimeAsync(appointmentSlot.AppointmentDate, timeBlock.TimeBlockId);
            if (existingSlot != null)
            {
                throw new Exception("Appointment Slot already exists for the given date and time block.");
            }

            var entity = new AppointmentSlot
            {
                AppointmentDate = appointmentSlot.AppointmentDate,
                TimeBlockId = timeBlock.TimeBlockId,
            };
            await _repository.AddAsync(entity);
        }


    }
}
