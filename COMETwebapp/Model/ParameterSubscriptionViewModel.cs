// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubscribedParameter.cs" company="RHEA System S.A.">
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
    /// Represent information for subscribed parameters
    /// </summary>
    public class ParameterSubscriptionViewModel
    {
        /// <summary>
        /// Name of the associated ElementDefinition
        /// </summary>
        public string? ElementName { get; set; }

        /// <summary>
        /// Type of the ParameterSubcription
        /// </summary>
        public string? ParameterType { get; set; }

        /// <summary>
        /// ModelCode of the associated Parameter
        /// </summary>
        public string? ModelCode { get; set; }

        /// <summary>
        /// Tells if the associated Parameter is option dependent
        /// </summary>
        public bool? IsOptionDependent { get; set; }

        /// <summary>
        /// Tells if the associated Parameter is state dependent
        /// </summary>
        public bool? IsStateDependent { get; set; }

        /// <summary>
        /// Tells if the associated Parameter has any default ParameterValueSet
        /// </summary>
        public bool? IsIncompleted { get; set; }

        /// <summary>
        /// Actual value of the ParameterSubcriptionValueSet
        /// </summary>
        public ValueArray<string>? ActualValue { get; set; }

        /// <summary>
        /// Value scale of the ParameterSubcriptionValueSet
        /// </summary>
        public string? Scale { get; set; }

        /// <summary>
        /// Switch Mode in the ParameterSubcriptionValueSet
        /// </summary>
        public string? SwitchMode { get; set; }

        /// <summary>
        /// Name of the option dependency
        /// </summary>
        public string? Option { get; set; }

        /// <summary>
        /// Name of the state dependency
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Defines if the ParameterSubcriptionValueSet is updated
        /// </summary>
        public bool IsUpdated { get; set; } = false;

        /// <summary>
        /// Actual value of the subscribed ParameterValueSet
        /// </summary>
        public ValueArray<string>? SubscribedActualValue { get; set; }

        /// <summary>
        /// Value scale of the subscribed ParameterValueSet
        /// </summary>
        public string? SubscribedScale { get; set; }

        /// <summary>
        /// Switch Mode of the subscribed ParameterValueSet
        /// </summary>
        public string? SubscribedSwitchMode { get; set; }

        /// <summary>
        /// ShortName of the subscribed ParameterValueSet owner
        /// </summary>
        public string? SubscribedOwner { get; set; }

        /// <summary>
        /// Defines if the subscribed ParameterValueSet is updated
        /// </summary>
        public bool IsSubscribedUpdated { get; set; } = false;

    }
}
