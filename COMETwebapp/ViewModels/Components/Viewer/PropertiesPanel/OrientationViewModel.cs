// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OrientationViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel
{
    using System.Globalization;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    
    using COMETwebapp.Enumerations;
    using COMETwebapp.Model;
    
    using Microsoft.AspNetCore.Components;
    
    using ReactiveUI;

    /// <summary>
    /// View Model for the <see cref="COMETwebapp.Components.Viewer.PropertiesPanel.OrientationComponent"/>
    /// </summary>
    public class OrientationViewModel : ReactiveObject, IOrientationViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="AngleFormat"/>
        /// </summary>
        private AngleFormat angleFormat = AngleFormat.Degrees;

        /// <summary>
        /// Gets or sets the angle format. 
        /// </summary>
        public AngleFormat AngleFormat
        {
            get => this.angleFormat;
            set => this.RaiseAndSetIfChanged(ref this.angleFormat, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Orientation"/>
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the selected parameter used for the details
        /// </summary>
        public ParameterBase SelectedParameter { get; set; }

        /// <summary>
        /// Gets or sets the current value set
        /// </summary>
        public IValueSet CurrentValueSet { get; set; }

        /// <summary> 
        /// Event callback for when a value of the <see cref="SelectedParameter"/> has changed 
        /// </summary> 
        public EventCallback<Dictionary<ParameterBase, IValueSet>> OnParameterValueChanged { get; set; }

        /// <summary>
        /// Creates a new instance of type <see cref="Orientation"/>
        /// </summary>
        /// <param name="currentValueSet">the current value set that's being changed</param>
        /// <param name="selectedParameter">the selected parameter for the current value set</param>
        /// <param name="orientation">the orientation of the selected <see cref="SceneObject"/></param>
        /// <param name="onParameterValueSetChanged">event callback for when a value has changed</param>
        public OrientationViewModel(Orientation orientation, IValueSet currentValueSet, ParameterBase selectedParameter, 
            EventCallback<Dictionary<ParameterBase, IValueSet>> onParameterValueSetChanged)
        {
            this.Orientation = orientation;
            this.CurrentValueSet = currentValueSet;
            this.SelectedParameter = selectedParameter;
            this.OnParameterValueChanged = onParameterValueSetChanged;
        }

        /// <summary>
        /// Event for when the euler angles changed
        /// </summary>
        /// <param name="sender">the sender of the event. Rx,Ry or Ry</param>
        /// <param name="e">the args of the event</param>
        public void OnEulerAnglesChanged(string sender, ChangeEventArgs e)
        {
            var valueText = e.Value as string;
            if (double.TryParse(valueText, out var value))
            {
                switch (sender)
                {
                    case "Rx":
                        this.Orientation.X = value;
                        break;
                    case "Ry":
                        this.Orientation.Y = value;
                        break;
                    case "Rz":
                        this.Orientation.Z = value;
                        break;
                }
            }

            this.SendMatrixBack();
        }

        /// <summary> 
        /// Event for when the matrix values changed 
        /// </summary> 
        public void OnMatrixValuesChanged(int index, ChangeEventArgs e)
        {
            var valueText = e.Value as string;
            var orientationMatrix = this.Orientation.Matrix;

            if (double.TryParse(valueText, out var value))
            {
                orientationMatrix[index] = value;
                this.Orientation  = new Orientation(orientationMatrix, this.AngleFormat);
            }

            this.SendMatrixBack();
        }

        /// <summary>
        /// Sends the values of the <see cref="Orientation"/> matrix back to the parent components
        /// </summary>
        private async void SendMatrixBack()
        {
            ValueArray<string> modifiedValueArray = new ValueArray<string>(this.CurrentValueSet.ActualValue);
            
            for (int i = 0; i<this.Orientation.Matrix.Length; i++)
            {
                modifiedValueArray[i] = this.Orientation.Matrix[i].ToString(CultureInfo.InvariantCulture);
            }
            await this.SendChangesBack(modifiedValueArray);
        }

        /// <summary>
        /// Send the changes back to the parent components
        /// </summary>
        /// <param name="index">the index of the value changed</param>
        /// <param name="valueText">the new value for the value set</param>
        /// <returns>an asynchronous operation</returns>
        private async Task SendChangesBack(ValueArray<string> modifiedValueArray)
        {
            if (this.CurrentValueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var sendingParameterValueSetBase = parameterValueSetBase.Clone(false);
                sendingParameterValueSetBase.Manual = modifiedValueArray;
                sendingParameterValueSetBase.ValueSwitch = ParameterSwitchKind.MANUAL;

                var parameterValueSetRelations = new Dictionary<ParameterBase, IValueSet>()
                {
                    {this.SelectedParameter, sendingParameterValueSetBase},
                };
                await this.OnParameterValueChanged.InvokeAsync(parameterValueSetRelations);
            }
        }

        /// <summary> 
        /// Event for when the angle format has changed 
        /// </summary> 
        /// <param name="angleFormat"></param> 
        public void OnAngleFormatChanged(AngleFormat angleFormat)
        {
            this.AngleFormat = angleFormat;
            this.Orientation.AngleFormat = angleFormat;
        }
    }
}
