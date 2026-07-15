using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TimeManager.Backend.TagHelpers
{
    [HtmlTargetElement("custom-input", Attributes="asp-for")]
    public class CustomInputTagHelper: TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public required ModelExpression For { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("classes")]
        public string? Classes { get; set; }

        [HtmlAttributeName("required")]
        public bool? Required { get; set; }

        [HtmlAttributeName("helperText")]
        public string? HelperText { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var propertyName = For.Name;
            var labelText = For.Metadata.DisplayName ?? For.Metadata.PropertyName ?? propertyName;
            var isRequired = Required ?? For.Metadata.IsRequired;
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
            else if (For.ModelExplorer.ModelType == typeof(DateTimeOffset) || For.ModelExplorer.ModelType == typeof(DateTimeOffset?) || For.ModelExplorer.ModelType == typeof(DateTime) || For.ModelExplorer.ModelType == typeof(DateTime?))
            {
                type = "datetime-local";
            } else if (For.ModelExplorer.ModelType == typeof(IFormFile))
            {
                type = "file";
            }

            var formattedValue = propertyValue;

            if (type == "datetime-local")
            {
                if (For.Model is DateTimeOffset dto)
                {
                    formattedValue = dto.ToLocalTime().ToString("yyyy-MM-ddTHH:mm");
                } else if (For.Model is DateTime dt)
                {
                    formattedValue = dt.ToLocalTime().ToString("yyyy-MM-ddTHH:mm");
                }
            } else if (type == "time")
            {
                if (For.Model is TimeOnly t)
                {
                    formattedValue = t.ToString("HH:mm");
                }
            }

            var hasErrors = ViewContext.ModelState.TryGetValue(propertyName, out var modelStateEntry) && modelStateEntry.Errors.Any();

            var errorMessage = hasErrors ? modelStateEntry?.Errors.First().ErrorMessage : string.Empty;

            var classes = hasErrors ? "form-input form-input-error" : "form-input";

            var input = isMultiLine
                ? $"<textarea name='{propertyName}' class='{classes}'>{System.Net.WebUtility.HtmlEncode(formattedValue)}</textarea>"
                : $"<input name='{propertyName}' {inputIsRequired} value='{formattedValue}' class='{classes}' type='{type}' />";

            if (type == "password")
            {
                input = $"<div class='password-toggle-wrapper'>" +
                            $"{input}" +
                            $"<button type='button' class='password-toggle-btn' onclick='togglePasswordVisibility(this)'>" +
                                $"<i class='ri-eye-off-line'></i>" +
                            $"</button>" +
                        $"</div>";
            }

            if (type == "datetime-local")
            {
                input = $"<div class='datetime-clear-wrapper'>" +
                            $"{input}" +
                            $"<button type='button' class='datetime-clear-btn' onclick='this.previousElementSibling.value=\"\"; this.previousElementSibling.dispatchEvent(new Event(\"change\"))'>" +
                                $"<i class='ri-close-line'></i>" +
                            $"</button>" +
                        $"</div>";
            }

            //if (type == "file")

            var helperSpan = !string.IsNullOrEmpty(HelperText)
                ? $"<span class='form-input-helper-text'>{HelperText}</span>"
                : string.Empty;

            var errorSpan = hasErrors
                   ? $"<span class='form-error'>{errorMessage}</span>"
                   : string.Empty;

            output.TagName = null;

            output.Content.SetHtmlContent(
                $"<div class='form-group {Classes}'>" +
                    $"<label class='form-label'>{labelText}{required}</label>" +
                    $"<div class='form-input-wrapper'>{input}</div>" +
                    $"{helperSpan}" +
                    $"{errorSpan}" +
                $"</div>"
            );
        }
    }
}
