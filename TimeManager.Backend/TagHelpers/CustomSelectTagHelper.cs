using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TimeManager.Backend.TagHelpers
{
    [HtmlTargetElement("custom-select", Attributes = "asp-for")]
    public class CustomSelectTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public required ModelExpression For { get; set; }

        [HtmlAttributeName("asp-items")]
        public IEnumerable<SelectListItem> Items { get; set; } = [];

        [HtmlAttributeName("placeholder")]
        public string? Placeholder { get; set; }

        [HtmlAttributeName("classes")]
        public string? Classes { get; set; }

        [HtmlAttributeName("required")]
        public bool? Required { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var propertyName = For.Name;
            var labelText = For.Metadata.DisplayName ?? For.Metadata.PropertyName ?? propertyName;
            var isRequired = Required ?? For.Metadata.IsRequired;

            output.TagName = null;

            var requiredAttr = isRequired ? "required" : string.Empty;
            var requiredSpan = isRequired ? "<span class='form-required'>*</span>" : string.Empty;

            var options = string.Empty;

            if (!string.IsNullOrEmpty(Placeholder))
                options += $"<option value=''>{Placeholder}</option>";

            if (Items != null)
            {
                foreach (var item in Items)
                {
                    var selected = item.Selected ? "selected" : string.Empty;
                    options += $"<option value='{item.Value}' {selected}>{item.Text}</option>";
                }
            }

            output.Content.SetHtmlContent($@"
                <div class='form-group {Classes}'>
                    <label for='{propertyName}' class='form-label fw-semibold'>
                        {labelText} {requiredSpan}
                    </label>
                    <select id='{propertyName}'
                            name='{propertyName}'
                            class='form-input'
                            {requiredAttr}>
                        {options}
                    </select>
                </div>
            ");
        }
    }
}