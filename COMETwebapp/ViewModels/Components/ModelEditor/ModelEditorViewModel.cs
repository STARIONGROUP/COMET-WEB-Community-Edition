// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelEditorViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4DalCommon.Protocol.Operations;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.CopySettings;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditElementDefinitionViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.EditElementUsageViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.ElementDefinitionCreationViewModel;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="ModelEditor" />
    /// </summary>
    public class ModelEditorViewModel : SingleIterationApplicationBaseViewModel, IModelEditorViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The <see cref="ICacheService" />
        /// </summary>
        private readonly ICacheService cacheService;

        /// <summary>
        /// The <see cref="ILogger{T}" /> used to record any exception thrown by the delete pipeline so that
        /// it surfaces in the application log without aborting the popup-close path.
        /// </summary>
        private readonly ILogger<ModelEditorViewModel> logger;

        /// <summary>
        /// Backing field for <see cref="IsOnAddingParameterMode" />
        /// </summary>
        private bool isOnAddingParameterMode;

        /// <summary>
        /// Backing field for <see cref="IsOnCreationMode" />
        /// </summary>
        private bool isOnCreationMode;

        /// <summary>
        /// Backing field for <see cref="isOnCopySettingsMode" />
        /// </summary>
        private bool isOnCopySettingsMode;

        /// <summary>
        /// Backing field for <see cref="IsSourceModelSameAsTargetModel" />
        /// </summary>
        private bool isSourceModelSameAsTargetModel;

        /// <summary>
        /// Backing field for <see cref="TargetIteration"/>
        /// </summary>
        private Iteration targetIteration;

        /// <summary>
        /// Backing field for <see cref="SourceIteration"/>
        /// </summary>
        private Iteration sourceIteration;

        /// <summary>
        /// Backing field for <see cref="SelectedElement" />.
        /// </summary>
        private ElementBase selectedElement;

        /// <summary>
        /// Backing field for <see cref="IsOnDeletionMode" />.
        /// </summary>
        private bool isOnDeletionMode;

        /// <summary>
        /// Backing field for <see cref="IsOnEditMode" />.
        /// </summary>
        private bool isOnEditMode;

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
        /// The <see cref="ElementUsage" /> currently selected in the Model Editor tree (or <c>null</c> when
        /// the selection is an <see cref="ElementDefinition" />). Captured during
        /// <see cref="SelectElement" /> so that the rendered <see cref="ElementDefinitionDetailsRowViewModel" />
        /// rows can compute override-related affordances against the host usage.
        /// </summary>
        private ElementUsage selectedHostElementUsage;

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
        /// Creates a new instance of <see cref="ModelEditorViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="cacheService">The <see cref="ICacheService"/></param>
        /// <param name="logger">The <see cref="ILogger{T}"/> used to record delete-pipeline exceptions.</param>
        public ModelEditorViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ICacheService cacheService, ILogger<ModelEditorViewModel> logger) : base(sessionService, messageBus)
        {
            this.sessionService = sessionService;
            this.cacheService = cacheService;
            this.logger = logger;
            var eventCallbackFactory = new EventCallbackFactory();

            this.ElementDefinitionCreationViewModel = new ElementDefinitionCreationViewModel.ElementDefinitionCreationViewModel(sessionService, messageBus)
            {
                OnValidSubmit = eventCallbackFactory.Create(this, this.AddingElementDefinitionAsync)
            };

            this.AddParameterViewModel = new AddParameterViewModel.AddParameterViewModel(sessionService, messageBus)
            {
                OnParameterAdded = eventCallbackFactory.Create(this, () => this.IsOnAddingParameterMode = false)
            };

            this.CopySettingsViewModel = new CopySettingsViewModel(cacheService)
            {
                OnSaveSettings = eventCallbackFactory.Create(this, () => this.IsOnCopySettingsMode = false)
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

            this.EditElementDefinitionViewModel = new EditElementDefinitionViewModel.EditElementDefinitionViewModel(sessionService, messageBus)
            {
                OnValidSubmit = eventCallbackFactory.Create(this, this.EditElementDefinitionAsync)
            };

            this.EditElementUsageViewModel = new EditElementUsageViewModel.EditElementUsageViewModel(sessionService, messageBus)
            {
                OnValidSubmit = eventCallbackFactory.Create(this, this.EditElementUsageAsync)
            };

            this.InitializeSubscriptions([typeof(ElementBase)]);

            this.Disposables.Add(
                this.WhenAnyValue(x => x.CurrentThing)
                    .Subscribe(x => this.TargetIteration = this.CurrentThing)
            );

            this.Disposables.Add(
                this.WhenAnyValue(
                        x => x.SourceIteration, 
                        x => x.TargetIteration)
                    .Subscribe(x => this.IsSourceModelSameAsTargetModel = (x.Item1 != null && x.Item1 == x.Item2))
            );
        }

        /// <summary>
        /// Represents the selected ElementDefinitionRowViewModel
        /// </summary>
        public ElementDefinition SelectedElementDefinition { get; set; }

        /// <summary>
        /// The <see cref="IElementDefinitionDetailsViewModel" />
        /// </summary>
        public IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; } = new ElementDefinitionDetailsViewModel();

        /// <summary>
        /// Gets the <see cref="IElementDefinitionCreationViewModel" />
        /// </summary>
        public IElementDefinitionCreationViewModel ElementDefinitionCreationViewModel { get; set; }

        /// <summary>
        /// Gets the <see cref="IEditElementDefinitionViewModel" /> driving the edit-Element-Definition popup.
        /// </summary>
        public IEditElementDefinitionViewModel EditElementDefinitionViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IEditElementUsageViewModel" /> driving the edit-Element-Usage popup.
        /// </summary>
        public IEditElementUsageViewModel EditElementUsageViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IAddParameterViewModel" />
        /// </summary>
        public IAddParameterViewModel AddParameterViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ICopySettingsViewModel" />
        /// </summary>
        public ICopySettingsViewModel CopySettingsViewModel { get; set; }

        /// <summary>
        /// Gets or sets target <see cref="Iteration" /> 
        /// </summary>
        public Iteration TargetIteration
        {
            get => this.targetIteration;
            set => this.RaiseAndSetIfChanged(ref this.targetIteration, value);
        }

        /// <summary>
        /// Gets or sets source <see cref="Iteration" />
        /// </summary>
        public Iteration SourceIteration
        {
            get => this.sourceIteration;
            set => this.RaiseAndSetIfChanged(ref this.sourceIteration, value);
        }

        /// <summary>
        /// Value indicating the user is currently setting the Copy settings that apply when a node is dropped 
        /// </summary>
        public bool IsOnCopySettingsMode
        {
            get => this.isOnCopySettingsMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnCopySettingsMode, value);
        }

        /// <summary>
        /// Opens the <see cref="CopySettings" /> popup
        /// </summary>
        public void OpenCopySettingsPopup()
        {
            this.CopySettingsViewModel.InitializeViewModel();
            this.IsOnCopySettingsMode = true;
        }

        /// <summary>
        /// Value indicating the user is currently creating a new <see cref="ElementDefinition" />
        /// </summary>
        public bool IsOnCreationMode
        {
            get => this.isOnCreationMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnCreationMode, value);
        }

        /// <summary>
        /// Value indicating the user is currently adding a new <see cref="Parameter" /> to a <see cref="ElementDefinition" />
        /// </summary>
        public bool IsOnAddingParameterMode
        {
            get => this.isOnAddingParameterMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnAddingParameterMode, value);
        }

        /// <summary>
        /// Gets a value indicating that source model and target model are based on the same <see cref="Iteration"/>
        /// </summary>
        public bool IsSourceModelSameAsTargetModel
        {
            get => this.isSourceModelSameAsTargetModel;
            set => this.RaiseAndSetIfChanged(ref this.isSourceModelSameAsTargetModel, value);
        }

        /// <summary>
        /// Set the selected <see cref="ElementDefinition" />
        /// </summary>
        /// <param name="selectedElementBase">The selected <see cref="ElementBase" /></param>
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

            this.ElementDefinitionDetailsViewModel.Rows = this.SelectedElementDefinition?.Parameter.Select(x => new ElementDefinitionDetailsRowViewModel(x, this.CurrentDomain, this.selectedHostElementUsage)).ToList();
            this.AddParameterViewModel.SetSelectedElementDefinition(this.SelectedElementDefinition);
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
        /// Gets or sets a value indicating whether the user is currently confirming deletion of
        /// <see cref="SelectedElement" />.
        /// </summary>
        public bool IsOnDeletionMode
        {
            get => this.isOnDeletionMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnDeletionMode, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently editing
        /// <see cref="SelectedElement" /> through the edit-Element popup.
        /// </summary>
        public bool IsOnEditMode
        {
            get => this.isOnEditMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnEditMode, value);
        }

        /// <summary>
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> that drives the delete-confirmation popup.
        /// </summary>
        public IConfirmCancelPopupViewModel DeleteElementPopupViewModel { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="SelectedElement" /> is the iteration's
        /// <see cref="Iteration.TopElement" /> — in which case deletion must be blocked because the
        /// iteration always requires a top element.
        /// </summary>
        public bool IsSelectedElementTopElement
            => this.SelectedElement is ElementDefinition selectedDefinition
               && this.CurrentThing?.TopElement != null
               && selectedDefinition.Iid == this.CurrentThing.TopElement.Iid;

        /// <summary>
        /// Opens the delete-confirmation popup with a content message describing the element to delete. No
        /// reference scan is performed because the COMET server cascades cleanup of any referencing
        /// <see cref="ElementUsage" />s when an <see cref="ElementDefinition" /> is deleted.
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
        /// <see cref="ISessionService.DeleteThingsWithNotification" /> with the cloned containing
        /// <see cref="Thing" /> as the operation top container. Clears the selection on success and always
        /// closes the popup.
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

                var result = await this.sessionService.DeleteThingsWithNotification(clonedContainer, new[] { clonedThing }, GetDeletionNotificationDescription(thing));

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
        /// operation through the existing <see cref="COMET.Web.Common.Services.NotificationService.INotificationService" /> pipeline.
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
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> driving the parameter delete-confirmation popup.
        /// </summary>
        public IConfirmCancelPopupViewModel DeleteParameterPopupViewModel { get; }

        /// <summary>
        /// Opens the delete-confirmation popup for the supplied <see cref="Parameter" />, captures the
        /// parameter as the deletion target, and composes a content message that identifies the parameter
        /// type and the containing <see cref="ElementDefinition" />.
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
        /// Performs the deletion of the parameter captured by <see cref="OpenDeleteParameterPopup" /> using
        /// <see cref="ISessionService.DeleteThingsWithNotification" /> with the cloned containing
        /// <see cref="ElementDefinition" /> as the operation top container. Refreshes the parameter rows
        /// shown by the details panel on success and always closes the popup.
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

                var result = await this.sessionService.DeleteThingsWithNotification(clonedContainer, new[] { clonedParameter }, GetParameterDeletionNotificationDescription(parameter));

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
        /// <see cref="Parameter" /> deletion through the existing notification pipeline.
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
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> driving the create-confirmation popup for a
        /// new <see cref="ParameterSubscription" /> by the currently logged-in
        /// <see cref="DomainOfExpertise" />.
        /// </summary>
        public IConfirmCancelPopupViewModel CreateSubscriptionPopupViewModel { get; }

        /// <summary>
        /// Opens the create-confirmation popup for the supplied <see cref="Parameter" />, captures the
        /// parameter as the subscription target, and composes a content message that names the parameter,
        /// its containing <see cref="ElementDefinition" /> and the current <see cref="DomainOfExpertise" />.
        /// No-op when there is no current domain or when the parameter is already owned by the current
        /// domain — neither case admits a subscription.
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
        /// Performs the creation of a new <see cref="ParameterSubscription" /> for the parameter captured by
        /// <see cref="OpenCreateSubscriptionPopup" /> using
        /// <see cref="ISessionService.CreateOrUpdateThingsWithNotification" />. The cloned parent
        /// <see cref="Parameter" /> is the operation top container; the new subscription and a
        /// <see cref="ParameterSubscriptionValueSet" /> per source <see cref="ParameterValueSet" /> are
        /// included as things-to-create. The visible card refresh is driven by the existing
        /// <see cref="ICDPMessageBus" /> end-update / session-refreshed pipeline that already reaches
        /// <see cref="OnSessionRefreshed" />.
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

                var thingsToCreate = new List<Thing> { clonedParameter, subscription };

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
        /// <see cref="ParameterSubscription" /> creation through the existing notification pipeline.
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
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> driving the delete-confirmation popup for a
        /// <see cref="ParameterSubscription" /> belonging to the currently logged-in
        /// <see cref="DomainOfExpertise" />.
        /// </summary>
        public IConfirmCancelPopupViewModel DeleteSubscriptionPopupViewModel { get; }

        /// <summary>
        /// Opens the delete-confirmation popup for the supplied <see cref="ParameterSubscription" />,
        /// captures the subscription as the deletion target, and composes a content message that makes it
        /// explicit only the subscription is removed — the parent <see cref="Parameter" /> is kept. No-op
        /// when the subscription is null or its container is not a <see cref="Parameter" />.
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
        /// <see cref="OpenDeleteSubscriptionPopup" /> using
        /// <see cref="ISessionService.DeleteThingsWithNotification" />. The cloned parent
        /// <see cref="Parameter" /> is the operation top container but is intentionally kept out of the
        /// things-to-delete collection — only the cloned subscription is deleted, so the parameter itself
        /// remains. The visible card refresh is driven by the existing message-bus end-update pipeline.
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

                await this.sessionService.DeleteThingsWithNotification(clonedParameter, new[] { (Thing)clonedSubscription }, GetSubscriptionDeletionNotificationDescription(subscription));
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
        /// <see cref="ParameterSubscription" /> deletion through the existing notification pipeline.
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
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> driving the create-confirmation popup for a
        /// new <see cref="ParameterOverride" /> on the currently selected <see cref="ElementUsage" /> by the
        /// currently logged-in <see cref="DomainOfExpertise" />.
        /// </summary>
        public IConfirmCancelPopupViewModel CreateOverridePopupViewModel { get; }

        /// <summary>
        /// Opens the create-confirmation popup for the supplied <see cref="Parameter" /> on the supplied
        /// <see cref="ElementUsage" />, captures both as the override target, and composes a content message
        /// that names the parameter, the host usage, and the current <see cref="DomainOfExpertise" />. No-op
        /// when there is no current domain, no host usage, or no parameter — none of which admit override
        /// creation.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> the user requested to override.</param>
        /// <param name="hostElementUsage">
        /// The <see cref="ElementUsage" /> on which the new <see cref="ParameterOverride" /> will be created.
        /// </param>
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
        /// captured by <see cref="OpenCreateOverridePopup" /> using
        /// <see cref="ISessionService.CreateOrUpdateThingsWithNotification" />. The cloned host
        /// <see cref="ElementUsage" /> is the operation top container; the new override is added to its
        /// <see cref="ElementUsage.ParameterOverride" /> collection. Per the IME's flow, the matching
        /// <see cref="ParameterOverrideValueSet" /> instances are generated server-side and are not included
        /// in the things-to-create collection. The visible card refresh is driven by the existing
        /// end-update / session-refreshed pipeline that already reaches <see cref="OnSessionRefreshed" />.
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

                await this.sessionService.CreateOrUpdateThingsWithNotification(clonedUsage, new[] { (Thing)clonedUsage, parameterOverride }, GetOverrideCreationNotificationDescription(parameter, hostUsage));
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
        /// Closes the create-override popup and clears the captured target.
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
        /// <see cref="ParameterOverride" /> creation through the existing notification pipeline.
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
        /// Gets the <see cref="IConfirmCancelPopupViewModel" /> driving the delete-confirmation popup for a
        /// <see cref="ParameterOverride" /> on the currently selected <see cref="ElementUsage" />.
        /// </summary>
        public IConfirmCancelPopupViewModel DeleteOverridePopupViewModel { get; }

        /// <summary>
        /// Opens the delete-confirmation popup for the supplied <see cref="ParameterOverride" />, captures
        /// the override as the deletion target, and composes a content message that makes it explicit only
        /// the override is removed — the source <see cref="Parameter" /> on the contained
        /// <see cref="ElementDefinition" /> is kept. No-op when the override is null or its container is not
        /// an <see cref="ElementUsage" />.
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
        /// <see cref="OpenDeleteOverridePopup" /> using
        /// <see cref="ISessionService.DeleteThingsWithNotification" />. The cloned parent
        /// <see cref="ElementUsage" /> is the operation top container but is intentionally kept out of the
        /// things-to-delete collection — only the cloned override is deleted, so the source
        /// <see cref="Parameter" /> on the contained <see cref="ElementDefinition" /> remains. The visible
        /// card refresh is driven by the existing message-bus end-update pipeline.
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

                await this.sessionService.DeleteThingsWithNotification(clonedUsage, new[] { (Thing)clonedOverride }, GetOverrideDeletionNotificationDescription(parameterOverride));
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
        /// <see cref="ParameterOverride" /> deletion through the existing notification pipeline.
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
        /// Opens the edit-Element popup for <see cref="SelectedElement" />, initializing whichever child
        /// view model corresponds to the kind of selection (<see cref="ElementDefinition" /> or
        /// <see cref="ElementUsage" />) with deep clones of the target so the form mutates a private copy.
        /// </summary>
        public void OpenEditElementPopup()
        {
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
        /// Performs the edit of the selected <see cref="ElementDefinition" /> using the working clones held
        /// by <see cref="EditElementDefinitionViewModel" />. Promotes the cloned definition to the
        /// iteration's <see cref="Iteration.TopElement" /> when the user opted in via the popup. Always
        /// closes the popup.
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

                var thingsToUpdate = new List<Thing> { clonedIteration, clonedElementDefinition };
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
        /// Performs the edit of the selected <see cref="ElementUsage" /> using the working clone held by
        /// <see cref="EditElementUsageViewModel" />. Always closes the popup.
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

                var clonedContainer = originalElementUsage.Container.Clone(false);

                await this.sessionService.CreateOrUpdateThingsWithNotification(clonedContainer, new[] { (Thing)clonedElementUsage }, GetElementUsageEditNotificationDescription(clonedElementUsage));
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
        /// Closes the edit-Element popup.
        /// </summary>
        private void CloseEditElementPopup()
        {
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Builds a <see cref="NotificationDescription" /> used to surface the success / failure of an
        /// <see cref="ElementDefinition" /> edit through the existing notification pipeline.
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
        /// <see cref="ElementUsage" /> edit through the existing notification pipeline.
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
        /// Opens the <see cref="ElementDefinitionCreation" /> popup
        /// </summary>
        public void OpenCreateElementDefinitionCreationPopup()
        {
            this.ElementDefinitionCreationViewModel.InitializeViewModel(this.SelectedElementDefinition.GetContainerOfType<Iteration>());
            this.ElementDefinitionCreationViewModel.ElementDefinition = new ElementDefinition();
            this.ElementDefinitionCreationViewModel.SelectedCategories = new List<Category>();
            this.IsOnCreationMode = true;
        }

        /// <summary>
        /// Opens the <see cref="AddParameter" /> popup
        /// </summary>
        public void OpenAddParameterPopup()
        {
            this.AddParameterViewModel.ResetValues();
            this.IsOnAddingParameterMode = true;
        }

        /// <summary>
        /// Add a new <see cref="ElementDefinition"/> based on an existing <see cref="ElementBase"/>
        /// </summary>
        /// <param name="elementDefinitionTree">The <see cref="ElementDefinitionTree"/> to copy the node to</param>
        /// <param name="elementBase">The <see cref="ElementBase"/> to copy</param>
        public Task CopyAndAddNewElementAsync(ElementDefinitionTree elementDefinitionTree, ElementBase elementBase)
        {
            ArgumentNullException.ThrowIfNull(elementDefinitionTree);
            ArgumentNullException.ThrowIfNull(elementBase);

            return this.CopyAndAddNewElementImplAsync(elementDefinitionTree, elementBase);
        }

        /// <summary>
        /// Add a new <see cref="ElementDefinition"/> based on an existing <see cref="ElementBase"/>
        /// </summary>
        /// <param name="elementDefinitionTree">The <see cref="ElementDefinitionTree"/> to copy the node to</param>
        /// <param name="elementBase">The <see cref="ElementBase"/> to copy</param>
        private async Task CopyAndAddNewElementImplAsync(ElementDefinitionTree elementDefinitionTree, ElementBase elementBase)
        {
            this.IsLoading = true;

            try
            {
                if (elementBase.GetContainerOfType<Iteration>() == elementDefinitionTree.ViewModel.Iteration)
                {
                    var copyCreator = new CopyElementDefinitionCreator(this.sessionService.Session);
                    await copyCreator.CopyAsync((ElementDefinition)elementBase, true);
                }
                else
                {
                    var copyCreator = new CopyCreator(this.sessionService.Session);

                    this.cacheService.TryGetOrAddBrowserSessionSetting(BrowserSessionSettingKey.CopyElementDefinitionOperationKind, OperationKind.Copy, out var selectedOperationKind);

                    await copyCreator.CopyAsync((ElementDefinition)elementBase, elementDefinitionTree.ViewModel.Iteration, selectedOperationKind is OperationKind operationKind ? operationKind : OperationKind.Copy);
                }
            }
            finally

            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Add a new <see cref="ElementUsage"/> based on an existing <see cref="ElementBase"/>
        /// </summary>
        /// <param name="fromElementBase">The <see cref="ElementBase"/> to be added as <see cref="ElementUsage"/></param>
        /// <param name="toElementBase">The <see cref="ElementBase"/> where to add the new <see cref="ElementUsage"/> to</param>
        public Task AddNewElementUsageAsync(ElementBase fromElementBase, ElementBase toElementBase)
        {
            ArgumentNullException.ThrowIfNull(fromElementBase);
            ArgumentNullException.ThrowIfNull(toElementBase);

            return this.AddNewElementUsageImplAsync(fromElementBase, toElementBase);
        }

        /// <summary>
        /// Add a new <see cref="ElementUsage"/> based on an existing <see cref="ElementBase"/>
        /// </summary>
        /// <param name="fromElementBase">The <see cref="ElementBase"/> to be added as <see cref="ElementUsage"/></param>
        /// <param name="toElementBase">The <see cref="ElementBase"/> where to add the new <see cref="ElementUsage"/> to</param>
        private async Task AddNewElementUsageImplAsync(ElementBase fromElementBase, ElementBase toElementBase)
        {
            if (fromElementBase.GetContainerOfType<Iteration>() == toElementBase.GetContainerOfType<Iteration>())
            {
                this.IsLoading = true;

                var thingCreator = new ThingCreator();

                try
                {
                    await thingCreator.CreateElementUsageAsync((ElementDefinition)toElementBase, (ElementDefinition)fromElementBase, this.sessionService.Session.OpenIterations.First(x => x.Key == toElementBase.GetContainerOfType<Iteration>()).Value.Item1, this.sessionService.Session);
                }
                finally
                {
                    this.IsLoading = false;
                }
            }
        }

        /// <summary>
        /// Tries to create a new <see cref="ElementDefinition" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task AddingElementDefinitionAsync()
        {
            var thingsToCreate = new List<Thing>();

            if (this.ElementDefinitionCreationViewModel.SelectedCategories.Any())
            {
                this.ElementDefinitionCreationViewModel.ElementDefinition.Category = this.ElementDefinitionCreationViewModel.SelectedCategories.ToList();
            }

            var iteration = this.SelectedElementDefinition.GetContainerOfType<Iteration>();
            this.ElementDefinitionCreationViewModel.ElementDefinition.Container = iteration;
            thingsToCreate.Add(this.ElementDefinitionCreationViewModel.ElementDefinition);
            var clonedIteration = iteration.Clone(false);

            if (this.ElementDefinitionCreationViewModel.IsTopElement)
            {
                clonedIteration.TopElement = this.ElementDefinitionCreationViewModel.ElementDefinition;
            }

            clonedIteration.Element.Add(this.ElementDefinitionCreationViewModel.ElementDefinition);
            thingsToCreate.Add(clonedIteration);

            try
            {
                await this.sessionService.CreateOrUpdateThings(clonedIteration, thingsToCreate);
            }
            finally
            {
                this.IsOnCreationMode = false;
            }
        }

        /// <summary>
        /// Handles the <see cref="SessionStatus.EndUpdate" /> message received
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnEndUpdate()
        {
            await this.OnSessionRefreshed();
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            this.SelectElement(this.SelectedElement ?? this.SelectedElementDefinition);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();

            if (this.CurrentThing == null)
            {
                return;
            }

            this.AddParameterViewModel.InitializeViewModel(this.CurrentThing);
            this.ElementDefinitionCreationViewModel.InitializeViewModel(this.CurrentThing);

            this.IsLoading = false;
        }
    }
}
