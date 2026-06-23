using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TimeManager.Backend.TagHelpers
{
    [HtmlTargetElement("custom-input", Attributes="asp-for")]
    public class CustomInputTagHelper: TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var propertyName = For.Name;
            var labelText = For.Metadata.DisplayName ?? For.Metadata.PropertyName ?? propertyName;
            var isRequired = For.Metadata.IsRequired;
            var isMultiLine = For.Metadata.DataTypeName == "MultilineText";

            var required = isRequired ? "<span class='form-required'>*</span>" : string.Empty;
            var inputIsRequired = isRequired ? "required" : string.Empty;

            var propertyValue = For.Model?.ToString() ?? string.Empty;

            string type = "text";
            
            if (For.Metadata.DataTypeName == "Password")
            {
                type = "password";
            } else if (For.Metadata.DataTypeName == "Time" || For.ModelExplorer.ModelType == typeof(TimeOnly) || For.ModelExplorer.ModelType == typeof(TimeOnly?))
            {
                type = "time";
            }
            else if (For.ModelExplorer.ModelType == typeof(int) || For.ModelExplorer.ModelType == typeof(int?) ||
            For.ModelExplorer.ModelType == typeof(decimal) || For.ModelExplorer.ModelType == typeof(decimal?))
            {
                type = "number";
            }

            var hasErrors = ViewContext.ModelState.TryGetValue(propertyName, out var modelStateEntry) && modelStateEntry.Errors.Any();

            var errorMessage = hasErrors ? modelStateEntry.Errors.First().ErrorMessage : string.Empty;

            var input = isMultiLine
                ? $"<textarea name='{propertyName}' value='{propertyValue}' class='form-input'></textarea>"
                : $"<input name='{propertyName}' {inputIsRequired} value='{propertyValue}' class='form-input' type='{type}' />";

            if (type == "password")
            {
                input = $"<div class='password-toggle-wrapper'>" +
                            $"{input}" +
                            $"<button type='button' class='password-toggle-btn' onclick='togglePasswordVisibility(this)'>" +
                                $"<i class='ri-eye-off-line'></i>" +
                            $"</button>" +
                        $"</div>";
            }

            var errorSpan = hasErrors
                   ? $"<span class='form-error'>{errorMessage}</span>"
                   : string.Empty;

            output.TagName = null;

            output.Content.SetHtmlContent(
                $"<div class='form-group'>" +
                    $"<label class='form-label'>{labelText}{required}</label>" +
                    $"<div class='form-input-wrapper'>{input}</div>" +
                    $"{errorSpan}" +
                $"</div>"
            );
        }
    }
}
