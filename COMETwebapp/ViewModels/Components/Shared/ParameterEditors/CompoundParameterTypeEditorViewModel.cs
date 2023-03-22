// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompoundParameterTypeEditorViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.Types;

    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    /// <summary>
    /// ViewModel used to edit <see cref="CompoundParameterType" />
    /// </summary>
    public class CompoundParameterTypeEditorViewModel : ParameterTypeEditorBaseViewModel<CompoundParameterType>, ICompoundParameterTypeEditorViewModel
    {
        /// <summary>
        /// Creates a new instance of type <see cref="CompoundParameterTypeEditorViewModel" />
        /// </summary>
        /// <param name="parameterType">the parameter type of this view model</param>
        /// <param name="valueSet">the value set asociated to this editor</param>
        /// <param name="isReadOnly">The readonly state</param>
        public CompoundParameterTypeEditorViewModel(CompoundParameterType parameterType, IValueSet valueSet, bool isReadOnly, int compoundIndex = -1) : base(parameterType, valueSet, isReadOnly, compoundIndex)
        {
        }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public override async Task OnParameterValueChanged(object value)
        {
            if (this.ValueSet is ParameterValueSetBase parameterValueSetBase && value is CompoundParameterTypeValueChangedEventArgs args)
            {
                var modifiedValueArray = new ValueArray<string>(this.ValueSet.ActualValue)
                {
                    [args.Index] = args.Value
                };

                await this.UpdateValueSet(parameterValueSetBase, modifiedValueArray);
            }
        }

        /// <summary>
        /// Creates a view model for the <see cref="COMETwebapp.Components.Viewer.PropertiesPanel.OrientationComponent" />
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
        /// <param name="compoundIndex" the index of the <see cref="CompoundParameterType"/> in the <see cref="ParameterTypeComponent"/></param>
        /// <returns>the view model</returns>
        public IParameterTypeEditorSelectorViewModel CreateParameterTypeEditorSelectorViewModel(ParameterType parameterType, int compoundIndex)
        {
            return new ParameterTypeEditorSelectorViewModel(parameterType, this.ValueSet, this.IsReadOnly, compoundIndex);
        }
    }
}
