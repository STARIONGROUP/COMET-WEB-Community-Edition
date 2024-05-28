// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementScalesTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.MeasurementScales
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.Wrappers;

    using ReactiveUI;

    using MeasurementScale = CDP4Common.SiteDirectoryData.MeasurementScale;

    /// <summary>
    /// View model used to manage <see cref="MeasurementScale" />s
    /// </summary>
    public class MeasurementScalesTableViewModel : DeprecatableDataItemTableViewModel<MeasurementScale, MeasurementScaleRowViewModel>, IMeasurementScalesTableViewModel
    {
        /// <summary>
        /// Gets the available <see cref="ClassKind" />s
        /// </summary>
        private static readonly IEnumerable<ClassKind> AvailableMeasurementScaleTypes =
        [
            ClassKind.CyclicRatioScale, ClassKind.IntervalScale, ClassKind.LogarithmicScale, ClassKind.OrdinalScale, ClassKind.RatioScale
        ];

        /// <summary>
        /// The backing field for <see cref="SelectedMeasurementScaleType" />
        /// </summary>
        private ClassKindWrapper selectedMeasurementScaleType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScalesTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public MeasurementScalesTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus,
            ILogger<MeasurementScalesTableViewModel> logger) : base(sessionService, messageBus, showHideDeprecatedThingsService, logger, [typeof(ReferenceDataLibrary)])
        {
            this.CurrentThing = new OrdinalScale();
            this.SelectedReferenceQuantityValue = new ScaleReferenceQuantityValue();
        }

        /// <summary>
        /// Gets the available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; private set; } = [];

        /// <summary>
        /// Gets the available reference <see cref="QuantityKind" />s
        /// </summary>
        public IEnumerable<QuantityKind> ReferenceQuantityKinds { get; private set; }

        /// <summary>
        /// Gets the available <see cref="ScaleValueDefinition" />s for reference scale value selection
        /// </summary>
        public IEnumerable<ScaleValueDefinition> ReferenceScaleValueDefinitions => this.SelectedReferenceDataLibrary?.Scale
            .SelectMany(x => x.ValueDefinition)
            .Where(x => this.CurrentThing.ValueDefinition.All(selected => selected.Iid != x.Iid));

        /// <summary>
        /// Gets the available <see cref="MeasurementUnit" />s
        /// </summary>
        public IEnumerable<MeasurementUnit> MeasurementUnits => this.SelectedReferenceDataLibrary?.QueryMeasurementUnitsFromChainOfRdls();

        /// <summary>
        /// Gets the available <see cref="MeasurementScale" />s
        /// </summary>
        public IEnumerable<MeasurementScale> MeasurementScales => this.SelectedReferenceDataLibrary?.QueryMeasurementScalesFromChainOfRdls();

        /// <summary>
        /// Gets the available measurement scale types <see cref="ClassKindWrapper" />s
        /// </summary>
        public IEnumerable<ClassKindWrapper> MeasurementScaleTypes { get; private set; } = AvailableMeasurementScaleTypes.Select(x => new ClassKindWrapper(x));

        /// <summary>
        /// Gets the available <see cref="NumberSetKind" />s
        /// </summary>
        public IEnumerable<NumberSetKind> NumberSetKinds { get; private set; } = Enum.GetValues<NumberSetKind>();

        /// <summary>
        /// Gets the available <see cref="LogarithmBaseKind" />s
        /// </summary>
        public IEnumerable<LogarithmBaseKind> LogarithmBaseKinds { get; private set; } = Enum.GetValues<LogarithmBaseKind>();

        /// <summary>
        /// Gets or sets the selected reference data library
        /// </summary>
        public ReferenceDataLibrary SelectedReferenceDataLibrary { get; set; }

        /// <summary>
        /// Gets or sets the selected reference quantity value
        /// </summary>
        public ScaleReferenceQuantityValue SelectedReferenceQuantityValue { get; set; }

        /// <summary>
        /// Gets or sets the selected measurement scale type
        /// </summary>
        public ClassKindWrapper SelectedMeasurementScaleType
        {
            get => this.selectedMeasurementScaleType;
            set
            {
                this.SelectMeasurementScaleType(value);
                this.RaiseAndSetIfChanged(ref this.selectedMeasurementScaleType, value);
            }
        }

        /// <summary>
        /// Initializes the <see cref="MeasurementScalesTableViewModel" />
        /// </summary>
        public override void InitializeViewModel()
        {
            base.InitializeViewModel();

            var siteDirectory = this.SessionService.GetSiteDirectory();

            this.ReferenceDataLibraries = siteDirectory.AvailableReferenceDataLibraries().Where(x => x.Unit.Count > 0);
            this.ReferenceQuantityKinds = siteDirectory.SiteReferenceDataLibrary.SelectMany(x => x.ParameterType).OfType<QuantityKind>().DistinctBy(x => x.Iid);

            this.SelectedReferenceDataLibrary = this.ReferenceDataLibraries.FirstOrDefault();
            this.SelectedMeasurementScaleType = this.MeasurementScaleTypes.First();
        }

        /// <summary>
        /// Creates or edits a <see cref="MeasurementScale" />
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="MeasurementScale" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateOrEditMeasurementScale(bool shouldCreate)
        {
            try
            {
                this.IsLoading = true;
                var hasRdlChanged = this.SelectedReferenceDataLibrary != this.CurrentThing.Container;
                var rdlClone = this.SelectedReferenceDataLibrary.Clone(false);
                var thingsToCreate = new List<Thing>();

                if (shouldCreate || hasRdlChanged)
                {
                    rdlClone.Scale.Add(this.CurrentThing);
                    thingsToCreate.Add(rdlClone);
                }

                if (this.CurrentThing is LogarithmicScale logarithmicScale)
                {
                    switch (logarithmicScale.ReferenceQuantityValue.Count)
                    {
                        case 0 when !string.IsNullOrWhiteSpace(this.SelectedReferenceQuantityValue.Value):
                            logarithmicScale.ReferenceQuantityValue.Add(this.SelectedReferenceQuantityValue);
                            thingsToCreate.Add(this.SelectedReferenceQuantityValue);
                            break;
                        case > 0 when string.IsNullOrWhiteSpace(this.SelectedReferenceQuantityValue.Value):
                            logarithmicScale.ReferenceQuantityValue.Clear();
                            break;
                        case > 0 when !string.IsNullOrWhiteSpace(this.SelectedReferenceQuantityValue.Value):
                            logarithmicScale.ReferenceQuantityValue[0] = this.SelectedReferenceQuantityValue;
                            thingsToCreate.Add(this.SelectedReferenceQuantityValue);
                            break;
                    }
                }

                thingsToCreate.AddRange(this.CurrentThing.ValueDefinition);
                thingsToCreate.AddRange(this.CurrentThing.MappingToReferenceScale);
                thingsToCreate.Add(this.CurrentThing);

                await this.SessionService.CreateOrUpdateThingsWithNotification(rdlClone, thingsToCreate, this.GetNotificationDescription(shouldCreate));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Create or Update MeasurementScale failed");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Update the view model properties when the thing has changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();
            this.SelectedReferenceDataLibrary = (ReferenceDataLibrary)this.CurrentThing.Container ?? this.ReferenceDataLibraries.FirstOrDefault();

            if (this.CurrentThing is LogarithmicScale logarithmicScale)
            {
                this.SelectedReferenceQuantityValue = logarithmicScale.ReferenceQuantityValue.FirstOrDefault() ?? new ScaleReferenceQuantityValue();
            }
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<MeasurementScale> QueryListOfThings()
        {
            return this.SessionService.GetSiteDirectory().AvailableReferenceDataLibraries().SelectMany(x => x.Scale).ToList();
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnSessionRefreshed()
        {
            var updatedRdls = this.UpdatedThings.OfType<ReferenceDataLibrary>().ToList();
            await base.OnSessionRefreshed();

            foreach (var rdl in updatedRdls)
            {
                this.RefreshContainerName(rdl);
            }
        }

        /// <summary>
        /// Selects a new measurement scale type for the attribute <see cref="SelectedMeasurementScaleType" />
        /// </summary>
        /// <param name="newKind">The new kind to which the <see cref="SelectedMeasurementScaleType" /> will be set</param>
        private void SelectMeasurementScaleType(ClassKindWrapper newKind)
        {
            this.CurrentThing = newKind.ClassKind switch
            {
                ClassKind.CyclicRatioScale => new CyclicRatioScale(),
                ClassKind.IntervalScale => new IntervalScale(),
                ClassKind.LogarithmicScale => new LogarithmicScale(),
                ClassKind.OrdinalScale => new OrdinalScale(),
                ClassKind.RatioScale => new RatioScale(),
                _ => this.CurrentThing
            };

            if (newKind.ClassKind == ClassKind.LogarithmicScale)
            {
                this.SelectedReferenceQuantityValue = new ScaleReferenceQuantityValue();
            }
        }
    }
}
