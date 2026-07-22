// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServerAdministrationPageModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.IntegrationTests.PageModels
{
    using System.Collections.Generic;

    using Microsoft.Playwright;

    /// <summary>
    /// Page object for the Server Administration (Site Directory) application. Use
    /// <see cref="ToolbarApplicationPageModel.SelectSectionAsync" /> with one of the <see cref="Sections" /> to switch sub-view.
    /// </summary>
    public class ServerAdministrationPageModel : ToolbarApplicationPageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerAdministrationPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public ServerAdministrationPageModel(IPage page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets the names of the sub-view sections shown on the toolbar.
        /// </summary>
        public override IReadOnlyList<string> Sections => ["Models", "Domains", "Organizations", "User Management", "Person Roles", "Participant Roles"];

        /// <summary>
        /// Gets a selector for an element that is only present once this page has rendered.
        /// </summary>
        protected override string Landmark => "#site-directory-toolbar";
    }
}
