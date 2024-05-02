// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IOrientationViewModel.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View Model for that provide information related to <see cref="Orientation" />
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
        /// Gets or sets the current value set
        /// </summary>
        IValueSet CurrentValueSet { get; set; }

        /// <summary>
        /// Event callback for when a value of the <see cref="IValueSet" /> has changed
        /// </summary>
        EventCallback<(IValueSet, int)> ParameterValueChanged { get; set; }

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
