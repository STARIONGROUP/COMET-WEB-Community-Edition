// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganizationsTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.SiteDirectory
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Organizations;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    ///     Support class for the <see cref="DomainsOfExpertiseTable"/>
    /// </summary>
    public partial class OrganizationsTable : SelectedDeprecatableDataItemBase<Organization, OrganizationRowViewModel>
    {
        /// <summary>
        /// The <see cref="IOrganizationsTableViewModel" /> for this component
        /// </summary>
        [Inject]
        public IOrganizationsTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Initialize(this.ViewModel);
        }

        /// <summary>
        /// Method invoked every time a row is selected
        /// </summary>
        /// <param name="row">The selected row</param>
        protected override void OnSelectedDataItemChanged(OrganizationRowViewModel row)
        {
            base.OnSelectedDataItemChanged(row);
            this.ShouldCreateThing = false;
            this.ViewModel.Thing = row.Thing.Clone(true);
        }

        /// <summary>
        /// Method invoked before creating a new thing
        /// </summary>
        private void OnAddThingClick()
        {
            this.ShouldCreateThing = true;
            this.IsOnEditMode = true;
            this.ViewModel.Thing = new Organization();
            this.InvokeAsync(this.StateHasChanged);
        }
    }
}
