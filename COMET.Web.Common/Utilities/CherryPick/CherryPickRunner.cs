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
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;

    using Thing = CDP4Common.DTO.Thing;

    /// <summary>
    /// Utility class that could run CherryPick query for <see cref="INeedCherryPickedData" />
    /// </summary>
    public class CherryPickRunner : ICherryPickRunner
    {
        /// <summary>
        /// Gets the collection of <see cref="INeedCherryPickedData" />
        /// </summary>
        private readonly List<INeedCherryPickedData> needCherryPicked = new();

        /// <summary>
        /// Gets the <see cref="ISessionService"/>
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Initializes a new <see cref="CherryPickRunner" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        public CherryPickRunner(ISessionService sessionService)
        {
            this.sessionService = sessionService;
        }

        /// <summary>
        /// Asserts that the cherrypick feature is on going
        /// </summary>
        public bool IsCherryPicking { get; private set; }

        /// <summary>
        /// Initializes the internal properties
        /// </summary>
        /// <param name="needCherryPickedData">A collection of <see cref="INeedCherryPickedData"/></param>
        public void InitializeProperties(IEnumerable<INeedCherryPickedData> needCherryPickedData)
        {
            this.needCherryPicked.Clear();
            this.needCherryPicked.AddRange(needCherryPickedData);
        }

        /// <summary>
        /// Gets the defined category ids to be used as a filter
        /// </summary>
        /// <returns>A <see cref="IEnumerable{Guid}"/></returns> with the IDs of the categories to filter on
        private IEnumerable<Guid> GetCategoryIdsForCherryPick()
        {
            var categoriesName = this.needCherryPicked.SelectMany(x => x.CategoriesOfInterest).Distinct();
            var categories = new List<Category>();

            foreach (var referenceDataLibrary in this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries())
            {
                categories.AddRange(referenceDataLibrary.DefinedCategory
                    .Where(x => categoriesName.Contains(x.Name))
                    .ToList());
            }

            var categoriesIds = categories.Select(x => x.Iid);
            return categoriesIds;
        }

        /// <summary>
        /// Gets the defined class kinds to be used as a filter
        /// </summary>
        /// <returns>A <see cref="IEnumerable{ClassKind}"/> with the <see cref="ClassKind"/> to filter on</returns>
        private IEnumerable<ClassKind> GetClassKindsForCherryPick()
        {
            return this.needCherryPicked.SelectMany(x => x.ClassKindsOfInterest).Distinct();
        }

        /// <summary>
        /// Processes the retrieved cherry picked data
        /// </summary>
        /// <param name="cherryPickedData">The cherry picked data</param>
        private void ProcessCherryPickedData(IReadOnlyCollection<IEnumerable<Thing>> cherryPickedData)
        {
            foreach (var needCherryPickedData in this.needCherryPicked)
            {
                needCherryPickedData.ProcessCherryPickedData(cherryPickedData);
            }
        }
        
        /// <summary>
        /// Runs the cherrypick features based on data required from <see cref="needCherryPicked" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task RunCherryPickAsync()
        {
            if (this.IsCherryPicking)
            {
                return;
            }

            this.IsCherryPicking = true;
            
            var classKinds = this.GetClassKindsForCherryPick();
            var categoriesIds = this.GetCategoryIdsForCherryPick();
            
            var availableEngineeringModelSetups = this.sessionService.GetParticipantModels().ToList();
            var cherryPicks = availableEngineeringModelSetups.Select(engineeringModelSetup => this.sessionService.Session.CherryPick(engineeringModelSetup.EngineeringModelIid, engineeringModelSetup.IterationSetup.Single(x => x.FrozenOn == null).IterationIid, classKinds, categoriesIds)).ToList();
            var results = (await Task.WhenAll(cherryPicks)).Where(x => x.Any()).ToList();

            this.ProcessCherryPickedData(results);
            this.IsCherryPicking = false;
        }

        /// <summary>
        /// Runs the cherrypick features for a specific Engineering Model and Iteration, based on data required from <see cref="needCherryPicked" />
        /// </summary>
        /// <param name="engineeringModelId">The engineering model Id that we want to cherry pick from</param>
        /// <param name="iterationId">The iteration Id that we want to cherry pick from</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task RunSingleCherryPickAsync(Guid engineeringModelId, Guid iterationId)
        {
            if (this.IsCherryPicking)
            {
                return;
            }

            this.IsCherryPicking = true;
            
            var classKinds = this.GetClassKindsForCherryPick();
            var categoriesIds = this.GetCategoryIdsForCherryPick();
            
            var cherryPicks = await this.sessionService.Session.CherryPick(engineeringModelId, iterationId, classKinds, categoriesIds);

            this.ProcessCherryPickedData(new []{ cherryPicks });

            this.IsCherryPicking = false;
        }
    }
}
