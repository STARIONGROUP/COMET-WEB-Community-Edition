// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTableViewModel.cs" company="Starion Group S.A.">
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
    using System.Collections.ObjectModel;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel;
    using COMETwebapp.ViewModels.Components.ModelEditor.Rows;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="ElementDefinitionTable" />
    /// </summary>
    public class ElementDefinitionTableViewModel : SingleIterationApplicationBaseViewModel, IElementDefinitionTableViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Backing field for <see cref="IsOnAddingParameterMode" />
        /// </summary>
        private bool isOnAddingParameterMode;

        /// <summary>
        /// Backing field for <see cref="IsOnCreationMode" />
        /// </summary>
        private bool isOnCreationMode;

        /// <summary>
        /// Creates a new instance of <see cref="ElementDefinitionTableViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        public ElementDefinitionTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.sessionService = sessionService;
            var eventCallbackFactory = new EventCallbackFactory();

            this.ElementDefinitionCreationViewModel = new ElementDefinitionCreationViewModel(sessionService, messageBus)
            {
                OnValidSubmit = eventCallbackFactory.Create(this, this.AddingElementDefinition)
            };

            this.AddParameterViewModel = new AddParameterViewModel.AddParameterViewModel(sessionService, messageBus)
            {
                OnParameterAdded = eventCallbackFactory.Create(this, () => this.IsOnAddingParameterMode = false)
            };

            this.InitializeSubscriptions([typeof(ElementBase)]);
            this.RegisterViewModelWithReusableRows(this);
        }

        /// <summary>
        /// All <see cref="ElementBase" /> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = [];

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
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel" /> for target model
        /// </summary>
        public ObservableCollection<ElementDefinitionRowViewModel> RowsTarget { get; } = [];

        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel" /> for source model
        /// </summary>
        public ObservableCollection<ElementDefinitionRowViewModel> RowsSource { get; } = [];

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
        /// Represents the selected ElementDefinitionRowViewModel
        /// </summary>
        public ElementDefinition SelectedElementDefinition { get; set; }

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
        /// Add rows related to <see cref="ElementBase" /> that has been added
        /// </summary>
        /// <param name="addedThings">A collection of added <see cref="Thing" /></param>
        public void AddRows(IEnumerable<Thing> addedThings)
        {
            var listOfAddedElementBases = addedThings.OfType<ElementBase>().ToList();
            this.RowsSource.AddRange(listOfAddedElementBases.Select(e => new ElementDefinitionRowViewModel(e)));
            this.RowsTarget.AddRange(listOfAddedElementBases.Select(e => new ElementDefinitionRowViewModel(e)));
        }

        /// <summary>
        /// Updates rows related to <see cref="ElementBase" /> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="ElementBase" /></param>
        public void UpdateRows(IEnumerable<Thing> updatedThings)
        {
            foreach (var element in updatedThings.OfType<ElementBase>())
            {
                var row = this.RowsSource.FirstOrDefault(x => x.ElementBase.Iid == element.Iid);
                row?.UpdateProperties(new ElementDefinitionRowViewModel(element));

                row = this.RowsTarget.FirstOrDefault(x => x.ElementBase.Iid == element.Iid);
                row?.UpdateProperties(new ElementDefinitionRowViewModel(element));

                if (element.Iid == this.SelectedElementDefinition.Iid)
                {
                    this.SelectElement(element);
                }
            }
        }

        /// <summary>
        /// Remove rows related to a <see cref="ElementBase" /> that has been deleted
        /// </summary>
        /// <param name="deletedThings">A collection of deleted <see cref="ElementBase" /></param>
        public void RemoveRows(IEnumerable<Thing> deletedThings)
        {
            foreach (var elementId in deletedThings.OfType<ElementBase>().Select(x => x.Iid))
            {
                var row = this.RowsSource.FirstOrDefault(x => x.ElementBase.Iid == elementId);

                if (row != null)
                {
                    this.RowsSource.Remove(row);
                }

                row = this.RowsTarget.FirstOrDefault(x => x.ElementBase.Iid == elementId);

                if (row != null)
                {
                    this.RowsTarget.Remove(row);
                }
            }
        }

        /// <summary>
        /// Tries to create a new <see cref="ElementDefinition" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task AddingElementDefinition()
        {
            var thingsToCreate = new List<Thing>();

            if (this.ElementDefinitionCreationViewModel.SelectedCategories.Any())
            {
                this.ElementDefinitionCreationViewModel.ElementDefinition.Category = this.ElementDefinitionCreationViewModel.SelectedCategories.ToList();
            }

            this.ElementDefinitionCreationViewModel.ElementDefinition.Container = this.CurrentThing;
            thingsToCreate.Add(this.ElementDefinitionCreationViewModel.ElementDefinition);
            var clonedIteration = this.CurrentThing.Clone(false);

            if (this.ElementDefinitionCreationViewModel.IsTopElement)
            {
                clonedIteration.TopElement = this.ElementDefinitionCreationViewModel.ElementDefinition;
            }

            clonedIteration.Element.Add(this.ElementDefinitionCreationViewModel.ElementDefinition);
            thingsToCreate.Add(clonedIteration);

            try
            {
                await this.sessionService.CreateOrUpdateThings(clonedIteration, thingsToCreate);
                this.IsOnCreationMode = false;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
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
        protected override async Task OnSessionRefreshed()
        {
            if (this.AddedThings.Count == 0 && this.DeletedThings.Count == 0 && this.UpdatedThings.Count == 0)
            {
                return;
            }

            this.IsLoading = true;
            await Task.Delay(1);

            this.UpdateInnerComponents();
            this.ClearRecordedChanges();
            this.IsLoading = false;
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

            this.Elements.Clear();
            this.RowsTarget.Clear();
            this.RowsSource.Clear();

            foreach (var element in this.CurrentThing.Element)
            {
                this.Elements.Add(element);
                this.Elements.AddRange(element.ContainedElement);
            }

            foreach (var element in this.Elements)
            {
                this.RowsTarget.Add(new ElementDefinitionRowViewModel(element));
                this.RowsSource.Add(new ElementDefinitionRowViewModel(element));
            }

            this.AddParameterViewModel.InitializeViewModel(this.CurrentThing);
            this.ElementDefinitionCreationViewModel.InitializeViewModel(this.CurrentThing);

            this.IsLoading = false;
        }
    }
}
