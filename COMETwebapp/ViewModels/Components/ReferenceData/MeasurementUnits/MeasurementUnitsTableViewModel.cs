// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitsTableViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.MeasurementUnits
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItem;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.Wrappers;

    using ReactiveUI;

    using MeasurementUnit = CDP4Common.SiteDirectoryData.MeasurementUnit;

    /// <summary>
    /// View model used to manage <see cref="MeasurementUnit" />s
    /// </summary>
    public class MeasurementUnitsTableViewModel : DeprecatableDataItemTableViewModel<MeasurementUnit, MeasurementUnitRowViewModel>, IMeasurementUnitsTableViewModel
    {
        /// <summary>
        /// The backing field for <see cref="SelectedMeasurementUnitType"/>
        /// </summary>
        private ClassKindWrapper selectedMeasurementUnitType;

        /// <summary>
        /// FGets the available <see cref="ClassKind"/>s
        /// </summary>
        private static readonly IEnumerable<ClassKind> AvailableMeasurementUnitTypes = [ClassKind.SimpleUnit, ClassKind.DerivedUnit, ClassKind.LinearConversionUnit, ClassKind.PrefixedUnit];

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnitsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public MeasurementUnitsTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus,
            ILogger<MeasurementUnitsTableViewModel> logger) : base(sessionService, messageBus, showHideDeprecatedThingsService, logger)
        {
        }

        /// <summary>
        /// Gets the available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; private set; }

        /// <summary>
        /// Gets the available <see cref="MeasurementUnit" />s from the same rdl as the <see cref="SelectedReferenceDataLibrary"/>
        /// </summary>
        public IEnumerable<MeasurementUnit> ReferenceUnits => this.SelectedReferenceDataLibrary?.QueryMeasurementUnitsFromChainOfRdls();

        /// <summary>
        /// Gets the available <see cref="Prefixes" />s from the same rdl as the <see cref="SelectedReferenceDataLibrary"/>
        /// </summary>
        public IEnumerable<UnitPrefix> Prefixes => this.SelectedReferenceDataLibrary?.QueryUnitPrefixesFromChainOfRdls();

        /// <summary>
        /// Gets the available measurement unit types <see cref="ClassKindWrapper" />s
        /// </summary>
        public IEnumerable<ClassKindWrapper> MeasurementUnitTypes { get; private set; } = AvailableMeasurementUnitTypes.Select(x => new ClassKindWrapper(x));

        /// <summary>
        /// Gets or sets the selected reference data library
        /// </summary>
        public ReferenceDataLibrary SelectedReferenceDataLibrary { get; set; }

        /// <summary>
        /// Gets or sets the selected measurement unit type
        /// </summary>
        public ClassKindWrapper SelectedMeasurementUnitType
        {
            get => this.selectedMeasurementUnitType;
            set
            {
                this.SelectMeasurementUnitType(value);
                this.RaiseAndSetIfChanged(ref this.selectedMeasurementUnitType, value);
            }
        }

        /// <summary>
        /// Initializes the <see cref="MeasurementUnitsTableViewModel"/>
        /// </summary>
        public override void InitializeViewModel()
        {
            this.IsLoading = true;

            base.InitializeViewModel();

            this.ReferenceDataLibraries = this.SessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries();
            this.SelectedReferenceDataLibrary = this.ReferenceDataLibraries.FirstOrDefault();
            this.SelectedMeasurementUnitType = this.MeasurementUnitTypes.First();

            this.IsLoading = false;
        }

        /// <summary>
        /// Creates or edits a <see cref="MeasurementUnit"/>
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="MeasurementUnit"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CreateOrEditMeasurementUnit(bool shouldCreate)
        {
            var hasRdlChanged = this.SelectedReferenceDataLibrary != this.Thing.Container;
            var rdlClone = this.SelectedReferenceDataLibrary.Clone(false);
            var thingsToCreate = new List<Thing>();

            if (shouldCreate || hasRdlChanged)
            {
                rdlClone.Unit.Add(this.Thing);
                thingsToCreate.Add(rdlClone);
            }

            thingsToCreate.Add(this.Thing);

            if (this.Thing is DerivedUnit derivedUnit)
            {
                thingsToCreate.AddRange(derivedUnit.UnitFactor);
            }

            await this.SessionService.UpdateThings(rdlClone, thingsToCreate);
            await this.SessionService.RefreshSession();
        }

        /// <summary>
        /// Selects a new measurement unit type for the attribute <see cref="SelectedMeasurementUnitType"/>
        /// </summary>
        /// <param name="newKind">The new kind to which the <see cref="SelectedMeasurementUnitType"/> will be set</param>
        private void SelectMeasurementUnitType(ClassKindWrapper newKind)
        {
            this.Thing = newKind.ClassKind switch
            {
                ClassKind.SimpleUnit => new SimpleUnit(),
                ClassKind.DerivedUnit => new DerivedUnit(),
                ClassKind.LinearConversionUnit => new LinearConversionUnit(),
                ClassKind.PrefixedUnit => new PrefixedUnit(),
                _ => this.Thing
            };
        }
    }
}
