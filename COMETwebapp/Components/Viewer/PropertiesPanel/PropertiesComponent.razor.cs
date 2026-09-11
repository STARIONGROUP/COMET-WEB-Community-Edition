// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PropertiesComponent.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Components.Viewer.PropertiesPanel
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Extensions;

    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// The properties component used for displaying data about the selected primitive
    /// </summary>
    public partial class PropertiesComponent : DisposableComponent
    {
        /// <summary>
        /// Gets or sets the <see cref="IPropertiesComponentViewModel" />
        /// </summary>
        [Parameter]
        public IPropertiesComponentViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the content rendered next to the Submit button, used by the host to place panel-level
        /// actions such as the collapse chevron.
        /// </summary>
        [Parameter]
        public RenderFragment HeaderActions { get; set; }

        /// <summary>
        /// Gets the properties component title
        /// </summary>
        private string Title => this.ViewModel.SelectionMediator.SelectedSceneObject is not null ? this.ViewModel.SelectionMediator.SelectedSceneObject.ElementBase.Name + " - Properties:" : "Properties";

        /// <summary>
        /// Gets or sets a value indicating whether the submit confirmation dialog is visible
        /// </summary>
        private bool ShowSubmitDialog { get; set; }
        
        /// <summary>
        /// Builds the CSS class for a parameter label, marking it as selected and/or changed.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /> the label represents</param>
        /// <returns>The space-separated CSS class list</returns>
        private string GetParameterItemCssClass(ParameterBase parameter)
        {
            var classNames = "parameter-item";

            if (parameter == this.ViewModel.SelectedParameter)
            {
                classNames += " parameter-item-selected";
            }

            if (this.ViewModel.HasChanges(parameter))
            {
                classNames += " parameter-item-changed";
            }

            return classNames;
        }

        /// <summary>
        /// Opens the submit confirmation dialog so the user can review and adjust the pending changes before persisting them
        /// </summary>
        private void OpenSubmitDialog()
        {
            this.ViewModel.OnSubmitDialogOpened();
            this.ShowSubmitDialog = true;
        }

        /// <summary>
        /// Confirms the pending changes by submitting them through the view model, closing the dialog only when the write
        /// succeeds so a failed submit can be retried.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ConfirmSubmit()
        {
            var result = await this.ViewModel.OnSubmit();

            if (result.IsSuccess)
            {
                this.ShowSubmitDialog = false;
            }
        }

        /// <summary>
        /// Handles the closing of the submit confirmation dialog, keeping the view model editor caches in sync
        /// </summary>
        private void OnSubmitDialogClosed()
        {
            this.ViewModel.OnSubmitDialogClosed();
        }

        /// <summary>
        /// Reverts an unsubmitted change on the given parameter from the properties panel
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /> whose change should be discarded</param>
        private void RevertChange(ParameterBase parameter)
        {
            this.ViewModel.RevertChange(parameter);
        }

        /// <summary>
        /// Reverts an unsubmitted change on the given parameter from the dialog, closing the dialog when nothing remains
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /> whose change should be discarded</param>
        private void RevertChangeInDialog(ParameterBase parameter)
        {
            this.ViewModel.RevertChange(parameter);

            if (this.ViewModel.GetChangedParameters().Count == 0)
            {
                this.ShowSubmitDialog = false;
            }
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.IsVisible,
                    x => x.ViewModel.SelectedParameter,
                    x => x.ViewModel.ParameterHaveChanges)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
