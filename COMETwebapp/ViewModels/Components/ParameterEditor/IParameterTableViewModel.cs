// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterTableViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using DynamicData;

    /// <summary>
    /// Interface for the <see cref="ParameterTableViewModel"/>
    /// </summary>
    public interface IParameterTableViewModel : IHaveReusableRows
    {
        /// <summary>
        /// Gets the collection of the <see cref="ParameterBaseRowViewModel"/>
        /// </summary>
        SourceList<ParameterBaseRowViewModel> Rows { get; }

        /// <summary>
        /// The <see cref="IHaveComponentParameterTypeEditor"/> to show in the popup
        /// </summary>
        IHaveComponentParameterTypeEditor HaveComponentParameterTypeEditorViewModel { get; set; }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        bool IsOnEditMode { get; set; }

        /// <summary>
        /// Initializes this <see cref="IParameterTableViewModel"/>
        /// </summary>
        /// <param name="currentIteration">The current <see cref="Iteration"/></param>
        /// <param name="currentDomain">The <see cref="DomainOfExpertise"/></param>
        /// <param name="selectedOption">The select <see cref="Option"/></param>
        void InitializeViewModel(Iteration currentIteration, DomainOfExpertise currentDomain, Option selectedOption);

        /// <summary>
        /// Update the current <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="currentDomain">The new <see cref="DomainOfExpertise"/></param>
        void UpdateDomain(DomainOfExpertise currentDomain);

        /// <summary>
        /// Apply filters based on <see cref="Option"/>, <see cref="ElementBase"/>, <see cref="ParameterType"/> and <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="selectedOption">The selected <see cref="Option"/></param>
        /// <param name="selectedElementBase">The selected <see cref="ElementBase"/></param>
        /// <param name="selectedParameterType">The selected <see cref="ParameterType"/></param>
        /// <param name="isOwnedParameters">Value asserting that the only <see cref="Thing"/> owned by the current <see cref="DomainOfExpertise"/> should be visible</param>
        void ApplyFilters(Option selectedOption, ElementBase selectedElementBase, ParameterType selectedParameterType, bool isOwnedParameters);
    }
}
