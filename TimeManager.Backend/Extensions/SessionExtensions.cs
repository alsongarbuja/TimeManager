namespace TimeManager.Backend.Extensions
{
    public static class SessionExtensions
    {
        public static int? GetDepartmentId(this ISession session)
            => session.GetInt32("DepartmentId");

        public static int? GetCurrentUserJobProfileId(this ISession session) => session.GetInt32("JobProfileId");
    }
}
