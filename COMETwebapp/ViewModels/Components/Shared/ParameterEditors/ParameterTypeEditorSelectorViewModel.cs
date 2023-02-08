// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeEditorSelectorViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMETwebapp.Model;
    using COMETwebapp.Services.SessionManagement;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.Shared.ParameterTypeEditors.ParameterTypeEditorSelector"/>
    /// </summary>
    public class ParameterTypeEditorSelectorViewModel : IParameterTypeEditorSelectorViewModel<ParameterType>
    {
        /// <summary>
        /// Gets or sets the <see cref="ISessionService"/>
        /// </summary>
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ParameterType { get; set; }

        /// <summary>
        /// Event Callback for when a value has changed on the parameter
        /// </summary>
        public EventCallback<IValueSet> ParameterValueChanged { get; set; }

        /// <summary>
        /// Gets or sets if the Editor is readonly.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the value set of this <see cref="ParameterType"/>
        /// </summary>
        public IValueSet ValueSet { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterTypeEditorSelectorViewModel"/>
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService"/></param>
        /// <param name="parameterType">the <see cref="ParameterType"/> used for this view model</param>
        /// <param name="valueSet">the value set asociated to the ParameterTypeEditor</param>
        public ParameterTypeEditorSelectorViewModel(ISessionService sessionService, ParameterType parameterType, IValueSet valueSet)
        {
            this.SessionService = sessionService;
            this.ParameterType = parameterType;
            this.ValueSet = valueSet;

            CDPMessageBus.Current.Listen<SwitchEvent>().Subscribe(x =>
            {
                this.ValueSet.ValueSwitch = x.SelectedSwitch;
            });
        }

        /// <summary>
        /// Creates a view model for the corresponding editor
        /// </summary>
        /// <typeparam name="T">the parameter type</typeparam>
        /// <returns>the view model</returns>
        public IParameterEditorBaseViewModel<T> CreateParameterEditorViewModel<T>() where T: ParameterType
        {
            switch (this.ParameterType)
            {
                case BooleanParameterType booleanParamType: return new BooleanParameterTypeEditorViewModel(booleanParamType, this.ValueSet) as IParameterEditorBaseViewModel<T>;
                case CompoundParameterType compoundParamType: return new CompoundParameterTypeEditorViewModel(compoundParamType, this.ValueSet) as IParameterEditorBaseViewModel<T>;
                case DateParameterType dateParamType: return new DateParameterTypeEditorViewModel(dateParamType, this.ValueSet) as IParameterEditorBaseViewModel<T>;
                case DateTimeParameterType dateTimeParamType: return new DateTimeParameterTypeEditorViewModel(dateTimeParamType, this.ValueSet) as IParameterEditorBaseViewModel<T>;
                case EnumerationParameterType enumParamType: return new EnumerationParameterTypeEditorViewModel(enumParamType, this.ValueSet) as IParameterEditorBaseViewModel<T>;
                case QuantityKind quantityKindParamType: return new QuantityKindParameterTypeEditorViewModel(quantityKindParamType, this.ValueSet) as IParameterEditorBaseViewModel<T>;
                case TextParameterType textParamType: return new TextParameterTypeEditorViewModel(textParamType, this.ValueSet) as IParameterEditorBaseViewModel<T>;
                case TimeOfDayParameterType timeDayParameterType: return new TimeOfDayParameterTypeEditorViewModel(timeDayParameterType, this.ValueSet) as IParameterEditorBaseViewModel<T>;
                
                default: throw new NotImplementedException($"The ViewModel for the {this.ParameterType} has not been implemented");
            }
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public async Task OnParameterValueChanged(IValueSet value)
        {
            if (value is ParameterValueSetBase parameterValueSetBase)
    {
                await this.SessionService.UpdateThings(this.SessionService.DefaultIteration, new List<Thing>() { parameterValueSetBase });
            }
        }
    }
}
