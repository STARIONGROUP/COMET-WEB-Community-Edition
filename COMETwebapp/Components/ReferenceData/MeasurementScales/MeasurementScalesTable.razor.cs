﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementScalesTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ReferenceData.MeasurementScales
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.ReferenceData.MeasurementScales;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="MeasurementScalesTable" />
    /// </summary>
    public partial class MeasurementScalesTable : SelectedDeprecatableDataItemBase<MeasurementScale, MeasurementScaleRowViewModel>
    {
        /// <summary>
        /// The <see cref="IMeasurementScalesTableViewModel" /> for this component
        /// </summary>
        [Inject]
        public IMeasurementScalesTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Initialize(this.ViewModel);
        }

        /// <summary>
        /// Method that is invoked when the edit/add thing form is being saved
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnEditThingSaving()
        {
            await this.ViewModel.CreateOrEditMeasurementScale(this.ShouldCreateThing);
        }

        /// <summary>
        /// Method invoked when creating a new thing
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        protected override void CustomizeEditThing(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (MeasurementScaleRowViewModel)e.DataItem;
            this.ShouldCreateThing = e.IsNew;
            this.IsOnEditMode = !this.ShouldCreateThing;

            if (this.ShouldCreateThing)
            {
                this.ViewModel.SelectedMeasurementScaleType = this.ViewModel.MeasurementScaleTypes.First(x => x.ClassKind == ClassKind.CyclicRatioScale);
            }

            this.ViewModel.SelectMeasurementScale(dataItem == null ? new CyclicRatioScale() : dataItem.Thing.Clone(true));
            e.EditModel = this.ViewModel.Thing;
        }

        /// <summary>
        /// Method invoked every time a row is selected
        /// </summary>
        /// <param name="row">The selected row</param>
        protected override void OnSelectedDataItemChanged(MeasurementScaleRowViewModel row)
        {
            base.OnSelectedDataItemChanged(row);
            this.ViewModel.SelectMeasurementScale(row.Thing.Clone(true));
        }
    }
}