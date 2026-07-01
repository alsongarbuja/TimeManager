namespace TimeManager.Backend.Models.Components
{
    public class InputFieldViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "text";
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Placeholder { get; set; } = string.Empty;
        public bool Required { get; set; } = true;
        public bool AutoFocus { get; set; }
        public string Error { get; set; } = string.Empty;
        public string OnChange { get; set; } = string.Empty;
    }
}
