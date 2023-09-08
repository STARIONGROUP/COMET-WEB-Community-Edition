// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TextConfigurationKind.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Enumerations
{
    /// <summary>
    /// An enumeration that defines possible values for the text configuration
    /// </summary>
    public enum TextConfigurationKind
    {
        /// <summary>
        /// Placeholder for the combobox to select an EngineeringModel
        /// </summary>
        OpenEngineeringModelPlaceholder = 0,

        /// <summary>
        /// Placeholder for the combobox to select an Iteration
        /// </summary>
        OpenIterationPlaceholder = 1,

        /// <summary>
        /// Placeholder for the combobox to select a DomainOfExpertise
        /// </summary>
        OpenDomainOfExpertisePlaceholder = 2,

        /// <summary>
        /// The caption to use as title for the model section
        /// </summary>
        ModelTitleCaption = 3,

        /// <summary>
        /// The caption to use as title for the iteration section
        /// </summary>
        IterationTitleCaption = 4,

        /// <summary>
        /// The caption to use as title for the domain section
        /// </summary>
        DomainTitleCaption = 5,

        /// <summary>
        /// The title to use in the landing page
        /// </summary>
        LandingPageTitle = 6,

        /// <summary>
        /// The caption to use as navigation application selector title
        /// </summary>
        NavigationApplicationSelectorTitle = 7,

        /// <summary>
        /// The caption to use as navigation model selector title
        /// </summary>
        NavigationModelSelectorTitle = 8,

        /// <summary>
        /// The caption to use as title for the open button
        /// </summary>
        ModelOpenButtonCaption = 9,
    }
}
