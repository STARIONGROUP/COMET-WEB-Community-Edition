// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TransformationBase.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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


namespace COMETwebapp.Components.PropertiesPanel
{
    using System.Threading.Tasks;

    using COMETwebapp.Primitives;
    
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component for the transformations in the <see cref="PropertiesPanel.Properties"/>
    /// </summary>
    public class TransformationBase : ComponentBase
    {
        /// <summary>
        /// Backing field for the <see cref="Primitive"/> property
        /// </summary>
        private BasicPrimitive primitive;

        /// <summary>
        /// The translation of <see cref="Primitives.Primitive"/> along the X axis 
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The translation of <see cref="Primitives.Primitive"/> along the Y axis 
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// The translation of <see cref="Primitives.Primitive"/> along the Z axis 
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// The rotation of <see cref="Primitives.Primitive"/> around the X axis 
        /// </summary>
        public double RX { get; set; }

        /// <summary>
        /// The rotation of <see cref="Primitives.Primitive"/> around the Y axis 
        /// </summary>
        public double RY { get; set; }

        /// <summary>
        /// The rotation of <see cref="Primitives.Primitive"/> around the Z axis 
        /// </summary>
        public double RZ { get; set; }

        /// <summary>
        /// Gets/Sets the selected primitive for this component.
        /// </summary>
        [Parameter]
        public BasicPrimitive Primitive
        {
            get => this.primitive;
            set
            {
                this.primitive = value;
                this.InitializeFields();
            }
        }

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
                this.InitializeFields();
            }
        }

        /// <summary>
        /// Initialize the fields with the transformations of <see cref="Primitive"/>
        /// </summary>
        private void InitializeFields()
        {
            this.X = this.Primitive.X;
            this.Y = this.Primitive.Y;
            this.Z = this.Primitive.Z;
            this.RX = this.Primitive.RX;
            this.RY = this.Primitive.RY;
            this.RZ = this.Primitive.RZ;
        }

        /// <summary>
        /// Event for when a value in the textboxes for the position have changed
        /// </summary>
        /// <param name="axisName">Name of the axis of translation</param>
        /// <param name="translation">The translation value</param>
        public void OnPositionChanged(string axisName, object? translation)
        {
            if(translation != null)
            {
                string? Translation = translation as string;

                if (double.TryParse(Translation, out double translationValue))
                {
                    switch (axisName)
                    {
                        case "X": this.X = translationValue; break;
                        case "Y": this.Y = translationValue; break;
                        case "Z": this.Z = translationValue; break;
                    }
                }

                this.Primitive.SetTranslation(this.X, this.Y, this.Z);
            }
        }

        /// <summary>
        /// Event for when a value in the textboxes for the rotation have changed
        /// </summary>
        /// <param name="axisName">Name of the axis of rotation</param>
        /// <param name="rotation">The rotation value</param>
        public void OnRotationsChanged(string axisName, object? rotation)
        {
            if (rotation != null)
            {
                string? Rotation = rotation as string;

                if (double.TryParse(Rotation, out double rotationValue))
                {
                    switch (axisName)
                    {
                        case "RX": this.RX = rotationValue; break;
                        case "RY": this.RY = rotationValue; break;
                        case "RZ": this.RZ = rotationValue; break;
                    }
                }

                this.Primitive.SetRotation(this.RX, this.RY, this.RZ);
            }
        }
    }
}
