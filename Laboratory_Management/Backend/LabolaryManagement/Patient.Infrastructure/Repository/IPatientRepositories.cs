using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patient.Infrastructure.Repository
{
    public interface IPatientRepositories
    {
        Task<Patient.Domain.Patient?> GetByIdAsync(long id);
        Task<IEnumerable<Patient.Domain.Patient>> GetAllAsync();
    }
}
