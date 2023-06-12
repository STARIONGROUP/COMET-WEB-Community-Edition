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
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using DynamicData;

    /// <summary>
    /// ViewModel for the <see cref="ElementDefinitionTable" />
    /// </summary>
    public class ElementDefinitionTableViewModel : SingleIterationApplicationBaseViewModel, IElementDefinitionTableViewModel
    {
        /// <summary>
        /// All <see cref="ElementBase" /> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new();

        /// <summary>
        /// A collection of added <see cref="Thing" />s
        /// </summary>
        private readonly List<Thing> addedThings = new();

        /// <summary>
        /// A collection of deleted <see cref="Thing" />s
        /// </summary>
        private readonly List<Thing> deletedThings = new();

        /// <summary>
        /// A collection of updated <see cref="Thing" />s
        /// </summary>
        private readonly List<Thing> updatedThings = new();

        /// <summary>
        /// Creates a new instance of <see cref="ElementDefinitionTableViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        public ElementDefinitionTableViewModel(ISessionService sessionService) : base(sessionService)
        {
            var observables = new List<IObservable<ObjectChangedEvent>>
            {
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ElementBase)),
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ElementBase)),
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ElementBase))
            };

            this.Disposables.Add(observables.Merge().Subscribe(this.RecordChange));
        }

        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel" /> for target model
        /// </summary>
        public ObservableCollection<ElementDefinitionRowViewModel> RowsTarget { get; } = new();

        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel" /> for source model
        /// </summary>
        public ObservableCollection<ElementDefinitionRowViewModel> RowsSource { get; } = new();

        /// <summary>
        ///  Populates the rows in the target and source collections with <see cref="ElementDefinitionRowViewModel"/> objects based on the <see cref="Elements"/>
        /// </summary>
        public void PopulateRows()
        {
            this.RowsTarget.Clear();
            this.RowsSource.Clear();
            this.Elements.ForEach(e => this.RowsTarget.Add(new ElementDefinitionRowViewModel(e)));
            this.Elements.ForEach(e => this.RowsSource.Add(new ElementDefinitionRowViewModel(e)));
        }

        /// <summary>
        /// Initialize <see cref="ElementBase" /> list
        /// </summary>
        private async Task InitializeElements()
        {
            this.IsLoading = true;
            await Task.Delay(1);
            if (this.CurrentIteration != null)
            {
                this.CurrentIteration.Element.ForEach(e =>
                {
                    this.Elements.Add(e);
                    this.Elements.AddRange(e.ContainedElement);
                });
                this.Elements.ForEach(e => this.RowsTarget.Add(new ElementDefinitionRowViewModel(e)));
                this.Elements.ForEach(e => this.RowsSource.Add(new ElementDefinitionRowViewModel(e)));
            }
            this.IsLoading = false;
        }

        /// <summary>
        /// Records an <see cref="ObjectChangedEvent" />
        /// </summary>
        /// <param name="objectChangedEvent">The <see cref="ObjectChangedEvent" /></param>
        protected override void RecordChange(ObjectChangedEvent objectChangedEvent)
        {
            if (this.CurrentIteration == null || objectChangedEvent.ChangedThing.GetContainerOfType<Iteration>().Iid != this.CurrentIteration.Iid)
            {
                return;
            }

            switch (objectChangedEvent.EventKind)
            {
                case EventKind.Added:
                    this.addedThings.Add(objectChangedEvent.ChangedThing);
                    break;
                case EventKind.Removed:
                    this.deletedThings.Add(objectChangedEvent.ChangedThing);
                    break;
                case EventKind.Updated:
                    this.updatedThings.Add(objectChangedEvent.ChangedThing);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(objectChangedEvent), "Unrecognised value EventKind value");
            }
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnSessionRefreshed()
        {
            if (!this.addedThings.Any() && !this.deletedThings.Any() && !this.updatedThings.Any())
            {
                return;
            }

            this.IsLoading = true;
            await Task.Delay(1);

            this.RemoveRows(this.deletedThings.OfType<ElementBase>());
            this.RowsSource.AddRange(this.addedThings.OfType<ElementBase>().Select(e => new ElementDefinitionRowViewModel(e)));
            this.RowsTarget.AddRange(this.addedThings.OfType<ElementBase>().Select(e => new ElementDefinitionRowViewModel(e)));
            this.UpdateRows(this.updatedThings.OfType<ElementBase>());

            this.ClearRecordedChange();
            this.IsLoading = false;
        }

        /// <summary>   
        /// Updates rows related to <see cref="ElementBase" /> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="ElementBase" /></param>
        public void UpdateRows(IEnumerable<ElementBase> updatedThings)
        {
            foreach (ElementBase element in updatedThings)
            {
                var row = this.RowsSource.FirstOrDefault(x => x.ElementBase.Iid == element.Iid);
                if (row != null)
                {
                    row.UpdateProperties(new ElementDefinitionRowViewModel(element));
                }
                row = this.RowsTarget.FirstOrDefault(x => x.ElementBase.Iid == element.Iid);
                if (row != null)
                {
                    row.UpdateProperties(new ElementDefinitionRowViewModel(element));
                }
            }
        }

        /// <summary>
        /// Remove rows related to a <see cref="ElementBase" /> that has been deleted
        /// </summary>
        /// <param name="deletedThings">A collection of deleted <see cref="ElementBase" /></param>
        public void RemoveRows(IEnumerable<ElementBase> deletedThings)
        {
            foreach (ElementBase element in deletedThings)
            {
                var row = this.RowsSource.FirstOrDefault(x => x.ElementBase.Iid == element.Iid);
                if (row != null)
                {
                    this.RowsSource.Remove(row);
                }
                row = this.RowsTarget.FirstOrDefault(x => x.ElementBase.Iid == element.Iid);
                if (row != null)
                {
                    this.RowsTarget.Remove(row);
                }
            }
        }

        /// <summary>
        /// Clears all recorded changed
        /// </summary>
        private void ClearRecordedChange()
        {
            this.deletedThings.Clear();
            this.updatedThings.Clear();
            this.addedThings.Clear();
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnIterationChanged()
        {
            await base.OnIterationChanged();
            await this.InitializeElements();
        }
    }
}
