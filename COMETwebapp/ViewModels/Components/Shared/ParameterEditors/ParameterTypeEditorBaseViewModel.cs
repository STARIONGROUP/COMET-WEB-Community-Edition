// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeEditorBaseViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.Shared.ParameterEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMETwebapp.Model;

    using Microsoft.AspNetCore.Components;using ReactiveUI;

    /// <summary>
    /// Base ViewModel for the <see cref="CDP4Common.SiteDirectoryData.ParameterType"/> editors
    /// </summary>
    /// <typeparam name="T">the type of the <see cref="Parameter"/></typeparam>
    public abstract class ParameterTypeEditorBaseViewModel<T> : ReactiveObject, IParameterEditorBaseViewModel<T> where T : ParameterType
    {
        /// <summary>
        /// Gets or sets the <see cref="CDP4Common.SiteDirectoryData.ParameterType"/>
        /// </summary>
        public T ParameterType { get; set; }

        /// <summary>
        /// Event Callback for when a value has changed on the parameter
        /// </summary>
        public EventCallback<IValueSet> ParameterValueChanged { get; set; }

        /// <summary>
        /// Backing field for the <see cref="IsReadOnly"/> property
        /// </summary>
        private bool isReadOnly;

        /// <summary>
        /// Gets or sets if the Editor is readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get => this.isReadOnly;
            set => this.RaiseAndSetIfChanged(ref this.isReadOnly, value);
        }

        /// <summary>
        /// Gets or sets the value set of this <see cref="T"/>
        /// </summary>
        public IValueSet ValueSet { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterTypeEditorBaseViewModel{T}"/>
        /// </summary>
        /// <param name="parameterType">the parameter type of this view model</param>
        /// <param name="valueSet">the value set asociated to this editor</param>
        protected ParameterTypeEditorBaseViewModel(T parameterType, IValueSet valueSet)
        {
            this.ParameterType = parameterType;
            this.ValueSet = valueSet;

            CDPMessageBus.Current.Listen<SwitchEvent>().Subscribe(x =>
            {
                this.ValueSet.ValueSwitch = x.SelectedSwitch;
                this.IsReadOnly = x.SelectedSwitch == ParameterSwitchKind.REFERENCE;
            });
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public abstract Task OnParameterValueChanged(object value);
    }
}
