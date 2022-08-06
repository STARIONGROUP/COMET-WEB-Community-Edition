// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwitchEvent.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Event when switch changes
    /// </summary>
    public class SwitchEvent
    {
        /// <summary>
        /// Iid of the associated ParameterValueSet
        /// </summary>
        public Guid? ParameterValuSetIid { get; set; }

        /// <summary>
        /// The selected switch mode to see
        /// </summary>
        public ParameterSwitchKind SelectedSwitch { get; set; }

        /// <summary>
        /// Sets if the change have to be submit
        /// </summary>
        public bool? SubmitChange { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchEvent"/> class.
        /// <param name="parameterValuSetIid">Iid of the associated ParameterValueSet</param>
        /// <param name="selectedSwitch">The selected ParameterSwitchKind</param>
        public SwitchEvent(Guid? parameterValuSetIid, ParameterSwitchKind selectedSwitch, bool? submitChange)
        {
            this.ParameterValuSetIid = parameterValuSetIid;
            this.SelectedSwitch = selectedSwitch;
            this.SubmitChange = submitChange;
        }   
    }
}
