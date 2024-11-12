 
using leads_hr_ltd.Data;
using Microsoft.EntityFrameworkCore;

namespace leads_hr_ltd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register DbContext with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register HRController and API routing
            builder.Services.AddControllers(); // This is required for API controllers to be recognized

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Map controller routes
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=EmployeeList}/{id?}");

            // Map API routes for HRController (not needed if you're already using [ApiController])
            app.MapControllers(); // Automatically maps API controllers

            app.Run();
        }
    }
}
