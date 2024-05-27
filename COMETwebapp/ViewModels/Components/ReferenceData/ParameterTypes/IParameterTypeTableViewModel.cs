// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterTypeTableViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.Wrappers;

    /// <summary>
    /// View model used to manage <see cref="ParameterType" />
    /// </summary>
    public interface IParameterTypeTableViewModel : IDeprecatableDataItemTableViewModel<ParameterType, ParameterTypeRowViewModel>
    {
        /// <summary>
        /// Gets the available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; }

        /// <summary>
        /// Gets or sets the selected <see cref="ReferenceDataLibrary" />
        /// </summary>
        ReferenceDataLibrary SelectedReferenceDataLibrary { get; set; }

        /// <summary>
        /// Gets the available parameter types <see cref="ClassKindWrapper" />s
        /// </summary>
        IEnumerable<ClassKindWrapper> ParameterTypes { get; }

        /// <summary>
        /// Gets or sets the selected parameter type
        /// </summary>
        ClassKindWrapper SelectedParameterType { get; set; }

        /// <summary>
        /// Gets the existing parameter types
        /// </summary>
        IEnumerable<ParameterType> ExistingParameterTypes { get; }

        /// <summary>
        /// Gets the available <see cref="MeasurementScale" />s
        /// </summary>
        IEnumerable<MeasurementScaleRowViewModel> MeasurementScales { get; }

        /// <summary>
        /// Creates or edits a <see cref="ParameterType" />
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="ParameterType" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        Task CreateOrEditParameterType(bool shouldCreate);
    }
}
