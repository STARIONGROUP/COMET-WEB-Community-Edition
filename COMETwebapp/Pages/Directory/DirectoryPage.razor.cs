// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DirectoryPage.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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

namespace COMETwebapp.Pages.Directory
{
    using COMETwebapp.Components.Directory;

    using DevExpress.Blazor;

    /// <summary>
    /// Support class for the <see cref="DirectoryPage"/>
    /// </summary>
    public partial class DirectoryPage
    {
        /// <summary>
        /// The selected component type
        /// </summary>
        private Type SelectedComponent { get; set; }

        /// <summary>
        /// A map with all the available components and their names
        /// </summary>
        private readonly Dictionary<Type, string> mapOfComponentsAndNames = new()
        {
            {typeof(DomainsOfExpertiseTable), "Domains"},
        };

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.SelectedComponent = this.mapOfComponentsAndNames.First().Key;
        }

        /// <summary>
        /// Method invoked to set the selected component from toolbar
        /// </summary>
        /// <param name="e"></param>
        private void OnItemClick(ToolbarItemClickEventArgs e)
        {
            this.SelectedComponent = this.mapOfComponentsAndNames.First(x => x.Value == e.ItemName).Key;
        }
    }
}
