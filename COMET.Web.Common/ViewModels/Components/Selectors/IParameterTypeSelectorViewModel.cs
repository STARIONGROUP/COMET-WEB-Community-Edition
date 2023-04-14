// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterTypeSelectorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components.Selectors
{
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// View Model that enables the user to select an <see cref="ParameterType" />
    /// </summary>
    public interface IParameterTypeSelectorViewModel : IBelongsToIterationSelectorViewModel
    {
        /// <summary>
        /// The currently selected <see cref="ParameterType" />
        /// </summary>
        ParameterType SelectedParameterType { get; set; }

        /// <summary>
        /// A collection of available <see cref="ParameterType" />
        /// </summary>
        IEnumerable<ParameterType> AvailableParameterTypes { get; }

        /// <summary>
        /// Filter the collection of the <see cref="AvailableParameterTypes" /> with provided values
        /// </summary>
        /// <param name="parameterTypesId">A collection of <see cref="Guid" /> for <see cref="ParameterType" /></param>
        void FilterAvailableParameterTypes(IEnumerable<Guid> parameterTypesId);
    }
}
