// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IApplicationPageModel.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;

    /// <summary>
    /// Common contract for an application page object: it can tell whether/when its page has loaded (via a landmark
    /// element that is only present once the page has rendered).
    /// </summary>
    public interface IApplicationPageModel
    {
        /// <summary>
        /// Waits for the page to be loaded.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        Task WaitForLoadedAsync();

        /// <summary>
        /// Gets a value indicating whether the page is currently loaded.
        /// </summary>
        /// <returns><c>true</c> if the page's landmark element is visible.</returns>
        Task<bool> IsLoadedAsync();
    }
}
