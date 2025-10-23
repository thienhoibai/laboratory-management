using Patient.Infrastructure.Data;
using Patient.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patient.Application
{
    public class PatientService: IPatientService
    {

        private readonly IPatientRepositories _repository;

        public PatientService(IPatientRepositories repository)
        {
            _repository = repository;
        }


        public async Task<Patient.Domain.Patient?> GetPatientByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Patient.Domain.Patient>> GetAllPatientsAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
