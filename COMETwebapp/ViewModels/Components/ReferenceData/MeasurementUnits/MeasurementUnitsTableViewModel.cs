// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementUnitsTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.MeasurementUnits
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
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
        /// Gets the available <see cref="ClassKind" />s
        /// </summary>
        private static readonly IEnumerable<ClassKind> AvailableMeasurementUnitTypes = [ClassKind.SimpleUnit, ClassKind.DerivedUnit, ClassKind.LinearConversionUnit, ClassKind.PrefixedUnit];

        /// <summary>
        /// The backing field for <see cref="SelectedMeasurementUnitType" />
        /// </summary>
        private ClassKindWrapper selectedMeasurementUnitType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnitsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public MeasurementUnitsTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus,
            ILogger<MeasurementUnitsTableViewModel> logger) : base(sessionService, messageBus, showHideDeprecatedThingsService, logger)
        {
        }

        /// <summary>
        /// Gets the available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; private set; } = [];

        /// <summary>
        /// Gets the available <see cref="MeasurementUnit" />s from the same rdl as the <see cref="SelectedReferenceDataLibrary" />
        /// </summary>
        public IEnumerable<MeasurementUnit> ReferenceUnits => this.SelectedReferenceDataLibrary?.QueryMeasurementUnitsFromChainOfRdls();

        /// <summary>
        /// Gets the available <see cref="Prefixes" />s from the same rdl as the <see cref="SelectedReferenceDataLibrary" />
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
        /// Initializes the <see cref="MeasurementUnitsTableViewModel" />
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
        /// Update this view model properties when the <see cref="SingleThingApplicationBaseViewModel{TThing}.CurrentThing" /> has changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();
            this.SelectedReferenceDataLibrary = (ReferenceDataLibrary)this.CurrentThing.Container ?? this.ReferenceDataLibraries.FirstOrDefault();
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<MeasurementUnit> QueryListOfThings()
        {
            return this.SessionService.GetSiteDirectory().AvailableReferenceDataLibraries().SelectMany(x => x.Unit).ToList();
        }

        /// <summary>
        /// Creates or edits a <see cref="MeasurementUnit" />
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="MeasurementUnit" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateOrEditMeasurementUnit(bool shouldCreate)
        {
            try
            {
                this.IsLoading = true;

                var hasRdlChanged = this.SelectedReferenceDataLibrary != this.CurrentThing.Container;
                var rdlClone = this.SelectedReferenceDataLibrary.Clone(false);
                var thingsToCreate = new List<Thing>();

                if (shouldCreate || hasRdlChanged)
                {
                    rdlClone.Unit.Add(this.CurrentThing);
                    thingsToCreate.Add(rdlClone);
                }

                thingsToCreate.Add(this.CurrentThing);

                if (this.CurrentThing is DerivedUnit derivedUnit)
                {
                    thingsToCreate.AddRange(derivedUnit.UnitFactor);
                }

                await this.SessionService.CreateOrUpdateThingsWithNotification(rdlClone, thingsToCreate, this.GetNotificationDescription(shouldCreate));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Create or Update MeasurementUnit failed");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Selects a new measurement unit type for the attribute <see cref="SelectedMeasurementUnitType" />
        /// </summary>
        /// <param name="newKind">The new kind to which the <see cref="SelectedMeasurementUnitType" /> will be set</param>
        private void SelectMeasurementUnitType(ClassKindWrapper newKind)
        {
            this.CurrentThing = newKind.ClassKind switch
            {
                ClassKind.SimpleUnit => new SimpleUnit(),
                ClassKind.DerivedUnit => new DerivedUnit(),
                ClassKind.LinearConversionUnit => new LinearConversionUnit(),
                ClassKind.PrefixedUnit => new PrefixedUnit(),
                _ => this.CurrentThing
            };
        }
    }
}
