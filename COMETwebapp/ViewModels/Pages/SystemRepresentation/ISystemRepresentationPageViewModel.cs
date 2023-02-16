// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISystemRepresentationPageViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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
namespace COMETwebapp.ViewModels.Pages.SystemRepresentation
{
    using COMETwebapp.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;

    /// <summary>
    ///     Interface definition for <see cref="SystemRepresentationPageViewModel" />
    /// </summary>
    public interface ISystemRepresentationPageViewModel
    {
        /// <summary>
        ///     The <see cref="ISystemTreeViewModel" /> for the <see cref="SystemTree" /> component
        /// </summary>
        ISystemTreeViewModel SystemTreeViewModel { get; }

        /// <summary>
        ///     The <see cref="IElementDefinitionDetailsViewModel" /> for the <see cref="ElementDefinitionDetails" /> component
        /// </summary>
        IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; }

        /// <summary>
        /// List of the names of <see cref="Option"/> available
        /// </summary>
        List<string> Options { get; set; }

        /// <summary>
        /// List of the names of available domains
        /// </summary>
        List<string> Domains { get; set; }

        /// <summary>
        /// Updates Elements list when a filter for option is selected
        /// </summary>
        /// <param name="option">Name of the selected option</param>
        void OnOptionFilterChange(string? option);

        /// <summary>
        /// Updates Elements list when a filter for domain is selected
        /// </summary>
        /// <param name="domain">Name of the selected domain</param>
        void OnDomainFilterChange(string? domain);
        
        /// <summary>
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        void OnInitializedAsync();
    }
}
