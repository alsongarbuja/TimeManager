using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TimeManager.Backend.TagHelpers
{
    [HtmlTargetElement("custom-input", Attributes="asp-for")]
    public class CustomInputTagHelper: TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var propertyName = For.Name;
            var labelText = For.Metadata.DisplayName ?? For.Metadata.PropertyName ?? propertyName;
            var isRequired = For.Metadata.IsRequired;
            var isMultiLine = For.Metadata.DataTypeName == "MultilineText";

            var required = isRequired ? "<span class='form-required'>*</span>" : string.Empty;
            var inputIsRequired = isRequired ? "required" : string.Empty;

            var propertyValue = For.Model?.ToString() ?? string.Empty;

            var input = isMultiLine
                ? $"<textarea name='{propertyName}' value='{propertyValue}' class='form-input'></textarea>"
                : $"<input name='{propertyName}' {inputIsRequired} value='{propertyValue}' class='form-input' type='text' />";

            output.TagName = null;

            output.Content.SetHtmlContent(
                $"<div class='form-group>'" +
                    $"<label class='form-label'>{labelText}{required}</label>" +
                    $"<div class='form-input-wrapper'>{input}</div>" +
                $"</div>"
            );
        }
    }
}
