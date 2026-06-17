namespace TimeManager.Backend.Models.Components
{
    public class ActionTdViewModel
    {
        public bool hasDelete { get; set; }
        public bool hasEdit { get; set; }

        public string Controller { get; set; }
        public int Id { get; set; }
    }
}
