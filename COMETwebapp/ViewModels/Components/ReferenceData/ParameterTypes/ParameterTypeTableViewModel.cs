// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.ParameterTypes
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

    /// <summary>
    /// View model used to manage <see cref="ParameterType" />
    /// </summary>
    public class ParameterTypeTableViewModel : DeprecatableDataItemTableViewModel<ParameterType, ParameterTypeRowViewModel>, IParameterTypeTableViewModel
    {
        /// <summary>
        /// Gets the available <see cref="ClassKind" />s
        /// </summary>
        private static readonly IEnumerable<ClassKind> AvailableParameterTypes =
        [
            ClassKind.BooleanParameterType,
            ClassKind.CompoundParameterType,
            ClassKind.DateParameterType,
            ClassKind.DateTimeParameterType,
            ClassKind.DerivedQuantityKind,
            ClassKind.EnumerationParameterType,
            ClassKind.SampledFunctionParameterType,
            ClassKind.SimpleQuantityKind,
            ClassKind.SpecializedQuantityKind,
            ClassKind.TextParameterType,
            ClassKind.TimeOfDayParameterType
        ];

        /// <summary>
        /// The backing field for <see cref="SelectedParameterType" />
        /// </summary>
        private ClassKindWrapper selectedParameterType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public ParameterTypeTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus, ILogger<ParameterTypeTableViewModel> logger)
            : base(sessionService, messageBus, showHideDeprecatedThingsService, logger)
        {
            this.Thing = new BooleanParameterType();
        }

        /// <summary>
        /// Gets the available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; private set; }

        /// <summary>
        /// Gets the possible available <see cref="MeasurementScale" />s
        /// </summary>
        public IEnumerable<MeasurementScaleRowViewModel> MeasurementScales => this.GetPossibleMeasurementScales();

        /// <summary>
        /// Gets the available parameter types <see cref="ClassKindWrapper" />s
        /// </summary>
        public IEnumerable<ClassKindWrapper> ParameterTypes { get; private set; } = AvailableParameterTypes.Select(x => new ClassKindWrapper(x));

        /// <summary>
        /// Gets the existing parameter types
        /// </summary>
        public IEnumerable<ParameterType> ExistingParameterTypes { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ReferenceDataLibrary" />
        /// </summary>
        public ReferenceDataLibrary SelectedReferenceDataLibrary { get; set; }

        /// <summary>
        /// Gets or sets the selected parameter type
        /// </summary>
        public ClassKindWrapper SelectedParameterType
        {
            get => this.selectedParameterType;
            set
            {
                this.SelectParameterType(value);
                this.RaiseAndSetIfChanged(ref this.selectedParameterType, value);
            }
        }

        /// <summary>
        /// Selects the current <see cref="ParameterType" />
        /// </summary>
        /// <param name="parameterType">The parameter type to be set</param>
        public void SelectParameterType(ParameterType parameterType)
        {
            this.Thing = parameterType;
            this.SelectedReferenceDataLibrary = (ReferenceDataLibrary)parameterType.Container ?? this.ReferenceDataLibraries.FirstOrDefault();
        }

        /// <summary>
        /// Initializes the current view model
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public override void InitializeViewModel()
        {
            base.InitializeViewModel();
            var siteDirectory = this.SessionService.GetSiteDirectory();

            this.ReferenceDataLibraries = siteDirectory.AvailableReferenceDataLibraries()
                .Where(x => x.Unit.Count > 0)
                .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);

            this.SelectedReferenceDataLibrary = this.ReferenceDataLibraries.FirstOrDefault();

            this.ExistingParameterTypes = this.SessionService.Session.OpenReferenceDataLibraries
                .SelectMany(x => x.ParameterType)
                .Distinct()
                .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Creates or edits a <see cref="ParameterType" />
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="ParameterType" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateOrEditParameterType(bool shouldCreate)
        {
            this.IsLoading = true;

            var hasRdlChanged = this.SelectedReferenceDataLibrary != this.Thing.Container;
            var rdlClone = this.SelectedReferenceDataLibrary.Clone(false);
            var thingsToCreate = new List<Thing>();

            if (shouldCreate || hasRdlChanged)
            {
                rdlClone.ParameterType.Add(this.Thing);
                thingsToCreate.Add(rdlClone);
            }

            switch (this.Thing)
            {
                case EnumerationParameterType enumerationParameterType:
                    thingsToCreate.AddRange(enumerationParameterType.ValueDefinition);
                    break;
                case CompoundParameterType compoundParameterType:
                    thingsToCreate.AddRange(compoundParameterType.Component);
                    break;
                case DerivedQuantityKind derivedQuantityKindParameterType:
                    thingsToCreate.AddRange(derivedQuantityKindParameterType.QuantityKindFactor);
                    break;
                case SampledFunctionParameterType sampledFunctionParameterType:
                    thingsToCreate.AddRange(sampledFunctionParameterType.IndependentParameterType);
                    thingsToCreate.AddRange(sampledFunctionParameterType.DependentParameterType);
                    break;
            }

            thingsToCreate.Add(this.Thing);

            await this.SessionService.CreateOrUpdateThings(rdlClone, thingsToCreate);
            await this.SessionService.RefreshSession();

            if (this.Thing.Original is not null)
            {
                this.Thing = (ParameterType)this.Thing.Original.Clone(true);
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Selects a new parameter type for the attribute <see cref="SelectedParameterType" />
        /// </summary>
        /// <param name="newKind">The new kind to which the <see cref="SelectedParameterType" /> will be set</param>
        private void SelectParameterType(ClassKindWrapper newKind)
        {
            this.Thing = newKind.ClassKind switch
            {
                ClassKind.BooleanParameterType => new BooleanParameterType(),
                ClassKind.CompoundParameterType => new CompoundParameterType(),
                ClassKind.DateParameterType => new DateParameterType(),
                ClassKind.DateTimeParameterType => new DateTimeParameterType(),
                ClassKind.DerivedQuantityKind => new DerivedQuantityKind(),
                ClassKind.EnumerationParameterType => new EnumerationParameterType(),
                ClassKind.SampledFunctionParameterType => new SampledFunctionParameterType(),
                ClassKind.SimpleQuantityKind => new SimpleQuantityKind(),
                ClassKind.SpecializedQuantityKind => new SpecializedQuantityKind(),
                ClassKind.TextParameterType => new TextParameterType(),
                ClassKind.TimeOfDayParameterType => new TimeOfDayParameterType(),
                _ => this.Thing
            };

            if (this.Thing is SpecializedQuantityKind specializedQuantityKindParameterType)
            {
                specializedQuantityKindParameterType.General = this.ExistingParameterTypes.OfType<QuantityKind>().FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the possible available <see cref="MeasurementScale" />s
        /// </summary>
        /// <returns>A collection of <see cref="MeasurementScaleRowViewModel"/>s</returns>
        private IEnumerable<MeasurementScaleRowViewModel> GetPossibleMeasurementScales()
        {
            var allMeasurementScales = this.SelectedReferenceDataLibrary?.QueryMeasurementScalesFromChainOfRdls()
                .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => new MeasurementScaleRowViewModel(x)) ?? Enumerable.Empty<MeasurementScaleRowViewModel>();

            if (this.Thing is not SpecializedQuantityKind specializedQuantity || specializedQuantity.General is null)
            {
                return allMeasurementScales;
            }

            var filteredMeasurementScales = allMeasurementScales.Where(x => !specializedQuantity.General.AllPossibleScale.Contains(x.Thing));
            return filteredMeasurementScales;
        }
    }
}
