using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using leads_hr_ltd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace leads_hr_ltd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
 
        // GET method for displaying the form
        [HttpGet]
        public IActionResult InsertEmployee()
        {
            return View(); // Return the view to display the form
        }

        // Method to insert a new employee into the database
        [HttpPost]
        public IActionResult InsertEmployee(Employee employee)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand("InsertEmployee", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Add parameters to the stored procedure
                    command.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@Division", employee.Division);
                    command.Parameters.AddWithValue("@Building", employee.Building);
                    command.Parameters.AddWithValue("@Title", employee.Title);
                    command.Parameters.AddWithValue("@Room", employee.Room);

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                // Optionally, you can add a success message or redirect
                TempData["Message"] = "Employee inserted successfully!";
                return RedirectToAction("EmployeeList"); // or wherever you want to redirect
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while inserting employee: {ex.Message}");
                TempData["ErrorMessage"] = "There was an error inserting the employee.";
                return RedirectToAction("EmployeeList"); // or handle error accordingly
            }
        }

        // Display employee list (optional)
        public IActionResult EmployeeList()
        {
            List<Employee> employees = new List<Employee>();

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand("GetEmployeeDetails", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            EmployeeID = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Division = reader.GetString(3),
                            Building = reader.GetString(4),
                            Title = reader.GetString(5),
                            Room = reader.GetString(6)
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching employee details: {ex.Message}");
            }

            return View(employees);
        }
    }
}
