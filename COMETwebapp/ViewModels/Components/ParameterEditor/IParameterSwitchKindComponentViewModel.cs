// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterSwitchKindComponentViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using CDP4Common.EngineeringModelData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Interface for the <see cref="ParameterSwitchKindComponentViewModel"/>
    /// </summary>
    public interface IParameterSwitchKindComponentViewModel
    {
        /// <summary>
        /// Iid of the associated ParametervalueSet
        /// </summary>
        public Guid ParameterValueSetIid { get; set; }

        /// <summary>
        /// The switch mode of the associated ParameterValueSet
        /// </summary>
        public ParameterSwitchKind ParameterValueSetSwitchMode { get; set; }

        /// <summary>
        /// Sets computed button active
        /// </summary>
        public ParameterSwitchKind SwitchValue { get; set; }

        /// <summary>
        /// Sets if the switch can be change in the ISession
        /// </summary>
        public bool IsEditable { get; set; }
        
        /// <summary>
        /// Sends an event with the selected switch
        /// </summary>
        public void OnClickComputed();

        /// <summary>
        /// Sends an event with the selected switch
        /// </summary>
        public void OnClickManual();

        /// <summary>
        /// Sends an event with the selected switch
        /// </summary>
        public void OnClickReference();

        /// <summary>
        /// Sends an event to write the selected switch on ISession
        /// </summary>
        public void OnSubmitSwitchChange();
    }
}
