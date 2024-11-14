using System;
using System.Collections.Generic;
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
        private readonly string _connectionString;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
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
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand("InsertEmployee", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Add parameters to the stored procedure
                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@Division", employee.Division);
                    command.Parameters.AddWithValue("@Building", employee.Building);
                    command.Parameters.AddWithValue("@Title", employee.Title);
                    command.Parameters.AddWithValue("@Room", employee.Room);

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                TempData["Message"] = "Employee inserted successfully!";
                return RedirectToAction("EmployeeList");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while inserting employee: {ex.Message}");
                TempData["ErrorMessage"] = "There was an error inserting the employee.";
                return RedirectToAction("EmployeeList");
            }
        }

        // Display employee list
        public IActionResult EmployeeList()
        {
            List<Employee> employees = new List<Employee>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand("GetEmployeeDetailst", connection)
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

 
        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand("DeleteEmployee", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddWithValue("@EmployeeID", id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                TempData["Message"] = "Employee deleted successfully!";
                return RedirectToAction("EmployeeList");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while deleting the employee: {ex.Message}");
                TempData["ErrorMessage"] = "There was an error deleting the employee.";
            }

            return RedirectToAction("EmployeeList");
        }

    }
}
