using leads_hr_ltd.Models;
using Microsoft.EntityFrameworkCore;

namespace leads_hr_ltd.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
        {
        }

/*        public DbSet<Employee> Employees { get; set; }
*/
    }
}
