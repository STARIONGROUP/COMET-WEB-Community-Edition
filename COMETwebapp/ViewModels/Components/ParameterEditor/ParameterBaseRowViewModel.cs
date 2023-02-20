// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterBaseRowViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    
    using COMETwebapp.Model;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;

    /// <summary>
    /// ViewModel for the rows asociated to a <see cref="ParameterBase"/>
    /// </summary>
    public class ParameterBaseRowViewModel : IParameterBaseRowViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISessionService"/>
        /// </summary>
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterBase"/> for this <see cref="ParameterBaseRowViewModel"/>
        /// </summary>
        public ParameterBase Parameter { get; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType"/> for this <see cref="ParameterBaseRowViewModel"/>
        /// </summary>
        public ParameterType ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ElementBase"/> used for grouping this <see cref="ParameterBaseRowViewModel"/>
        /// </summary>
        public string ElementBaseName { get; }

        /// <summary>
        /// Gets the <see cref="Parameter"/> type name
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Gets the <see cref="Parameter"/> owner name
        /// </summary>
        public string OwnerName { get; }

        /// <summary>
        /// Gets the switch for the published value
        /// </summary>
        public ParameterSwitchKind Switch { get; private set; }

        /// <summary>
        /// Gets the <see cref="Parameter"/> model code
        /// </summary>
        public string ModelCode { get; }

        /// <summary>
        /// Gets the <see cref="Option"/> name this <see cref="Parameter"/> is dependant on
        /// </summary>
        public string Option { get; } = string.Empty;

        /// <summary>
        /// Gets the <see cref="ActualFiniteState"/> name this <see cref="Parameter"/> is dependant on
        /// </summary>
        public string State { get; } = string.Empty;

        /// <summary>
        /// Gets or sets the <see cref="IValueSet"/> of this <see cref="ParameterBaseRowViewModel"/>
        /// </summary>
        private IValueSet ValueSet { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterBaseRowViewModel"/>
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService"/></param>
        /// <param name="parameterBase">the parameter of this row</param>
        /// /// <param name="valueSet">the valueSet of the parameter</param>
        public ParameterBaseRowViewModel(ISessionService sessionService,ParameterBase parameterBase, IValueSet valueSet)
        {
            this.SessionService = sessionService;
            this.Parameter = parameterBase ?? throw new ArgumentNullException(nameof(parameterBase));
            this.ParameterType = this.Parameter.ParameterType;
            this.ParameterName = this.Parameter.ParameterType is not null ? this.Parameter.ParameterType.Name : string.Empty;
            this.OwnerName = this.Parameter.Owner is not null ? this.Parameter.Owner.ShortName : string.Empty;
            this.ModelCode = this.Parameter.ModelCode();
            this.ElementBaseName = (parameterBase.Container as ElementBase)?.ShortName;
            this.ValueSet = valueSet;
            this.Option = valueSet.ActualOption is not null ? valueSet.ActualOption?.Name : string.Empty;
            this.State = valueSet.ActualState is not null && valueSet.ActualState.Name is not null ? valueSet.ActualState?.Name : string.Empty;
            this.Switch = valueSet.ValueSwitch;

            CDPMessageBus.Current.Listen<SwitchEvent>().Subscribe(x =>
            {
                this.Switch = x.SelectedSwitch;
                this.ValueSet.ValueSwitch = x.SelectedSwitch;
            });
        }

        /// <summary>
        /// Creates a <see cref="IParameterTypeEditorSelectorViewModel{T}"/> based on the data of this <see cref="IParameterBaseRowViewModel"/>
        /// </summary>
        /// <returns></returns>
        public IParameterTypeEditorSelectorViewModel<T> CreateParameterTypeEditorSelectorViewModel<T>() where T : ParameterType 
        {
            return new ParameterTypeEditorSelectorViewModel(this.SessionService,this.ParameterType, this.ValueSet) as IParameterTypeEditorSelectorViewModel<T>;
        }

        /// <summary>
        /// Creates a <see cref="IParameterSwitchKindComponentViewModel"/> based on the data of this <see cref="IParameterBaseRowViewModel"/>
        /// </summary>
        /// <returns>a <see cref="IParameterSwitchKindComponentViewModel"/></returns>
        public IParameterSwitchKindComponentViewModel CreateParameterSwitchKindComponentViewModel()
        {
            return new ParameterSwitchKindComponentViewModel(this.SessionService, this.ValueSet);
        }
    }
}
