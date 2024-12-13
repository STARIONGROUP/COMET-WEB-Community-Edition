
namespace COMET.Web.Common.Components.CardView
{
    using System.Text.RegularExpressions;

    using FastMember;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Rendering;

    public class CardField<T> : ComponentBase
    {
        private static TypeAccessor typeAccessor { get; set; }

        static CardField()
        {
            typeAccessor = TypeAccessor.Create(typeof(T));
        }

        [CascadingParameter(Name="CardView")]
        private CardView<T> CardView { get; set; }

        [CascadingParameter(Name = "SearchTerm")]
        private string SearchTerm { get; set; }

        [Parameter]
        public T Context { get; set; }

        [Parameter]
        public string FieldName { get; set; }

        [Parameter]
        public bool AllowSort { get; set; } = true;

        [Parameter]
        public bool AllowSearch { get; set; } = true;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.CardView.RegisterCardField(this);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            var value = typeAccessor[this.Context, this.FieldName].ToString();

            if (this.AllowSearch && !string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(this.SearchTerm))
            {
                var separatorPattern = $"({this.SearchTerm})";
                var result = Regex.Split(value, separatorPattern, RegexOptions.IgnoreCase);
                var elementCounter = 0;

                foreach (var element in result)
                {
                    if (string.Equals(element, this.SearchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        builder.OpenElement(elementCounter, "span");
                        elementCounter++;
                        builder.AddAttribute(elementCounter, "class", "search-mark");
                        elementCounter++;
                        builder.AddContent(elementCounter, element);
                        elementCounter++;
                        builder.CloseElement();
                    }
                    else
                    {
                        builder.AddContent(elementCounter, element);
                        elementCounter++;
                    }
                }
            }
            else
            {
                builder.AddContent(0, value);
            }
        }
    }
}