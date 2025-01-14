// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelEditorViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.CopySettings;
    using COMETwebapp.ViewModels.Components.ModelEditor.ElementDefinitionCreationViewModel;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using Microsoft.AspNetCore.Components;

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
        /// Creates a new instance of <see cref="ModelEditorViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="cacheService">The <see cref="ICacheService"/></param>
        public ModelEditorViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ICacheService cacheService) : base(sessionService, messageBus)
        {
            this.sessionService = sessionService;
            this.cacheService = cacheService;
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
