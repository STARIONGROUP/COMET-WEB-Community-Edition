// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IPropertiesComponentViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Components.Viewer.PropertiesPanel;
    using COMETwebapp.Model;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View Model for the <see cref="PropertiesComponent" />
    /// </summary>
    public interface IPropertiesComponentViewModel
    {
        /// <summary>
        /// Injected property to get access to <see cref="ISessionService" />
        /// </summary>
        ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator" />
        /// </summary>
        ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Gets or sets the property used for the Interoperability
        /// </summary>
        IBabylonInterop BabylonInterop { get; set; }

        /// <summary>
        /// The collection of <see cref="ParameterBase" /> and <see cref="IValueSet" /> of the selected <see cref="SceneObject" />
        /// </summary>
        Dictionary<ParameterBase, IValueSet> ParameterValueSetRelations { get; set; }

        /// <summary>
        /// The list of parameters that the selected <see cref="SceneObject" /> uses
        /// </summary>
        List<ParameterBase> ParametersInUse { get; set; }

        /// <summary>
        /// Gets or sets if the parameters have changes
        /// </summary>
        bool ParameterHaveChanges { get; set; }

        /// <summary>
        /// Gets or sets if this component is visible
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterBase" /> to fill the details
        /// </summary>
        ParameterBase SelectedParameter { get; set; }

        /// <summary>
        /// Event callback for when a <see cref="IValueSet" /> asociated to a <see cref="ParameterBase" /> has changed
        /// </summary>
        EventCallback<IValueSet> OnParameterValueSetChanged { get; set; }

        /// <summary>
        /// When the button for submit changes is clicked
        /// </summary>
        void OnSubmit();

        /// <summary>
        /// Gets the current used <see cref="IValueSet" />
        /// </summary>
        /// <returns>the <see cref="IValueSet" /></returns>
        IValueSet GetUsedValueSet();

        /// <summary>
        /// Creates a new <see cref="IDetailsComponentViewModel" />
        /// </summary>
        /// <returns>
        /// a <see cref="IDetailsComponentViewModel" /> based on this <see cref="IPropertiesComponentViewModel" />
        /// </returns>
        IDetailsComponentViewModel CreateDetailsComponentViewModel();
    }
}
