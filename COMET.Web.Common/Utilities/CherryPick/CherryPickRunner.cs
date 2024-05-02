﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CherryPickRunner.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Utilities.CherryPick
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;

    using Dasync.Collections;

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
        /// Gets the number of threads used in the cherry pick operation
        /// </summary>
        private int numberOfThreads = 1;

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
        /// <param name="maxNumberOfThreads">The number of threads to use when doing the cherry pick</param>
        public void InitializeProperties(IEnumerable<INeedCherryPickedData> needCherryPickedData, int maxNumberOfThreads = 1)
        {
            this.IsCherryPicking = false;
            this.numberOfThreads = maxNumberOfThreads;
            this.needCherryPicked.Clear();
            this.needCherryPicked.AddRange(needCherryPickedData);
        }

        /// <summary>
        /// Runs the cherrypick features based on data required from <see cref="INeedCherryPickedData" /> for all the Engineering Models the user is participating on
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel any tasks upon request. The default is <see cref="CancellationToken.None"/></param>
        /// <returns>A <see cref="Task" /></returns>
        public Task RunCherryPickAsync(CancellationToken cancellationToken = default)
        {
            var availableEngineeringModelSetups = this.sessionService.GetParticipantModels().ToList();
            var engineeringModelAndIterationIdTuple = availableEngineeringModelSetups.Select(x => (x.EngineeringModelIid, x.IterationSetup.Single(c => c.FrozenOn == null).IterationIid));
            return this.RunCherryPickAsync(engineeringModelAndIterationIdTuple, cancellationToken);
        }

        /// <summary>
        /// Runs the cherrypick features based on data required from <see cref="INeedCherryPickedData" /> and a particular set of EngineeringModelId and IterationId.
        /// </summary>
        /// <param name="ids">A <see cref="Tuple{Guid,Guid}"/> to run the cherry pick for a particular set of engineeringModelIds and iterationIds</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel any tasks upon request. The default is <see cref="CancellationToken.None"/></param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task RunCherryPickAsync(IEnumerable<(Guid engineeringModelId, Guid iterationId)> ids, CancellationToken cancellationToken = default)
        {
            if (this.IsCherryPicking)
            {
                return;
            }

            this.IsCherryPicking = true;
            var classKinds = this.GetClassKindsForCherryPick();
            var categoryIds = this.GetCategoryIdsForCherryPick();
            
            await ids.ParallelForEachAsync(async pair =>
                {
                    var result = (await this.sessionService.Session.CherryPick(pair.engineeringModelId, pair.iterationId, classKinds, categoryIds)).ToList();
                    
                    foreach (var needCherryPickedData in this.needCherryPicked.Where(_ => result.Count != 0))
                    {
                        needCherryPickedData.ProcessCherryPickedData(result);
                    }
                },
            this.numberOfThreads,
            cancellationToken);

            this.IsCherryPicking = false;
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
    }
}
