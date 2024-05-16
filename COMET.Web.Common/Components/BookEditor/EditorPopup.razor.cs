// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditorPopup.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Components.BookEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.Validation;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.BookEditor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

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
        /// Gets or sets the <see cref="IConfiguration" />
        /// </summary>
        [Inject]
        public IConfiguration Configuration { get; set; }

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
        /// Gets or sets the injected <see cref="ILogger{TCategoryName}" />
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
            var configurations = this.Configuration.GetSection(ConfigurationKeys.ServerConfigurationKey).Get<ServerConfiguration>();

            //The fields will be shown by default
            this.showName = configurations.BookInputConfiguration?.ShowName ?? this.showName;
            this.showShortName = configurations.BookInputConfiguration?.ShowShortName ?? this.showShortName;
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
