// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterSwitchKindSelectorViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.Shared.Selectors
{
    using CDP4Common.EngineeringModelData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View Model to sets the current <see cref="ParameterSwitchKind" /> value
    /// </summary>
    public class ParameterSwitchKindSelectorViewModel : ReactiveObject, IParameterSwitchKindSelectorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsReadOnly" />
        /// </summary>
        private bool isReadOnly;

        /// <summary>
        /// Backing field for <see cref="SwitchValue" />
        /// </summary>
        private ParameterSwitchKind switchValue;

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterSwitchKindSelectorViewModel" />
        /// </summary>
        /// <param name="switchKind">The <see cref="ParameterSwitchKind" /></param>
        /// <param name="isReadOnly">The readonly state</param>
        public ParameterSwitchKindSelectorViewModel(ParameterSwitchKind switchKind, bool isReadOnly)
        {
            this.InitialSwitchValue = switchKind;
            this.SwitchValue = switchKind;
            this.IsReadOnly = isReadOnly;
        }

        /// <summary>
        /// The initial <see cref="ParameterSwitchKind"/>
        /// </summary>
        public ParameterSwitchKind InitialSwitchValue { get; }

        /// <summary>
        /// Gets or sets the <see cref="EventCallback{TValue}" /> to be called to perfom an update
        /// </summary>
        public EventCallback OnUpdate { get; set; }

        /// <summary>
        /// Get or set the <see cref="ParameterSwitchKind" />
        /// </summary>
        public ParameterSwitchKind SwitchValue
        {
            get => this.switchValue;
            set => this.RaiseAndSetIfChanged(ref this.switchValue, value);
        }

        /// <summary>
        /// Gets or sets the readonly state
        /// </summary>
        public bool IsReadOnly
        {
            get => this.isReadOnly;
            set => this.RaiseAndSetIfChanged(ref this.isReadOnly, value);
        }
    }
}
