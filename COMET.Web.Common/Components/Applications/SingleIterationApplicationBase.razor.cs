// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationBase.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Components.Applications
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.Applications;

    /// <summary>
    /// Base component for any application that will need only one <see cref="Iteration" />
    /// </summary>
    /// <typeparam name="TViewModel">An <see cref="ISingleIterationApplicationBaseViewModel" /></typeparam>
    public abstract partial class SingleIterationApplicationBase<TViewModel>
    {
        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.NumberOfUrlRequiredParameters = 4;
        }

        /// <summary>
        /// Sets the URL parameters that are required for this application
        /// </summary>
        /// <param name="currentOptions">A <see cref="Dictionary{TKey,TValue}" /> of current URL parameters that comes form the URI</param>
        /// <param name="urlParameters">A <see cref="Dictionary{TKey,TValue}" /> of parameters that have to be included</param>
        protected override void SetUrlParameters(Dictionary<string, string> currentOptions, Dictionary<string, string> urlParameters)
        {
            base.SetUrlParameters(currentOptions, urlParameters);

            if (currentOptions.TryGetValue(QueryKeys.IterationKey, out var iterationValue))
            {
                urlParameters[QueryKeys.IterationKey] = iterationValue;
            }

            if (currentOptions.TryGetValue(QueryKeys.DomainKey, out var domainValue))
            {
                urlParameters[QueryKeys.DomainKey] = domainValue;
            }

            if (currentOptions.TryGetValue(QueryKeys.ModelKey, out var modelValue))
            {
                urlParameters[QueryKeys.ModelKey] = modelValue;
            }
        }
    }
}
