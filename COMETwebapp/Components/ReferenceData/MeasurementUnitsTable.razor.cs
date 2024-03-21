// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitsTable.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.ReferenceData
{
    using System.Threading.Tasks;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.ViewModels.Components.ReferenceData;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="MeasurementUnitsTable"/>
    /// </summary>
    public partial class MeasurementUnitsTable : SelectedReferenceDataItem<MeasurementUnit>
    {
        /// <summary>
        /// The <see cref="IMeasurementUnitsTableViewModel" /> for this component
        /// </summary>
        [Inject]
        public IMeasurementUnitsTableViewModel ViewModel { get; set; }

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
        /// Method that is invoked when the edit/add measurement unit form is being saved
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        protected override void OnEditMeasurementUnitSaving()
        {
            if (!this.ShouldCreateMeasurementUnit)
            {
                // update measurement unit
            }

            // create measurement unit
        }

        /// <summary>
        /// Method invoked when creating a new measurement unit
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        protected override void CustomizeEditMeasurementUnit(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (MeasurementUnitRowViewModel)e.DataItem;
            this.ShouldCreateMeasurementUnit = e.IsNew;

            if (dataItem == null)
            {
                e.EditModel = new SimpleUnit();
                this.ViewModel.MeasurementUnit = new SimpleUnit();
                return;
            }

            e.EditModel = dataItem;
            this.ViewModel.MeasurementUnit = dataItem.Thing.Clone(true);
        }
    }
}
