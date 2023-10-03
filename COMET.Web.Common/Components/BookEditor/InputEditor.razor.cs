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
    using System.Text.Json;

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
        /// Gets or sets the <see cref="HttpClient"/>
        /// </summary>
        [Inject]
        public HttpClient HttpClient { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IOptions{GlobalOptions}"/>
        /// </summary>
        [Inject]
        public IOptions<GlobalOptions> Options { get; set; }

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
        /// Sets if the component should show the name field
        /// </summary>
        private bool showName;

        /// <summary>
        /// The name of the ShowName property on the configuration file
        /// </summary>
        private const string showNameConfigurationProperty = "ShowName";

        /// <summary>
        /// Sets if the component should show the shorname field
        /// </summary>
        private bool showShortName;

        /// <summary>
        /// The name of the ShowShortName property on the configuration file
        /// </summary>
        private const string showShortNameConfigurationProperty = "ShowShortName";

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

            var jsonFile = this.Options.Value.JsonConfigurationFile ?? "BookInputConfiguration.json";

            try
            {
                var path = ContentPathBuilder.BuildPath(jsonFile);
                var jsonContent = await this.HttpClient.GetStreamAsync(path);
                var configurations = JsonSerializer.Deserialize<Dictionary<string, bool>>(jsonContent);

                if (configurations.TryGetValue(showNameConfigurationProperty, out var showNameValue))
                {
                    this.showName = showNameValue;
                }

                if (configurations.TryGetValue(showShortNameConfigurationProperty, out var showShortNameValue))
                {
                    this.showShortName = showShortNameValue;
                }

                if (this.Item is IOwnedThing ownedThing)
                {
                    ownedThing.Owner = this.SessionService.Session.ActivePerson.DefaultDomain;
                }
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Error while getting the configuration file.");
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
