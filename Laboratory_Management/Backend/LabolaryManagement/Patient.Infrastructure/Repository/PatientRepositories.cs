using Patient.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Patient.Domain;



namespace Patient.Infrastructure.Repository
{
    public class PatientRepositories: IPatientRepositories
    {
        private readonly PatientDBContext _context;

        public PatientRepositories(PatientDBContext context)
        {
            _context = context;
        }

        public async Task<Patient.Domain.Patient?> GetByIdAsync(long id)
        {
            return await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == id);
        }

        public async Task<IEnumerable<Patient.Domain.Patient>> GetAllAsync()
        {
            return await _context.Patients.ToListAsync();
        }
    }
}
