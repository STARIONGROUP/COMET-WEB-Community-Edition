﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicationsTable.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.EngineeringModel
{
    using System.ComponentModel.DataAnnotations;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Publications;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="PublicationsTable"/>
    /// </summary>
    public partial class PublicationsTable : SelectedDataItemBase<Publication, PublicationRowViewModel>
    {
        /// <summary>
        /// The <see cref="IPublicationsTableViewModel" /> for this component
        /// </summary>
        [Parameter, Required]
        public IPublicationsTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets the selected row from the grid selection
        /// </summary>
        public PublicationRowViewModel SelectedRow { get; private set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Initialize(this.ViewModel);

            this.Disposables.Add(this.ViewModel.WhenAnyValue(x => x.IsRefreshing).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method invoked every time a row is selected
        /// </summary>
        /// <param name="row">The selected row</param>
        protected override void OnSelectedDataItemChanged(PublicationRowViewModel row)
        {
            base.OnSelectedDataItemChanged(row);
            this.SelectedRow = row;
        }

        /// <summary>
        /// Method used to publish the selected parameters
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task PublishSelectedParameters()
        {
           await this.ViewModel.CreatePublication();
        }
    }
}
