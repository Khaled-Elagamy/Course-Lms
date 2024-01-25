using Course_Lms.Data.Interfaces;
using Course_Lms.Models;
using Microsoft.EntityFrameworkCore;

namespace Course_Lms.Data.Repositories
{
    public class InstructorRepository : IInstructorRepository
    {
        private readonly ApplicationDbContext _context;
        public InstructorRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Instructor?> GetByIdAsync(string id)
        {
            return await _context.Instructors.FirstOrDefaultAsync(n => n.UserId == id);
        }

        public async Task CreateAsync(Instructor instructor)
        {
            await _context.Instructors.AddAsync(instructor);
            await _context.SaveChangesAsync();

        }
        public async Task<bool> IsFound(string id)
        {
            return await _context.Instructors.AnyAsync(i => i.UserId == id);
        }
    }
}
