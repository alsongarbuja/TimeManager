using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TimeManager.Backend.TagHelpers;

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

    [HtmlAttributeName("searchable")]
    public bool Searchable { get; set; } = false;

    [HtmlAttributeName("multiple")]
    public bool Multiple { get; set; } = false;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    private readonly IHtmlGenerator _generator;

    public CustomSelectTagHelper(IHtmlGenerator generator)
    {
        _generator = generator;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        bool enhance = Searchable || Multiple;

        var classNames = new List<string> { "form-input" };
        if (enhance)
        {
            classNames.Add("tom-select");
            classNames.Add("tm-select");
        }
        if (!string.IsNullOrWhiteSpace(Classes))
        {
            classNames.Add(Classes);
        }

        var select = _generator.GenerateSelect(
            ViewContext,
            For.ModelExplorer,
            Placeholder,
            For.Name,
            Items,
            null,
            Multiple,
            new
            {
                @class = string.Join(" ", classNames)
            });

        select.Attributes["data-placeholder"] = Placeholder ?? "";
        select.Attributes["data-searchable"] = Searchable ? "true" : "false";

        if (Required ?? For.Metadata.IsRequired)
            select.Attributes["required"] = "required";

        output.Content.AppendHtml($"<div class='form-group {Classes}'>");

        output.Content.AppendHtml(
            _generator.GenerateLabel(
                ViewContext,
                For.ModelExplorer,
                For.Name,
                null,
                new { @class = "form-label fw-semibold" }));

        output.Content.AppendHtml(select);

        output.Content.AppendHtml(
            _generator.GenerateValidationMessage(
                ViewContext,
                For.ModelExplorer,
                For.Name,
                null,
                null,
                new { @class = "text-danger" }));

        output.Content.AppendHtml("</div>");
    }
}