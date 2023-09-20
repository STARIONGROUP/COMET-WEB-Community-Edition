// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="InputEditor.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Components.BookEditor
{
    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the InputEditor component
    /// </summary>
    public partial class InputEditor<TItem>
    {
        /// <summary>
        /// Gets or sets the item for which the input is being provided
        /// </summary>
        [Parameter]
        public TItem Item { get; set; }

        /// <summary>
        /// Gets or sets the active <see cref="DomainOfExpertise"/>
        /// </summary>
        [Parameter]
        public IEnumerable<DomainOfExpertise> ActiveDomains { get; set; }

        /// <summary>
        /// Gets or sets the collection of available <see cref="Category"/>
        /// </summary>
        [Parameter]
        public IEnumerable<Category> AvailableCategories { get; set; }
        
        /// <summary>
        /// Handler for when the selected categories changed
        /// </summary>
        /// <param name="categories"></param>
        private void OnCategoryChange(IEnumerable<Category> categories)
        {
            if (this.Item is ICategorizableThing categorizableThing)
            {
                categorizableThing.Category = categories.ToList();
            }
        }
    }
}
