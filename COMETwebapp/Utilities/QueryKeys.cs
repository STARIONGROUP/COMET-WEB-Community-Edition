// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="QueryKeys.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Utilities
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
		/// The query key for the <see cref="EngineeringModel" /> id
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
		/// The query key for the <see cref="Option"/>
		/// </summary>
        public const string OptionKey = "option";

		/// <summary>
		/// The query key for the <see cref="ActualFiniteState"/>
		/// </summary>
        public const string StateKey = "state";

        /// <summary>
        /// The query key for the <see cref="ParameterType"/>
        /// </summary>
        public const string ParameterKey = "parameter";
    }
}
