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

    using DevExpress.Blazor;

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
        /// Method that is invoked when the edit/add thing form is being saved
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnEditThingSaving()
        {
            await this.ViewModel.CreateOrEditIteration(this.ShouldCreateThing);
        }

        /// <summary>
        /// Method invoked when creating a new thing
        /// </summary>
        /// <param name="e">A <see cref="DevExpress.Blazor.GridCustomizeEditModelEventArgs" /></param>
        protected override void CustomizeEditThing(GridCustomizeEditModelEventArgs e)
        {
            base.CustomizeEditThing(e);

            var dataItem = (IterationSetupRowViewModel)e.DataItem;
            this.ShouldCreateThing = e.IsNew;
            this.ViewModel.CurrentThing = dataItem == null ? new IterationSetup() : dataItem.Thing.Clone(true);

            if (this.ShouldCreateThing && this.ViewModel.SourceIterations.Any())
            {
                this.ViewModel.CurrentThing.SourceIterationSetup = this.ViewModel.SourceIterations.OrderByDescending(x => x.IterationNumber).First();
            }

            e.EditModel = this.ViewModel.CurrentThing;
        }

        /// <summary>
        /// Sets the selected values for the <see cref="IterationSetup" /> creation and submits the form
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task SetSelectedValuesAndSubmit()
        {
            await this.Grid.SaveChangesAsync();
        }
    }
}
