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

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

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
        /// The current <see cref="Iteration" />
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// Creates a new instance of <see cref="ElementDefinitionTableViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        public ElementDefinitionTableViewModel(ISessionService sessionService) : base(sessionService)
        {
            this.iteration = sessionService.OpenIterations.Items.FirstOrDefault();
            this.InitializeElements();

            this.InitializeSubscriptions(new List<Type> { typeof(ElementBase) });
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
        /// Update this view model properties
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnIterationChanged()
        {
            await base.OnIterationChanged();
            this.IsLoading = false;
        }
    }
}
