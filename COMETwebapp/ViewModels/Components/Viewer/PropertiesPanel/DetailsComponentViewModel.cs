// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DetailsComponentViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View Model that provide details for <see cref="ParameterBase" />
    /// </summary>
    public class DetailsComponentViewModel : ReactiveObject, IDetailsComponentViewModel
    {
        /// <summary>
        /// Creates a new instance of type <see cref="DetailsComponentViewModel" />
        /// </summary>
        /// <param name="isVisible">if the component is visible/></param>
        /// <param name="parameterType">the ParameterType of the selected parameter</param>
        /// <param name="valueSet">the current <see cref="IValueSet" /></param>
        /// <param name="parameterValueSetChanged">
        /// event callback for when a <see cref="IValueSet" /> asociated to a
        /// <see cref="ParameterBase" /> has changed
        /// </param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public DetailsComponentViewModel(bool isVisible, ParameterType parameterType, IValueSet valueSet, EventCallback<(IValueSet, int)> parameterValueSetChanged, ICDPMessageBus messageBus)
        {
            this.IsVisible = isVisible;
            this.ParameterType = parameterType;

            this.ParameterEditorSelector = new ParameterTypeEditorSelectorViewModel(this.ParameterType, valueSet, false, messageBus)
            {
                ParameterValueChanged = parameterValueSetChanged
            };
        }

        /// <summary>
        /// The <see cref="IParameterTypeEditorSelectorViewModel" />
        /// </summary>
        public IParameterTypeEditorSelectorViewModel ParameterEditorSelector { get; }

        /// <summary>
        /// Gets or sets if the component is visible
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType" /> of the selected parameter
        /// </summary>
        public ParameterType ParameterType { get; set; }
    }
}
