// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IEditElementUsageViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor.EditElementUsageViewModel
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model contract driving the edit-Element-Usage popup.
    /// </summary>
    public interface IEditElementUsageViewModel
    {
        /// <summary>
        /// Gets the working clone of the <see cref="ElementUsage" /> bound to the form. The clone is
        /// mutated by the form fields; the original is left untouched until the surrounding service
        /// successfully commits the operation.
        /// </summary>
        ElementUsage ElementUsage { get; }

        /// <summary>
        /// Gets the full list of <see cref="Option" />s available in the iteration, used to populate the
        /// option-allocation multi-select in the edit form.
        /// </summary>
        IReadOnlyList<Option> AvailableOptions { get; }

        /// <summary>
        /// Gets or sets the <see cref="Option" />s the element usage is included in (the complement of
        /// <see cref="ElementUsage.ExcludeOption" />). The form binds this to a multi-select; the calling
        /// code derives the excluded options from the complement before writing.
        /// </summary>
        IEnumerable<Option> SelectedOptions { get; set; }

        /// <summary>
        /// Gets the selector view model used by the form to pick the owning
        /// <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" />.
        /// </summary>
        IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Gets or sets the callback invoked by the form when the user submits a valid edit.
        /// </summary>
        EventCallback OnValidSubmit { get; set; }

        /// <summary>
        /// Initializes the view model with a clone of the <see cref="ElementUsage" /> so the form can
        /// mutate state without leaking changes back into the cached domain graph until the commit
        /// succeeds.
        /// </summary>
        /// <param name="elementUsage">The <see cref="ElementUsage" /> the user is editing.</param>
        /// <param name="iteration">The <see cref="Iteration" /> containing the usage's element definition.</param>
        void InitializeViewModel(ElementUsage elementUsage, Iteration iteration);
    }
}
