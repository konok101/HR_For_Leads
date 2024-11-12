using leads_hr_ltd.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace leads_hr_ltd.Controllers
{
    public class HRController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HRController> _logger;
        private readonly string _connectionString;

        public HRController(IConfiguration configuration, ILogger<HRController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // Display the HR Data List with optional EmployeeID filter
        public IActionResult HRDataList(int? EmployeeID)
        {
            // Fetch employees based on EmployeeID if it is passed, otherwise fetch all employees
            var employees = EmployeeID.HasValue ? FetchEmployeeDataByID(EmployeeID.Value) : FetchEmployeeData();
            return View(employees ?? new List<Employee>());
        }

        // Delete Employee Action
        [HttpPost]
        public JsonResult DeleteEmployee(int employeeId)
        {
            bool isDeleted = DeleteEmployeeByID(employeeId);
            return Json(new { success = isDeleted });
        }

        // Method to update employee (displays update form)
        public IActionResult UpdateEmployee(int employeeId)
        {
            var employee = FetchEmployeeDataByID(employeeId)?.FirstOrDefault();
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // Method to handle update form submission
        [HttpPost]
        public IActionResult UpdateEmployee(Employee updatedEmployee)
        {
            // Ensure that the EmployeeID is valid before attempting to update
            if (updatedEmployee.EmployeeID == 0)
            {
                ModelState.AddModelError("EmployeeID", "Employee ID is required");
                return View(updatedEmployee);
            }

            // Check for required fields
            if (string.IsNullOrEmpty(updatedEmployee.FirstName) || string.IsNullOrEmpty(updatedEmployee.LastName))
            {
                ModelState.AddModelError("FirstName", "First Name is required");
                ModelState.AddModelError("LastName", "Last Name is required");
                return View(updatedEmployee);
            }

            try
            {
                // Call the method to update the employee data in the database
                bool isUpdated = UpdateEmployeeInDB(updatedEmployee);
                if (isUpdated)
                {
                    // Log success (optional)
                    _logger.LogInformation($"Employee updated successfully: {updatedEmployee.EmployeeID}");

                    // Redirect to HRDataList after successful update and pass the EmployeeID if needed
                    return RedirectToAction("HRDataList", new { EmployeeID = updatedEmployee.EmployeeID });
                }
                else
                {
                    // Log the failure (you can include more details as needed)
                    _logger.LogWarning($"Employee update failed for EmployeeID: {updatedEmployee.EmployeeID}");

                    // If the update failed, add a model error and return to the view
                    ModelState.AddModelError("", "An error occurred while updating the employee.");
                    return View(updatedEmployee);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError($"An exception occurred while updating employee {updatedEmployee.EmployeeID}: {ex.Message}");

                // Handle the error (you can add more specific messages if needed)
                ModelState.AddModelError("", "An error occurred while processing your request.");
                return View(updatedEmployee);
            }
        }

        // Fetch all employees
        private List<Employee> FetchEmployeeData()
        {
            List<Employee> employees = new List<Employee>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
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

        // Fetch employee by ID
        private List<Employee> FetchEmployeeDataByID(int employeeId)
        {
            var employees = new List<Employee>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("GetEmployeeByID", connection) { CommandType = CommandType.StoredProcedure };
                command.Parameters.AddWithValue("@EmployeeID", employeeId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
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
                }
            }
            return employees;
        }

        // Delete employee by ID
        private bool DeleteEmployeeByID(int employeeId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("DeleteEmployeeByID", connection) { CommandType = CommandType.StoredProcedure };
                command.Parameters.AddWithValue("@EmployeeID", employeeId);
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        // Update employee in DB
        private bool UpdateEmployeeInDB(Employee updatedEmployee)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand("dbo.UpdateEmployee", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    command.Parameters.AddWithValue("@EmployeeID", updatedEmployee.EmployeeID);  // Add EmployeeID
                    command.Parameters.AddWithValue("@FirstName", updatedEmployee.FirstName);
                    command.Parameters.AddWithValue("@LastName", updatedEmployee.LastName);
                    command.Parameters.AddWithValue("@Division", updatedEmployee.Division);
                    command.Parameters.AddWithValue("@Building", updatedEmployee.Building);
                    command.Parameters.AddWithValue("@Title", updatedEmployee.Title);
                    command.Parameters.AddWithValue("@Room", updatedEmployee.Room);

                    connection.Open();
                    var result = command.ExecuteScalar();  // Use ExecuteScalar to get the return value

                    return result != null && Convert.ToInt32(result) == 1;  // If return value is 1, update was successful
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while updating employee: {ex.Message}");
                return false;
            }
        }
    }
}
