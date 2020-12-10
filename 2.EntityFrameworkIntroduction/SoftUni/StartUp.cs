using SoftUni.Data;
using SoftUni.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        public static StringBuilder sb = new StringBuilder();

        public static void Main(string[] args)
        {
            SoftUniContext context = new SoftUniContext();
            string result = RemoveTown(context);
            Console.WriteLine(result);
        }

        //Exercise 3.Employees Full Information
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context
                .Employees
                .OrderBy(e => e.EmployeeId)
                .Select(e => new
                {
                    e.FirstName,
                    e.MiddleName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary
                })               
                .ToList();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 4.Employees with Salary Over 50 000
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Where(e => e.Salary > 50000)
                .Select(e => new
                {
                    e.FirstName,
                    e.Salary
                })
                .OrderBy(e => e.FirstName)
                .ToList();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} - {e.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 5.Employees from Research and Development
        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Where(e => e.Department.Name == "Research and Development")
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    DepartmentName = e.Department.Name,
                    e.Salary
                })
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.FirstName)
                .ToList();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} from {e.DepartmentName} - {e.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 6.Adding a New Address and Updating Employee
        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            Address newAddress = new Address()
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };

            Employee employeeNakov = context
                .Employees
                .First(e => e.LastName == "Nakov");

            context.Addresses.Add(newAddress); //Can be omitted
            employeeNakov.Address = newAddress;
            context.SaveChanges();

            List<string> addresses = context
                .Employees
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e => e.Address.AddressText)
                .ToList();

            foreach (string address in addresses)
            {
                sb.AppendLine(address);
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 7.Employees and Projects
        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Where(e => e.EmployeesProjects
                      .Any(ep => ep.Project.StartDate.Year >= 2001 && ep.Project.StartDate.Year <= 2003))
                .Take(10)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    ManagerFirstName = e.Manager.FirstName,
                    ManagerLastName = e.Manager.LastName,
                    Projects = e.EmployeesProjects
                           .Select(ep => new
                           {
                               ProjectName = ep.Project.Name,
                               StartDate = ep.Project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                               EndDate = ep.Project.EndDate.HasValue ? ep.Project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)
                                                                     : "not finished"
                           })
                           .ToList()
                })
                .ToList();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - Manager: {e.ManagerFirstName} {e.ManagerLastName}");

                foreach (var p in e.Projects)
                {
                    sb.AppendLine($"--{p.ProjectName} - {p.StartDate} - {p.EndDate}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 8.Addresses by Town
        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context
                .Addresses               
                .Select(a => new
                {
                    AddressText = a.AddressText,
                    TownName = a.Town.Name,
                    EmployeesCount = a.Employees.Count
                })
                .OrderByDescending(a => a.EmployeesCount)
                .ThenBy(a => a.TownName)
                .ThenBy(a => a.AddressText)
                .Take(10)
                .ToList();

            foreach (var a in addresses)
            {
                sb.AppendLine($"{a.AddressText} , {a.TownName} {a.EmployeesCount} - employees");
            }

            return sb.ToString().TrimEnd();  
        }

        //Exercise 9.Employee 147
        public static string GetEmployee147(SoftUniContext context)
        {
            var employee147 = context
                .Employees
                .Where(e => e.EmployeeId == 147)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    Projects = e.EmployeesProjects
                                .Select(ep => ep.Project.Name)
                                .OrderBy(pn => pn)
                                .ToList()
                })
                .Single();

            sb.AppendLine($"{employee147.FirstName} {employee147.LastName} - {employee147.JobTitle}");

            foreach (var project in employee147.Projects)
            {
                sb.AppendLine(project);
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 10.Departments with More Than 5 Employees
        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context
                .Departments
                .Where(d => d.Employees.Count > 5)
                .OrderBy(d => d.Employees.Count)
                .ThenBy(d => d.Name)
                .Select(d => new
                {
                    d.Name,
                    ManagerFirstName = d.Manager.FirstName,
                    ManagerLastName = d.Manager.LastName,
                    DepEmployees = d.Employees
                                    .Select(e => new
                                    {
                                        e.FirstName,
                                        e.LastName,
                                        e.JobTitle
                                    })
                                    .OrderBy(e => e.FirstName)
                                    .ThenBy(e => e.LastName)
                                    .ToList()
                })
                .ToList();

            foreach (var d in departments)
            {
                sb.AppendLine($"{d.Name} - {d.ManagerFirstName} {d.ManagerLastName}");

                foreach (var e in d.DepEmployees)
                {
                    sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 11.Find Latest 10 Projects
        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context
                .EmployeesProjects
                .Select(ep => new
                {
                    ep.Project.Name,
                    ProjectStartDate = ep.Project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                    ep.Project.Description
                })
                .OrderBy(ep => ep.Name)
                .Take(10)
                .ToList();

            foreach (var p in projects)
            {
                sb.AppendLine($"{p.Name}");
                sb.AppendLine($"{p.Description}");
                sb.AppendLine($"{p.ProjectStartDate}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 12.Increase Salaries
        public static string IncreaseSalaries(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Where(e => e.Department.Name == "Engineering"
                         || e.Department.Name == "Tool Design"
                         || e.Department.Name == "Marketing"
                         || e.Department.Name == "Information Services")
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    IncreasedSalary = e.Salary + e.Salary * 0.12m
                })
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} (${e.IncreasedSalary:F2})");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 13.Find Employees by First Name Starting with "Sa"
        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Where(e => e.FirstName.StartsWith("Sa"))
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary
                })
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary:F2})");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 14.Delete Project by Id
        public static string DeleteProjectById(SoftUniContext context)
        {
            var projectToDelete = context
                .EmployeesProjects
                .Where(ep => ep.ProjectId == 2);

            context.EmployeesProjects.RemoveRange(projectToDelete);

            var project = context.Projects.Find(2);
            context.Projects.Remove(project);

            context.SaveChanges();

            var projects = context
                .Projects
                .Take(10)
                .ToList();

            foreach (var p in projects)
            {
                sb.AppendLine(p.Name);
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 15.Remove Town
        public static string RemoveTown(SoftUniContext context)
        {
            var townToDelete = context
                .Towns
                .First(t => t.Name == "Seattle");

            var addressesToDelete = context
                .Addresses
                .Where(a => a.TownId == townToDelete.TownId);

            int addressesCount = addressesToDelete.Count();

            var employeesOnDeletedAddresses = context
                .Employees
                .Where(e => addressesToDelete.Any(a => a.AddressId == e.AddressId));

            foreach (var e in employeesOnDeletedAddresses)
            {
                e.AddressId = null;
            }

            foreach (var address in addressesToDelete)
            {
                context.Addresses.Remove(address);
            }

            context.Towns.Remove(townToDelete);

            context.SaveChanges();

            return $"{addressesCount} addresses in {townToDelete.Name} were deleted";
        }
    }
}
