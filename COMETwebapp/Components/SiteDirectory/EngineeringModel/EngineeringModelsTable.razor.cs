﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelsTable.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.SiteDirectory.EngineeringModel
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    ///     Support class for the <see cref="DomainsOfExpertiseTable"/>
    /// </summary>
    public partial class EngineeringModelsTable : SelectedDataItemBase<EngineeringModelSetup, EngineeringModelRowViewModel>
    {
        /// <summary>
        /// The <see cref="IEngineeringModelsTableViewModel" /> for this component
        /// </summary>
        [Inject]
        public IEngineeringModelsTableViewModel ViewModel { get; set; }

        /// <summary>
        /// The selected component type
        /// </summary>
        private Type SelectedComponent { get; set; }

        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}" /> for the <see cref="DynamicComponent.Parameters" />
        /// </summary>
        private Dictionary<string, object> parameters = new();

        /// <summary>
        /// Gets or sets the value to check if a model has been selected
        /// </summary>
        private bool IsModelSelected { get; set; }

        /// <summary>
        /// A map with all the available components and their names
        /// </summary>
        private readonly Dictionary<Type, string> mapOfComponentsAndNames = new()
        {
            {typeof(ParticipantsTable), "Participants"},
        };

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Initialize(this.ViewModel);
            this.SelectedComponent = this.mapOfComponentsAndNames.First().Key;
        }

        /// <summary>
        /// Method that is invoked when the edit/add thing form is being saved
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnEditThingSaving()
        {
            await this.ViewModel.CreateOrEditEngineeringModel(this.ShouldCreateThing);
        }

        /// <summary>
        /// Method invoked when creating a new thing
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        protected override void CustomizeEditThing(GridCustomizeEditModelEventArgs e)
        {
            base.CustomizeEditThing(e);

            var dataItem = (EngineeringModelRowViewModel)e.DataItem;
            this.ShouldCreateThing = e.IsNew;

            if (dataItem == null)
            {
                this.ViewModel.Thing = new EngineeringModelSetup();
                e.EditModel = this.ViewModel.Thing;
                return;
            }

            e.EditModel = dataItem;
            this.ViewModel.Thing = dataItem.Thing.Clone(true);
        }

        /// <summary>
        /// Metgid invoked everytime a row is selected
        /// </summary>
        /// <param name="row">The selected row</param>
        private void OnSelectedDataItemChanged(EngineeringModelRowViewModel row)
        {
            this.ViewModel.Thing = row.Thing;
            this.IsModelSelected = true;
            this.parameters[nameof(EngineeringModelSetup)] = row.Thing;
        }

        /// <summary>
        /// Method invoked to set the selected component from toolbar
        /// </summary>
        /// <param name="e"></param>
        private void OnDetailsItemClick(ToolbarItemClickEventArgs e)
        {
            this.SelectedComponent = this.mapOfComponentsAndNames.First(x => x.Value == e.ItemName).Key;
        }
    }
}