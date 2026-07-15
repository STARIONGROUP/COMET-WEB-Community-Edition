// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IEditRequirementThingViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Interface for the <see cref="EditRequirementThingViewModel" />, the reusable form that creates or edits a
    /// <see cref="Requirement" />, a <see cref="RequirementsGroup" /> or a <see cref="RequirementsSpecification" />.
    /// </summary>
    public interface IEditRequirementThingViewModel
    {
        /// <summary>
        /// Gets the working clone (edit) or fresh instance (create) bound to the form.
        /// </summary>
        Thing Thing { get; }

        /// <summary>
        /// Gets the <see cref="Thing" /> as a <see cref="DefinedThing" /> so the form can bind its short name, name and definitions.
        /// </summary>
        DefinedThing DefinedThing { get; }

        /// <summary>
        /// Gets the <see cref="Thing" /> as an <see cref="ICategorizableThing" /> so the form can bind its categories.
        /// </summary>
        ICategorizableThing CategorizableThing { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Thing" /> can be deprecated (only <see cref="Requirement" /> and
        /// <see cref="RequirementsSpecification" /> implement <see cref="IDeprecatableThing" />).
        /// </summary>
        bool IsDeprecatable { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Thing" /> is deprecated. No-op when it is not deprecatable.
        /// </summary>
        bool IsDeprecated { get; set; }

        /// <summary>
        /// Gets a value indicating whether the group selector should be shown (only when editing a <see cref="Requirement" />).
        /// </summary>
        bool ShowGroupSelector { get; }

        /// <summary>
        /// Gets the <see cref="RequirementsGroup" />s the edited <see cref="Requirement" /> may be placed under.
        /// </summary>
        IReadOnlyList<RequirementsGroup> AvailableGroups { get; }

        /// <summary>
        /// Gets or sets the <see cref="RequirementsGroup" /> the edited <see cref="Requirement" /> is placed under; null means
        /// directly under the specification. No-op when the <see cref="Thing" /> is not a <see cref="Requirement" />.
        /// </summary>
        RequirementsGroup SelectedGroup { get; set; }

        /// <summary>
        /// Gets the <see cref="NaturalLanguage" />s defined in the model, offered when choosing a definition language.
        /// </summary>
        IReadOnlyList<NaturalLanguage> AvailableLanguages { get; }

        /// <summary>
        /// Gets a value indicating whether the edited <see cref="Thing" /> is a <see cref="Requirement" />.
        /// </summary>
        bool IsRequirement { get; }

        /// <summary>
        /// Gets the edited <see cref="Thing" /> as a <see cref="Requirement" />, or null when it is not one.
        /// </summary>
        Requirement RequirementThing { get; }

        /// <summary>
        /// Gets the <see cref="ParameterType" />s available to add a simple parameter value.
        /// </summary>
        IReadOnlyList<ParameterType> AvailableParameterTypes { get; }

        /// <summary>
        /// Gets or sets the language code of the primary (first) definition shown on the Basic tab.
        /// </summary>
        string PrimaryDefinitionLanguageCode { get; set; }

        /// <summary>
        /// Gets or sets the content of the primary (first) definition shown on the Basic tab.
        /// </summary>
        string PrimaryDefinitionContent { get; set; }

        /// <summary>
        /// Gets the <see cref="Category" />s applicable to the <see cref="Thing" />'s class kind.
        /// </summary>
        IEnumerable<Category> AvailableCategories { get; }

        /// <summary>
        /// Gets the selector used to pick the owning <see cref="DomainOfExpertise" />.
        /// </summary>
        IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Gets or sets the callback invoked when the user submits a valid edit.
        /// </summary>
        EventCallback OnValidSubmit { get; set; }

        /// <summary>
        /// Initializes the form for the given <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The working clone (edit) or fresh instance (create) to bind.</param>
        /// <param name="iteration">The original <see cref="Iteration" /> (must be the one open on the session).</param>
        /// <param name="availableGroups">The groups a requirement may be placed under; ignored for groups and specifications.</param>
        void InitializeViewModel(Thing thing, Iteration iteration, IReadOnlyList<RequirementsGroup> availableGroups);
    }
}
