// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IModelEditorViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.CopySettings;
    using COMETwebapp.ViewModels.Components.ModelEditor.ElementDefinitionCreationViewModel;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;

    /// <summary>
    /// Interface for the <see cref="ModelEditorViewModel" />
    /// </summary>
    public interface IModelEditorViewModel : ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Gets the target <see cref="Iteration"/> />
        /// </summary>
        Iteration TargetIteration { get; set; }

        /// <summary>
        /// Gets the source <see cref="Iteration" />
        /// </summary>
        Iteration SourceIteration { get; set; }

        /// <summary>
        /// Value indicating the user is currently creating a new <see cref="ElementDefinition" />
        /// </summary>
        bool IsOnCreationMode { get; set; }

        /// <summary>
        /// Represents the selected ElementDefinitionRowViewModel
        /// </summary>
        ElementDefinition SelectedElementDefinition { get; set; }

        /// <summary>
        /// The <see cref="IElementDefinitionDetailsViewModel" />
        /// </summary>
        IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IElementDefinitionCreationViewModel" />
        /// </summary>
        IElementDefinitionCreationViewModel ElementDefinitionCreationViewModel { get; set; }

        /// <summary>
        /// Gets the <see cref="IAddParameterViewModel" />
        /// </summary>
        IAddParameterViewModel AddParameterViewModel { get; set; }

        /// <summary>
        /// Gets the <see cref="ICopySettingsViewModel" />
        /// </summary>
        ICopySettingsViewModel CopySettingsViewModel { get; set; }

        /// <summary>
        /// Value indicating the user is currently adding a new <see cref="Parameter" /> to a <see cref="ElementDefinition" />
        /// </summary>
        bool IsOnAddingParameterMode { get; set; }

        /// <summary>
        /// Value indicating the user is currently setting the Copy settings that apply when a node is dropped 
        /// </summary>
        bool IsOnCopySettingsMode { get; set; }

        /// <summary>
        /// Gets a value indicating that source model and target model are based on the same <see cref="Iteration"/>
        /// </summary>
        bool IsSourceModelSameAsTargetModel { get; }

        /// <summary>
        /// Opens the <see cref="ElementDefinitionCreation" /> popup
        /// </summary>
        void OpenCreateElementDefinitionCreationPopup();

        /// <summary>
        /// Set the selected <see cref="ElementDefinition" />
        /// </summary>
        /// <param name="selectedElementBase">The selected <see cref="ElementBase" /></param>
        void SelectElement(ElementBase selectedElementBase);

        /// <summary>
        /// Opens the <see cref="AddParameter" /> popup
        /// </summary>
        void OpenAddParameterPopup();

        /// <summary>
        /// Opens the <see cref="COMETwebapp.Components.ModelEditor.CopySettings" /> popup
        /// </summary>
        void OpenCopySettingsPopup();

        /// <summary>
        /// Add a new <see cref="ElementDefinition"/> based on an existing <see cref="ElementBase"/>
        /// </summary>
        /// <param name="elementDefinitionTree">The <see cref="ElementDefinitionTree"/> to copy the node to</param>
        /// <param name="elementBase">The <see cref="ElementBase"/> to copy</param>
        Task CopyAndAddNewElementAsync(ElementDefinitionTree elementDefinitionTree, ElementBase elementBase);

        /// <summary>
        /// Add a new <see cref="ElementUsage"/> based on an existing <see cref="ElementBase"/>
        /// </summary>
        /// <param name="fromElementBase">The <see cref="ElementBase"/> to be added as <see cref="ElementUsage"/></param>
        /// <param name="toElementBase">The <see cref="ElementBase"/> where to add the new <see cref="ElementUsage"/> to</param>
        Task AddNewElementUsageAsync(ElementBase fromElementBase, ElementBase toElementBase);

        /// <summary>
        /// Gets the currently selected <see cref="ElementBase" /> — either an <see cref="ElementDefinition" /> or
        /// an <see cref="ElementUsage" />. Mirrors <see cref="SelectedElementDefinition" /> for the
        /// <see cref="ElementDefinition" /> case, but preserves the <see cref="ElementUsage" /> identity so the
        /// delete affordance can target the usage rather than its containing definition.
        /// </summary>
        ElementBase SelectedElement { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently confirming deletion of
        /// <see cref="SelectedElement" />.
        /// </summary>
        bool IsOnDeletionMode { get; set; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> driving the delete-confirmation popup.
        /// </summary>
        IConfirmCancelPopupViewModel DeleteElementPopupViewModel { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="SelectedElement" /> is the iteration's
        /// <see cref="Iteration.TopElement" />. The Delete affordance must be disabled in that case because
        /// the iteration always requires a top element.
        /// </summary>
        bool IsSelectedElementTopElement { get; }

        /// <summary>
        /// Opens the delete-confirmation popup, composing a content message that names the selected
        /// <see cref="ElementUsage" /> and its parent <see cref="ElementDefinition" />, or names the selected
        /// <see cref="ElementDefinition" /> directly. No reference scan is performed: the COMET server
        /// cascades cleanup of any referencing <see cref="ElementUsage" />s automatically when an
        /// <see cref="ElementDefinition" /> is deleted.
        /// </summary>
        void OpenDeleteElementPopup();

        /// <summary>
        /// Performs the deletion of <see cref="SelectedElement" /> via the session service and clears the
        /// selection on success.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous delete operation.</returns>
        Task DeleteSelectedElementAsync();

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> driving the delete-confirmation popup for
        /// a <see cref="Parameter" />. Kept separate from <see cref="DeleteElementPopupViewModel" /> so the
        /// confirm callbacks remain unambiguous.
        /// </summary>
        IConfirmCancelPopupViewModel DeleteParameterPopupViewModel { get; }

        /// <summary>
        /// Opens the delete-confirmation popup for a <see cref="Parameter" />. Composes a content message
        /// that names the parameter (its <see cref="ParameterType" />) and the containing
        /// <see cref="ElementDefinition" />.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> the user requested to delete.</param>
        void OpenDeleteParameterPopup(Parameter parameter);

        /// <summary>
        /// Performs the deletion of the parameter previously passed to
        /// <see cref="OpenDeleteParameterPopup" /> via the session service. Refreshes the details rows on
        /// success and always closes the popup.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous delete operation.</returns>
        Task DeleteSelectedParameterAsync();
    }
}
