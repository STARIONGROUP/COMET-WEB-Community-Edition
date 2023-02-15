// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IOrientationViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Enumerations;
    using COMETwebapp.Model;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View Model for that provide information related to <see cref="Orientation"/>
    /// </summary>
    public interface IOrientationViewModel
    {
        /// <summary>
        /// Gets or sets the angle format.
        /// </summary>
        AngleFormat AngleFormat { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Orientation" />
        /// </summary>
        Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the selected parameter used for the details
        /// </summary>
        ParameterBase SelectedParameter { get; set; }

        /// <summary>
        /// Gets or sets the current value set
        /// </summary>
        IValueSet CurrentValueSet { get; set; }

        /// <summary>
        /// Event callback for when a value of the <see cref="SelectedParameter" /> has changed
        /// </summary>
        EventCallback<Dictionary<ParameterBase, IValueSet>> OnParameterValueChanged { get; set; }

        /// <summary>
        /// Gets all the possible <see cref="AngleFormat" />
        /// </summary>
        public IEnumerable<AngleFormat> AngleFormats { get; }

        /// <summary>
        /// Event for when the euler angles changed
        /// </summary>
        /// <param name="sender">the sender of the event. Rx,Ry or Ry</param>
        /// <param name="value">the value of the changed property</param>
        /// <returns>A <see cref="Task" /></returns>
        Task OnEulerAnglesChanged(string sender, string value);

        /// <summary>
        /// Event for when the euler angles changed
        /// </summary>
        /// <param name="sender">the sender of the event. Rx,Ry or Ry</param>
        /// <param name="e">the args of the event</param>
        /// <returns>A <see cref="Task" /></returns>
        Task OnEulerAnglesChanged(string sender, ChangeEventArgs e);

        /// <summary>
        /// Event for when the matrix values changed
        /// </summary>
        /// <param name="index">the index of the matrix changed</param>
        /// <param name="value">the new value for that index</param>
        /// <returns>A <see cref="Task" /></returns>
        Task OnMatrixValuesChanged(int index, string value);

        /// <summary>
        /// Event for when the matrix values changed
        /// </summary>
        Task OnMatrixValuesChanged(int index, ChangeEventArgs e);

        /// <summary>
        /// Event for when the angle format has changed
        /// </summary>
        /// <param name="angle">The new format for the angle</param>
        /// <returns>A <see cref="Task" /></returns>
        void OnAngleFormatChanged(AngleFormat angle);
    }
}
