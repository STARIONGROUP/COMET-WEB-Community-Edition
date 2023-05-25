// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OrientationViewModel.cs" company="RHEA System S.A.">
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
    using System.Globalization;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View Model for that provide information related to <see cref="Orientation" />
    /// </summary>
    public class OrientationViewModel : ReactiveObject, IOrientationViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="AngleFormat" />
        /// </summary>
        private AngleFormat angleFormat = AngleFormat.Degrees;

        /// <summary>
        /// Creates a new instance of type <see cref="Orientation" />
        /// </summary>
        /// <param name="valueSet">the current value set that's being changed</param>
        /// <param name="onParameterValueSetChanged">event callback for when a value has changed</param>
        public OrientationViewModel(IValueSet valueSet, EventCallback<(IValueSet, int)> onParameterValueSetChanged)
        {
            this.CurrentValueSet = valueSet ?? throw new ArgumentNullException(nameof(valueSet));
            this.Orientation = valueSet.ParseIValueToOrientation(AngleFormat.Degrees);
            this.ParameterValueChanged = onParameterValueSetChanged;
        }

        /// <summary>
        /// Gets or sets the angle format.
        /// </summary>
        public AngleFormat AngleFormat
        {
            get => this.angleFormat;
            set => this.RaiseAndSetIfChanged(ref this.angleFormat, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Orientation" />
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the current value set
        /// </summary>
        public IValueSet CurrentValueSet { get; set; }

        /// <summary>
        /// Event callback for when a value of the <see cref="IValueSet" /> has changed
        /// </summary>
        public EventCallback<(IValueSet,int)> ParameterValueChanged { get; set; }

        /// <summary>
        /// Gets all the possible <see cref="AngleFormat" />
        /// </summary>
        public IEnumerable<AngleFormat> AngleFormats { get; } = Enum.GetValues(typeof(AngleFormat)).Cast<AngleFormat>();

        /// <summary>
        /// Event for when the euler angles changed
        /// </summary>
        /// <param name="sender">the sender of the event. Rx,Ry or Ry</param>
        /// <param name="value">the value of the changed property</param>
        /// <returns>A <see cref="Task" /></returns>
        public Task OnEulerAnglesChanged(string sender, string value)
        {
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var valueParsed))
            {
                switch (sender)
                {
                    case "Rx":
                        this.Orientation.X = valueParsed;
                        break;
                    case "Ry":
                        this.Orientation.Y = valueParsed;
                        break;
                    case "Rz":
                        this.Orientation.Z = valueParsed;
                        break;
                }
            }

            return this.SendMatrixBack();
        }

        /// <summary>
        /// Event for when the euler angles changed
        /// </summary>
        /// <param name="sender">the sender of the event. Rx,Ry or Ry</param>
        /// <param name="e">the args of the event</param>
        /// <returns>A <see cref="Task" /></returns>
        public Task OnEulerAnglesChanged(string sender, ChangeEventArgs e)
        {
            var valueText = e.Value as string;
            return this.OnEulerAnglesChanged(sender, valueText);
        }

        /// <summary>
        /// Event for when the matrix values changed
        /// </summary>
        /// <param name="index">the index of the matrix changed</param>
        /// <param name="value">the new value for that index</param>
        /// <returns>A <see cref="Task" /></returns>
        public Task OnMatrixValuesChanged(int index, string value)
        {
            var orientationMatrix = this.Orientation.Matrix;

            if (double.TryParse(value, out var valueParsed))
            {
                orientationMatrix[index] = valueParsed;
                this.Orientation = new Orientation(orientationMatrix, this.AngleFormat);
            }

            return this.SendMatrixBack();
        }

        /// <summary>
        /// Event for when the matrix values changed
        /// </summary>
        /// <param name="index">the index of the matrix changed</param>
        /// <param name="e">the args of the events</param>
        /// <returns>A <see cref="Task" /></returns>
        public Task OnMatrixValuesChanged(int index, ChangeEventArgs e)
        {
            var valueText = e.Value as string;
            return this.OnMatrixValuesChanged(index, valueText);
        }

        /// <summary>
        /// Event for when the angle format has changed
        /// </summary>
        /// <param name="angle">the new format for the angle</param>
        /// <returns>A <see cref="Task" /></returns>
        public void OnAngleFormatChanged(AngleFormat angle)
        {
            this.AngleFormat = angle;
            this.Orientation.AngleFormat = angle;
        }

        /// <summary>
        /// Sends the values of the <see cref="Orientation" /> matrix back to the parent components
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task SendMatrixBack()
        {
            var modifiedValueArray = new ValueArray<string>(this.CurrentValueSet.ActualValue);

            for (var i = 0; i < this.Orientation.Matrix.Length; i++)
            {
                modifiedValueArray[i] = this.Orientation.Matrix[i].ToString(CultureInfo.InvariantCulture);
            }

            await this.SendChangesBack(modifiedValueArray);
        }

        /// <summary>
        /// Send the changes back to the parent components
        /// </summary>
        /// <param name="modifiedValueArray">the value array to send back</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task SendChangesBack(ValueArray<string> modifiedValueArray)
        {
            if (this.CurrentValueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var sendingParameterValueSetBase = parameterValueSetBase.Clone(false);
                sendingParameterValueSetBase.Manual = modifiedValueArray;
                sendingParameterValueSetBase.ValueSwitch = ParameterSwitchKind.MANUAL;

                await this.ParameterValueChanged.InvokeAsync((sendingParameterValueSetBase,0));
            }
        }
    }
}
