// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IHaveObjectChangedTracking.cs" company="RHEA System S.A.">
//     Copyright (c) 2024 RHEA System S.A.
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

namespace COMET.Web.Common.Utilities.HaveObjectChangedTracking
{
    using CDP4Common.CommonData;

    using CDP4Dal.Events;

    using COMET.Web.Common.Utilities.DisposableObject;

    /// <summary>
    /// Base interface for any class that needs to track <see cref="ObjectChangedEvent" /> for one or multiple types
    /// </summary>
    public interface IHaveObjectChangedTracking: IDisposableObject
    {
        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}" /> of added <see cref="Thing" />s
        /// </summary>
        IReadOnlyCollection<Thing> GetAddedThings();

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}" /> of deleted <see cref="Thing" />s
        /// </summary>
        IReadOnlyCollection<Thing> GetDeletedThings();

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}" /> of updated <see cref="Thing" />s
        /// </summary>
        IReadOnlyCollection<Thing> GetUpdatedThings();

        /// <summary>
        /// Clears all recorded changes
        /// </summary>
        void ClearRecordedChanges();
    }
}
