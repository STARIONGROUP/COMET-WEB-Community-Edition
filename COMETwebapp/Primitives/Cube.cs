// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cube.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Primitives
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Components.Viewer;

    /// <summary>
    /// Cube primitive type
    /// </summary>
    public class Cube : BasicPrimitive
    {
        /// <summary>
        /// The width of the cube
        /// </summary>
        public double Width { get; private set; }

        /// <summary>
        /// The height of the cube
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// The depth of the cube
        /// </summary>
        public double Depth { get; private set; }

        /// <summary>
        /// Basic type name
        /// </summary>
        public override string Type { get; protected set; } = "Cube";

        /// <summary>
        /// Initializes a new instance of <see cref="Cube"/> class
        /// </summary>
        public Cube()
        {
            this.ParameterActions.Add(SceneProvider.WidthShortName,  (vs) => this.SetWidth(vs));
            this.ParameterActions.Add(SceneProvider.HeightShortName, (vs) => this.SetHeight(vs));
            this.ParameterActions.Add(SceneProvider.LengthShortName, (vs) => this.SetDepth(vs));
        }

        /// <summary>
        /// Sets the width of the <see cref="Cube"/>
        /// </summary>
        /// <param name="newValue">if the value is null the value it's computed from the asociated parameter</param>
        private void SetWidth(IValueSet newValue = null)
        {
            var valueSet = newValue is null ? this.GetValueSet(SceneProvider.WidthShortName) : newValue;

            if (valueSet is not null && double.TryParse(valueSet.ActualValue.First(), out double w))
            {
                this.Width = w;
            }
        }

        /// <summary>
        /// Sets the height of the <see cref="Cube"/>
        /// </summary>
        /// <param name="newValue">if the value is null the value it's computed from the asociated parameter</param>
        private void SetHeight(IValueSet newValue = null)
        {
            var valueSet = newValue is null ? this.GetValueSet(SceneProvider.HeightShortName) : newValue;

            if (valueSet is not null && double.TryParse(valueSet.ActualValue.First(), out double h))
            {
                this.Height = h;
            }
        }

        /// <summary>
        /// Sets the depth of the <see cref="Cube"/>
        /// </summary>
        /// <param name="newValue">if the value is null the value it's computed from the asociated parameter</param>
        private void SetDepth(IValueSet newValue = null)
        {
            var lengthValueSet = newValue is null ? this.GetValueSet(SceneProvider.LengthShortName) : newValue;

            if (lengthValueSet is not null && double.TryParse(lengthValueSet.ActualValue.First(), out double d))
            {
                this.Depth = d;
            }
        }
    }
}
