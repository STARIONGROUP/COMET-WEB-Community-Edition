// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="InputEditor.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Components.BookEditor
{
    using CDP4Common.ReportingData;
    using CDP4Common.Validation;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.ViewModels.Components.BookEditor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using INamedThing = CDP4Common.CommonData.INamedThing;
    using IOwnedThing = CDP4Common.EngineeringModelData.IOwnedThing;
    using IShortNamedThing = CDP4Common.CommonData.IShortNamedThing;

    /// <summary>
    /// Support class for the BookEditorPopup component
    /// </summary>
    public partial class EditorPopup
    {
        /// <summary>
        /// Sets if the component should show the name field
        /// </summary>
        private bool showName;

        /// <summary>
        /// Sets if the component should show the shorname field
        /// </summary>
        private bool showShortName;

        /// <summary>
        /// Gets or sets the <see cref="IEditorPopupViewModel" />
        /// </summary>
        [Parameter]
        public IEditorPopupViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpClient" />
        /// </summary>
        [Inject]
        public IConfigurationService ConfigurationService { get; set; }

        /// <summary>
        /// Gets or sets the injected <see cref="IValidationService" />
        /// </summary>
        [Inject]
        public IValidationService ValidationService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IOptions{TOptions}" />
        /// </summary>
        [Inject]
        public IOptions<GlobalOptions> Options { get; set; }

        /// <summary>
        /// Gets or sets the injected <see cref="ILogger{TCategoryName}"/>
        /// </summary>
        [Inject]
        public ILogger<EditorPopup> Logger { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            this.Disposables.Add(this.ViewModel.ValidationErrors.Connect().Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
            var configurations = this.ConfigurationService.ServerConfiguration.BookInputConfiguration;

            //The fields will be shown by default
            this.showName = configurations?.ShowName ?? true;
            this.showShortName = configurations?.ShowShortName ?? true;
        }

        /// <summary>
        /// Handler for when the confirm button has been clicked
        /// </summary>
        private Task OnConfirmClick()
        {
            var validationErrors = new List<string>();

            if (this.ViewModel.Item is INamedThing namedThing && this.showName)
            {
                var error = this.ValidationService.ValidateProperty(nameof(namedThing.Name), namedThing.Name);
                validationErrors.Add(error);
            }

            if (this.ViewModel.Item is IShortNamedThing shortNamedThing && this.showShortName)
            {
                var error = this.ValidationService.ValidateProperty(nameof(shortNamedThing.ShortName), shortNamedThing.ShortName);
                validationErrors.Add(error);
            }

            if (this.ViewModel.Item is IOwnedThing ownedThing)
            {
                var error = ownedThing.Owner == null ? "The thing must be owned by a DoE" : string.Empty;
                validationErrors.Add(error);
            }

            if (this.ViewModel.Item is TextualNote textualNote)
            {
                var error = string.IsNullOrEmpty(textualNote.Content) ? "The textual note must contain a content" : string.Empty;
                validationErrors.Add(error);
            }

            validationErrors = validationErrors.Where(x => !string.IsNullOrEmpty(x)).ToList();

            this.ViewModel.ValidationErrors.Edit(inner =>
            {
                inner.Clear();
                inner.AddRange(validationErrors);
            });

            return !this.ViewModel.ValidationErrors.Items.Any() ? this.ViewModel.OnConfirmClick.InvokeAsync() : Task.CompletedTask;
        }
    }
}
