// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IOpenModelViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.ViewModels.Components
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Utilities.DisposableObject;
    using FluentResults;

    /// <summary>
    /// View Model that enables a user to open an <see cref="EngineeringModel" />
    /// </summary>
    public interface IOpenModelViewModel : IDisposableObject
    {
        /// <summary>
        /// The selected <see cref="EngineeringModelSetup" />
        /// </summary>
        EngineeringModelSetup SelectedEngineeringModel { get; set; }

        /// <summary>
        /// The selected <see cref="IterationData" />
        /// </summary>
        IterationData SelectedIterationSetup { get; set; }

        /// <summary>
        /// The selected <see cref="DomainOfExpertise" />
        /// </summary>
        DomainOfExpertise SelectedDomainOfExpertise { get; set; }

        /// <summary>
        /// A collection of available <see cref="EngineeringModelSetup" />
        /// </summary>
        IEnumerable<EngineeringModelSetup> AvailableEngineeringModelSetups { get; set; }

        /// <summary>
        /// A collection of available <see cref="IterationData" />
        /// </summary>
        IEnumerable<IterationData> AvailableIterationSetups { get; set; }

        /// <summary>
        /// A collection of available <see cref="DomainOfExpertise" />
        /// </summary>
        IEnumerable<DomainOfExpertise> AvailablesDomainOfExpertises { get; set; }

        /// <summary>
        /// Value asserting that the session is on way to open
        /// </summary>
        bool IsOpeningSession { get; set; }

        /// <summary>
        /// Initializes this view model properties
        /// </summary>
        void InitializesProperties();

        /// <summary>
        /// Opens the <see cref="EngineeringModel" /> based on the selected field
        /// </summary>
        /// <returns>A <see cref="Task"/> containing the operation <see cref="Result"/></returns>
        Task<Result<Iteration>> OpenSession();

        /// <summary>
        /// Preselects the <see cref="Iteration" /> to open
        /// </summary>
        /// <param name="modelId">The <see cref="Guid" /> of the <see cref="EngineeringModel" /></param>
        /// <param name="iterationId">The <see cref="Guid" /> of the <see cref="Iteration" /> to open</param>
        /// <param name="domainId">The <see cref="Guid" /> of the <see cref="DomainOfExpertise" /> to select</param>
        void PreSelectIteration(Guid modelId, Guid iterationId, Guid domainId);
    }
}
