// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IEditElementDefinitionViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor.EditElementDefinitionViewModel
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model contract driving the edit-Element-Definition popup.
    /// </summary>
    public interface IEditElementDefinitionViewModel
    {
        /// <summary>
        /// Gets the working clone of the <see cref="ElementDefinition" /> bound to the form. The clone is
        /// mutated by the form fields (and by <see cref="COMETwebapp.Components.Common.DefinitionsTable" />
        /// via its <see cref="DefinedThing.Definition" /> collection); the original is left untouched until
        /// the surrounding service successfully commits the operation.
        /// </summary>
        ElementDefinition ElementDefinition { get; }

        /// <summary>
        /// Gets the original <see cref="Iteration" /> containing <see cref="ElementDefinition" />. Held
        /// uncloned so the domain-of-expertise selector can resolve the iteration through the open
        /// session. The iteration is cloned at submit time when an <see cref="Iteration.TopElement" />
        /// change must be persisted.
        /// </summary>
        Iteration Iteration { get; }

        /// <summary>
        /// Gets or sets the categories selected on the form.
        /// </summary>
        IEnumerable<Category> SelectedCategories { get; set; }

        /// <summary>
        /// Gets the categories permitted on an <see cref="ElementDefinition" /> across all open
        /// reference data libraries.
        /// </summary>
        IEnumerable<Category> AvailableCategories { get; }

        /// <summary>
        /// Gets the <see cref="NaturalLanguage" />s available for selection in definition fields.
        /// </summary>
        IEnumerable<NaturalLanguage> AvailableLanguages { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the edited <see cref="ElementDefinition" /> should be
        /// promoted to <see cref="Iteration.TopElement" /> on save.
        /// </summary>
        bool IsTopElement { get; set; }

        /// <summary>
        /// Gets the selector view model used by the form to pick the owning
        /// <see cref="DomainOfExpertise" />.
        /// </summary>
        IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Gets or sets the callback invoked by the form when the user submits a valid edit.
        /// </summary>
        EventCallback OnValidSubmit { get; set; }

        /// <summary>
        /// Initializes the view model with deep clones of the <see cref="ElementDefinition" /> and its
        /// containing <see cref="Iteration" /> so the form can mutate state without leaking changes back
        /// into the cached domain graph until the commit succeeds.
        /// </summary>
        /// <param name="elementDefinition">The <see cref="ElementDefinition" /> the user is editing.</param>
        /// <param name="iteration">The <see cref="Iteration" /> containing the element definition.</param>
        void InitializeViewModel(ElementDefinition elementDefinition, Iteration iteration);
    }
}
