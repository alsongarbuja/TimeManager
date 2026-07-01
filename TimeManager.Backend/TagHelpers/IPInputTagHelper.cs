using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TimeManager.Backend.TagHelpers
{
    [HtmlTargetElement("ip-input", Attributes = "asp-for")]
    public class IPInputTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public required ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var propertyName = For.Name;
            var labelText = For.Metadata.DisplayName ?? For.Metadata.PropertyName ?? propertyName;
            var isRequired = For.Metadata.IsRequired;

            var requiredIndicator = isRequired ? "<span class='form-required'>*</span>" : string.Empty;
            var inputIsRequired = isRequired ? "required" : string.Empty;
            var propertyValue = For.Model?.ToString() ?? string.Empty;

            var ipPattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            var placeholder = "198.168.1.1";
            var titleMessage = "Please enter a valid IPV4 address (e.g. 192.168.1.1)";

            var input = $"<input name='{propertyName}' " +
                             $"{inputIsRequired} " +
                             $"value='{propertyValue}' " +
                             $"pattern='{ipPattern}' " +
                             $"placeholder='{placeholder}' " +
                             $"title='{titleMessage}' " +
                             $"class='form-input' " +
                             $"type='text' />";

            output.TagName = null;

            output.Content.SetHtmlContent(
                $"<div class='form-group'>" +
                    $"<label class='form-label'>{labelText}{requiredIndicator}</label>" +
                    $"<div class='form-input-wrapper'>{input}</div>" +
                $"</div>"
            );
        }
    }
}
