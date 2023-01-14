using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace Libria.TagHelpers
{
	[HtmlTargetElement("input", Attributes = "bs-for", TagStructure = TagStructure.WithoutEndTag)]
	public class BsInputTagHelper : InputTagHelper
	{
		public BsInputTagHelper(IHtmlGenerator generator) : base(generator)
		{
		}

		[HtmlAttributeName("bs-for")]
		public ModelExpression BsFor
		{
			get => For;
			set => For = value;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			base.Process(context, output);

			if (ViewContext.ViewData.ModelState.TryGetValue(For.Name, out var entry) && entry.Errors.Count > 0)
			{
				output.AddClass("is-invalid", HtmlEncoder.Default);
				output.RemoveClass("input-validation-error", HtmlEncoder.Default);
			}
		}
	}
}
