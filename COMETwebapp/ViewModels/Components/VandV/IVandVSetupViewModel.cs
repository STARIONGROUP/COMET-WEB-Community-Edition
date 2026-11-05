// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IVandVSetupViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Utilities.VandV;

    using FluentResults;

    /// <summary>
    /// View model of the V&amp;V set-up dialog: it checks the model's reference data against the
    /// <see cref="VandVManifest" />, lets the user define the stage gates, and seeds what is missing into the model's library.
    /// </summary>
    public interface IVandVSetupViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether the set-up dialog is open.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets a value indicating whether a write is in progress.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Gets a value indicating whether the model has a library to seed into and something in it is missing.
        /// </summary>
        bool NeedsSetup { get; }

        /// <summary>
        /// Gets the model's reference data library the missing things are seeded into, or null when the model has none.
        /// </summary>
        ModelReferenceDataLibrary TargetLibrary { get; }

        /// <summary>
        /// Gets the result of the last check of the model's chain of libraries.
        /// </summary>
        VandVReferenceDataCheck Check { get; }

        /// <summary>
        /// Gets one line per missing thing, for the dialog.
        /// </summary>
        IReadOnlyList<string> MissingItems { get; }

        /// <summary>
        /// Gets a value indicating whether the current user may create everything that is missing in <see cref="TargetLibrary" />.
        /// </summary>
        bool CanSeed { get; }

        /// <summary>
        /// Gets the reason the last set-up attempt failed, or null; cleared when the dialog is opened again.
        /// </summary>
        string LastError { get; }

        /// <summary>
        /// Gets a value indicating whether the stage gate type is missing, so the user defines the stage gates before seeding.
        /// </summary>
        bool NeedsStageGates { get; }

        /// <summary>
        /// Gets the stage gates being defined, in project order.
        /// </summary>
        IReadOnlyList<string> StageGates { get; }

        /// <summary>
        /// Gets the error in <see cref="StageGates" />, or null when they can be seeded.
        /// </summary>
        string StageGateError { get; }

        /// <summary>
        /// Points the view model at the given iteration and re-checks its reference data.
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" />, or null when none is open.</param>
        void SetIteration(Iteration iteration);

        /// <summary>
        /// Re-checks the reference data, resets the stage gates to the defaults and opens the dialog.
        /// </summary>
        void Open();

        /// <summary>
        /// Appends an empty stage gate.
        /// </summary>
        void AddStageGate();

        /// <summary>
        /// Removes the stage gate at the given position.
        /// </summary>
        /// <param name="index">The position.</param>
        void RemoveStageGate(int index);

        /// <summary>
        /// Moves the stage gate at the given position by the given offset, when the target position exists.
        /// </summary>
        /// <param name="index">The position.</param>
        /// <param name="offset">-1 to move it up, 1 to move it down.</param>
        void MoveStageGate(int index, int offset);

        /// <summary>
        /// Renames the stage gate at the given position.
        /// </summary>
        /// <param name="index">The position.</param>
        /// <param name="name">The new name.</param>
        void SetStageGate(int index, string name);

        /// <summary>
        /// Refreshes the session, re-checks the reference data and seeds whatever is missing in one transaction; closes the
        /// dialog on success, records <see cref="LastError" /> on failure, and refuses a second call while one is running.
        /// </summary>
        /// <returns>The <see cref="Result" /> of the write.</returns>
        Task<Result> SeedAsync();
    }
}
