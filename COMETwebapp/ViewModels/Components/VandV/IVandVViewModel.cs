// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IVandVViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.VandV
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Utilities.VandV;

    /// <summary>
    /// View model of the V&amp;V tab of the Requirements Editor: it builds the V&amp;V snapshot of the open iteration once per
    /// (re)load and feeds the set-up dialog and the register from it.
    /// </summary>
    public interface IVandVViewModel
    {
        /// <summary>
        /// Gets the V&amp;V snapshot of the current iteration.
        /// </summary>
        VandVModel Model { get; }

        /// <summary>
        /// Gets the view model of the set-up dialog.
        /// </summary>
        IVandVSetupViewModel SetupViewModel { get; }

        /// <summary>
        /// Gets the view model of the register.
        /// </summary>
        IVandVRegisterViewModel RegisterViewModel { get; }

        /// <summary>
        /// Gets a value indicating whether the tab shows its empty state: the model is not set up for V&amp;V and holds no
        /// V&amp;V data yet.
        /// </summary>
        bool ShowEmptyState { get; }

        /// <summary>
        /// Gets the message shown when the V&amp;V data of the iteration could not be read, or null.
        /// </summary>
        string LoadError { get; }

        /// <summary>
        /// Rebuilds the snapshot for the given iteration and passes it on. A failure is kept in <see cref="LoadError" /> and never
        /// reaches the caller, so V&amp;V data that cannot be read leaves the other tabs of the editor working.
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" />, or null when none is open.</param>
        void SetIteration(Iteration iteration);
    }
}
