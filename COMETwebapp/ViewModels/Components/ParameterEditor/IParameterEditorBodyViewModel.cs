// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterEditorBodyViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.ViewModels.Components.Shared;
    using COMETwebapp.ViewModels.Components.Shared.Selectors;

    using DynamicData;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.ParameterEditor.ParameterEditorBody"/>
    /// </summary>
    public interface IParameterEditorBodyViewModel : ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISubscriptionService"/>
        /// </summary>
        ISubscriptionService SubscriptionService { get; set; }

        /// <summary>
        /// Gets the <see cref="IElementBaseSelectorViewModel"/>
        /// </summary>
        public IElementBaseSelectorViewModel ElementSelector { get; }

        /// <summary>
        /// Gets the <see cref="IOptionSelectorViewModel" />
        /// </summary>
        public IOptionSelectorViewModel OptionSelector { get; }

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        public IParameterTypeSelectorViewModel ParameterTypeSelector { get; } 

        /// <summary>
        /// All <see cref="ElementBase"/> of the iteration without filtering
        /// </summary>
        public List<ElementBase> Elements { get; }

        /// <summary>
        /// Gets or sets the filtered <see cref="ElementBase"/>
        /// </summary>
        public SourceList<ElementBase> FilteredElements { get; }

        /// <summary>
        /// Sets if only parameters owned by the active domain are shown
        /// </summary>
        bool IsOwnedParameters { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IParameterTableViewModel"/>
        /// </summary>
        public IParameterTableViewModel ParameterTableViewModel { get; set; }

        /// <summary>
        /// Apply all the filters selected in the <param name="elements"/> and replace the data of <see cref="FilteredElements"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        void ApplyFilters(IEnumerable<ElementBase> elements);

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="ElementSelector"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        IEnumerable<ElementBase> FilterByElement(IEnumerable<ElementBase> elements);

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="ParameterTypeSelector"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        IEnumerable<ElementBase> FilterByParameterType(IEnumerable<ElementBase> elements);

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="OptionSelector"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        IEnumerable<ElementBase> FilterByOption(IEnumerable<ElementBase> elements);

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        IEnumerable<ElementBase> FilterByOwnedByActiveDomain(IEnumerable<ElementBase> elements);

        /// <summary>
        /// Queries the <see cref="DomainOfExpertise"/> of the current <see cref="Iteration"/>
        /// </summary>
        /// <returns>the name of the <see cref="DomainOfExpertise"/></returns>
        string QueryDomainOfExpertiseName();

        /// <summary>
        /// Initializes the <see cref="ParameterEditorBodyViewModel"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        Task InitializeViewModel();
    }
}
