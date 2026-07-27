// ---------------// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ThingCreator.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMET.Web.Common.Utilities
{
    using System;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The purpose of the <see cref="ThingCreator"/> is to encapsulate create logic for different things
    /// </summary>
    public class ThingCreator 
    {
        /// <summary>
        /// Create a new <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="container">
        /// The container <see cref="ElementDefinition"/> of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="referencedDefinition">
        /// The referenced <see cref="ElementDefinition"/> of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Parameter"/> is to be added
        /// </param>
        public Task CreateElementUsageAsync(ElementDefinition container, ElementDefinition referencedDefinition, DomainOfExpertise owner, ISession session)
        {
            ArgumentNullException.ThrowIfNull(container);
            ArgumentNullException.ThrowIfNull(referencedDefinition);
            ArgumentNullException.ThrowIfNull(owner);
            ArgumentNullException.ThrowIfNull(session);

            return CreateElementUsageImplAsync(container, referencedDefinition, owner, session);
        }

        /// <summary>
        /// Create a new <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="container">
        /// The container <see cref="ElementDefinition"/> of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="referencedDefinition">
        /// The referenced <see cref="ElementDefinition"/> of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Parameter"/> is to be added
        /// </param>
        private static async Task CreateElementUsageImplAsync(ElementDefinition container, ElementDefinition referencedDefinition, DomainOfExpertise owner, ISession session)
        {
            var clone = container.Clone(false);

            var usage = new ElementUsage
            {
                Name = referencedDefinition.Name,
                ShortName = referencedDefinition.ShortName,
                Owner = owner,
                ElementDefinition = referencedDefinition
            };

            clone.ContainedElement.Add(usage);

            var transactionContext = TransactionContextResolver.ResolveContext(container);
            var transaction = new ThingTransaction(transactionContext, clone);
            transaction.Create(usage);

            var operationContainer = transaction.FinalizeTransaction();
            await session.Write(operationContainer);
        }

        /// <summary>
        /// Move (re-parent) an existing <see cref="ElementUsage"/> so that it becomes contained by a different
        /// <see cref="ElementDefinition"/>, keeping its <see cref="ElementUsage.Iid"/>, name, owner, options and
        /// <see cref="ElementUsage.ParameterOverride"/>s.
        /// </summary>
        /// <param name="elementUsage">
        /// The existing <see cref="ElementUsage"/> to move.
        /// </param>
        /// <param name="targetContainer">
        /// The <see cref="ElementDefinition"/> that becomes the new container of the <see cref="ElementUsage"/>.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the move is written.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous move operation.</returns>
        public Task MoveElementUsageAsync(ElementUsage elementUsage, ElementDefinition targetContainer, ISession session)
        {
            ArgumentNullException.ThrowIfNull(elementUsage);
            ArgumentNullException.ThrowIfNull(targetContainer);
            ArgumentNullException.ThrowIfNull(session);

            return MoveElementUsageImplAsync(elementUsage, targetContainer, session);
        }

        /// <summary>
        /// Move (re-parent) an existing <see cref="ElementUsage"/> to a different container <see cref="ElementDefinition"/>.
        /// </summary>
        /// <param name="elementUsage">
        /// The existing <see cref="ElementUsage"/> to move.
        /// </param>
        /// <param name="targetContainer">
        /// The <see cref="ElementDefinition"/> that becomes the new container of the <see cref="ElementUsage"/>.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the move is written.
        /// </param>
        /// <remarks>
        /// Only the target container clone (with the moved <see cref="ElementUsage"/> clone added to its
        /// <see cref="ElementDefinition.ContainedElement"/>) is written. The server re-parents the usage and removes it
        /// from its former container itself, so the old container must not be sent in the same transaction - mirroring the
        /// composite re-parent already done for a <see cref="CDP4Common.EngineeringModelData.RequirementsGroup"/> move.
        /// </remarks>
        private static async Task MoveElementUsageImplAsync(ElementUsage elementUsage, ElementDefinition targetContainer, ISession session)
        {
            var targetClone = targetContainer.Clone(false);
            var usageClone = elementUsage.Clone(false);

            targetClone.ContainedElement.Add(usageClone);

            var transactionContext = TransactionContextResolver.ResolveContext(targetContainer);
            var transaction = new ThingTransaction(transactionContext, targetClone);
            transaction.CreateOrUpdate(usageClone);

            var operationContainer = transaction.FinalizeTransaction();
            await session.Write(operationContainer);
        }
    }
}
