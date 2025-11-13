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

        public AppointmentSlotService(
            AppointmentSlotRepository repository,
            TimeBlockRepository timeBlockRepository)
        {
            _repository = repository;
            _timeBlockRepository = timeBlockRepository;
        }

        public async Task<IEnumerable<AppointmentSlot>> GetAllAppointmentSlot(int pageNumber)
        {
            return await _repository.GetAllPagedAsync(pageNumber);
        }

        public async Task<IEnumerable<AppointmentSlot>> GetAppointmentSlotsByDateAsync(DateOnly appointmentDate, int pageNumber, int pageSize)
        {
            return await _repository.GetByDateAsync(appointmentDate, pageNumber, pageSize);
        }

        public async Task<AppointmentSlot> GetAppointmentSlotByIdAsync(Guid appointmentSlotId)
        {
            return await _repository.GetByIdAsync(appointmentSlotId);
        }

        public async Task<AppointmentSlotDTO> GetAppointmentSlotInfo (Guid slotId)
        {
            var slotEntity = await _repository.GetByIdAsync(slotId);
            if (slotEntity == null)
            {
                throw new Exception("Appointment Slot not found");
            }
            var timeBlockEntity = await _timeBlockRepository.GetByIdAsync(slotEntity.TimeBlockId);
            return new AppointmentSlotDTO
            {
                AppointmentDate = slotEntity.AppointmentDate,
                TimeBlock = timeBlockEntity.TimeBlock1
            };
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

        public async Task<int> GetBookingsCountForSlotAsync(Guid slotId)
        {
            return await _repository.GetBookingsCountForSlot(slotId);
        }

        public async Task<List<int>> GetBookingsCountForMultipleSlotsAsync(List<Guid> slotIds)
        {
            var bookingsCounts = new List<int>();
            foreach (var slotId in slotIds)
            {
                var count = await _repository.GetBookingsCountForSlot(slotId);
                bookingsCounts.Add(count);
            }
            return bookingsCounts;
        }

        public async Task<List<int>?> GetBookingCountForAllSlotAsync(int pageNumber)
        {
            var slots = await _repository.GetAllPagedAsync(pageNumber);
            if (slots == null || !slots.Any())
            {
                return null;
            }
            var bookingCounts = new List<int>();
            foreach (var slot in slots)
            {
                var count = await _repository.GetBookingsCountForSlot(slot.SlotId);
                bookingCounts.Add(count);
            }
            return bookingCounts;

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
