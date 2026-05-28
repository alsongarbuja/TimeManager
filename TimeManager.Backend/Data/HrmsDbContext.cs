using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.Models.Organization_Management;
using TimeManager.Backend.Models.Punch_Management;

namespace TimeManager.Backend.Data
{
    public class HrmsDbContext: DbContext
    {
        public HrmsDbContext(DbContextOptions<HrmsDbContext> options): base(options)
        {
        }

        // Employee Management Models
        public DbSet<Employee> Employee { get; set; }
        public DbSet<EmployeeType> EmployeeType { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<PayFrequency> PayFrequency { get; set; }
        public DbSet<ProfileTemplate> ProfileTemplate { get; set; }
        public DbSet<JobProfile> JobProfile { get; set; }

        // Organization Management Models
        public DbSet<Department> Department { get; set; }
        public DbSet<Unit> Unit { get; set; }

        // Punch Management Models
        public DbSet<PayPeriod> PayPeriod { get; set; }
        public DbSet<PunchEntry> PunchEntry { get; set; }
    }
}
