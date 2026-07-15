// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IElementDetailsPanelViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.Common
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditElementDefinitionViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditParameterSubscriptionViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditParameterViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditElementUsageViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.ElementDefinitionCreationViewModel;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;

    /// <summary>
    /// Interface for the <see cref="ElementDetailsPanelViewModel" /> that manages the editable element details
    /// panel, which can be reused across multiple features (Model Editor, System Representation, etc.).
    /// </summary>
    public interface IElementDetailsPanelViewModel
    {
        /// <summary>
        /// Gets the currently selected <see cref="CDP4Common.CommonData.ElementBase" /> — either an
        /// <see cref="ElementDefinition" /> or an <see cref="ElementUsage" />.
        /// </summary>
        ElementBase SelectedElement { get; }

        /// <summary>
        /// Gets or sets the <see cref="ElementDefinition" /> derived from <see cref="SelectedElement" />.
        /// </summary>
        ElementDefinition SelectedElementDefinition { get; set; }

        /// <summary>
        /// Gets the <see cref="IElementDefinitionDetailsViewModel" /> that drives the details card rows.
        /// </summary>
        IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; }

        /// <summary>
        /// Gets or sets the current <see cref="Iteration" /> the panel operates against.
        /// </summary>
        Iteration CurrentIteration { get; set; }

        /// <summary>
        /// Gets or sets the currently logged-in <see cref="DomainOfExpertise" />.
        /// </summary>
        DomainOfExpertise CurrentDomain { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Option" /> currently selected in the product tree. When set, option-dependent
        /// parameters display only the value set that matches this option. <c>null</c> means no option filtering is
        /// applied (the first available value set is used), which is appropriate for the Model Editor.
        /// </summary>
        Option CurrentOption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an asynchronous operation is in progress.
        /// </summary>
        bool IsLoading { get; set; }

        /// <summary>
        /// Gets the <see cref="IElementDefinitionCreationViewModel" /> that drives the create-element-definition popup.
        /// </summary>
        IElementDefinitionCreationViewModel ElementDefinitionCreationViewModel { get; set; }

        /// <summary>
        /// Gets the <see cref="IAddParameterViewModel" /> that drives the add-parameter popup.
        /// </summary>
        IAddParameterViewModel AddParameterViewModel { get; set; }

        /// <summary>
        /// Gets the <see cref="IEditParameterViewModel" /> that drives the edit-parameter popup.
        /// </summary>
        IEditParameterViewModel EditParameterViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IEditParameterSubscriptionViewModel" /> that drives the edit-parameter-subscription popup.
        /// </summary>
        IEditParameterSubscriptionViewModel EditParameterSubscriptionViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IEditElementDefinitionViewModel" /> that drives the edit-Element-Definition popup.
        /// </summary>
        IEditElementDefinitionViewModel EditElementDefinitionViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IEditElementUsageViewModel" /> that drives the edit-Element-Usage popup.
        /// </summary>
        IEditElementUsageViewModel EditElementUsageViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the delete-element confirmation popup.
        /// </summary>
        IConfirmCancelPopupViewModel DeleteElementPopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the delete-parameter confirmation popup.
        /// </summary>
        IConfirmCancelPopupViewModel DeleteParameterPopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the create-subscription confirmation popup.
        /// </summary>
        IConfirmCancelPopupViewModel CreateSubscriptionPopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the delete-subscription confirmation popup.
        /// </summary>
        IConfirmCancelPopupViewModel DeleteSubscriptionPopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the create-override confirmation popup.
        /// </summary>
        IConfirmCancelPopupViewModel CreateOverridePopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the delete-override confirmation popup.
        /// </summary>
        IConfirmCancelPopupViewModel DeleteOverridePopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the delete-parameter-group confirmation popup.
        /// </summary>
        IConfirmCancelPopupViewModel DeleteParameterGroupPopupViewModel { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently creating or editing a
        /// <see cref="ParameterGroup" />. Drives the create/edit popup visibility.
        /// </summary>
        bool IsOnParameterGroupEditMode { get; set; }

        /// <summary>
        /// Gets or sets the name being typed in the parameter-group create/edit form.
        /// </summary>
        string ParameterGroupName { get; set; }

        /// <summary>
        /// Gets or sets the optional containing group selected in the parameter-group create/edit form.
        /// </summary>
        ParameterGroup ParameterGroupContainingGroup { get; set; }

        /// <summary>
        /// Gets the list of <see cref="ParameterGroup" />s available for the currently selected
        /// <see cref="ElementDefinition" />.
        /// </summary>
        IReadOnlyList<ParameterGroup> AvailableParameterGroups { get; }

        /// <summary>
        /// Gets the <see cref="ParameterGroup" />s selectable as the containing group in the create/edit
        /// popup, excluding the group being edited and its descendants to prevent containing-group cycles.
        /// </summary>
        IReadOnlyList<ParameterGroup> AvailableContainingGroups { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently creating a new <see cref="ElementDefinition" />.
        /// </summary>
        bool IsOnCreationMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently adding a new <see cref="Parameter" />.
        /// </summary>
        bool IsOnAddingParameterMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently editing the selected element.
        /// </summary>
        bool IsOnEditMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently editing a <see cref="Parameter" /> or
        /// <see cref="ParameterOverride" /> through the edit-parameter popup.
        /// </summary>
        bool IsOnEditParameterMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently editing a <see cref="ParameterSubscription" />
        /// through the edit-parameter-subscription popup.
        /// </summary>
        bool IsOnEditSubscriptionMode { get; set; }

        /// <summary>
        /// Gets a value indicating whether the edit popup is currently targeting an
        /// <see cref="ElementDefinition" /> specifically — driven by <see cref="OpenEditDefinitionPopup" />.
        /// When <c>false</c> and the selection is an <see cref="ElementUsage" />, the popup targets the usage.
        /// </summary>
        bool IsEditingDefinition { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently confirming deletion of
        /// <see cref="SelectedElement" />.
        /// </summary>
        bool IsOnDeletionMode { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="SelectedElement" /> is the iteration's
        /// <see cref="Iteration.TopElement" /> — in which case deletion must be blocked.
        /// </summary>
        bool IsSelectedElementTopElement { get; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="AddingElementDefinitionAsync" /> should also
        /// create an <see cref="ElementUsage" /> of the new <see cref="ElementDefinition" /> under
        /// <see cref="SelectedElementDefinition" /> in the same operation. Set to <see langword="true" /> by
        /// the System Representation feature; the Model Editor leaves it at the default <see langword="false" />.
        /// </summary>
        bool AutoAddCreatedDefinitionAsUsage { get; set; }

        /// <summary>
        /// Sets the selected element to the supplied <see cref="CDP4Common.CommonData.ElementBase" /> and
        /// refreshes the details rows.
        /// </summary>
        /// <param name="selectedElementBase">The <see cref="CDP4Common.CommonData.ElementBase" /> to select.</param>
        void SelectElement(ElementBase selectedElementBase);

        /// <summary>
        /// Re-selects the currently selected element, refreshing the details rows. Typically called when the
        /// session is refreshed.
        /// </summary>
        /// <remarks>
        /// This toggles <see cref="IsLoading" /> so that the hosting application component re-renders. A caller that
        /// mirrors <see cref="IsLoading" /> onto its own must not invoke this from inside its own loading bracket.
        /// </remarks>
        void RefreshSelectedElement();

        /// <summary>
        /// Initializes the panel against the supplied <see cref="Iteration" />, setting the current
        /// iteration and propagating it to the child popup view models.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> to initialise against.</param>
        void Initialize(Iteration iteration);

        /// <summary>
        /// Opens the delete-confirmation popup for the supplied <see cref="Parameter" />.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> the user requested to delete.</param>
        void OpenDeleteParameterPopup(Parameter parameter);

        /// <summary>
        /// Opens the create-subscription confirmation popup for the supplied <see cref="Parameter" />.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> the user wants to subscribe to.</param>
        void OpenCreateSubscriptionPopup(Parameter parameter);

        /// <summary>
        /// Opens the delete-subscription confirmation popup for the supplied <see cref="ParameterSubscription" />.
        /// </summary>
        /// <param name="subscription">The <see cref="ParameterSubscription" /> the user wants to delete.</param>
        void OpenDeleteSubscriptionPopup(ParameterSubscription subscription);

        /// <summary>
        /// Opens the create-override confirmation popup for the supplied <see cref="Parameter" /> and host
        /// <see cref="ElementUsage" />.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> the user wants to override.</param>
        /// <param name="hostElementUsage">The host <see cref="ElementUsage" /> on which the override will be created.</param>
        void OpenCreateOverridePopup(Parameter parameter, ElementUsage hostElementUsage);

        /// <summary>
        /// Opens the delete-override confirmation popup for the supplied <see cref="ParameterOverride" />.
        /// </summary>
        /// <param name="parameterOverride">The <see cref="ParameterOverride" /> the user wants to delete.</param>
        void OpenDeleteOverridePopup(ParameterOverride parameterOverride);

        /// <summary>
        /// Opens the edit-element popup for <see cref="SelectedElement" />.
        /// </summary>
        void OpenEditElementPopup();

        /// <summary>
        /// Opens the edit popup targeting the <see cref="ElementDefinition" /> behind the current
        /// selection — the usage's referenced definition when an <see cref="ElementUsage" /> is selected,
        /// or the selected definition itself. No-op when <see cref="SelectedElementDefinition" /> is
        /// <c>null</c>.
        /// </summary>
        void OpenEditDefinitionPopup();

        /// <summary>
        /// Opens the delete-element confirmation popup for <see cref="SelectedElement" />.
        /// </summary>
        void OpenDeleteElementPopup();

        /// <summary>
        /// Opens the add-parameter popup.
        /// </summary>
        void OpenAddParameterPopup();

        /// <summary>
        /// Opens the edit-parameter popup for the supplied <see cref="ParameterOrOverrideBase" />.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase" /> the user requested to edit.</param>
        void OpenEditParameterPopup(ParameterOrOverrideBase parameter);

        /// <summary>
        /// Opens the create-element-definition popup.
        /// </summary>
        void OpenCreateElementDefinitionCreationPopup();

        /// <summary>
        /// Opens the create-parameter-group popup, resetting the form fields. No-op when no
        /// <see cref="ElementDefinition" /> is selected.
        /// </summary>
        void OpenCreateParameterGroupPopup();

        /// <summary>
        /// Opens the edit-parameter-group popup for the supplied <see cref="ParameterGroup" />,
        /// pre-populating the form fields from its current state.
        /// </summary>
        /// <param name="group">The <see cref="ParameterGroup" /> the user requested to edit.</param>
        void OpenEditParameterGroupPopup(ParameterGroup group);

        /// <summary>
        /// Saves the parameter group currently described by the create/edit form.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous save operation.</returns>
        Task SaveParameterGroupAsync();

        /// <summary>
        /// Opens the delete-confirmation popup for the supplied <see cref="ParameterGroup" />.
        /// </summary>
        /// <param name="group">The <see cref="ParameterGroup" /> the user requested to delete.</param>
        void OpenDeleteParameterGroupPopup(ParameterGroup group);

        /// <summary>
        /// Performs the deletion of the <see cref="ParameterGroup" /> captured by
        /// <see cref="OpenDeleteParameterGroupPopup" />.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous delete operation.</returns>
        Task DeleteSelectedParameterGroupAsync();

        /// <summary>
        /// Reassigns the supplied <see cref="Parameter" /> to the supplied <see cref="ParameterGroup" />
        /// (which may be <c>null</c> to ungroup the parameter).
        /// </summary>
        /// <param name="args">
        /// A tuple of the <see cref="Parameter" /> to reassign and the target <see cref="ParameterGroup" />.
        /// </param>
        /// <returns>A <see cref="Task" /> representing the asynchronous assign operation.</returns>
        Task AssignParameterToGroupAsync((Parameter Parameter, ParameterGroup Group) args);

        /// <summary>
        /// Reassigns the supplied <see cref="ParameterGroup" /> so that it is contained by the new
        /// containing group (or promoted to the top level when the new containing group is <c>null</c>).
        /// Guards against null groups, no-op moves, self-nesting and cycle creation.
        /// </summary>
        /// <param name="args">
        /// A tuple of the <see cref="ParameterGroup" /> to move and its new containing group (or <c>null</c>
        /// to move to the top level).
        /// </param>
        /// <returns>A <see cref="Task" /> representing the asynchronous reassign operation.</returns>
        Task ReassignGroupContainingAsync((ParameterGroup Group, ParameterGroup NewContaining) args);
    }
}
