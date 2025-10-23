using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patient.Application
{
    public interface IPatientService
    {
        Task<Patient.Domain.Patient?> GetPatientByIdAsync(int id);
        Task<IEnumerable<Patient.Domain.Patient>> GetAllPatientsAsync();
    }
}
