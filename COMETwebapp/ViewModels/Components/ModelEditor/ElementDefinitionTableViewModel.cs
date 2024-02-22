// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTableViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="ElementDefinitionTable" />
    /// </summary>
    public class ElementDefinitionTableViewModel : SingleIterationApplicationBaseViewModel, IElementDefinitionTableViewModel
    {
        /// <summary>
        /// The current <see cref="Iteration" />
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

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
            this.iteration = sessionService?.OpenIterations.Items.FirstOrDefault();
            this.sessionService = sessionService;
            this.InitializeElements();

            this.ElementDefinitionCreationViewModel = new ElementDefinitionCreationViewModel(sessionService)
            {
                OnValidSubmit = new EventCallbackFactory().Create(this, this.AddingElementDefinition)
            };

            this.InitializeSubscriptions(new List<Type> { typeof(ElementBase) });
        }

        /// <summary>
        /// All <see cref="ElementBase" /> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new();

        /// <summary>
        /// The <see cref="IElementDefinitionDetailsViewModel" />
        /// </summary>
        public IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; } = new ElementDefinitionDetailsViewModel();

        /// <summary>
        /// Gets the <see cref="IElementDefinitionCreationViewModel" />
        /// </summary>
        public IElementDefinitionCreationViewModel ElementDefinitionCreationViewModel { get; set; }

        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel" /> for target model
        /// </summary>
        public ObservableCollection<ElementDefinitionRowViewModel> RowsTarget { get; } = new();

        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel" /> for source model
        /// </summary>
        public ObservableCollection<ElementDefinitionRowViewModel> RowsSource { get; } = new();

        /// <summary>
        /// Value indicating the user is currently creating a new <see cref="ElementDefinition" />
        /// </summary>
        public bool IsOnCreationMode
        {
            get => this.isOnCreationMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnCreationMode, value);
        }

        /// <summary>
        /// Represents the selected ElementDefinitionRowViewModel
        /// </summary>
        public object SelectedElementDefinition { get; set; }

        /// <summary>
        /// set the selected <see cref="ElementDefinitionRowViewModel" /> 
        /// </summary>
        /// <param name="args">The <see cref="GridRowClickEventArgs" /></param>
        public void SelectElement(GridRowClickEventArgs args)
        {
            var selectedNode = (ElementDefinitionRowViewModel)args.Grid.GetDataItem(args.VisibleIndex);

            // It is preferable to have a selection based on the Iid of the Thing
            this.ElementDefinitionDetailsViewModel.SelectedSystemNode = selectedNode.ElementBase;

            this.ElementDefinitionDetailsViewModel.Rows = this.ElementDefinitionDetailsViewModel.SelectedSystemNode switch
            {
                ElementDefinition elementDefinition => elementDefinition.Parameter.Select(x => new ElementDefinitionDetailsRowViewModel(x)).ToList(),
                ElementUsage elementUsage => elementUsage.ElementDefinition.Parameter.Select(x => new ElementDefinitionDetailsRowViewModel(x)).ToList(),
                _ => null
            };
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
        /// Updates rows related to <see cref="ElementBase" /> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="ElementBase" /></param>
        public void UpdateRows(IEnumerable<ElementBase> updatedThings)
        {
            foreach (var element in updatedThings)
            {
                var row = this.RowsSource.FirstOrDefault(x => x.ElementBase.Iid == element.Iid);

                row?.UpdateProperties(new ElementDefinitionRowViewModel(element));

                row = this.RowsTarget.FirstOrDefault(x => x.ElementBase.Iid == element.Iid);
                row?.UpdateProperties(new ElementDefinitionRowViewModel(element));
            }
        }

        /// <summary>
        /// Remove rows related to a <see cref="ElementBase" /> that has been deleted
        /// </summary>
        /// <param name="deletedThings">A collection of deleted <see cref="ElementBase" /></param>
        public void RemoveRows(IEnumerable<ElementBase> deletedThings)
        {
            foreach (var elementId in deletedThings.Select(x => x.Iid))
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

            this.ElementDefinitionCreationViewModel.ElementDefinition.Container = this.iteration;
            thingsToCreate.Add(this.ElementDefinitionCreationViewModel.ElementDefinition);
            var clonedIteration = this.iteration.Clone(false);

            if (this.ElementDefinitionCreationViewModel.IsTopElement)
            {
                clonedIteration.TopElement = this.ElementDefinitionCreationViewModel.ElementDefinition;
            }

            clonedIteration.Element.Add(this.ElementDefinitionCreationViewModel.ElementDefinition);

            try
            {
                await this.sessionService.CreateThings(clonedIteration, thingsToCreate);
                this.IsOnCreationMode = false;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnSessionRefreshed()
        {
            if (!this.AddedThings.Any() && !this.DeletedThings.Any() && !this.UpdatedThings.Any())
            {
                return;
            }

            this.IsLoading = true;
            await Task.Delay(1);

            this.RemoveRows(this.DeletedThings.OfType<ElementBase>());
            this.RowsSource.AddRange(this.AddedThings.OfType<ElementBase>().Select(e => new ElementDefinitionRowViewModel(e)));
            this.RowsTarget.AddRange(this.AddedThings.OfType<ElementBase>().Select(e => new ElementDefinitionRowViewModel(e)));
            this.UpdateRows(this.UpdatedThings.OfType<ElementBase>());

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
            this.IsLoading = false;
        }

        /// <summary>
        /// Initialize <see cref="ElementBase" /> list
        /// </summary>
        private void InitializeElements()
        {
            if (this.iteration != null)
            {
                this.iteration.Element.ForEach(e =>
                {
                    this.Elements.Add(e);
                    this.Elements.AddRange(e.ContainedElement);
                });

                this.Elements.ForEach(e => this.RowsTarget.Add(new ElementDefinitionRowViewModel(e)));
                this.Elements.ForEach(e => this.RowsSource.Add(new ElementDefinitionRowViewModel(e)));
            }
        }
    }
}
