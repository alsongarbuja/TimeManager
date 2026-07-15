namespace TimeManager.Backend.ViewModels.PartialViews
{
    public class ButtonViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = "button";
        public string Classes { get; set; } = String.Empty;
        public bool Loading { get; set; } = false;
        public bool Disabled { get; set; } = false;
        public string Text { get; set; } = string.Empty;
        public string? IconClass { get; set; } = string.Empty;

    }
}
