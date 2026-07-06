namespace TimeManager.Backend.Common
{
    public class AppConstants
    {
        public const string SUPER_ADMIN_ROLE = "SuperAdmin";
        public const string ADMIN_ROLE = "Admin";
        public const string MANAGER_ROLE = "Manager";
        public const string LEAD_ROLE = "Lead";
        public const string EMPLOYEE_ROLE = "Employee";
        public const string TEMP_EMPLOYEE_ROLE = "Temp Employee";
        public const string STUDENT_ROLE = "Student Employee";

        public static string[] AllRoles() 
        {
            return [
                SUPER_ADMIN_ROLE,
                ADMIN_ROLE,
                MANAGER_ROLE,
                LEAD_ROLE,
                EMPLOYEE_ROLE,
                TEMP_EMPLOYEE_ROLE,
                STUDENT_ROLE
                ];
        }

        public static string[] AdminRoles()
        {
            return [
                SUPER_ADMIN_ROLE,
                ADMIN_ROLE,
                ];
        }

        public static string[] TopLevelRoles()
        {
            return [
                SUPER_ADMIN_ROLE,
                ADMIN_ROLE,
                MANAGER_ROLE,
                ];
        }

        public static string[] EmployeeRoles()
        {
            return [
                EMPLOYEE_ROLE,
                TEMP_EMPLOYEE_ROLE,
                STUDENT_ROLE,
                LEAD_ROLE,
                ];
        }
    }
}
