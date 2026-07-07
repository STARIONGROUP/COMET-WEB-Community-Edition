// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDetailsPanelViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditElementDefinitionViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditParameterSubscriptionViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditParameterViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditElementUsageViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.ElementDefinitionCreationViewModel;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// ViewModel that manages the editable element details panel — the card showing parameter rows plus
    /// all associated CRUD popups. Can be reused by multiple features (Model Editor, System Representation, …).
    /// </summary>
    public class ElementDetailsPanelViewModel : DisposableObject, IElementDetailsPanelViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" /> used to perform session operations.
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The <see cref="ILogger{T}" /> used to record any exception thrown by the operation pipelines so
        /// that errors surface in the application log without aborting the popup-close path.
        /// </summary>
        private readonly ILogger<ElementDetailsPanelViewModel> logger;

        /// <summary>
        /// Backing field for <see cref="IsLoading" />.
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Backing field for <see cref="IsOnCreationMode" />.
        /// </summary>
        private bool isOnCreationMode;

        /// <summary>
        /// Backing field for <see cref="IsOnAddingParameterMode" />.
        /// </summary>
        private bool isOnAddingParameterMode;

        /// <summary>
        /// Backing field for <see cref="IsOnEditMode" />.
        /// </summary>
        private bool isOnEditMode;

        /// <summary>
        /// Backing field for <see cref="IsEditingDefinition" />.
        /// </summary>
        private bool isEditingDefinition;

        /// <summary>
        /// Backing field for <see cref="IsOnDeletionMode" />.
        /// </summary>
        private bool isOnDeletionMode;

        /// <summary>
        /// Backing field for <see cref="IsOnEditParameterMode" />.
        /// </summary>
        private bool isOnEditParameterMode;

        /// <summary>
        /// Backing field for <see cref="IsOnEditSubscriptionMode" />.
        /// </summary>
        private bool isOnEditSubscriptionMode;

        /// <summary>
        /// Backing field for <see cref="SelectedElement" />.
        /// </summary>
        private ElementBase selectedElement;

        /// <summary>
        /// The <see cref="ElementUsage" /> currently selected in the tree (or <c>null</c> when the
        /// selection is an <see cref="ElementDefinition" />). Captured during <see cref="SelectElement" />
        /// so that the rendered <see cref="ElementDefinitionDetailsRowViewModel" /> rows can compute
        /// override-related affordances against the host usage.
        /// </summary>
        private ElementUsage selectedHostElementUsage;

        /// <summary>
        /// The <see cref="Parameter" /> the user requested to delete, captured when
        /// <see cref="OpenDeleteParameterPopup" /> is invoked and consumed by
        /// <see cref="DeleteSelectedParameterAsync" />.
        /// </summary>
        private Parameter selectedParameterToDelete;

        /// <summary>
        /// The <see cref="Parameter" /> the user requested to subscribe to, captured when
        /// <see cref="OpenCreateSubscriptionPopup" /> is invoked and consumed by
        /// <see cref="CreateSubscriptionAsync" />.
        /// </summary>
        private Parameter selectedParameterToSubscribe;

        /// <summary>
        /// The <see cref="ParameterSubscription" /> the user requested to delete, captured when
        /// <see cref="OpenDeleteSubscriptionPopup" /> is invoked and consumed by
        /// <see cref="DeleteSelectedSubscriptionAsync" />.
        /// </summary>
        private ParameterSubscription selectedSubscriptionToDelete;

        /// <summary>
        /// The <see cref="Parameter" /> the user requested to override, captured when
        /// <see cref="OpenCreateOverridePopup" /> is invoked and consumed by
        /// <see cref="CreateOverrideAsync" />.
        /// </summary>
        private Parameter selectedParameterToOverride;

        /// <summary>
        /// The <see cref="ElementUsage" /> on which the user requested to create a
        /// <see cref="ParameterOverride" />, captured alongside <see cref="selectedParameterToOverride" />
        /// when <see cref="OpenCreateOverridePopup" /> is invoked.
        /// </summary>
        private ElementUsage selectedHostUsageForOverride;

        /// <summary>
        /// The <see cref="ParameterOverride" /> the user requested to delete, captured when
        /// <see cref="OpenDeleteOverridePopup" /> is invoked and consumed by
        /// <see cref="DeleteSelectedOverrideAsync" />.
        /// </summary>
        private ParameterOverride selectedOverrideToDelete;

        /// <summary>
        /// Backing field for <see cref="IsOnParameterGroupEditMode" />.
        /// </summary>
        private bool isOnParameterGroupEditMode;

        /// <summary>
        /// Backing field for <see cref="ParameterGroupName" />.
        /// </summary>
        private string parameterGroupName;

        /// <summary>
        /// Backing field for <see cref="ParameterGroupContainingGroup" />.
        /// </summary>
        private ParameterGroup parameterGroupContainingGroup;

        /// <summary>
        /// The <see cref="ParameterGroup" /> currently being edited, or <c>null</c> when the popup is in
        /// create mode.
        /// </summary>
        private ParameterGroup parameterGroupUnderEdit;

        /// <summary>
        /// The <see cref="ParameterGroup" /> the user requested to delete, captured when
        /// <see cref="OpenDeleteParameterGroupPopup" /> is invoked and consumed by
        /// <see cref="DeleteSelectedParameterGroupAsync" />.
        /// </summary>
        private ParameterGroup parameterGroupToDelete;

        /// <summary>
        /// Creates a new instance of <see cref="ElementDetailsPanelViewModel" />.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" />.</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /> passed to child popup view models.</param>
        /// <param name="logger">The <see cref="ILogger{T}" /> used to record operation-pipeline exceptions.</param>
        public ElementDetailsPanelViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<ElementDetailsPanelViewModel> logger)
        {
            this.sessionService = sessionService;
            this.logger = logger;

            var eventCallbackFactory = new EventCallbackFactory();

            this.ElementDefinitionCreationViewModel = new COMETwebapp.ViewModels.Components.ModelEditor.ElementDefinitionCreationViewModel.ElementDefinitionCreationViewModel(sessionService, messageBus)
            {
                OnValidSubmit = eventCallbackFactory.Create(this, this.AddingElementDefinitionAsync)
            };

            this.AddParameterViewModel = new COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel.AddParameterViewModel(sessionService, messageBus)
            {
                OnParameterAdded = eventCallbackFactory.Create(this, () => this.IsOnAddingParameterMode = false)
            };

            this.EditParameterViewModel = new COMETwebapp.ViewModels.Components.ModelEditor.EditParameterViewModel.EditParameterViewModel(sessionService, messageBus)
            {
                OnParameterEdited = eventCallbackFactory.Create(this, () => this.IsOnEditParameterMode = false)
            };

            this.EditParameterSubscriptionViewModel = new COMETwebapp.ViewModels.Components.ModelEditor.EditParameterSubscriptionViewModel.EditParameterSubscriptionViewModel(sessionService, messageBus)
            {
                OnSubscriptionEdited = eventCallbackFactory.Create(this, () => this.IsOnEditSubscriptionMode = false)
            };

            this.DeleteElementPopupViewModel = new ConfirmCancelPopupViewModel
            {
                HeaderText = "Delete element",
                ConfirmRenderStyle = ButtonRenderStyle.Danger,
                CancelRenderStyle = ButtonRenderStyle.Secondary,
                OnCancel = eventCallbackFactory.Create(this, this.OnDeleteCancelled),
                OnConfirm = eventCallbackFactory.Create(this, this.DeleteSelectedElementAsync)
            };

            this.DeleteParameterPopupViewModel = new ConfirmCancelPopupViewModel
            {
                HeaderText = "Delete parameter",
                ConfirmRenderStyle = ButtonRenderStyle.Danger,
                CancelRenderStyle = ButtonRenderStyle.Secondary,
                OnCancel = eventCallbackFactory.Create(this, this.OnDeleteParameterCancelled),
                OnConfirm = eventCallbackFactory.Create(this, this.DeleteSelectedParameterAsync)
            };

            this.CreateSubscriptionPopupViewModel = new ConfirmCancelPopupViewModel
            {
                HeaderText = "Create Parameter Subscription",
                ConfirmRenderStyle = ButtonRenderStyle.Primary,
                CancelRenderStyle = ButtonRenderStyle.Secondary,
                OnCancel = eventCallbackFactory.Create(this, this.OnCreateSubscriptionCancelled),
                OnConfirm = eventCallbackFactory.Create(this, this.CreateSubscriptionAsync)
            };

            this.DeleteSubscriptionPopupViewModel = new ConfirmCancelPopupViewModel
            {
                HeaderText = "Delete Parameter Subscription",
                ConfirmRenderStyle = ButtonRenderStyle.Danger,
                CancelRenderStyle = ButtonRenderStyle.Secondary,
                OnCancel = eventCallbackFactory.Create(this, this.OnDeleteSubscriptionCancelled),
                OnConfirm = eventCallbackFactory.Create(this, this.DeleteSelectedSubscriptionAsync)
            };

            this.CreateOverridePopupViewModel = new ConfirmCancelPopupViewModel
            {
                HeaderText = "Create Parameter Override",
                ConfirmRenderStyle = ButtonRenderStyle.Primary,
                CancelRenderStyle = ButtonRenderStyle.Secondary,
                OnCancel = eventCallbackFactory.Create(this, this.OnCreateOverrideCancelled),
                OnConfirm = eventCallbackFactory.Create(this, this.CreateOverrideAsync)
            };

            this.DeleteOverridePopupViewModel = new ConfirmCancelPopupViewModel
            {
                HeaderText = "Delete Parameter Override",
                ConfirmRenderStyle = ButtonRenderStyle.Danger,
                CancelRenderStyle = ButtonRenderStyle.Secondary,
                OnCancel = eventCallbackFactory.Create(this, this.OnDeleteOverrideCancelled),
                OnConfirm = eventCallbackFactory.Create(this, this.DeleteSelectedOverrideAsync)
            };

            this.EditElementDefinitionViewModel = new COMETwebapp.ViewModels.Components.ModelEditor.EditElementDefinitionViewModel.EditElementDefinitionViewModel(sessionService, messageBus)
            {
                OnValidSubmit = eventCallbackFactory.Create(this, this.EditElementDefinitionAsync)
            };

            this.EditElementUsageViewModel = new COMETwebapp.ViewModels.Components.ModelEditor.EditElementUsageViewModel.EditElementUsageViewModel(sessionService, messageBus)
            {
                OnValidSubmit = eventCallbackFactory.Create(this, this.EditElementUsageAsync)
            };

            this.DeleteParameterGroupPopupViewModel = new ConfirmCancelPopupViewModel
            {
                HeaderText = "Delete Parameter Group",
                ConfirmRenderStyle = ButtonRenderStyle.Danger,
                CancelRenderStyle = ButtonRenderStyle.Secondary,
                OnCancel = eventCallbackFactory.Create(this, this.OnDeleteParameterGroupCancelled),
                OnConfirm = eventCallbackFactory.Create(this, this.DeleteSelectedParameterGroupAsync)
            };
        }

        /// <summary>
        /// Gets the currently selected <see cref="ElementBase" /> — preserves the <see cref="ElementUsage" />
        /// identity so that delete operates on the usage rather than its containing definition.
        /// </summary>
        public ElementBase SelectedElement
        {
            get => this.selectedElement;
            private set => this.RaiseAndSetIfChanged(ref this.selectedElement, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ElementDefinition" /> derived from <see cref="SelectedElement" />.
        /// </summary>
        public ElementDefinition SelectedElementDefinition { get; set; }

        /// <summary>
        /// Gets the <see cref="IElementDefinitionDetailsViewModel" /> that drives the details card rows.
        /// </summary>
        public IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; } = new ElementDefinitionDetailsViewModel();

        /// <summary>
        /// Gets or sets the current <see cref="Iteration" /> the panel operates against.
        /// </summary>
        public Iteration CurrentIteration { get; set; }

        /// <summary>
        /// Gets or sets the currently logged-in <see cref="DomainOfExpertise" />.
        /// </summary>
        public DomainOfExpertise CurrentDomain { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Option" /> currently selected in the product tree. When set, option-dependent
        /// parameters display only the value set that matches this option. <c>null</c> means no option filtering is
        /// applied (the first available value set is used), which is appropriate for the Model Editor.
        /// </summary>
        public Option CurrentOption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an asynchronous operation is in progress.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.RaiseAndSetIfChanged(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets the <see cref="IElementDefinitionCreationViewModel" /> that drives the create-element-definition popup.
        /// </summary>
        public IElementDefinitionCreationViewModel ElementDefinitionCreationViewModel { get; set; }

        /// <summary>
        /// Gets the <see cref="IAddParameterViewModel" /> that drives the add-parameter popup.
        /// </summary>
        public IAddParameterViewModel AddParameterViewModel { get; set; }

        /// <summary>
        /// Gets the <see cref="IEditParameterViewModel" /> that drives the edit-parameter popup.
        /// </summary>
        public IEditParameterViewModel EditParameterViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IEditParameterSubscriptionViewModel" /> that drives the edit-parameter-subscription popup.
        /// </summary>
        public IEditParameterSubscriptionViewModel EditParameterSubscriptionViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IEditElementDefinitionViewModel" /> that drives the edit-Element-Definition popup.
        /// </summary>
        public IEditElementDefinitionViewModel EditElementDefinitionViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IEditElementUsageViewModel" /> that drives the edit-Element-Usage popup.
        /// </summary>
        public IEditElementUsageViewModel EditElementUsageViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the delete-element confirmation popup.
        /// </summary>
        public IConfirmCancelPopupViewModel DeleteElementPopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the delete-parameter confirmation popup.
        /// </summary>
        public IConfirmCancelPopupViewModel DeleteParameterPopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the create-subscription confirmation popup.
        /// </summary>
        public IConfirmCancelPopupViewModel CreateSubscriptionPopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the delete-subscription confirmation popup.
        /// </summary>
        public IConfirmCancelPopupViewModel DeleteSubscriptionPopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the create-override confirmation popup.
        /// </summary>
        public IConfirmCancelPopupViewModel CreateOverridePopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the delete-override confirmation popup.
        /// </summary>
        public IConfirmCancelPopupViewModel DeleteOverridePopupViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the delete-parameter-group confirmation popup.
        /// </summary>
        public IConfirmCancelPopupViewModel DeleteParameterGroupPopupViewModel { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently creating or editing a
        /// <see cref="ParameterGroup" />. Drives the create/edit popup visibility.
        /// </summary>
        public bool IsOnParameterGroupEditMode
        {
            get => this.isOnParameterGroupEditMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnParameterGroupEditMode, value);
        }

        /// <summary>
        /// Gets or sets the name being typed in the parameter-group create/edit form.
        /// </summary>
        public string ParameterGroupName
        {
            get => this.parameterGroupName;
            set => this.RaiseAndSetIfChanged(ref this.parameterGroupName, value);
        }

        /// <summary>
        /// Gets or sets the optional containing group selected in the parameter-group create/edit form.
        /// </summary>
        public ParameterGroup ParameterGroupContainingGroup
        {
            get => this.parameterGroupContainingGroup;
            set => this.RaiseAndSetIfChanged(ref this.parameterGroupContainingGroup, value);
        }

        /// <summary>
        /// Gets the list of <see cref="ParameterGroup" />s available for the currently selected
        /// <see cref="ElementDefinition" />, or an empty list when no element is selected.
        /// </summary>
        public IReadOnlyList<ParameterGroup> AvailableParameterGroups
            => this.SelectedElementDefinition?.ParameterGroup.ToList() ?? [];

        /// <summary>
        /// Gets the <see cref="ParameterGroup" />s that may be chosen as the containing group in the
        /// create/edit popup. When editing an existing group this excludes the group itself and all of its
        /// descendants, so the user cannot create a containing-group cycle.
        /// </summary>
        public IReadOnlyList<ParameterGroup> AvailableContainingGroups
        {
            get
            {
                var groups = this.AvailableParameterGroups;

                if (this.parameterGroupUnderEdit is null)
                {
                    return groups;
                }

                return groups.Where(g => !WouldCreateContainingGroupCycle(this.parameterGroupUnderEdit, g)).ToList();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently creating a new <see cref="ElementDefinition" />.
        /// </summary>
        public bool IsOnCreationMode
        {
            get => this.isOnCreationMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnCreationMode, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently adding a new <see cref="Parameter" />.
        /// </summary>
        public bool IsOnAddingParameterMode
        {
            get => this.isOnAddingParameterMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnAddingParameterMode, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently editing the selected element.
        /// </summary>
        public bool IsOnEditMode
        {
            get => this.isOnEditMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnEditMode, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently editing a <see cref="Parameter" /> or
        /// <see cref="ParameterOverride" /> through the edit-parameter popup.
        /// </summary>
        public bool IsOnEditParameterMode
        {
            get => this.isOnEditParameterMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnEditParameterMode, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently editing a <see cref="ParameterSubscription" />
        /// through the edit-parameter-subscription popup.
        /// </summary>
        public bool IsOnEditSubscriptionMode
        {
            get => this.isOnEditSubscriptionMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnEditSubscriptionMode, value);
        }

        /// <summary>
        /// Gets a value indicating whether the edit popup is currently targeting an
        /// <see cref="ElementDefinition" /> specifically — either because an
        /// <see cref="ElementDefinition" /> was selected, or because the user clicked "Edit Element
        /// Definition" while an <see cref="ElementUsage" /> is selected (which edits the usage's
        /// referenced definition). When <c>false</c> and the selection is an
        /// <see cref="ElementUsage" />, the popup targets the usage itself.
        /// </summary>
        public bool IsEditingDefinition
        {
            get => this.isEditingDefinition;
            private set => this.RaiseAndSetIfChanged(ref this.isEditingDefinition, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently confirming deletion of
        /// <see cref="SelectedElement" />.
        /// </summary>
        public bool IsOnDeletionMode
        {
            get => this.isOnDeletionMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnDeletionMode, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="AddingElementDefinitionAsync" /> should also
        /// create an <see cref="ElementUsage" /> of the new <see cref="ElementDefinition" /> under
        /// <see cref="SelectedElementDefinition" /> in the same operation. Set to <see langword="true" /> by
        /// the System Representation feature; the Model Editor leaves it at the default <see langword="false" />.
        /// </summary>
        public bool AutoAddCreatedDefinitionAsUsage { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="SelectedElement" /> is the iteration's
        /// <see cref="Iteration.TopElement" /> — in which case deletion must be blocked because the
        /// iteration always requires a top element.
        /// </summary>
        public bool IsSelectedElementTopElement
            => this.SelectedElement is ElementDefinition selectedDefinition
               && this.CurrentIteration?.TopElement != null
               && selectedDefinition.Iid == this.CurrentIteration.TopElement.Iid;

        /// <summary>
        /// Sets the selected element to the supplied <see cref="ElementBase" /> and refreshes the details rows.
        /// </summary>
        /// <param name="selectedElementBase">The <see cref="ElementBase" /> to select.</param>
        public void SelectElement(ElementBase selectedElementBase)
        {
            // It is preferable to have a selection based on the Iid of the Thing
            this.ElementDefinitionDetailsViewModel.SelectedSystemNode = selectedElementBase;
            this.SelectedElement = selectedElementBase;

            this.SelectedElementDefinition = selectedElementBase switch
            {
                ElementDefinition definition => definition,
                ElementUsage usage => usage.ElementDefinition,
                _ => null
            };

            this.selectedHostElementUsage = selectedElementBase as ElementUsage;

            this.ElementDefinitionDetailsViewModel.Rows = this.SelectedElementDefinition?.Parameter.Select(x => new ElementDefinitionDetailsRowViewModel(x, this.CurrentDomain, this.selectedHostElementUsage, this.CurrentOption)).ToList();
            this.AddParameterViewModel.SetSelectedElementDefinition(this.SelectedElementDefinition);
        }

        /// <summary>
        /// Re-selects the currently selected element, refreshing the details rows from the current session
        /// cache. The element is re-resolved by Iid from <see cref="CurrentIteration" /> so that any
        /// in-place mutations (e.g. a newly-added <see cref="Parameter" />) are reflected. When the element
        /// no longer exists in the iteration (e.g. because another user deleted it) the selection is cleared.
        /// </summary>
        public void RefreshSelectedElement()
        {
            var current = this.SelectedElement ?? this.SelectedElementDefinition;

            if (current is null)
            {
                this.SelectElement(null);
                return;
            }

            var fresh = this.ResolveFromIteration(current);
            this.SelectElement(fresh);
        }

        /// <summary>
        /// Resolves the supplied <see cref="ElementBase" /> from the current iteration by Iid, returning the
        /// live instance from the cache. Returns <c>null</c> when <see cref="CurrentIteration" /> is not set
        /// or the element can no longer be found (i.e. it was deleted by another user).
        /// </summary>
        /// <param name="element">The <see cref="ElementBase" /> whose live counterpart to locate.</param>
        /// <returns>
        /// The matching live <see cref="ElementBase" /> instance, or <c>null</c> when not found.
        /// </returns>
        private ElementBase ResolveFromIteration(ElementBase element)
        {
            if (this.CurrentIteration is null || element is null)
            {
                return null;
            }

            return element switch
            {
                ElementDefinition def => this.CurrentIteration.Element.FirstOrDefault(e => e.Iid == def.Iid),
                ElementUsage usage => this.CurrentIteration.Element
                    .SelectMany(e => e.ContainedElement)
                    .FirstOrDefault(u => u.Iid == usage.Iid),
                _ => null
            };
        }

        /// <summary>
        /// Initializes the panel against the supplied <see cref="Iteration" />, setting the current
        /// iteration and propagating it to the child popup view models.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> to initialise against.</param>
        public void Initialize(Iteration iteration)
        {
            this.CurrentIteration = iteration;
            this.AddParameterViewModel.InitializeViewModel(iteration);
            this.ElementDefinitionCreationViewModel.InitializeViewModel(iteration);
        }

        /// <summary>
        /// Opens the delete-confirmation popup with a content message describing the element to delete.
        /// </summary>
        public void OpenDeleteElementPopup()
        {
            if (this.SelectedElement is null || this.IsSelectedElementTopElement)
            {
                return;
            }

            this.DeleteElementPopupViewModel.ContentText = this.SelectedElement switch
            {
                ElementUsage usage => $"You are about to delete the Element Usage '{usage.Name}' ({usage.ShortName}) from {((ElementDefinition)usage.Container).Name}. This cannot be undone.",
                ElementDefinition definition => $"You are about to delete the Element Definition '{definition.Name}' ({definition.ShortName}). This cannot be undone.",
                _ => $"You are about to delete '{this.SelectedElement.Name}'. This cannot be undone."
            };

            this.DeleteElementPopupViewModel.IsVisible = true;
            this.IsOnDeletionMode = true;
        }

        /// <summary>
        /// Performs the deletion of <see cref="SelectedElement" /> using
        /// <see cref="ISessionService.DeleteThingsWithNotification" />. Clears the selection on success
        /// and always closes the popup.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous delete operation.</returns>
        public async Task DeleteSelectedElementAsync()
        {
            var thing = this.SelectedElement;

            if (thing is null || this.IsSelectedElementTopElement)
            {
                this.CloseDeleteElementPopup();
                return;
            }

            try
            {
                this.IsLoading = true;

                var clonedContainer = thing.Container.Clone(false);
                var clonedThing = thing.Clone(false);

                var result = await this.sessionService.DeleteThingsWithNotification(clonedContainer, [clonedThing], GetDeletionNotificationDescription(thing));

                if (result.IsSuccess)
                {
                    this.SelectElement(null);
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while deleting the {Kind} with iid {Iid}", thing.GetType().Name, thing.Iid);
            }
            finally
            {
                this.IsLoading = false;
                this.CloseDeleteElementPopup();
            }
        }

        /// <summary>
        /// Closes the delete-confirmation popup and resets the deletion-mode flag.
        /// </summary>
        private void CloseDeleteElementPopup()
        {
            this.DeleteElementPopupViewModel.IsVisible = false;
            this.IsOnDeletionMode = false;
        }

        /// <summary>
        /// Cancellation handler bound to <see cref="IConfirmCancelPopupViewModel.OnCancel" />.
        /// </summary>
        private void OnDeleteCancelled()
        {
            this.CloseDeleteElementPopup();
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of a delete
        /// operation through the notification pipeline.
        /// </summary>
        /// <param name="thing">The <see cref="ElementBase" /> being deleted.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetDeletionNotificationDescription(ElementBase thing)
        {
            var kind = thing switch
            {
                ElementUsage => "Element Usage",
                ElementDefinition => "Element Definition",
                _ => "Element"
            };

            var label = string.IsNullOrWhiteSpace(thing.Name) ? thing.ShortName : thing.Name;

            return new NotificationDescription
            {
                OnSuccess = $"{kind} '{label}' deleted",
                OnError = $"Failed to delete {kind} '{label}'"
            };
        }

        /// <summary>
        /// Opens the delete-confirmation popup for the supplied <see cref="Parameter" />, captures the
        /// parameter as the deletion target, and composes a content message.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> the user requested to delete.</param>
        public void OpenDeleteParameterPopup(Parameter parameter)
        {
            if (parameter is null)
            {
                return;
            }

            this.selectedParameterToDelete = parameter;

            var parameterTypeName = parameter.ParameterType?.Name ?? "Parameter";
            var containerName = parameter.Container is ElementDefinition containing ? containing.Name : null;

            this.DeleteParameterPopupViewModel.ContentText = string.IsNullOrWhiteSpace(containerName)
                ? $"You are about to delete the Parameter '{parameterTypeName}'. This cannot be undone."
                : $"You are about to delete the Parameter '{parameterTypeName}' from {containerName}. This cannot be undone.";

            this.DeleteParameterPopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Performs the deletion of the parameter captured by <see cref="OpenDeleteParameterPopup" />.
        /// Refreshes the parameter rows on success and always closes the popup.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous delete operation.</returns>
        public async Task DeleteSelectedParameterAsync()
        {
            var parameter = this.selectedParameterToDelete;

            if (parameter is null || parameter.Container is not ElementDefinition containingDefinition)
            {
                this.CloseDeleteParameterPopup();
                return;
            }

            try
            {
                this.IsLoading = true;

                var clonedContainer = containingDefinition.Clone(false);
                var clonedParameter = parameter.Clone(false);

                var result = await this.sessionService.DeleteThingsWithNotification(clonedContainer, [clonedParameter], GetParameterDeletionNotificationDescription(parameter));

                if (result.IsSuccess && this.SelectedElementDefinition is not null)
                {
                    this.ElementDefinitionDetailsViewModel.Rows = this.SelectedElementDefinition.Parameter
                        .Where(x => x.Iid != parameter.Iid)
                        .Select(x => new ElementDefinitionDetailsRowViewModel(x, this.CurrentDomain, this.selectedHostElementUsage))
                        .ToList();
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while deleting the Parameter with iid {Iid}", parameter.Iid);
            }
            finally
            {
                this.IsLoading = false;
                this.CloseDeleteParameterPopup();
            }
        }

        /// <summary>
        /// Closes the parameter delete-confirmation popup and clears the captured target.
        /// </summary>
        private void CloseDeleteParameterPopup()
        {
            this.DeleteParameterPopupViewModel.IsVisible = false;
            this.selectedParameterToDelete = null;
        }

        /// <summary>
        /// Cancellation handler bound to <see cref="DeleteParameterPopupViewModel" />'s
        /// <see cref="IConfirmCancelPopupViewModel.OnCancel" />.
        /// </summary>
        private void OnDeleteParameterCancelled()
        {
            this.CloseDeleteParameterPopup();
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of a
        /// <see cref="Parameter" /> deletion through the notification pipeline.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> being deleted.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetParameterDeletionNotificationDescription(Parameter parameter)
        {
            var label = parameter.ParameterType?.Name ?? "Parameter";

            return new NotificationDescription
            {
                OnSuccess = $"Parameter '{label}' deleted",
                OnError = $"Failed to delete Parameter '{label}'"
            };
        }

        /// <summary>
        /// Opens the create-subscription confirmation popup for the supplied <see cref="Parameter" />. No-op
        /// when there is no current domain or when the parameter is already owned by the current domain.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> the user requested to subscribe to.</param>
        public void OpenCreateSubscriptionPopup(Parameter parameter)
        {
            if (parameter is null || this.CurrentDomain is null)
            {
                return;
            }

            if (parameter.Owner != null && parameter.Owner.Iid == this.CurrentDomain.Iid)
            {
                return;
            }

            this.selectedParameterToSubscribe = parameter;

            var parameterTypeName = parameter.ParameterType?.Name ?? "Parameter";
            var containerName = parameter.Container is ElementDefinition containing ? containing.Name : null;

            this.CreateSubscriptionPopupViewModel.ContentText = string.IsNullOrWhiteSpace(containerName)
                ? $"You are about to subscribe to Parameter '{parameterTypeName}' as Domain '{this.CurrentDomain.ShortName}'."
                : $"You are about to subscribe to Parameter '{parameterTypeName}' on Element Definition '{containerName}' as Domain '{this.CurrentDomain.ShortName}'.";

            this.CreateSubscriptionPopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Performs the creation of a new <see cref="ParameterSubscription" /> for the parameter captured
        /// by <see cref="OpenCreateSubscriptionPopup" />.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous create operation.</returns>
        public async Task CreateSubscriptionAsync()
        {
            var parameter = this.selectedParameterToSubscribe;

            if (parameter is null || this.CurrentDomain is null)
            {
                this.CloseCreateSubscriptionPopup();
                return;
            }

            try
            {
                this.IsLoading = true;

                var clonedParameter = parameter.Clone(false);

                var subscription = new ParameterSubscription
                {
                    Iid = Guid.NewGuid(),
                    Owner = this.CurrentDomain
                };

                List<Thing> thingsToCreate = [clonedParameter, subscription];

                foreach (var sourceValueSet in parameter.ValueSet)
                {
                    var subscriptionValueSet = new ParameterSubscriptionValueSet
                    {
                        Iid = Guid.NewGuid(),
                        SubscribedValueSet = sourceValueSet,
                        ValueSwitch = sourceValueSet.ValueSwitch,
                        Manual = new ValueArray<string>(["-"])
                    };

                    subscription.ValueSet.Add(subscriptionValueSet);
                    thingsToCreate.Add(subscriptionValueSet);
                }

                clonedParameter.ParameterSubscription.Add(subscription);

                await this.sessionService.CreateOrUpdateThingsWithNotification(clonedParameter, thingsToCreate, GetSubscriptionCreationNotificationDescription(parameter));
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while creating a ParameterSubscription on the Parameter with iid {Iid}", parameter.Iid);
            }
            finally
            {
                this.IsLoading = false;
                this.CloseCreateSubscriptionPopup();
            }
        }

        /// <summary>
        /// Closes the create-subscription popup and clears the captured target.
        /// </summary>
        private void CloseCreateSubscriptionPopup()
        {
            this.CreateSubscriptionPopupViewModel.IsVisible = false;
            this.selectedParameterToSubscribe = null;
        }

        /// <summary>
        /// Cancellation handler bound to <see cref="CreateSubscriptionPopupViewModel" />'s
        /// <see cref="IConfirmCancelPopupViewModel.OnCancel" />.
        /// </summary>
        private void OnCreateSubscriptionCancelled()
        {
            this.CloseCreateSubscriptionPopup();
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of a
        /// <see cref="ParameterSubscription" /> creation through the notification pipeline.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> being subscribed to.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetSubscriptionCreationNotificationDescription(Parameter parameter)
        {
            var label = parameter.ParameterType?.Name ?? "Parameter";

            return new NotificationDescription
            {
                OnSuccess = $"Subscribed to Parameter '{label}'",
                OnError = $"Failed to subscribe to Parameter '{label}'"
            };
        }

        /// <summary>
        /// Opens the delete-subscription confirmation popup for the supplied <see cref="ParameterSubscription" />.
        /// No-op when the subscription is null or its container is not a <see cref="Parameter" />.
        /// </summary>
        /// <param name="subscription">The <see cref="ParameterSubscription" /> the user requested to delete.</param>
        public void OpenDeleteSubscriptionPopup(ParameterSubscription subscription)
        {
            if (subscription is null || subscription.Container is not Parameter parentParameter)
            {
                return;
            }

            this.selectedSubscriptionToDelete = subscription;

            var parameterTypeName = parentParameter.ParameterType?.Name ?? "Parameter";

            this.DeleteSubscriptionPopupViewModel.ContentText =
                $"You are about to delete your subscription to Parameter '{parameterTypeName}'. The Parameter itself will not be removed.";

            this.DeleteSubscriptionPopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Performs the deletion of the <see cref="ParameterSubscription" /> captured by
        /// <see cref="OpenDeleteSubscriptionPopup" />.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous delete operation.</returns>
        public async Task DeleteSelectedSubscriptionAsync()
        {
            var subscription = this.selectedSubscriptionToDelete;

            if (subscription is null || subscription.Container is not Parameter parentParameter)
            {
                this.CloseDeleteSubscriptionPopup();
                return;
            }

            try
            {
                this.IsLoading = true;

                var clonedParameter = parentParameter.Clone(false);
                var clonedSubscription = subscription.Clone(false);

                await this.sessionService.DeleteThingsWithNotification(clonedParameter, [(Thing)clonedSubscription], GetSubscriptionDeletionNotificationDescription(subscription));
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while deleting the ParameterSubscription with iid {Iid}", subscription.Iid);
            }
            finally
            {
                this.IsLoading = false;
                this.CloseDeleteSubscriptionPopup();
            }
        }

        /// <summary>
        /// Closes the delete-subscription popup and clears the captured target.
        /// </summary>
        private void CloseDeleteSubscriptionPopup()
        {
            this.DeleteSubscriptionPopupViewModel.IsVisible = false;
            this.selectedSubscriptionToDelete = null;
        }

        /// <summary>
        /// Cancellation handler bound to <see cref="DeleteSubscriptionPopupViewModel" />'s
        /// <see cref="IConfirmCancelPopupViewModel.OnCancel" />.
        /// </summary>
        private void OnDeleteSubscriptionCancelled()
        {
            this.CloseDeleteSubscriptionPopup();
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of a
        /// <see cref="ParameterSubscription" /> deletion through the notification pipeline.
        /// </summary>
        /// <param name="subscription">The <see cref="ParameterSubscription" /> being deleted.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetSubscriptionDeletionNotificationDescription(ParameterSubscription subscription)
        {
            var label = subscription.Container is Parameter parentParameter
                ? parentParameter.ParameterType?.Name ?? "Parameter"
                : "Parameter";

            return new NotificationDescription
            {
                OnSuccess = $"Subscription to Parameter '{label}' deleted",
                OnError = $"Failed to delete subscription to Parameter '{label}'"
            };
        }

        /// <summary>
        /// Opens the create-override confirmation popup for the supplied <see cref="Parameter" /> on the
        /// supplied <see cref="ElementUsage" />. No-op when there is no current domain, no host usage, or
        /// no parameter.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> the user requested to override.</param>
        /// <param name="hostElementUsage">The <see cref="ElementUsage" /> on which the override will be created.</param>
        public void OpenCreateOverridePopup(Parameter parameter, ElementUsage hostElementUsage)
        {
            if (parameter is null || hostElementUsage is null || this.CurrentDomain is null)
            {
                return;
            }

            this.selectedParameterToOverride = parameter;
            this.selectedHostUsageForOverride = hostElementUsage;

            var parameterTypeName = parameter.ParameterType?.Name ?? "Parameter";
            var usageLabel = string.IsNullOrWhiteSpace(hostElementUsage.Name) ? hostElementUsage.ShortName : hostElementUsage.Name;

            this.CreateOverridePopupViewModel.ContentText =
                $"You are about to override Parameter '{parameterTypeName}' on Element Usage '{usageLabel}' as Domain '{this.CurrentDomain.ShortName}'.";

            this.CreateOverridePopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Performs the creation of a new <see cref="ParameterOverride" /> for the parameter and host usage
        /// captured by <see cref="OpenCreateOverridePopup" />.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous create operation.</returns>
        public async Task CreateOverrideAsync()
        {
            var parameter = this.selectedParameterToOverride;
            var hostUsage = this.selectedHostUsageForOverride;

            if (parameter is null || hostUsage is null || this.CurrentDomain is null)
            {
                this.CloseCreateOverridePopup();
                return;
            }

            try
            {
                this.IsLoading = true;

                var clonedUsage = hostUsage.Clone(false);

                var parameterOverride = new ParameterOverride
                {
                    Iid = Guid.NewGuid(),
                    Parameter = parameter,
                    Owner = this.CurrentDomain
                };

                clonedUsage.ParameterOverride.Add(parameterOverride);

                await this.sessionService.CreateOrUpdateThingsWithNotification(clonedUsage, [(Thing)clonedUsage, parameterOverride], GetOverrideCreationNotificationDescription(parameter, hostUsage));
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while creating a ParameterOverride on Element Usage with iid {Iid} for Parameter with iid {ParameterIid}", hostUsage.Iid, parameter.Iid);
            }
            finally
            {
                this.IsLoading = false;
                this.CloseCreateOverridePopup();
            }
        }

        /// <summary>
        /// Closes the create-override popup and clears the captured targets.
        /// </summary>
        private void CloseCreateOverridePopup()
        {
            this.CreateOverridePopupViewModel.IsVisible = false;
            this.selectedParameterToOverride = null;
            this.selectedHostUsageForOverride = null;
        }

        /// <summary>
        /// Cancellation handler bound to <see cref="CreateOverridePopupViewModel" />'s
        /// <see cref="IConfirmCancelPopupViewModel.OnCancel" />.
        /// </summary>
        private void OnCreateOverrideCancelled()
        {
            this.CloseCreateOverridePopup();
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of a
        /// <see cref="ParameterOverride" /> creation through the notification pipeline.
        /// </summary>
        /// <param name="parameter">The source <see cref="Parameter" /> being overridden.</param>
        /// <param name="hostElementUsage">The host <see cref="ElementUsage" /> on which the override is created.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetOverrideCreationNotificationDescription(Parameter parameter, ElementUsage hostElementUsage)
        {
            var parameterLabel = parameter.ParameterType?.Name ?? "Parameter";
            var usageLabel = string.IsNullOrWhiteSpace(hostElementUsage.Name) ? hostElementUsage.ShortName : hostElementUsage.Name;

            return new NotificationDescription
            {
                OnSuccess = $"Override of Parameter '{parameterLabel}' created on Element Usage '{usageLabel}'",
                OnError = $"Failed to create override of Parameter '{parameterLabel}' on Element Usage '{usageLabel}'"
            };
        }

        /// <summary>
        /// Opens the delete-override confirmation popup for the supplied <see cref="ParameterOverride" />.
        /// No-op when the override is null or its container is not an <see cref="ElementUsage" />.
        /// </summary>
        /// <param name="parameterOverride">The <see cref="ParameterOverride" /> the user requested to delete.</param>
        public void OpenDeleteOverridePopup(ParameterOverride parameterOverride)
        {
            if (parameterOverride is null || parameterOverride.Container is not ElementUsage hostUsage)
            {
                return;
            }

            this.selectedOverrideToDelete = parameterOverride;

            var parameterTypeName = parameterOverride.Parameter?.ParameterType?.Name ?? "Parameter";
            var usageLabel = string.IsNullOrWhiteSpace(hostUsage.Name) ? hostUsage.ShortName : hostUsage.Name;

            this.DeleteOverridePopupViewModel.ContentText =
                $"You are about to delete the override of Parameter '{parameterTypeName}' on Element Usage '{usageLabel}'. The Parameter on the Element Definition is not affected.";

            this.DeleteOverridePopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Performs the deletion of the <see cref="ParameterOverride" /> captured by
        /// <see cref="OpenDeleteOverridePopup" />.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous delete operation.</returns>
        public async Task DeleteSelectedOverrideAsync()
        {
            var parameterOverride = this.selectedOverrideToDelete;

            if (parameterOverride is null || parameterOverride.Container is not ElementUsage hostUsage)
            {
                this.CloseDeleteOverridePopup();
                return;
            }

            try
            {
                this.IsLoading = true;

                var clonedUsage = hostUsage.Clone(false);
                var clonedOverride = parameterOverride.Clone(false);

                await this.sessionService.DeleteThingsWithNotification(clonedUsage, [(Thing)clonedOverride], GetOverrideDeletionNotificationDescription(parameterOverride));
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while deleting the ParameterOverride with iid {Iid}", parameterOverride.Iid);
            }
            finally
            {
                this.IsLoading = false;
                this.CloseDeleteOverridePopup();
            }
        }

        /// <summary>
        /// Closes the delete-override popup and clears the captured target.
        /// </summary>
        private void CloseDeleteOverridePopup()
        {
            this.DeleteOverridePopupViewModel.IsVisible = false;
            this.selectedOverrideToDelete = null;
        }

        /// <summary>
        /// Cancellation handler bound to <see cref="DeleteOverridePopupViewModel" />'s
        /// <see cref="IConfirmCancelPopupViewModel.OnCancel" />.
        /// </summary>
        private void OnDeleteOverrideCancelled()
        {
            this.CloseDeleteOverridePopup();
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of a
        /// <see cref="ParameterOverride" /> deletion through the notification pipeline.
        /// </summary>
        /// <param name="parameterOverride">The <see cref="ParameterOverride" /> being deleted.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetOverrideDeletionNotificationDescription(ParameterOverride parameterOverride)
        {
            var parameterLabel = parameterOverride.Parameter?.ParameterType?.Name ?? "Parameter";
            var usageLabel = parameterOverride.Container is ElementUsage hostUsage
                ? string.IsNullOrWhiteSpace(hostUsage.Name) ? hostUsage.ShortName : hostUsage.Name
                : "Element Usage";

            return new NotificationDescription
            {
                OnSuccess = $"Override of Parameter '{parameterLabel}' on Element Usage '{usageLabel}' deleted",
                OnError = $"Failed to delete override of Parameter '{parameterLabel}' on Element Usage '{usageLabel}'"
            };
        }

        /// <summary>
        /// Opens the edit-element popup for <see cref="SelectedElement" />, initializing whichever child
        /// view model corresponds to the kind of selection. When the selection is an
        /// <see cref="ElementUsage" /> the popup targets the usage; when it is an
        /// <see cref="ElementDefinition" /> it targets the definition.
        /// </summary>
        public void OpenEditElementPopup()
        {
            this.IsEditingDefinition = false;

            switch (this.SelectedElement)
            {
                case ElementDefinition elementDefinition:
                {
                    var iteration = elementDefinition.GetContainerOfType<Iteration>();
                    this.EditElementDefinitionViewModel.InitializeViewModel((ElementDefinition)elementDefinition.Clone(true), iteration);
                    this.IsOnEditMode = true;
                    break;
                }

                case ElementUsage elementUsage:
                {
                    var iteration = elementUsage.GetContainerOfType<Iteration>();
                    this.EditElementUsageViewModel.InitializeViewModel((ElementUsage)elementUsage.Clone(false), iteration);
                    this.IsOnEditMode = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Opens the edit popup targeting the <see cref="ElementDefinition" /> behind the current
        /// selection — i.e. the usage's <see cref="ElementUsage.ElementDefinition" /> when an
        /// <see cref="ElementUsage" /> is selected, or <see cref="SelectedElementDefinition" /> itself
        /// when an <see cref="ElementDefinition" /> is selected. No-op when
        /// <see cref="SelectedElementDefinition" /> is <c>null</c>.
        /// </summary>
        public void OpenEditDefinitionPopup()
        {
            if (this.SelectedElementDefinition is null)
            {
                return;
            }

            var iteration = this.SelectedElementDefinition.GetContainerOfType<Iteration>();
            this.EditElementDefinitionViewModel.InitializeViewModel((ElementDefinition)this.SelectedElementDefinition.Clone(true), iteration);
            this.IsEditingDefinition = true;
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Performs the edit of the selected <see cref="ElementDefinition" />. Always closes the popup.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous edit operation.</returns>
        public async Task EditElementDefinitionAsync()
        {
            var clonedElementDefinition = this.EditElementDefinitionViewModel.ElementDefinition;
            var originalIteration = this.EditElementDefinitionViewModel.Iteration;

            if (clonedElementDefinition is null || originalIteration is null)
            {
                this.CloseEditElementPopup();
                return;
            }

            try
            {
                this.IsLoading = true;

                clonedElementDefinition.Category = this.EditElementDefinitionViewModel.SelectedCategories.ToList();

                var clonedIteration = originalIteration.Clone(false);

                if (this.EditElementDefinitionViewModel.IsTopElement && clonedIteration.TopElement?.Iid != clonedElementDefinition.Iid)
                {
                    clonedIteration.TopElement = clonedElementDefinition;
                }

                List<Thing> thingsToUpdate = [clonedIteration, clonedElementDefinition];
                thingsToUpdate.AddRange(clonedElementDefinition.Definition);

                await this.sessionService.CreateOrUpdateThingsWithNotification(clonedIteration, thingsToUpdate, GetElementDefinitionEditNotificationDescription(clonedElementDefinition));
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while editing the Element Definition with iid {Iid}", clonedElementDefinition.Iid);
            }
            finally
            {
                this.IsLoading = false;
                this.CloseEditElementPopup();
            }
        }

        /// <summary>
        /// Performs the edit of the selected <see cref="ElementUsage" />. Always closes the popup.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous edit operation.</returns>
        public async Task EditElementUsageAsync()
        {
            var clonedElementUsage = this.EditElementUsageViewModel.ElementUsage;

            if (clonedElementUsage is null || this.SelectedElement is not ElementUsage originalElementUsage)
            {
                this.CloseEditElementPopup();
                return;
            }

            try
            {
                this.IsLoading = true;

                // Compute the excluded options from the complement of the selected options.
                clonedElementUsage.ExcludeOption = this.EditElementUsageViewModel.AvailableOptions
                    .Where(o => this.EditElementUsageViewModel.SelectedOptions.All(s => s.Iid != o.Iid))
                    .ToList();

                var clonedContainer = originalElementUsage.Container.Clone(false);

                await this.sessionService.CreateOrUpdateThingsWithNotification(clonedContainer, [(Thing)clonedElementUsage], GetElementUsageEditNotificationDescription(clonedElementUsage));
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while editing the Element Usage with iid {Iid}", clonedElementUsage.Iid);
            }
            finally
            {
                this.IsLoading = false;
                this.CloseEditElementPopup();
            }
        }

        /// <summary>
        /// Closes the edit-element popup and resets the <see cref="IsEditingDefinition" /> flag.
        /// </summary>
        private void CloseEditElementPopup()
        {
            this.IsOnEditMode = false;
            this.IsEditingDefinition = false;
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of an
        /// <see cref="ElementDefinition" /> edit through the notification pipeline.
        /// </summary>
        /// <param name="elementDefinition">The <see cref="ElementDefinition" /> being edited.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetElementDefinitionEditNotificationDescription(ElementDefinition elementDefinition)
        {
            return new NotificationDescription
            {
                OnSuccess = $"Element Definition '{elementDefinition.Name}' updated",
                OnError = $"Failed to update Element Definition '{elementDefinition.Name}'"
            };
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of an
        /// <see cref="ElementUsage" /> edit through the notification pipeline.
        /// </summary>
        /// <param name="elementUsage">The <see cref="ElementUsage" /> being edited.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetElementUsageEditNotificationDescription(ElementUsage elementUsage)
        {
            return new NotificationDescription
            {
                OnSuccess = $"Element Usage '{elementUsage.Name}' updated",
                OnError = $"Failed to update Element Usage '{elementUsage.Name}'"
            };
        }

        /// <summary>
        /// Opens the create-element-definition popup, initializing the creation view model against the
        /// current iteration.
        /// </summary>
        public void OpenCreateElementDefinitionCreationPopup()
        {
            this.ElementDefinitionCreationViewModel.InitializeViewModel(this.CurrentIteration);
            this.ElementDefinitionCreationViewModel.ElementDefinition = new ElementDefinition();
            this.ElementDefinitionCreationViewModel.SelectedCategories = [];
            this.IsOnCreationMode = true;
        }

        /// <summary>
        /// Opens the add-parameter popup.
        /// </summary>
        public void OpenAddParameterPopup()
        {
            this.AddParameterViewModel.ResetValues();
            this.IsOnAddingParameterMode = true;
        }

        /// <summary>
        /// Opens the appropriate edit popup for the supplied <see cref="ParameterOrOverrideBase" />: when the current
        /// domain does not own it but has a <see cref="ParameterSubscription" /> on it, the Edit Parameter Subscription
        /// popup is opened (editing the subscription's values); otherwise the Edit Parameter popup is opened.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase" /> the user requested to edit.</param>
        public void OpenEditParameterPopup(ParameterOrOverrideBase parameter)
        {
            if (parameter is null)
            {
                return;
            }

            var currentDomainSubscription = this.CurrentDomain is null
                ? null
                : parameter.ParameterSubscription.FirstOrDefault(subscription => subscription.Owner != null && subscription.Owner.Iid == this.CurrentDomain.Iid);

            var isOwnedByCurrentDomain = parameter.Owner != null && this.CurrentDomain != null && parameter.Owner.Iid == this.CurrentDomain.Iid;

            if (currentDomainSubscription is not null && !isOwnedByCurrentDomain)
            {
                this.EditParameterSubscriptionViewModel.SetSubscription(currentDomainSubscription, this.CurrentIteration);
                this.IsOnEditSubscriptionMode = true;
                return;
            }

            this.EditParameterViewModel.SetParameter(parameter, this.CurrentIteration, this.CurrentDomain);
            this.IsOnEditParameterMode = true;
        }

        /// <summary>
        /// Opens the create-parameter-group popup, resetting the form fields. No-op when no
        /// <see cref="SelectedElementDefinition" /> is set.
        /// </summary>
        public void OpenCreateParameterGroupPopup()
        {
            if (this.SelectedElementDefinition is null)
            {
                return;
            }

            this.parameterGroupUnderEdit = null;
            this.ParameterGroupName = string.Empty;
            this.ParameterGroupContainingGroup = null;
            this.IsOnParameterGroupEditMode = true;
        }

        /// <summary>
        /// Opens the edit-parameter-group popup for the supplied <see cref="ParameterGroup" />, pre-populating
        /// the form fields from its current state.
        /// </summary>
        /// <param name="group">The <see cref="ParameterGroup" /> the user requested to edit.</param>
        public void OpenEditParameterGroupPopup(ParameterGroup group)
        {
            if (group is null)
            {
                return;
            }

            this.parameterGroupUnderEdit = group;
            this.ParameterGroupName = group.Name;
            this.ParameterGroupContainingGroup = group.ContainingGroup;
            this.IsOnParameterGroupEditMode = true;
        }

        /// <summary>
        /// Saves the parameter group currently described by the create/edit form: creates a new
        /// <see cref="ParameterGroup" /> when <see cref="parameterGroupUnderEdit" /> is <c>null</c>,
        /// or updates the existing group otherwise. Always closes the popup.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous save operation.</returns>
        public async Task SaveParameterGroupAsync()
        {
            if (this.SelectedElementDefinition is null || string.IsNullOrWhiteSpace(this.ParameterGroupName))
            {
                this.CloseParameterGroupEditPopup();
                return;
            }

            if (this.parameterGroupUnderEdit is not null && WouldCreateContainingGroupCycle(this.parameterGroupUnderEdit, this.ParameterGroupContainingGroup))
            {
                // Defensive guard: a group cannot be nested under itself or one of its own descendants.
                // The combo in the edit popup already hides these options, so this only blocks a stale selection.
                this.CloseParameterGroupEditPopup();
                return;
            }

            try
            {
                this.IsLoading = true;

                var clonedEd = (ElementDefinition)this.SelectedElementDefinition.Clone(false);

                List<Thing> things;

                if (this.parameterGroupUnderEdit is null)
                {
                    var newGroup = new ParameterGroup
                    {
                        Iid = Guid.NewGuid(),
                        Name = this.ParameterGroupName,
                        ContainingGroup = this.ParameterGroupContainingGroup
                    };

                    clonedEd.ParameterGroup.Add(newGroup);
                    things = [clonedEd, newGroup];
                }
                else
                {
                    var clonedGroup = (ParameterGroup)this.parameterGroupUnderEdit.Clone(false);
                    clonedGroup.Name = this.ParameterGroupName;
                    clonedGroup.ContainingGroup = this.ParameterGroupContainingGroup;
                    things = [clonedEd, clonedGroup];
                }

                await this.sessionService.CreateOrUpdateThingsWithNotification(clonedEd, things, GetParameterGroupSaveNotificationDescription(this.ParameterGroupName, this.parameterGroupUnderEdit is null));
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while saving the ParameterGroup '{Name}'", this.ParameterGroupName);
            }
            finally
            {
                this.IsLoading = false;
                this.CloseParameterGroupEditPopup();
            }
        }

        /// <summary>
        /// Closes the create/edit parameter-group popup and resets the associated state.
        /// </summary>
        private void CloseParameterGroupEditPopup()
        {
            this.IsOnParameterGroupEditMode = false;
            this.parameterGroupUnderEdit = null;
        }

        /// <summary>
        /// Determines whether nesting <paramref name="group" /> under <paramref name="proposedContainingGroup" />
        /// would create a containing-group cycle — i.e. the proposed parent is the group itself or one of its
        /// descendants.
        /// </summary>
        /// <param name="group">The <see cref="ParameterGroup" /> being edited.</param>
        /// <param name="proposedContainingGroup">The candidate containing group, or <c>null</c> for top-level.</param>
        /// <returns><c>true</c> when the assignment would create a cycle; otherwise <c>false</c>.</returns>
        private static bool WouldCreateContainingGroupCycle(ParameterGroup group, ParameterGroup proposedContainingGroup)
        {
            var ancestor = proposedContainingGroup;

            while (ancestor is not null)
            {
                if (ancestor.Iid == group.Iid)
                {
                    return true;
                }

                ancestor = ancestor.ContainingGroup;
            }

            return false;
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of a
        /// <see cref="ParameterGroup" /> save through the notification pipeline.
        /// </summary>
        /// <param name="name">The name of the group being saved.</param>
        /// <param name="isCreate"><c>true</c> when creating a new group; <c>false</c> when editing.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetParameterGroupSaveNotificationDescription(string name, bool isCreate)
        {
            var verb = isCreate ? "created" : "updated";
            var verbFail = isCreate ? "create" : "update";

            return new NotificationDescription
            {
                OnSuccess = $"Parameter Group '{name}' {verb}",
                OnError = $"Failed to {verbFail} Parameter Group '{name}'"
            };
        }

        /// <summary>
        /// Opens the delete-confirmation popup for the supplied <see cref="ParameterGroup" />, capturing it
        /// as the deletion target and composing a descriptive content message.
        /// </summary>
        /// <param name="group">The <see cref="ParameterGroup" /> the user requested to delete.</param>
        public void OpenDeleteParameterGroupPopup(ParameterGroup group)
        {
            if (group is null)
            {
                return;
            }

            this.parameterGroupToDelete = group;

            var parentDescription = group.ContainingGroup is null
                ? "moved to the top level"
                : $"moved to the parent group '{group.ContainingGroup.Name}'";

            this.DeleteParameterGroupPopupViewModel.ContentText =
                $"You are about to delete the Parameter Group '{group.Name}'. Parameters in it and nested groups will be {parentDescription}.";

            this.DeleteParameterGroupPopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Performs the deletion of the <see cref="ParameterGroup" /> captured by
        /// <see cref="OpenDeleteParameterGroupPopup" />. Parameters whose group matches the deleted group are
        /// moved to the deleted group's parent (or ungrouped when it is a top-level group); nested groups that
        /// referenced the deleted group as their containing group are similarly re-parented. Refreshes the
        /// selection on completion.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the asynchronous delete operation.</returns>
        public async Task DeleteSelectedParameterGroupAsync()
        {
            var group = this.parameterGroupToDelete;

            if (group is null || this.SelectedElementDefinition is null)
            {
                this.CloseDeleteParameterGroupPopup();
                return;
            }

            try
            {
                this.IsLoading = true;

                // Resolve the parent group — null when the deleted group is already at the top level.
                var parent = group.ContainingGroup;

                // Parameters that reference the deleted group must be moved to the parent group (or ungrouped).
                var affectedParameters = this.SelectedElementDefinition.Parameter
                    .Where(p => p.Group != null && p.Group.Iid == group.Iid)
                    .Select(p =>
                    {
                        var cloned = (Parameter)p.Clone(false);
                        cloned.Group = parent;
                        return (Thing)cloned;
                    })
                    .ToList();

                // Nested groups that reference the deleted group must be re-parented to the parent group.
                var affectedChildGroups = this.SelectedElementDefinition.ParameterGroup
                    .Where(g => g.Iid != group.Iid && g.ContainingGroup != null && g.ContainingGroup.Iid == group.Iid)
                    .Select(g =>
                    {
                        var cloned = (ParameterGroup)g.Clone(false);
                        cloned.ContainingGroup = parent;
                        return (Thing)cloned;
                    })
                    .ToList();

                var thingsToUpdate = affectedParameters.Concat(affectedChildGroups).ToList();

                if (thingsToUpdate.Count > 0)
                {
                    var clonedEdForUpdate = (ElementDefinition)this.SelectedElementDefinition.Clone(false);
                    thingsToUpdate.Add(clonedEdForUpdate);

                    await this.sessionService.CreateOrUpdateThingsWithNotification(
                        clonedEdForUpdate,
                        thingsToUpdate,
                        null);
                }

                var clonedEd = (ElementDefinition)this.SelectedElementDefinition.Clone(false);
                var clonedGroup = (ParameterGroup)group.Clone(false);

                await this.sessionService.DeleteThingsWithNotification(
                    clonedEd,
                    [clonedGroup],
                    GetParameterGroupDeletionNotificationDescription(group));

                this.RefreshSelectedElement();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while deleting the ParameterGroup with iid {Iid}", group.Iid);
            }
            finally
            {
                this.IsLoading = false;
                this.CloseDeleteParameterGroupPopup();
            }
        }

        /// <summary>
        /// Closes the delete-parameter-group popup and clears the captured target.
        /// </summary>
        private void CloseDeleteParameterGroupPopup()
        {
            this.DeleteParameterGroupPopupViewModel.IsVisible = false;
            this.parameterGroupToDelete = null;
        }

        /// <summary>
        /// Cancellation handler bound to <see cref="DeleteParameterGroupPopupViewModel" />'s
        /// <see cref="IConfirmCancelPopupViewModel.OnCancel" />.
        /// </summary>
        private void OnDeleteParameterGroupCancelled()
        {
            this.CloseDeleteParameterGroupPopup();
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of a
        /// <see cref="ParameterGroup" /> deletion through the notification pipeline.
        /// </summary>
        /// <param name="group">The <see cref="ParameterGroup" /> being deleted.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetParameterGroupDeletionNotificationDescription(ParameterGroup group)
        {
            return new NotificationDescription
            {
                OnSuccess = $"Parameter Group '{group.Name}' deleted",
                OnError = $"Failed to delete Parameter Group '{group.Name}'"
            };
        }

        /// <summary>
        /// Reassigns the supplied <see cref="Parameter" /> to the supplied <see cref="ParameterGroup" />
        /// (which may be <c>null</c> to ungroup the parameter). Refreshes the selection so the card reflects
        /// the updated group.
        /// </summary>
        /// <param name="args">
        /// A tuple of the <see cref="Parameter" /> to reassign and the target <see cref="ParameterGroup" />
        /// (or <c>null</c> to ungroup).
        /// </param>
        /// <returns>A <see cref="Task" /> representing the asynchronous assign operation.</returns>
        public async Task AssignParameterToGroupAsync((Parameter Parameter, ParameterGroup Group) args)
        {
            var parameter = args.Parameter;

            if (parameter is null || parameter.Container is not ElementDefinition containingDefinition)
            {
                return;
            }

            if (parameter.Group?.Iid == args.Group?.Iid)
            {
                // The parameter is already in the target group (or already ungrouped) — no write needed.
                return;
            }

            try
            {
                this.IsLoading = true;

                var clonedContainer = (ElementDefinition)containingDefinition.Clone(false);
                var clonedParameter = (Parameter)parameter.Clone(false);
                clonedParameter.Group = args.Group;

                await this.sessionService.CreateOrUpdateThingsWithNotification(
                    clonedContainer,
                    [clonedParameter],
                    GetParameterGroupAssignNotificationDescription(parameter, args.Group));

                this.RefreshSelectedElement();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while assigning Parameter with iid {Iid} to group", parameter.Iid);
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of a
        /// parameter-group assignment through the notification pipeline.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> being reassigned.</param>
        /// <param name="group">The target <see cref="ParameterGroup" />, or <c>null</c> when ungrouping.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetParameterGroupAssignNotificationDescription(Parameter parameter, ParameterGroup group)
        {
            var parameterLabel = parameter.ParameterType?.Name ?? "Parameter";

            return group is null
                ? new NotificationDescription
                {
                    OnSuccess = $"Parameter '{parameterLabel}' removed from its group",
                    OnError = $"Failed to remove Parameter '{parameterLabel}' from its group"
                }
                : new NotificationDescription
                {
                    OnSuccess = $"Parameter '{parameterLabel}' assigned to group '{group.Name}'",
                    OnError = $"Failed to assign Parameter '{parameterLabel}' to group '{group.Name}'"
                };
        }

        /// <summary>
        /// Reassigns the supplied <see cref="ParameterGroup" /> so that it is contained by
        /// <paramref name="args" />.NewContaining (or promoted to the top level when that is <c>null</c>).
        /// Guards against null groups, no-op moves, self-nesting and cycle creation.
        /// </summary>
        /// <param name="args">
        /// A tuple of the <see cref="ParameterGroup" /> to move and its new containing group (or <c>null</c>
        /// to move to the top level).
        /// </param>
        /// <returns>A <see cref="Task" /> representing the asynchronous reassign operation.</returns>
        public async Task ReassignGroupContainingAsync((ParameterGroup Group, ParameterGroup NewContaining) args)
        {
            var group = args.Group;
            var newContaining = args.NewContaining;

            if (group is null)
            {
                return;
            }

            // No-op: already at the requested location.
            if (group.ContainingGroup?.Iid == newContaining?.Iid)
            {
                return;
            }

            // Reject self-nesting or a move that would create a cycle.
            if (newContaining is not null && (newContaining.Iid == group.Iid || WouldCreateContainingGroupCycle(group, newContaining)))
            {
                return;
            }

            if (group.Container is not ElementDefinition containingDefinition)
            {
                return;
            }

            try
            {
                this.IsLoading = true;

                var clonedEd = (ElementDefinition)containingDefinition.Clone(false);
                var clonedGroup = (ParameterGroup)group.Clone(false);
                clonedGroup.ContainingGroup = newContaining;

                await this.sessionService.CreateOrUpdateThingsWithNotification(
                    clonedEd,
                    [clonedEd, clonedGroup],
                    GetGroupReassignNotificationDescription(group, newContaining));

                this.RefreshSelectedElement();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while reassigning ParameterGroup '{Name}' (iid {Iid})", group.Name, group.Iid);
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of a
        /// <see cref="ParameterGroup" /> containing-group reassignment through the notification pipeline.
        /// </summary>
        /// <param name="group">The <see cref="ParameterGroup" /> being moved.</param>
        /// <param name="newContaining">The new containing group, or <c>null</c> when moving to the top level.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetGroupReassignNotificationDescription(ParameterGroup group, ParameterGroup newContaining)
        {
            return newContaining is null
                ? new NotificationDescription
                {
                    OnSuccess = $"Parameter Group '{group.Name}' moved to top level",
                    OnError = $"Failed to move Parameter Group '{group.Name}' to top level"
                }
                : new NotificationDescription
                {
                    OnSuccess = $"Parameter Group '{group.Name}' moved into '{newContaining.Name}'",
                    OnError = $"Failed to move Parameter Group '{group.Name}' into '{newContaining.Name}'"
                };
        }

        /// <summary>
        /// Tries to create a new <see cref="ElementDefinition" />.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task AddingElementDefinitionAsync()
        {
            List<Thing> thingsToCreate = [];

            if (this.ElementDefinitionCreationViewModel.SelectedCategories.Any())
            {
                this.ElementDefinitionCreationViewModel.ElementDefinition.Category = this.ElementDefinitionCreationViewModel.SelectedCategories.ToList();
            }

            var iteration = this.CurrentIteration;
            this.ElementDefinitionCreationViewModel.ElementDefinition.Container = iteration;
            thingsToCreate.Add(this.ElementDefinitionCreationViewModel.ElementDefinition);
            var clonedIteration = iteration.Clone(false);

            if (this.ElementDefinitionCreationViewModel.IsTopElement)
            {
                clonedIteration.TopElement = this.ElementDefinitionCreationViewModel.ElementDefinition;
            }

            clonedIteration.Element.Add(this.ElementDefinitionCreationViewModel.ElementDefinition);
            thingsToCreate.Add(clonedIteration);

            if (this.AutoAddCreatedDefinitionAsUsage && this.SelectedElementDefinition is not null && this.CurrentDomain is not null)
            {
                var clonedContainer = this.SelectedElementDefinition.Clone(false);

                var usage = new ElementUsage
                {
                    Iid = Guid.NewGuid(),
                    Name = this.ElementDefinitionCreationViewModel.ElementDefinition.Name,
                    ShortName = this.ElementDefinitionCreationViewModel.ElementDefinition.ShortName,
                    ElementDefinition = this.ElementDefinitionCreationViewModel.ElementDefinition,
                    Owner = this.CurrentDomain
                };

                clonedContainer.ContainedElement.Add(usage);
                thingsToCreate.Add(clonedContainer);
                thingsToCreate.Add(usage);
            }

            var elementName = this.ElementDefinitionCreationViewModel.ElementDefinition.Name;

            try
            {
                await this.sessionService.CreateOrUpdateThingsWithNotification(clonedIteration, thingsToCreate, GetElementDefinitionCreationNotificationDescription(elementName));
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while creating the Element Definition '{Name}'", elementName);
            }
            finally
            {
                this.IsOnCreationMode = false;
            }
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of a new
        /// <see cref="ElementDefinition" /> creation through the notification pipeline.
        /// </summary>
        /// <param name="name">The name of the <see cref="ElementDefinition" /> being created.</param>
        /// <returns>The <see cref="NotificationDescription" /> describing the operation.</returns>
        private static NotificationDescription GetElementDefinitionCreationNotificationDescription(string name)
        {
            return new NotificationDescription
            {
                OnSuccess = $"Element Definition '{name}' created",
                OnError = $"Failed to create Element Definition '{name}'"
            };
        }
    }
}
