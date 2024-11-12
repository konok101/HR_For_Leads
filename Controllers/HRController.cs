using Microsoft.AspNetCore.Mvc;
using leads_hr_ltd.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace leads_hr_ltd.Controllers
{
    public class HRController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HRController> _logger;

        // Inject IConfiguration and ILogger into the constructor
        public HRController(IConfiguration configuration, ILogger<HRController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Display the employee insertion form
        public IActionResult InsertEmployeeInfo()
        {
            return View();
        }

        // Method to get employee data for the view
        public IActionResult HRDataList()
        {
            List<Employee> employees = FetchEmployeeData();
            return View(employees);
        }

        // API to get employee data as JSON
        [HttpGet("api/hr/employees")]
        public IActionResult GetEmployeeData()
        {
            List<Employee> employees = FetchEmployeeData();
            if (employees == null)
            {
                return StatusCode(500, "Internal server error");
            }
            return Ok(employees);
        }

        // Helper method to fetch data from the database
        private List<Employee> FetchEmployeeData()
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
                _logger.LogError($"An error occurred while fetching HR data: {ex.Message}");
                return null;
            }

            return employees;
        }
    }
}
