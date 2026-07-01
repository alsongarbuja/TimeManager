namespace TimeManager.Backend.Models.Components
{
    public class ActionTdViewModel
    {
        public bool HasDelete { get; set; } = true;
        public bool HasEdit { get; set; } = true;

        public string Controller { get; set; } = string.Empty;
        public int Id { get; set; }
    }
}
