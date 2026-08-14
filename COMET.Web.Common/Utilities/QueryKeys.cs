// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="QueryKeys.cs" company="Starion Group S.A.">
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Static class that provide const value for url query parameters
    /// </summary>
    public static class QueryKeys
    {
        /// <summary>
        /// The query key for the server url
        /// </summary>
        public const string ServerKey = "server";

        /// <summary>
        /// The query key for the <see cref="EngineeringModelSetup" /> id
        /// </summary>
        public const string ModelKey = "modelId";

        /// <summary>
        /// The query key for the <see cref="Iteration" /> id
        /// </summary>
        public const string IterationKey = "iterationId";

        /// <summary>
        /// The query key for the <see cref="DomainOfExpertise" /> id
        /// </summary>
        public const string DomainKey = "domainId";

        /// <summary>
        /// The query key for the <see cref="Option" />
        /// </summary>
        public const string OptionKey = "option";

        /// <summary>
        /// The query key for the <see cref="ActualFiniteState" />
        /// </summary>
        public const string StateKey = "state";

        /// <summary>
        /// The query key for the <see cref="ParameterType" />
        /// </summary>
        public const string ParameterKey = "parameter";

        /// <summary>
        /// The query key for a comma-delimited list of <see cref="ParameterType" /> short-form GUIDs, used by
        /// multi-select filters such as the Parameter Editor's parameter-type filter.
        /// </summary>
        public const string ParametersKey = "parameters";

        /// <summary>
        /// The query key for a comma-delimited list of <see cref="Category" /> short-form GUIDs, used by the
        /// Parameter Editor's category filter.
        /// </summary>
        public const string CategoriesKey = "categories";

        /// <summary>
        /// The query key for a comma-delimited list of <see cref="Option" /> short-form GUIDs, used by
        /// multi-select option filters.
        /// </summary>
        public const string OptionsKey = "options";

        /// <summary>
        /// The query key for a comma-delimited list of <see cref="ElementBase" /> short-form GUIDs, used by the
        /// Parameter Editor's element filter.
        /// </summary>
        public const string ElementsKey = "elements";

        /// <summary>
        /// The query key for a comma-delimited list of <see cref="ActualFiniteState" /> short-form GUIDs, used by
        /// multi-select finite state filters.
        /// </summary>
        public const string StatesKey = "states";

        /// <summary>
        /// The query key for the confirmed parameter used during logout callback
        /// </summary>
        public const string ConfirmedKey = "confirmed";
    }
}
