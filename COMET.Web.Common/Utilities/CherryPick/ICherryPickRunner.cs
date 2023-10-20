// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ICherryPickRunner.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Utilities.CherryPick
{
    /// <summary>
    /// Utility class that could run CherryPick query for <see cref="INeedCherryPickedData" />
    /// </summary>
    public interface ICherryPickRunner
    {
        /// <summary>
        /// Asserts that the cherrypick feature is on going
        /// </summary>
        bool IsCherryPicking { get; }

        /// <summary>
        /// Initializes the internal properties
        /// </summary>
        /// <param name="needCherryPickedData">A collection of <see cref="INeedCherryPickedData"/></param>
        void InitializeProperties(IEnumerable<INeedCherryPickedData> needCherryPickedData);

        /// <summary>
        /// Runs the cherrypick features for a specific Engineering Model and Iteration, based on data required from <see cref="NeedCherryPicked" />
        /// </summary>
        /// <param name="engineeringModelId">The engineering model Id that we want to cherry pick from</param>
        /// <param name="iterationId">The iteration Id that we want to cherry pick from</param>
        /// <returns>A <see cref="Task" /></returns>
        Task RunSingleCherryPickAsync(Guid engineeringModelId, Guid iterationId);
        
        /// <summary>
        /// Runs the cherrypick features based on data required from <see cref="CherryPickRunner.NeedCherryPicked" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task RunCherryPickAsync();
    }
}
