// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DragEventArgsExtensions.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Utilities
{
    using CDP4DalCommon.Protocol.Operations;

    using Microsoft.AspNetCore.Components.Web;

    /// <summary>
    /// Extension methods for the <see cref="DragEventArgs" />
    /// </summary>
    public static class DragEventArgsExtensions
    {
        /// <summary>
        /// Gets the <see cref="OperationKind" /> that the modifier keys held during a drag-and-drop operation ask for.
        /// The mapping mirrors the COMET IME, so that the muscle memory of its users carries over.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DragEventArgs" /> of the drop</param>
        /// <returns>
        /// The requested <see cref="OperationKind" />, or null when no modifier key is held, in which case the copy mode
        /// that the user selected in the copy settings applies
        /// </returns>
        public static OperationKind? GetCopyOperationKind(this DragEventArgs eventArgs)
        {
            return (eventArgs.CtrlKey, eventArgs.ShiftKey) switch
            {
                (true, false) => OperationKind.CopyKeepValuesChangeOwner,
                (false, true) => OperationKind.Copy,
                (true, true) => OperationKind.CopyKeepValues,
                _ => null
            };
        }
    }
}
