namespace TimeManager.Backend.ViewModels
{
    public class ProfileSelectViewModel
    {
    }
    public class ProfileSelectionItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
    }

    public class SelectProfileViewModel
    {
        public List<ProfileSelectionItem> Profiles { get; set; } = new();
        public int SelectedProfileId { get; set; }
    }
}
