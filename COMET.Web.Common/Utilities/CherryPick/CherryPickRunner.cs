// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CherryPickRunner.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;

    /// <summary>
    /// Utility class that could run CherryPick query for <see cref="INeedCherryPickedData" />
    /// </summary>
    public class CherryPickRunner : ICherryPickRunner
    {
        /// <summary>
        /// Gets the collection of <see cref="INeedCherryPickedData" />
        /// </summary>
        private readonly List<INeedCherryPickedData> NeedCherryPicked = new();

        /// <summary>
        /// Gets the <see cref="ISessionService"/>
        /// </summary>
        protected readonly ISessionService SessionService;

        /// <summary>
        /// Initializes a new <see cref="CherryPickRunner" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        public CherryPickRunner(ISessionService sessionService)
        {
            this.SessionService = sessionService;
        }

        /// <summary>
        /// Asserts that the cherrypick feature is on going
        /// </summary>
        public bool IsCherryPicking { get; private set; }

        /// <summary>
        /// Initializes the internal properties
        /// </summary>
        /// <param name="needCherryPicked">A collection of <see cref="INeedCherryPickedData"/></param>
        public void InitializeProperties(IEnumerable<INeedCherryPickedData> needCherryPicked)
        {
            this.NeedCherryPicked.Clear();
            this.NeedCherryPicked.AddRange(needCherryPicked);
        }

        /// <summary>
        /// Runs the cherrypick features based on data required from <see cref="NeedCherryPicked" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task RunCherryPick()
        {
            if (this.IsCherryPicking)
            {
                return;
            }

            this.IsCherryPicking = true;
            var classKinds = this.NeedCherryPicked.SelectMany(x => x.ClassKindsOfInterest).Distinct();
            var categoriesName = this.NeedCherryPicked.SelectMany(x => x.CategoriesOfInterest).Distinct();

            var categories = new List<Category>();

            foreach (var referenceDataLibrary in this.SessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries())
            {
                categories.AddRange(referenceDataLibrary.DefinedCategory
                    .Where(x => categoriesName.Contains(x.Name))
                    .ToList());
            }

            var availableEngineeringModelSetups = this.SessionService.GetParticipantModels().ToList();

            var cherryPicks = availableEngineeringModelSetups.Select(engineeringModelSetup => this.SessionService.Session.CherryPick(engineeringModelSetup.EngineeringModelIid, engineeringModelSetup.IterationSetup.Single(x => x.FrozenOn == null).IterationIid, classKinds, categories.Select(x => x.Iid)))
                .ToList();

            var results = (await Task.WhenAll(cherryPicks)).Where(x => x.Any()).ToList();

            foreach (var needCherryPickedData in this.NeedCherryPicked)
            {
                needCherryPickedData.ProcessCherryPickedData(results);
            }

            this.IsCherryPicking = false;
        }
    }
}
