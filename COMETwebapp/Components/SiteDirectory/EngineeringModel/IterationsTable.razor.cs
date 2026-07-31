// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationsTable.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.SiteDirectory.EngineeringModel
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="IterationsTable" />
    /// </summary>
    public partial class IterationsTable : SelectedDataItemBase<IterationSetup, IterationSetupRowViewModel>
    {
        /// <summary>
        /// Gets or sets the <see cref="IIterationsTableViewModel" />
        /// </summary>
        [Parameter]
        public IIterationsTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component is initialized
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Initialize(this.ViewModel);
        }

        /// <summary>
        /// Starts the creation flow for a new <see cref="IterationSetup" /> item.
        /// </summary>
        public override void StartCreate()
        {
            base.StartCreate();
            this.ViewModel.CurrentThing = new IterationSetup();

            if (this.ViewModel.SourceIterations.Any())
            {
                this.ViewModel.CurrentThing.SourceIterationSetup = this.ViewModel.SourceIterations.OrderByDescending(x => x.IterationNumber).First();
            }
        }

        /// <summary>
        /// Starts the edit flow for the specified <paramref name="row" />.
        /// </summary>
        /// <param name="row">The selected row to edit.</param>
        public override void StartEdit(IterationSetupRowViewModel row)
        {
            if (row == null)
            {
                return;
            }

            this.OnSelectedDataItemChanged(row);
        }

        /// <summary>
        /// Method invoked every time a row is selected
        /// </summary>
        /// <param name="row">The selected row</param>
        protected override void OnSelectedDataItemChanged(IterationSetupRowViewModel row)
        {
            base.OnSelectedDataItemChanged(row);
            this.ShouldCreateThing = false;
            this.ViewModel.CurrentThing = row.Thing.Clone(true);
        }

        /// <summary>
        /// Method invoked whenever a form is saved
        /// </summary>
        protected override void OnSaved()
        {
            base.OnSaved();
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Gets a value indicating whether delete is enabled for a row
        /// </summary>
        /// <param name="row">The row view model</param>
        /// <returns>True if allowed to delete</returns>
        private bool IsDeleteEnabled(IterationSetupRowViewModel row)
        {
            if (row.Thing.IsDeleted || !row.IsAllowedToWrite)
            {
                return false;
            }

            var maxIterationNumber = this.ViewModel.SourceIterations
                .Where(x => !x.IsDeleted)
                .Select(x => x.IterationNumber)
                .DefaultIfEmpty(0)
                .Max();

            return row.Thing.IterationNumber != maxIterationNumber;
        }
    }
}
