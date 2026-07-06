using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeManager.Backend.Models.AuthManagement;
using TimeManager.Backend.Models.Device_Management;
using TimeManager.Backend.Models.Employee_Management;
using TimeManager.Backend.Models.Organization_Management;
using TimeManager.Backend.Models.Punch_Management;

namespace TimeManager.Backend.Data
{
    public class HrmsDbContext: IdentityDbContext<User, Role, int>
    {
        public HrmsDbContext(DbContextOptions<HrmsDbContext> options): base(options)
        {
        }

        // Employee Management Models
        public DbSet<Employee> Employee { get; set; }
        public DbSet<EmployeeType> EmployeeType { get; set; }
        public DbSet<PayFrequency> PayFrequency { get; set; }
        public DbSet<ProfileTemplate> ProfileTemplate { get; set; }
        public DbSet<JobProfile> JobProfile { get; set; }
        //public DbSet<EmployeeDepartment> EmployeeDepartment { get; set; }

        // Organization Management Models
        public DbSet<Department> Department { get; set; }
        public DbSet<Unit> Unit { get; set; }

        // Punch Management Models
        public DbSet<PayPeriod> PayPeriod { get; set; }
        public DbSet<PunchEntry> PunchEntry { get; set; }

        // Device Management Models
        public DbSet<Kiosk> Kiosk { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProfileTemplate>()
                .HasIndex(p => new { p.RoleId, p.EmployeeTypeId, p.UnitId, p.PayFrequencyId })
                .IsUnique()
                .HasDatabaseName("UX_ProfileTemplate_EmployeeType_Role_PayFrequency_Unit");

            //modelBuilder.Entity<Employee>()
            //    .HasOne(e => e.User)
            //    .WithOne()
            //    .HasForeignKey<Employee>(e => e.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => new { e.UniqueId })
                .IsUnique();

            modelBuilder.Entity<EmployeeType>()
                .HasIndex(et => new { et.Name })
                .IsUnique();

            modelBuilder.Entity<PayFrequency>()
                .HasIndex(pf => new { pf.Name })
                .IsUnique();

            //modelBuilder.Entity<EmployeeDepartment>()
            //    .HasKey(ed => new { ed.EmployeeId, ed.DepartmentId });

            //modelBuilder.Entity<EmployeeDepartment>()
            //    .HasOne(ed => ed.Employee)
            //    .WithMany(e => e.EmployeeDepartments)
            //    .HasForeignKey(ed => ed.EmployeeId);

            //modelBuilder.Entity<EmployeeDepartment>()
            //    .HasOne(ed => ed.Department)
            //    .WithMany(d => d.EmployeeDepartments)
            //    .HasForeignKey(ed => ed.DepartmentId);

            modelBuilder.Entity<JobProfile>()
                .HasOne(jp => jp.ProfileTemplate)
                .WithMany(pt => pt.JobProfile)
                .HasForeignKey(jp => jp.ProfileTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PunchEntry>()
                .HasIndex(pe => pe.JobProfileId)
                .IsUnique()
                .HasFilter("[ClockOut] IS NULL")
                .HasDatabaseName("UX_PunchEntry_JobProfileId_OpenPunch");
        }
    }
}
