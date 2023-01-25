// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Orientation.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.Viewer.PropertiesPanel
{
    using System.Threading.Tasks;

    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Enumerations;
    using COMETwebapp.Extensions;
    using COMETwebapp.Model;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// The component used to change the orientation of the selected mesh.
    /// </summary>
    public partial class OrientationComponent
    {
        /// <summary>
        /// Gets or sets the orientation parameter type.
        /// </summary>
        [Parameter]
        public CompoundParameterType OrientationParameterType { get; set; }

        /// <summary>
        /// Gets or sets the parent <see cref="DetailsComponent"/>
        /// </summary>
        [Parameter]
        public DetailsComponent DetailsComponent { get; set; }

        /// <summary>
        /// Gets or sets the angle format. 
        /// </summary>
        public string AngleFormat { get; set; } = "Degrees";

        /// <summary>
        /// Gets or sets the orientation matrix computed by the euler angles.
        /// </summary>
        public double[] OrientationMatrix { get; set; } 

        /// <summary>
        /// Gets or sets the rotation around the X axis
        /// </summary>
        public double Rx { get; set; }

        /// <summary>
        /// Gets or sets the rotation around the Y axis
        /// </summary>
        public double Ry { get; set; }

        /// <summary>
        /// Gets or sets the rotation around the Z axis
        /// </summary>
        public double Rz { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="OnAfterRenderAsync(bool)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                var orientation = this.DetailsComponent.GetSelectedParameterValueSet().ParseIValueToOrientation(Enumerations.AngleFormat.Degrees);
                this.OrientationMatrix = orientation.Matrix;

                this.Rx = orientation.X;
                this.Ry = orientation.Y;
                this.Rz = orientation.Z;

                this.StateHasChanged();
            }
        }

        /// <summary>
        /// Event for when the euler angles changed
        /// </summary>
        /// <param name="sender">the sender of the event. Rx,Ry or Ry</param>
        /// <param name="e">the args of the event</param>
        public void OnEulerAnglesChanged(string sender, ChangeEventArgs e)
        {
            var valueText = e.Value as string;

            if(double.TryParse(valueText, out var value))
            {
                switch (sender)
                {
                    case "Rx": this.Rx = value; 
                        break;
                    case "Ry": this.Ry = value; 
                        break;
                    case "Rz": this.Rz = value; 
                        break;
                }
            }

            Enum.TryParse<AngleFormat>(this.AngleFormat, out var angleFormat);
            var orientation = new Orientation(this.Rx, this.Ry, this.Rz) { AngleFormat = angleFormat };
            this.OrientationMatrix = orientation.Matrix;

            for(int i = 0; i< this.OrientationMatrix.Length; i++)
            {
                this.DetailsComponent.ParameterChanged(i, this.OrientationMatrix[i].ToString());
            }

            this.StateHasChanged();
        }

        /// <summary> 
        /// Event for when the matrix values changed 
        /// </summary> 
        private void OnMatrixValuesChanged(int index, ChangeEventArgs e)
        {
            var valueText = e.Value as string;

            if (double.TryParse(valueText, out var value))
            {
                this.OrientationMatrix[index] = value;
                Enum.TryParse<AngleFormat>(this.AngleFormat, out var enumAngleFormat);
                var orientation = new Orientation(this.OrientationMatrix, enumAngleFormat);
                this.Rx = orientation.X;
                this.Ry = orientation.Y;
                this.Rz = orientation.Z;
            }

            for (int i = 0; i < this.OrientationMatrix.Length; i++)
            {
                this.DetailsComponent.ParameterChanged(i, this.OrientationMatrix[i].ToString());
            }

            this.StateHasChanged();
        }

        /// <summary> 
        /// Event for when the angle format has changed 
        /// </summary> 
        /// <param name="angleFormat"></param> 
        private void OnAngleFormatChanged(string angleFormat)
        {
            this.AngleFormat = angleFormat;

            Enum.TryParse<AngleFormat>(this.AngleFormat, out var enumAngleFormat);
            var orientation = this.DetailsComponent.GetSelectedParameterValueSet().ParseIValueToOrientation(enumAngleFormat);
            this.OrientationMatrix = orientation.Matrix;

            this.Rx = orientation.X;
            this.Ry = orientation.Y;
            this.Rz = orientation.Z;
        }
    }
}
