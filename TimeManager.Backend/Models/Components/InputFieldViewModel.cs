namespace TimeManager.Backend.Models.Components
{
    public class InputFieldViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } = "text";
        public string Value { get; set; }
        public string Label { get; set; }
        public string Placeholder { get; set; }
        public bool Required { get; set; } = true;
        public bool AutoFocus { get; set; }
        public string Error { get; set; }
        public string OnChange { get; set; }
    }
}
