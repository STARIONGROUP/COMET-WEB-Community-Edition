// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IPublicationsViewModel.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.ViewModels.Components.Publications
{
    using CDP4Dal;

    using CDP4Common.SiteDirectoryData;
    using CDP4Common.EngineeringModelData;
    
    using COMET.Web.Common.ViewModels.Components.Publications.Rows;
    
    using DynamicData;

    /// <summary>
    /// ViewModel for the Publications component
    /// </summary>
    public interface IPublicationsViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="Iteration"/> that's being used
        /// </summary>
        Iteration CurrentIteration { get; }

        /// <summary>
        /// Gets or set the list of <see cref="ParameterOrOverrideBase"/> that can be published
        /// </summary>
        List<ParameterOrOverrideBase> PublishableParameters { get; set; } 

        /// <summary>
        /// Gets or sets the rows used in the Publications component
        /// </summary>
        SourceList<PublicationRowViewModel> Rows { get; set; }

        /// <summary>
        /// Gets or sets if the publication is possible
        /// </summary>
        bool CanPublish { get; set; }

        /// <summary>
        /// Gets or sets the DataSourceUri
        /// </summary>
        string DataSource { get; }

        /// <summary>
        /// Gets or sets the name of the current <see cref="Person"/> in the <see cref="ISession"/>
        /// </summary>
        string PersonName { get; }

        /// <summary>
        /// Gets or sets the name of the current <see cref="EngineeringModel"/>
        /// </summary>
        string ModelName { get; }

        /// <summary>
        /// Gets or sets the name of the current <see cref="Iteration"/>
        /// </summary>
        string IterationName { get; set; }

        /// <summary>
        /// Gets or sets the name of the current <see cref="DomainOfExpertise"/>
        /// </summary>
        string DomainName { get; }

        /// <summary>
        /// Updates the properties of this ViewModel
        /// </summary>
        /// <param name="iteration">the iteration to use</param>
        void UpdateProperties(Iteration iteration);

        /// <summary>
        /// Execute the publication.
        /// </summary>
        /// <returns>An asynchronous operation</returns>
        Task ExecutePublish();
    }
}
