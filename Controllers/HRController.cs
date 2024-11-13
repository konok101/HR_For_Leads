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
        /*   public IActionResult HRDataList(int? EmployeeID)
           {
               // Fetch employees based on EmployeeID if it is passed, otherwise fetch all employees
               var employees = EmployeeID.HasValue ? FetchEmployeeDataByID(EmployeeID.Value) : FetchEmployeeData();
               return View(employees ?? new List<Employee>());
           }*/

        public IActionResult HRDataList(int? EmployeeID)
        {
            // Fetch all employees if EmployeeID is not provided
            var employees = EmployeeID.HasValue ? FetchEmployeeDataByID(EmployeeID.Value) : FetchEmployeeData();
            return View(employees ?? new List<Employee>());
        }


        [HttpPost]
        public JsonResult DeleteEmployee(int employeeId)
        {
            bool isDeleted = DeleteEmployeeByID(employeeId);
            return Json(new { success = isDeleted });
        }

        public IActionResult UpdateEmployee(int employeeId)
        {
            var employee = FetchEmployeeDataByID(employeeId)?.FirstOrDefault();
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        public IActionResult UpdateEmployee(Employee updatedEmployee)
        {
            if (updatedEmployee.EmployeeID == 0 || string.IsNullOrEmpty(updatedEmployee.FirstName) || string.IsNullOrEmpty(updatedEmployee.LastName))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View(updatedEmployee);
            }

            try
            {
                bool isUpdated = UpdateEmployeeInDB(updatedEmployee);
                if (isUpdated)
                {
                    _logger.LogInformation($"Employee updated successfully: {updatedEmployee.EmployeeID}");
                    return RedirectToAction("HRDataList", new { EmployeeID = updatedEmployee.EmployeeID });
                }
                else
                {
                    _logger.LogWarning($"Employee update failed for EmployeeID: {updatedEmployee.EmployeeID}");
                    ModelState.AddModelError("", "An error occurred while updating the employee.");
                    return View(updatedEmployee);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while updating employee {updatedEmployee.EmployeeID}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while processing your request.");
                return View(updatedEmployee);
            }
        }

        private List<Employee> FetchEmployeeData()
        {
            var employees = new List<Employee>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("GetEmployeeDetails", connection) { CommandType = CommandType.StoredProcedure };
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
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching HR data: {ex.Message}");
            }

            return employees;
        }

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

        private bool UpdateEmployeeInDB(Employee updatedEmployee)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("dbo.UpdateEmployee", connection) { CommandType = CommandType.StoredProcedure };

                command.Parameters.AddWithValue("@EmployeeID", updatedEmployee.EmployeeID);
                command.Parameters.AddWithValue("@FirstName", updatedEmployee.FirstName);
                command.Parameters.AddWithValue("@LastName", updatedEmployee.LastName);
                command.Parameters.AddWithValue("@Division", updatedEmployee.Division);
                command.Parameters.AddWithValue("@Building", updatedEmployee.Building);
                command.Parameters.AddWithValue("@Title", updatedEmployee.Title);
                command.Parameters.AddWithValue("@Room", updatedEmployee.Room);

                connection.Open();
                var result = command.ExecuteScalar();

                return result != null && Convert.ToInt32(result) == 1;
            }
        }
    }
}
