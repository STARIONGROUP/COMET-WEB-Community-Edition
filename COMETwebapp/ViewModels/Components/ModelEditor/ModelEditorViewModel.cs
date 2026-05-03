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
        /// The <see cref="Parameter" /> the user requested to delete, captured when
        /// <see cref="OpenDeleteParameterPopup" /> is invoked and consumed by
        /// <see cref="DeleteSelectedParameterAsync" />.
        /// </summary>
        private Parameter selectedParameterToDelete;

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

            this.ElementDefinitionDetailsViewModel.Rows = this.SelectedElementDefinition?.Parameter.Select(x => new ElementDefinitionDetailsRowViewModel(x)).ToList();
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
                        .Select(x => new ElementDefinitionDetailsRowViewModel(x))
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
            this.SelectElement(this.SelectedElementDefinition);
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
