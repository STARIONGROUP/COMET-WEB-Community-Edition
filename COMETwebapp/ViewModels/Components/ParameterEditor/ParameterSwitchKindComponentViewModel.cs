// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterSwitchKindComponentViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    
    using CDP4Dal;

    using COMETwebapp.Components.ParameterEditor;
    using COMETwebapp.Model;
    using COMETwebapp.Services.SessionManagement;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="ParameterSwitchKindComponent"/>
    /// </summary>
    public class ParameterSwitchKindComponentViewModel : ReactiveObject, IParameterSwitchKindComponentViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISessionService"/>
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IValueSet"/>
        /// </summary>
        public IValueSet ValueSet { get; set; }

        /// <summary>
        /// Sets computed button active
        /// </summary>
        public ParameterSwitchKind SwitchValue { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterSwitchKindComponentViewModel"/>
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService"/></param>
        /// <param name="valueSet">the <see cref="IValueSet"/></param>
        public ParameterSwitchKindComponentViewModel(ISessionService sessionService, IValueSet valueSet)
        {
            this.SessionService = sessionService;
            this.ValueSet = valueSet;
        }

        /// <summary>
        /// Event for when the <see cref="ParameterSwitchKind"/> value has changed
        /// </summary>
        /// <param name="switchValue">the new value of the switch</param>
        public async Task OnSwitchChanged(ParameterSwitchKind switchValue)
        {
            this.SwitchValue = switchValue;

            if (this.ValueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var clonedParameterValueSet = parameterValueSetBase.Clone(false);
                clonedParameterValueSet.ValueSwitch = switchValue;
                await this.SessionService.UpdateThings(this.SessionService.DefaultIteration, new List<Thing>() { clonedParameterValueSet });

                CDPMessageBus.Current.SendMessage(new SwitchEvent(parameterValueSetBase.Iid, switchValue, false));
            }
        }
    }
}
