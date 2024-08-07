﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EngineeringModelBody.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.EngineeringModel
{
    using COMET.Web.Common.Components.Applications;

    using COMETwebapp.Components.EngineeringModel.DomainFileStore;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Core component for the Engineering model body application
    /// </summary>
    public partial class EngineeringModelBody
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}" /> for the <see cref="DynamicComponent.Parameters" />
        /// </summary>
        private readonly Dictionary<string, object> dynamicComponentParameters = [];

        /// <summary>
        /// Gets the selected component type
        /// </summary>
        public Type SelectedComponent { get; private set; }

        /// <summary>
        /// A map with all the available components and their parameters => view model and name
        /// </summary>
        private Dictionary<Type, (object, string)> MapOfComponentsAndParameters => new()
        {
            { typeof(OptionsTable), (this.ViewModel.OptionsTableViewModel, "Options") },
            { typeof(PublicationsTable), (this.ViewModel.PublicationsTableViewModel, "Publications") },
            { typeof(CommonFileStoresTable), (this.ViewModel.CommonFileStoreTableViewModel, "Common File Store") },
            { typeof(DomainFileStoresTable), (this.ViewModel.DomainFileStoreTableViewModel, "Domain File Store") }
        };

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
        }

        /// <summary>
        /// Handles the post-assignement flow of the <see cref="ApplicationBase{TViewModel}.ViewModel" /> property
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            base.OnViewModelAssigned();
            this.SelectComponent(this.MapOfComponentsAndParameters.First().Key);
        }

        /// <summary>
        /// Method invoked to set the selected component from toolbar
        /// </summary>
        /// <param name="e">The <see cref="ToolbarItemClickEventArgs" /></param>
        private void OnItemClick(ToolbarItemClickEventArgs e)
        {
            var selectedComponentAndParametersKvp = this.MapOfComponentsAndParameters.FirstOrDefault(x => x.Value.Item2 == e.ItemName);
            this.SelectComponent(selectedComponentAndParametersKvp.Key);
        }

        /// <summary>
        /// Selects a component to display and loads its parameters
        /// </summary>
        /// <param name="selectedComponent">The selected component</param>
        private void SelectComponent(Type selectedComponent)
        {
            this.SelectedComponent = selectedComponent;
            this.dynamicComponentParameters["ViewModel"] = this.MapOfComponentsAndParameters[this.SelectedComponent].Item1;
        }
    }
}
