using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TimeManager.Backend.TagHelpers
{
    [HtmlTargetElement("sn-cell")]
    public class SerialNumberTagHelper : TagHelper
    {
        [HtmlAttributeName("counter-name")]
        public string CounterName { get; set; } = "default";

        [HtmlAttributeName("start")]
        public int Start { get; set; } = 0;

        [ViewContext]
        public ViewContext ViewContext { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var key = $"SnCounter_{CounterName}";
            var items = ViewContext.HttpContext.Items;

            int current;
            if (!items.ContainsKey(key))
            {
                current = Start + 1;
            }
            else
            {
                current = (int)items[key]! + 1;
            }
            items[key] = current;

            output.TagName = null;
            output.Content.SetHtmlContent(
                $"<td class='table-cell'>" +
                current.ToString() +
                $"</td>"
            );
        }
    }
}
