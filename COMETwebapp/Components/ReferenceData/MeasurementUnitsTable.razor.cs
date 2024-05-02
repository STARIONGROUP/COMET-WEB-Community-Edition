﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitsTable.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.ReferenceData.MeasurementUnits;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.Wrappers;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="MeasurementUnitsTable"/>
    /// </summary>
    public partial class MeasurementUnitsTable : SelectedDeprecatableDataItemBase<MeasurementUnit, MeasurementUnitRowViewModel>
    {
        /// <summary>
        /// The <see cref="IMeasurementUnitsTableViewModel" /> for this component
        /// </summary>
        [Inject]
        public IMeasurementUnitsTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Condition to check if the shortname and name fields should be readonly
        /// </summary>
        private bool ShouldNameAndShortNameBeReadOnly => this.ViewModel.Thing is PrefixedUnit;

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
        protected override async Task OnEditThingSaving()
        {
            await this.ViewModel.CreateOrEditMeasurementUnit(this.ShouldCreateThing);
        }

        /// <summary>
        /// Method invoked when creating a new measurement unit
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        protected override void CustomizeEditThing(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (MeasurementUnitRowViewModel)e.DataItem;
            this.ShouldCreateThing = e.IsNew;

            if (dataItem == null)
            {
                this.ViewModel.SelectedMeasurementUnitType = new ClassKindWrapper(ClassKind.SimpleUnit);
                e.EditModel = this.ViewModel.Thing;
                return;
            }

            e.EditModel = dataItem;
            this.ViewModel.Thing = dataItem.Thing.Clone(true);
            this.ViewModel.SelectedReferenceDataLibrary = (ReferenceDataLibrary)dataItem.Thing.Container;
        }
    }
}
