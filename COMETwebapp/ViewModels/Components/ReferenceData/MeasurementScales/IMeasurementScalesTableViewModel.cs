// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IMeasurementScalesTableViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.Wrappers;

    /// <summary>
    /// View model used to manage <see cref="MeasurementScale" />s
    /// </summary>
    public interface IMeasurementScalesTableViewModel : IDeprecatableDataItemTableViewModel<MeasurementScale, MeasurementScaleRowViewModel>
    {
        /// <summary>
        /// Gets the available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; }

        /// <summary>
        /// Gets the available <see cref="MeasurementUnit" />s
        /// </summary>
        IEnumerable<MeasurementUnit> MeasurementUnits { get; }

        /// <summary>
        /// Gets the available <see cref="NumberSetKind" />s
        /// </summary>
        IEnumerable<NumberSetKind> NumberSetKinds { get; }

        /// <summary>
        /// Gets the available measurement scale types <see cref="ClassKindWrapper" />s
        /// </summary>
        IEnumerable<ClassKindWrapper> MeasurementScaleTypes { get; }

        /// <summary>
        /// Gets or sets the selected measurement scale type
        /// </summary>
        ClassKindWrapper SelectedMeasurementScaleType { get; set; }

        /// <summary>
        /// Gets the selected <see cref="ScaleValueDefinition" />s
        /// </summary>
        IEnumerable<ScaleValueDefinition> SelectedScaleValueDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the selected reference data library
        /// </summary>
        ReferenceDataLibrary SelectedReferenceDataLibrary { get; set; }

        /// <summary>
        /// Gets the selected <see cref="MappingToReferenceScale" />s
        /// </summary>
        IEnumerable<MappingToReferenceScale> SelectedMappingToReferenceScale { get; set; }

        /// <summary>
        /// Gets the available <see cref="ScaleValueDefinition" />s for reference scale value selection
        /// </summary>
        IEnumerable<ScaleValueDefinition> ReferenceScaleValueDefinitions { get; }

        /// <summary>
        /// Gets the available <see cref="LogarithmBaseKind" />s
        /// </summary>
        IEnumerable<LogarithmBaseKind> LogarithmBaseKinds { get; }

        /// <summary>
        /// Gets the available reference <see cref="QuantityKind" />s
        /// </summary>
        IEnumerable<QuantityKind> ReferenceQuantityKinds { get; }

        /// <summary>
        /// Gets the available <see cref="MeasurementScale" />s
        /// </summary>
        IEnumerable<MeasurementScale> MeasurementScales { get; }

        /// <summary>
        /// Gets or sets the selected reference quantity value
        /// </summary>
        ScaleReferenceQuantityValue SelectedReferenceQuantityValue { get; set; }

        /// <summary>
        /// Selects the current <see cref="MeasurementScale" />
        /// </summary>
        /// <param name="measurementScale">The measurement scale to be set</param>
        void SelectMeasurementScale(MeasurementScale measurementScale);

        /// <summary>
        /// Creates or edits a <see cref="MeasurementScale" />
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="MeasurementScale" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        Task CreateOrEditMeasurementScale(bool shouldCreate);
    }
}
