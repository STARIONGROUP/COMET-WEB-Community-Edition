// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="InputEditor.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    using System.Text.Json;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities;
   
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Support class for the InputEditor component
    /// </summary>
    public partial class InputEditor<TItem>
    {
        /// <summary>
        /// Gets or sets the <see cref="ILogger"/>
        /// </summary>
        [Inject]
        public ILogger<InputEditor<TItem>> Logger { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISessionService"/>
        /// </summary>
        [Inject] 
        public ISessionService SessionService { get; set; }

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
        /// Gets or sets if the InputEditor should display the Name field
        /// </summary>
        [Parameter]
        public bool ShowName { get; set; }
        
        /// <summary>
        /// Gets or sets if the InputEditor should display the ShortName field
        /// </summary>
        [Parameter]
        public bool ShowShortName { get; set; }
        
        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        ///
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                if (this.Item is IOwnedThing ownedThing)
                {
                    ownedThing.Owner = this.SessionService.Session.ActivePerson.DefaultDomain;
                }

                //Name and ShortName are required fields on the SDK - setting these to - as default, as per request on the ticket.
                if (this.Item is INamedThing namedThing && !this.ShowName)
                {
                    namedThing.Name = "-";
                }

                if (this.Item is IShortNamedThing shortNamedThing && !this.ShowShortName)
                {
                    shortNamedThing.ShortName = "-";
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Exception while setting default values of the InputEditor.");
            }
        }

        /// <summary>
        /// Handler for when the selected categories changed
        /// </summary>
        /// <param name="categories">the changed categories</param>
        private void OnCategoryChange(IEnumerable<Category> categories)
        {
            if (this.Item is ICategorizableThing categorizableThing)
            {
                categorizableThing.Category = categories.ToList();
            }
        }
    }
}
