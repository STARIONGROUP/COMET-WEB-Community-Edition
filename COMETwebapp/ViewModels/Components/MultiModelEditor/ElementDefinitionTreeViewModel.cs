// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTreeViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.MultiModelEditor
{
    using System.Collections.ObjectModel;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.MultiModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.MultiModelEditor.Rows;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="ElementDefinitionTree" />
    /// </summary>
    public class ElementDefinitionTreeViewModel : ApplicationBaseViewModel, IElementDefinitionTreeViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Backing field for <see cref="Iteration"/>
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// Gets the Description of the selected model and iteration
        /// </summary>
        public string Description => this.selectedIterationData?.IterationName ?? "Please select a model";

        /// <summary>
        /// Backing field for the <see cref="SelectedIterationData"/> property
        /// </summary>
        private IterationData selectedIterationData;

        /// <summary>
        /// Creates a new instance of <see cref="ElementDefinitionTableViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        public ElementDefinitionTreeViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.sessionService = sessionService;
            this.RegisterViewModelWithReusableRows(this);
            this.Iterations.Add(null);

            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedIterationData).Subscribe(x => this.Iteration = this.sessionService.OpenIterations.Items.SingleOrDefault(y => y.IterationSetup.Iid == x?.IterationSetupId)));

            this.Disposables.Add(this.WhenAnyValue(x => x.Iteration).Subscribe(x =>
            {
                this.Rows.Clear();

                if (x != null)
                {
                    this.AddRows(x.Element.OrderBy(x => x.Name));
                    this.SelectedIterationData = this.Iterations.SingleOrDefault(y => y?.IterationSetupId == x.IterationSetup.Iid);
                }
            }));

            this.Disposables.Add(this.sessionService.OpenIterations.Connect().Subscribe(this.RefreshIterations));

            this.InitializeSubscriptions([typeof(ElementBase)]);
        }

        /// <summary>
        /// The <see cref="Iteration"/> from which to build the tree
        /// </summary>
        public Iteration Iteration
        {
            get => this.iteration;
            set => this.RaiseAndSetIfChanged(ref this.iteration, value);
        }

        /// <summary>
        /// The <see cref="Iteration"/> from which to build the tree
        /// </summary>
        public IterationData SelectedIterationData
        {
            get => this.selectedIterationData;
            set => this.RaiseAndSetIfChanged(ref this.selectedIterationData, value);
        }

        /// <summary>
        /// Gets or a collection of selectable <see cref="Iteration"/>s
        /// </summary>
        public ObservableCollection<IterationData> Iterations { get; } = [];

        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionTreeRowViewModel" /> for target model
        /// </summary>
        public ObservableCollection<ElementDefinitionTreeRowViewModel> Rows { get; private set; } = [];

        /// <summary>
        /// Add rows related to <see cref="ElementDefinition" /> that has been added
        /// </summary>
        /// <param name="addedThings">A collection of added <see cref="Thing" /></param>
        public void AddRows(IEnumerable<Thing> addedThings)
        {
            var listOfAddedElementBases = addedThings.OfType<ElementDefinition>().Where(x => this.Iteration?.Element.Contains(x) ?? false).ToList();
            this.Rows.AddRange(listOfAddedElementBases.Select(e => new ElementDefinitionTreeRowViewModel(e)));

            if (listOfAddedElementBases.Count > 0)
            {
                this.SortRows();
            }
        }

        /// <summary>
        /// Updates rows related to <see cref="ElementDefinition" /> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="ElementDefinition" /></param>
        public void UpdateRows(IEnumerable<Thing> updatedThings)
        {
            var sortCollection = false;

            foreach (var element in updatedThings.OfType<ElementDefinition>().Where(x => this.Iteration?.Element.Contains(x) ?? false).ToList())
            {
                var row = this.Rows.FirstOrDefault(x => x.ElementBase.Iid == element.Iid);

                if (row != null)
                {
                    sortCollection = true;
                    row.UpdateProperties(new ElementDefinitionTreeRowViewModel(element));
                }
            }

            if (sortCollection)
            {
                this.SortRows();
            }
        }

        /// <summary>
        /// Remove rows related to a <see cref="ElementDefinition" /> that has been deleted
        /// </summary>
        /// <param name="deletedThings">A collection of deleted <see cref="ElementDefinition" /></param>
        public void RemoveRows(IEnumerable<Thing> deletedThings)
        {
            var sortCollection = false;

            foreach (var elementId in deletedThings.OfType<ElementDefinition>().Select(x => x.Iid))
            {
                var row = this.Rows.FirstOrDefault(x => x.ElementBase.Iid == elementId);

                if (row != null)
                {
                    sortCollection = true;
                    this.Rows.Remove(row);
                }
            }

            if (sortCollection)
            {
                this.SortRows();
            }
        }

        /// <summary>
        /// Sorts the rows collection
        /// </summary>
        public void SortRows()
        {
            var sortableList = this.Rows.OrderBy(x => x.ElementName).ToList();

            for (int i = 0; i < sortableList.Count; i++)
            {
                this.Rows.Move(this.Rows.IndexOf(sortableList[i]), i);
            }
        }

        /// <summary>
        /// Handles the <see cref="SessionStatus.EndUpdate" /> message received
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnEndUpdate()
        {
            return this.OnSessionRefreshed();
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            if (this.AddedThings.Count == 0 && this.DeletedThings.Count == 0 && this.UpdatedThings.Count == 0)
            {
                return Task.CompletedTask;
            }

            this.IsLoading = true;

            this.UpdateInnerComponents();
            this.ClearRecordedChanges();
            this.IsLoading = false;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Refreshes the <see cref="Iterations"/> property
        /// </summary>
        /// <param name="changeSet">The <see cref="IChangeSet"/> containing all the necessary changes</param>
        private void RefreshIterations(IChangeSet<Iteration> changeSet)
        {
            foreach (var change in changeSet)
            {
                switch (change.Reason)
                {
                    case ListChangeReason.AddRange:

                        var toBeAdded = change.Range
                            .Select(x => new IterationData(x.IterationSetup, true))
                            .Where(x => !this.Iterations.Contains(x))
                            .ToArray();

                        if (toBeAdded.Length > 0)
                        {
                            this.Iterations.AddRange(toBeAdded);
                        }

                        break;

                    case ListChangeReason.Add:
                        var newIterationData = new IterationData(change.Item.Current.IterationSetup, true);

                        if (!this.Iterations.Contains(newIterationData))
                        {
                            this.Iterations.Add(newIterationData);
                        }

                        break;

                    case ListChangeReason.Remove:

                        if (this.Iterations.FirstOrDefault(x => x?.IterationSetupId == change.Item.Current.IterationSetup.Iid) is { } currentItem)
                        {
                            this.Iterations.Remove(currentItem);
                        }

                        break;
                }
            }
        }
    }
}
