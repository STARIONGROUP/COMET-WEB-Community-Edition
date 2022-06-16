// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTable.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Model
{
    using CDP4Common.Types;

    /// <summary>
    /// Parameters to add in the ParameterTable component
    /// </summary>
    public class BuiltParameter
    {
        /// <summary>
        /// The parameter name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The element name
        /// </summary>
        public string? ElementName { get; set; }

        /// <summary>
        /// The <see cref="DomainOfExpertise"/> name of the parameter
        /// </summary>
        public string? Domain { get; set; }

        /// <summary>
        /// The <see cref="Option"/> name of the parameter
        /// </summary>
        public string? Option { get; set; }

        /// <summary>
        /// The <see cref="State"/> name of the parameter
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// The <see cref="Scale"/> shortName of the parameter
        /// </summary>
        public string? Scale { get; set; }

        /// <summary>
        /// The parameter actual value 
        /// </summary>
        public ValueArray<string>? ActualValue { get; set; }

        /// <summary>
        /// The parameter published value 
        /// </summary>
        public ValueArray<string>? PublishedValue { get; set; }

        /// <summary>
        /// The parameter manual value 
        /// </summary>
        public ValueArray<string>? ManualValue { get; set; }

        /// <summary>
        /// The parameter computed value 
        /// </summary>
        public ValueArray<string>? ComputedValue { get; set; }

        /// <summary>
        /// The parameter reference value 
        /// </summary>
        public ValueArray<string>? ReferenceValue { get; set; }

        /// <summary>
        /// Switch Mode of the ParameterValueSet
        /// </summary>
        public string? SwitchMode { get; set; }

    }
}
