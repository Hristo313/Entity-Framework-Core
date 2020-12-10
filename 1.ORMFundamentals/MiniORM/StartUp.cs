namespace MiniORM
{
    using System;
    using System.Linq;

    class StartUp
    {
        static void Main(string[] args)
        {
            //var connectionString = @"Server=.;Database=MiniORM;Integrated Security=True";

            //var context = new SoftUniDbContext(connectionString);

            //context.Employees.Add(new Employee()
            //{
            //    FirstName = "asdqwd12",
            //    LastName = "d12d1d2",
            //    DepartmentId = context.Departments.First().Id,
            //    IsEmployed = true
            //});

            //var employee = context.Employees.Last();
            //employee.MiddleName = "asd";

            //var projections = context.Employees.Select(x => new EmployeeViewModel()
            //{
            //    FullName = $"{x.FirstName} {x.MiddleName} {x.LastName}",
            //    IsEmployed = x.IsEmployed,
            //    Department = x.Department
            //});

            //foreach (var projection in projections)
            //{
            //    Console.WriteLine(projection);
            //}

            //context.SaveChanges();
        }
    }
}
