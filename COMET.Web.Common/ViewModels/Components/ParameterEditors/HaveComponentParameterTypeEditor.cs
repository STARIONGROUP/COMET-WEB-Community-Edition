﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HaveComponentParameterTypeEditor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components.ParameterEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Utilities;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Base class for <see cref="ParameterTypeEditorBaseViewModel{TParameterType}" /> that contains component
    /// <see cref="ParameterType" />
    /// </summary>
    /// <typeparam name="TParameterType">A <see cref="ParameterType" /></typeparam>
    public abstract class HaveComponentParameterTypeEditor<TParameterType> : ParameterTypeEditorBaseViewModel<TParameterType>, IHaveComponentParameterTypeEditor where TParameterType : ParameterType
    {
        /// <summary>
        /// Gets the <see cref="ICDPMessageBus" />
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterTypeEditorBaseViewModel{T}" />
        /// </summary>
        /// <param name="parameterType">the parameter type of this view model</param>
        /// <param name="valueSet">the value set asociated to this editor</param>
        /// <param name="isReadOnly">The readonly state</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="valueArrayIndex">the index of the value changed in the value sets</param>
        protected HaveComponentParameterTypeEditor(TParameterType parameterType, IValueSet valueSet, bool isReadOnly, ICDPMessageBus messageBus, int valueArrayIndex)
            : base(parameterType, valueSet, isReadOnly, valueArrayIndex)
        {
            this.messageBus = messageBus;
        }

        /// <summary>
        /// Event for when the edit button is clicked
        /// </summary>
        public void OnComponentSelected()
        {
            this.messageBus.SendMessage(new HaveComponentParameterTypeSelectedEvent(this));
        }

        /// <summary>
        /// Creates a <see cref="IParameterTypeEditorSelectorViewModel" />
        /// </summary>
        /// <param name="parameterType">the parameter type</param>
        /// <param name="valueArrayIndex">
        /// the index of the inside the value array
        /// </param>
        /// <param name="scale">The <see cref="MeasurementScale" /></param>
        /// <param name="onParameterUpdate">The <see cref="EventCallback{TValue}" /> to call on <see cref="IValueSet" /> update</param>
        /// <returns>the <see cref="IParameterTypeEditorSelectorViewModel" /></returns>
        public IParameterTypeEditorSelectorViewModel CreateParameterTypeEditorSelectorViewModel(ParameterType parameterType, int valueArrayIndex,
            MeasurementScale scale, EventCallback<(IValueSet, int)> onParameterUpdate)
        {
            var parameterTypeEditorSelectorViewModel = new ParameterTypeEditorSelectorViewModel(parameterType, this.ValueSet, this.IsReadOnly, this.messageBus, valueArrayIndex,
                this.CurrentParameterSwitchKind)
            {
                ParameterValueChanged = onParameterUpdate,
                Scale = scale
            };

            return parameterTypeEditorSelectorViewModel;
        }
    }
}
