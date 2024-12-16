// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CardField.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Components.CardView
{
    using System.Text.RegularExpressions;

    using FastMember;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Rendering;

    /// <summary>
    /// A component that represents a data field in a <see cref="CardView{T}"/>'s ItemTemplate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CardField<T> : ComponentBase
    {
        /// <summary>
        /// The <see cref="TypeAccessor"/> used to read properties from the instance of <see cref="T"/>.
        /// This is a static property on a generic type, so it will have different static values for each used generic type in the application
        /// </summary>
        private static TypeAccessor typeAccessor { get; set; }

        /// <summary>
        /// Initializes the static properties of this <see cref="CardField{T}"/> class
        /// </summary>
        static CardField()
        {
            typeAccessor = TypeAccessor.Create(typeof(T));
        }

        /// <summary>
        /// Gets or sets The parent <see cref="CardView{T}"/>t
        /// </summary>
        [CascadingParameter(Name="CardView")]
        private CardView<T> CardView { get; set; }

        /// <summary>
        /// The SearchTerm of the <see cref="CardView{T}"/> used to visually show the SearchTerm in this <see cref="CardField{T}"/>
        /// </summary>
        [CascadingParameter(Name = "SearchTerm")]
        private string SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the context of this <see cref="CardField{T}"/>
        /// </summary>
        [Parameter]
        public T Context { get; set; }

        /// <summary>
        /// Gets or sets the FieldName (propertyname of <see cref="T"/>) to show in the UI 
        /// </summary>
        [Parameter]
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that sorting is allowed for this <see cref="CardField{T}"/>
        /// </summary>
        [Parameter]
        public bool AllowSort { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that searching is allowed for this <see cref="CardField{T}"/>
        /// </summary>
        [Parameter]
        public bool AllowSearch { get; set; } = true;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.CardView.InitializeCardField(this);
        }

        /// <summary>
        /// Renders the component to the supplied <see cref="RenderTreeBuilder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="RenderTreeBuilder"/> that will receive the render output.</param>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            var value = typeAccessor[this.Context, this.FieldName].ToString();

            if (this.AllowSearch && !string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(this.SearchTerm))
            {
                var separatorPattern = $"({this.SearchTerm})";
                var result = Regex.Split(value, separatorPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(30));
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
