﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CopyElementDefinitionCreator.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The class responsible for copying Element Definition
    /// </summary>
    public class CopyElementDefinitionCreator
    {
        /// <summary>
        /// The string added to the element definition name
        /// </summary>
        private const string CopyAffix = " - Copy";

        /// <summary>
        /// The <see cref="ISession"/> in which the copy is performed
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The original-clone <see cref="ParameterGroup"/> map
        /// </summary>
        private readonly Dictionary<ParameterGroup, ParameterGroup> groupMap = new Dictionary<ParameterGroup, ParameterGroup>();

        /// <summary>
        /// The original-clone <see cref="ParameterValueSetBase"/> map
        /// </summary>
        private readonly Dictionary<ParameterValueSetBase, ParameterValueSetBase> valueSetMap = new Dictionary<ParameterValueSetBase, ParameterValueSetBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyElementDefinitionCreator"/> class
        /// </summary>
        /// <param name="session">The associated <see cref="ISession"/></param>
        public CopyElementDefinitionCreator(ISession session)
        {
            this.session = session;
        }

        /// <summary>
        /// Perform the copy operation of an <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/> to copy</param>
        /// <param name="areUsagesCopied">Do we need to copy the ElementUsages also?</param>
        public async Task Copy(ElementDefinition elementDefinition, bool areUsagesCopied)
        {
            var iterationClone = (Iteration)elementDefinition.Container.Clone(false);
            var transactionContext = TransactionContextResolver.ResolveContext(iterationClone);
            var transaction = new ThingTransaction(transactionContext, iterationClone);

            var clone = elementDefinition.Clone(true);
            clone.Iid = Guid.NewGuid();
            clone.Name += CopyAffix;

            if (!areUsagesCopied)
            {
                clone.ContainedElement.Clear();
            }

            this.ResolveReferences(elementDefinition, clone);
            iterationClone.Element.Add(clone);

            transaction.CopyDeep(clone);
            await this.session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Resolve the references of the copy
        /// </summary>
        /// <param name="original">The original <see cref="ElementDefinition"/></param>
        /// <param name="deepClone">The clone</param>
        private void ResolveReferences(ElementDefinition original, ElementDefinition deepClone)
        {
            // Order of the item in a list is should be kept when cloning
            // register mapping between original and copy
            for (var i = 0; i < original.ParameterGroup.Count; i++)
            {
                this.groupMap.Add(original.ParameterGroup[i], deepClone.ParameterGroup[i]);
            }

            for (var i = 0; i < deepClone.Parameter.Count; i++)
            {
                var originalParameter = original.Parameter[i];
                var cloneParameter = deepClone.Parameter[i];

                for (var j = 0; j < originalParameter.ValueSet.Count; j++)
                {
                    this.valueSetMap.Add(originalParameter.ValueSet[j], cloneParameter.ValueSet[j]);
                }
            }

            for (var i = 0; i < deepClone.ContainedElement.Count; i++)
            {
                var originalUsage = original.ContainedElement[i];
                var cloneUsage = deepClone.ContainedElement[i];
                
                for (var j = 0; j < originalUsage.ParameterOverride.Count; j++)
                {
                    var originalOverride = originalUsage.ParameterOverride[j];
                    var cloneOverride = cloneUsage.ParameterOverride[j];
                
                    for (var k = 0; k < originalOverride.ValueSet.Count; k++)
                    {
                        this.valueSetMap.Add(originalOverride.ValueSet[k], cloneOverride.ValueSet[k]);
                    }
                }
            }

            // Resolve references
            foreach (var group in this.groupMap.Values)
            {
                if (group.ContainingGroup != null)
                {
                    // use the mapped group
                    group.ContainingGroup = this.groupMap[group.ContainingGroup];
                }
            }

            // fix the group of the cloned parameters
            foreach (var parameter in deepClone.Parameter)
            {
                if (parameter.Group != null)
                {
                    parameter.Group = this.groupMap[parameter.Group];
                }

                foreach (var parameterSubscription in parameter.ParameterSubscription)
                {
                    foreach (var parameterSubscriptionValueSet in parameterSubscription.ValueSet)
                    {
                        parameterSubscriptionValueSet.SubscribedValueSet =
                            this.valueSetMap[parameterSubscriptionValueSet.SubscribedValueSet];
                    }
                }
            }

            // fix the references of the subscription value set
            foreach (var elementUsage in deepClone.ContainedElement)
            {
                foreach (var parameterOverride in elementUsage.ParameterOverride)
                {
                    foreach (var parameterSubscription in parameterOverride.ParameterSubscription)
                    {
                        foreach (var parameterSubscriptionValueSet in parameterSubscription.ValueSet)
                        {
                            parameterSubscriptionValueSet.SubscribedValueSet =
                                this.valueSetMap[parameterSubscriptionValueSet.SubscribedValueSet];
                        }
                    }
                }
            }
        }
    }
}