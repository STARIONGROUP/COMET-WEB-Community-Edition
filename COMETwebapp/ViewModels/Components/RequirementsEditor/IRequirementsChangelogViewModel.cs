// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IRequirementsChangelogViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Interface for the <see cref="RequirementsChangelogViewModel" />, driving the requirements changelog between the
    /// current iteration and a baseline (frozen) iteration.
    /// </summary>
    public interface IRequirementsChangelogViewModel
    {
        /// <summary>
        /// Gets the current <see cref="Iteration" /> the changelog compares its baseline against.
        /// </summary>
        Iteration CurrentIteration { get; }

        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise" /> used to open the baseline iteration.
        /// </summary>
        DomainOfExpertise CurrentDomain { get; }

        /// <summary>
        /// Gets the frozen <see cref="IterationSetup" />s of the current model that are older than the current iteration
        /// and can therefore be selected as a baseline.
        /// </summary>
        IReadOnlyList<IterationSetup> AvailableBaselines { get; }

        /// <summary>
        /// Gets or sets the <see cref="IterationSetup" /> the current iteration is compared against.
        /// </summary>
        IterationSetup SelectedBaseline { get; set; }

        /// <summary>
        /// Gets the <see cref="RequirementChange" />s computed by the last <see cref="CompareAsync" />.
        /// </summary>
        IReadOnlyList<RequirementChange> Changes { get; }

        /// <summary>
        /// Gets a value indicating whether the baseline iteration is being loaded and compared.
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="CompareAsync" /> has completed at least once.
        /// </summary>
        bool HasCompared { get; }

        /// <summary>
        /// Gets the message to show the user when the comparison could not be performed.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets the <see cref="CDP4Common.CommonData.Thing.Iid" /> of the changelog row to scroll into view, set by
        /// <see cref="RequestScrollTo" /> and cleared once the scroll has been performed, or null when no scroll is
        /// pending.
        /// </summary>
        Guid? ScrollToElementId { get; }

        /// <summary>
        /// Sets the current <see cref="Iteration" /> and <see cref="DomainOfExpertise" />, recomputing the available
        /// baselines and resetting any previous comparison.
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" />, or null when none is open</param>
        /// <param name="domain">The current <see cref="DomainOfExpertise" /></param>
        void SetIteration(Iteration iteration, DomainOfExpertise domain);

        /// <summary>
        /// Loads the <see cref="SelectedBaseline" /> iteration and computes the <see cref="Changes" /> against the
        /// <see cref="CurrentIteration" />.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task CompareAsync();

        /// <summary>
        /// Exports the given <paramref name="changes" /> to an Excel workbook and offers it for download.
        /// </summary>
        /// <param name="changes">The <see cref="RequirementChange" />s to export</param>
        /// <returns>A <see cref="Task" /></returns>
        Task ExportAsync(IReadOnlyList<RequirementChange> changes);

        /// <summary>
        /// Requests that the changelog row of the given <paramref name="elementId" /> is scrolled into view, setting
        /// <see cref="ScrollToElementId" />.
        /// </summary>
        /// <param name="elementId">The <see cref="CDP4Common.CommonData.Thing.Iid" /> of the changelog row to scroll to</param>
        void RequestScrollTo(Guid elementId);

        /// <summary>
        /// Clears <see cref="ScrollToElementId" /> once the pending scroll has been performed.
        /// </summary>
        void ClearScrollTarget();
    }
}
