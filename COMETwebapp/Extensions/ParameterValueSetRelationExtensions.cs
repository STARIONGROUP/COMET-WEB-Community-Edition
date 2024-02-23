// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="ParameterValueSetRelationExtensions.cs" company="RHEA System S.A."> 
//    Copyright (c) 2022 RHEA System S.A. 
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar 
// 
//    This file is part of CDP4-COMET WEB Community Edition 
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C. 
// 
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or 
//    modify it under the terms of the GNU Affero General Public 
//    License as published by the Free Software Foundation; either 
//    version 3 of the License, or (at your option) any later version. 
// 
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful, 
//    but WITHOUT ANY WARRANTY; without even the implied warranty of 
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU 
//    Affero General Public License for more details. 
// 
//    You should have received a copy of the GNU Affero General Public License 
//    along with this program.  If not, see <http://www.gnu.org/licenses/>. 
// </copyright> 
// -------------------------------------------------------------------------------------------------------------------- 

namespace COMETwebapp.Extensions
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;

    using COMET.Web.Common.Extensions;

    /// <summary> 
    /// Class used for extension of the relation between <see cref="ParameterBase"/> and <see cref="IValueSet"/> 
    /// </summary> 
    public static class ParameterValueSetRelationExtensions
    {
        /// <summary> 
        /// Gets the parameters and values of the <paramref name="relation2"/> that are different from <paramref name="relation1"/> for the same <see cref="ParameterBase"/> 
        /// </summary> 
        /// <param name="relation1">the first relation</param> 
        /// <param name="relation2">the second relation</param> 
        /// <returns>the collection with the changes</returns> 
        public static Dictionary<ParameterBase, IValueSet> GetChangesOnParameters(this Dictionary<ParameterBase, IValueSet> relation1, Dictionary<ParameterBase, IValueSet> relation2)
        {
            if(relation1 == null)
            {
                throw new ArgumentNullException(nameof(relation1));
            }

            if(relation2 == null)
            {
                throw new ArgumentNullException(nameof(relation2));
            }

            var changes = new Dictionary<ParameterBase, IValueSet>();

            foreach (var originalKeyValuePair in relation1)
            {
                var orignalValueSet = originalKeyValuePair.Value.ActualValue;

                if (relation2.TryGetValue(originalKeyValuePair.Key, out var cloneValueSet) && !orignalValueSet.ContainsSameValues(cloneValueSet.ActualValue))
                {
                    changes.Add(originalKeyValuePair.Key, cloneValueSet);
                }
            }

            return changes;
        }
    }
}