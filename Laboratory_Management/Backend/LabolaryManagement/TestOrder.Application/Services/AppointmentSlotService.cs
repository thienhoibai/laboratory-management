using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Repository;
using TestOrder.Infrastructure.Models;
using TestOrder.Application.DTOs.AppointmentSlots;
using System.ComponentModel;
namespace TestOrder.Application.Services
{
    public class AppointmentSlotService
    {
        private readonly AppointmentSlotRepository _repository;
        private readonly TimeBlockRepository _timeBlockRepository;

        public AppointmentSlotService(AppointmentSlotRepository repository, TimeBlockRepository timeBlockRepository)
        {
            _repository = repository;
            _timeBlockRepository = timeBlockRepository;
        }

        public AppointmentSlotService()
        {
            _repository = new AppointmentSlotRepository();
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

        public async Task<AppointmentSlot> GetAppointmentSlotByIdAsync(long appointmentSlotId)
        {
            return await _repository.GetByIdAsync(appointmentSlotId);
        }


        public async Task<AppointmentSlot> GetAppointmentSlotByDateAndTimeAsync(DateOnly appointmentDate, TimeOnly timeBlock)
        {
            var timeBlockEntity = await _timeBlockRepository.GetByTime(timeBlock);
            if (timeBlockEntity == null)
            {
                throw new Exception("Invalid Time Block");
            }
            return await _repository.GetByDateAndTimeAsync(appointmentDate, timeBlockEntity.TimeBlockId);
        }

        public bool IsAppointmentSlotMaxedOut(AppointmentSlotDTO appointmentSlot)
        {
            var timeBlockEntity = _timeBlockRepository.GetByTime(appointmentSlot.TimeBlock).Result;
            var existingSlot = _repository.GetByDateAndTimeAsync(appointmentSlot.AppointmentDate, timeBlockEntity.TimeBlockId).Result;

            var bookingsCount = _repository.GetBookingsCountForSlot(existingSlot.SlotId).Result;
            return bookingsCount >= existingSlot.MaxBooking;
        }

        public List<bool> CheckAvailabilityForMultipleSlotsAsync(List<AppointmentSlotDTO> appointmentSlots)
        {
            var availabilityResults = new List<bool>();
            foreach (var slot in appointmentSlots)
            {
                var isMaxedOut = IsAppointmentSlotMaxedOut(slot);
                availabilityResults.Add(!isMaxedOut);
            }
            return  availabilityResults;
        }


        public bool IsAppointmentsDateValid(DateOnly appointmentDate)
        {
            return appointmentDate > DateOnly.FromDateTime(DateTime.Now);
        }
        public bool IsAppointmentSlotExists(DateOnly appointmentDate, TimeOnly timeBlock)
        {
            var timeBlockEntity = _timeBlockRepository.GetByTime(timeBlock).Result;
            if (timeBlockEntity == null)
            {
                throw new Exception("Invalid Time Block");
            }
            var existingSlot = _repository.GetByDateAndTimeAsync(appointmentDate, timeBlockEntity.TimeBlockId).Result;
            return existingSlot != null;
        }

        public async Task AddAppointmentSlotAsync(AppointmentSlotDTO appointmentSlot)
        {
            var timeBlockEntity = await _timeBlockRepository.GetByTime(appointmentSlot.TimeBlock);

            var entity = new AppointmentSlot
            {
                SlotId = Guid.NewGuid(),
                AppointmentDate = appointmentSlot.AppointmentDate,
                TimeBlockId = timeBlockEntity.TimeBlockId,
            };
            await _repository.AddAsync(entity);
        }


    }
}
