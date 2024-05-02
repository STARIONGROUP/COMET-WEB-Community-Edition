// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomainOfExpertiseSubscriptionTableViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.SubscriptionDashboard
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    using DynamicData;

    /// <summary>
    /// View Model that provides content related to owned <see cref="ParameterOrOverrideBase" /> where other
    /// <see cref="DomainOfExpertise" /> placed <see cref="ParameterSubscription" />
    /// </summary>
    public class DomainOfExpertiseSubscriptionTableViewModel : IDomainOfExpertiseSubscriptionTableViewModel
    {
        /// <summary>
        /// A collection of all <see cref="OwnedParameterOrOverrideBaseRowViewModel" />
        /// </summary>
        private IEnumerable<OwnedParameterOrOverrideBaseRowViewModel> allRows = new List<OwnedParameterOrOverrideBaseRowViewModel>();

        /// <summary>
        /// A reactive collection of <see cref="OwnedParameterOrOverrideBaseRowViewModel" />
        /// </summary>
        public SourceList<OwnedParameterOrOverrideBaseRowViewModel> Rows { get; } = new();

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="parameters">A collection of <see cref="ParameterOrOverrideBase" /></param>
        public void UpdateProperties(IEnumerable<ParameterOrOverrideBase> parameters)
        {
            this.allRows = parameters.Select(x => new OwnedParameterOrOverrideBaseRowViewModel(x));
            this.UpdateRows(this.allRows);
        }

        /// <summary>
        /// Apply filters on <see cref="OwnedParameterOrOverrideBaseRowViewModel" /> based on the <see cref="Option" /> and
        /// <see cref="ParameterType" />
        /// </summary>
        /// <param name="selectedOption">The selected <see cref="Option" /></param>
        /// <param name="selectedParameterType">The selected <see cref="ParameterType" /></param>
        public void ApplyFilters(Option selectedOption, ParameterType selectedParameterType)
        {
            var filteredRows = new List<OwnedParameterOrOverrideBaseRowViewModel>(this.allRows);

            if (selectedOption != null)
            {
                filteredRows.RemoveAll(x => x.Element is ElementUsage usage
                                            && usage.ExcludeOption.Any(o => o.Iid == selectedOption.Iid));
            }

            if (selectedParameterType != null)
            {
                filteredRows.RemoveAll(x => x.Parameter.ParameterType.Iid != selectedParameterType.Iid);
            }

            this.UpdateRows(filteredRows);
        }

        /// <summary>
        /// Update the <see cref="Rows" /> collection based on a collection of
        /// <see cref="OwnedParameterOrOverrideBaseRowViewModel" /> to display.
        /// </summary>
        /// <param name="rowsToDisplay">A collection of <see cref="OwnedParameterOrOverrideBaseRowViewModel" /></param>
        private void UpdateRows(IEnumerable<OwnedParameterOrOverrideBaseRowViewModel> rowsToDisplay)
        {
            rowsToDisplay = rowsToDisplay.ToList();

            var deletedRows = this.Rows.Items.Where(x => rowsToDisplay.All(r => r.Parameter.Iid != x.Parameter.Iid)).ToList();
            var addedRows = rowsToDisplay.Where(x => this.Rows.Items.All(r => r.Parameter.Iid != x.Parameter.Iid)).ToList();
            var existingRows = rowsToDisplay.Where(x => this.Rows.Items.Any(r => r.Parameter.Iid == x.Parameter.Iid)).ToList();

            this.Rows.RemoveMany(deletedRows);
            this.Rows.AddRange(addedRows);

            foreach (var existingRow in existingRows)
            {
                this.Rows.Items.First(x => x.Parameter.Iid == existingRow.Parameter.Iid).UpdateProperties(existingRow);
            }
        }
    }
}
