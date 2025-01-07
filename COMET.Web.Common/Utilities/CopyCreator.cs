// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CopyCreator.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    /// <summary>
    /// The class responsible for copy operations
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "No coverage for now, as test code would be related to CopyPermissionHelper methods for most part")]
    public class CopyCreator
    {
        /// <summary>
        /// The <see cref="ISession"/> in which the copy is performed
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyCreator"/> class
        /// </summary>
        /// <param name="session">The associated <see cref="ISession"/></param>
        public CopyCreator(ISession session)
        {
            this.session = session;
        }

        /// <summary>
        /// Perform the copy operation of an <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/> to copy</param>
        /// <param name="targetIteration">The target container</param>
        public async Task CopyAsync(ElementDefinition elementDefinition, Iteration targetIteration)
        {
            // copy the payload to this iteration
            var copyOperationHelper = new CopyPermissionHelper(this.session, false);
            var copyPermissionResult = await copyOperationHelper.ComputeCopyPermissionAsync(elementDefinition, targetIteration);

            if (copyPermissionResult.ErrorList.Any() || copyPermissionResult.CopyableThings.Any())
            {
                await this.WriteCopyOperationAsync(elementDefinition, targetIteration, OperationKind.CopyKeepValuesChangeOwner);
            }
        }

        /// <summary>
        /// Create and write the copy operation
        /// </summary>
        /// <param name="thingToCopy">The <see cref="Thing"/> to copy</param>
        /// <param name="targetContainer">The target container</param>
        /// <param name="operationKind">The <see cref="OperationKind"/></param>
        private async Task WriteCopyOperationAsync(Thing thingToCopy, Thing targetContainer, OperationKind operationKind)
        {
            var clone = thingToCopy.Clone(false);
            var containerClone = targetContainer.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(targetContainer);
            var transaction = new ThingTransaction(transactionContext, containerClone);
            transaction.Copy(clone, containerClone, operationKind);

            await this.session.Write(transaction.FinalizeTransaction());
        }
    }
}
