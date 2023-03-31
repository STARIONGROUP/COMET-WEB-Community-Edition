﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompoundParameterTypeEditorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Components.ParameterTypeEditors;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Utilities;

    using ReactiveUI;

    /// <summary>
    /// ViewModel used to edit <see cref="CompoundParameterType" />
    /// </summary>
    public class CompoundParameterTypeEditorViewModel : ParameterTypeEditorBaseViewModel<CompoundParameterType>, ICompoundParameterTypeEditorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsOnEditMode" />
        /// </summary>
        private bool isOnEditMode;

        /// <summary>
        /// Creates a new instance of type <see cref="CompoundParameterTypeEditorViewModel" />
        /// </summary>
        /// <param name="parameterType">the parameter type of this view model</param>
        /// <param name="valueSet">the value set asociated to this editor</param>
        /// <param name="isReadOnly">The readonly state</param>
        /// <param name="valueArrayIndex">the index of the value changed in the value sets</param>
        public CompoundParameterTypeEditorViewModel(CompoundParameterType parameterType, IValueSet valueSet, bool isReadOnly, int valueArrayIndex = 0) : base(parameterType, valueSet, isReadOnly, valueArrayIndex)
        {
        }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        public bool IsOnEditMode
        {
            get => this.isOnEditMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnEditMode, value);
        }

        /// <summary>
        /// Event for when the edit button is clicked
        /// </summary>
        public void OnComponentSelected()
        {
            CDPMessageBus.Current.SendMessage(new CompoundComponentSelectedEvent(this));
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Creates a view model for the <see cref="OrientationComponent" />
        /// </summary>
        /// <returns>The <see cref="IOrientationViewModel" /></returns>
        public IOrientationViewModel CreateOrientationViewModel()
        {
            return new OrientationViewModel(this.ValueSet, this.ParameterValueChanged);
        }

        /// <summary>
        /// Creates a view model for the corresponding editor
        /// </summary>
        /// <param name="parameterType">the parameter type</param>
        /// <param name="valueArrayIndex">
        /// the index of the
        /// <see cref="CompoundParameterType" /> in the <see cref="ParameterTypeComponent" />
        /// </param>
        /// <returns>the view model</returns>
        public IParameterTypeEditorSelectorViewModel CreateParameterTypeEditorSelectorViewModel(ParameterType parameterType, int valueArrayIndex)
        {
            var parameterTypeEditorSelectorViewModel = new ParameterTypeEditorSelectorViewModel(parameterType, this.ValueSet, this.IsReadOnly, valueArrayIndex)
            {
                ParameterValueChanged = this.ParameterValueChanged
            };

            return parameterTypeEditorSelectorViewModel;
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public override async Task OnParameterValueChanged(object value)
        {
            if (this.ValueSet is ParameterValueSetBase parameterValueSetBase && value is CompoundParameterTypeValueChangedEventArgs args)
            {
                var modifiedValueArray = new ValueArray<string>(this.ValueArray)
                {
                    [args.Index] = args.Value
                };

                await this.UpdateValueSet(parameterValueSetBase, modifiedValueArray);
            }
        }
    }
}