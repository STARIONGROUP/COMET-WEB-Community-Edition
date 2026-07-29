// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditRequirementThingViewModel.cs" company="Starion Group S.A.">
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

    using CDP4Dal;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Extensions;
    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Reusable view model that drives the create/edit dialog for a <see cref="Requirement" />, a
    /// <see cref="RequirementsGroup" /> or a <see cref="RequirementsSpecification" />. It holds a working clone (edit) or
    /// a fresh instance (create) of the target so the form can mutate short name, name, owner, categories, definitions and
    /// deprecation without leaking into the cached domain graph until the surrounding commit succeeds.
    /// </summary>
    public class EditRequirementThingViewModel : DisposableObject, IEditRequirementThingViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" /> queried for the available categories.
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The language code currently selected on the Basic tab; it selects which <see cref="Definition" /> (by language)
        /// is shown and edited there.
        /// </summary>
        private string selectedLanguageCode = "en";

        /// <summary>
        /// Backing field for the <see cref="OnValidSubmit" /> property
        /// </summary>
        private EventCallback onValidSubmit;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditRequirementThingViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /> used to look up reference data libraries.</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /> used by the domain selector.</param>
        public EditRequirementThingViewModel(ISessionService sessionService, ICDPMessageBus messageBus)
        {
            this.sessionService = sessionService;

            this.DomainOfExpertiseSelectorViewModel = new DomainOfExpertiseSelectorViewModel(sessionService, messageBus)
            {
                OnSelectedDomainOfExpertiseChange = new EventCallbackFactory().Create<DomainOfExpertise>(this, selectedOwner =>
                {
                    if (this.Thing is IOwnedThing ownedThing)
                    {
                        ownedThing.Owner = selectedOwner;
                    }
                })
            };

            this.Disposables.Add(this.DomainOfExpertiseSelectorViewModel);
        }

        /// <summary>
        /// Gets the working clone (edit) or fresh instance (create) bound to the form.
        /// </summary>
        public Thing Thing { get; private set; }

        /// <summary>
        /// Gets the <see cref="Thing" /> as a <see cref="DefinedThing" />.
        /// </summary>
        public DefinedThing DefinedThing => (DefinedThing)this.Thing;

        /// <summary>
        /// Gets the <see cref="Thing" /> as an <see cref="ICategorizableThing" />.
        /// </summary>
        public ICategorizableThing CategorizableThing => (ICategorizableThing)this.Thing;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Thing" /> can be deprecated.
        /// </summary>
        public bool IsDeprecatable => this.Thing is IDeprecatableThing;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Thing" /> is deprecated.
        /// </summary>
        public bool IsDeprecated
        {
            get => this.Thing is IDeprecatableThing { IsDeprecated: true };
            set
            {
                if (this.Thing is IDeprecatableThing deprecatable)
                {
                    deprecatable.IsDeprecated = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the group selector should be shown.
        /// </summary>
        public bool ShowGroupSelector => this.Thing is Requirement;

        /// <summary>
        /// Gets the <see cref="RequirementsGroup" />s the edited <see cref="Requirement" /> may be placed under.
        /// </summary>
        public IReadOnlyList<RequirementsGroup> AvailableGroups { get; private set; } = [];

        /// <summary>
        /// Gets or sets the <see cref="RequirementsGroup" /> the edited <see cref="Requirement" /> is placed under.
        /// </summary>
        public RequirementsGroup SelectedGroup
        {
            get => (this.Thing as Requirement)?.Group;
            set
            {
                if (this.Thing is Requirement requirement)
                {
                    requirement.Group = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the language selected on the Basic tab. Changing it only switches which <see cref="Definition" />
        /// is shown/edited (by language) — it never renames an existing definition, so two definitions can never share a language.
        /// </summary>
        public string PrimaryDefinitionLanguageCode
        {
            get => this.selectedLanguageCode;
            set => this.selectedLanguageCode = string.IsNullOrWhiteSpace(value) ? this.selectedLanguageCode : value;
        }

        /// <summary>
        /// Gets or sets the content of the <see cref="Definition" /> for the currently selected language. Reading returns the
        /// existing definition's content for that language (or empty); writing upserts — it edits the definition for the
        /// selected language, creating one lazily on first non-empty write so an empty edit adds nothing.
        /// </summary>
        public string PrimaryDefinitionContent
        {
            get => this.GetDefinitionForSelectedLanguage()?.Content ?? string.Empty;
            set
            {
                var definition = this.GetDefinitionForSelectedLanguage();

                if (definition == null)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return;
                    }

                    definition = new Definition { Iid = Guid.NewGuid(), LanguageCode = this.selectedLanguageCode };
                    this.DefinedThing.Definition.Add(definition);
                }

                definition.Content = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="NaturalLanguage" />s defined in the model, offered when choosing a definition language. Held
        /// as a stable list (not a recomputed enumerable) so the language combo keeps its selection across tab switches.
        /// </summary>
        public IReadOnlyList<NaturalLanguage> AvailableLanguages { get; private set; } = [];

        /// <summary>
        /// Gets a value indicating whether the edited <see cref="Thing" /> is a <see cref="Requirement" /> (so the Simple
        /// Parameter Values and Parametric Constraints tabs apply).
        /// </summary>
        public bool IsRequirement => this.Thing is Requirement;

        /// <summary>
        /// Gets the edited <see cref="Thing" /> as a <see cref="Requirement" />, or null when it is not one.
        /// </summary>
        public Requirement RequirementThing => this.Thing as Requirement;

        /// <summary>
        /// Gets the <see cref="ParameterType" />s available to add a simple parameter value, from the open reference data libraries.
        /// </summary>
        public IReadOnlyList<ParameterType> AvailableParameterTypes { get; private set; } = [];

        /// <summary>
        /// Gets the <see cref="Category" />s applicable to the <see cref="Thing" />'s class kind.
        /// </summary>
        public IEnumerable<Category> AvailableCategories { get; private set; } = [];

        /// <summary>
        /// Gets the selector used to pick the owning <see cref="DomainOfExpertise" />.
        /// </summary>
        public IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Gets a value indicating whether the form can be saved. A <see cref="Requirement" /> must carry at least one
        /// non-empty <see cref="Definition" /> (in any language); groups and specifications are always saveable.
        /// </summary>
        public bool CanSave => !this.IsRequirement || this.DefinedThing.Definition.Any(x => !string.IsNullOrWhiteSpace(x.Content));

        /// <summary>
        /// Gets or sets the callback invoked when the user submits a valid edit.
        /// </summary>
        public EventCallback OnValidSubmit
        {
            get => this.onValidSubmit;
            set => this.RaiseAndSetIfChanged(ref this.onValidSubmit, value);
        }

        /// <summary>
        /// Initializes the form for the given <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The working clone (edit) or fresh instance (create) to bind.</param>
        /// <param name="iteration">The original <see cref="Iteration" /> (must be the one open on the session).</param>
        /// <param name="availableGroups">The groups a requirement may be placed under; ignored for groups and specifications.</param>
        public void InitializeViewModel(Thing thing, Iteration iteration, IReadOnlyList<RequirementsGroup> availableGroups)
        {
            ArgumentNullException.ThrowIfNull(thing);
            ArgumentNullException.ThrowIfNull(iteration);

            this.Thing = thing;
            this.AvailableGroups = availableGroups ?? [];
            this.AvailableCategories = this.GetAvailableCategories(thing.ClassKind);
            this.AvailableLanguages = this.sessionService.GetAvailableNaturalLanguages();

            this.selectedLanguageCode = this.DefinedThing.Definition.FirstOrDefault()?.LanguageCode ?? this.GetDirectoryDefaultLanguageCode();

            this.AvailableParameterTypes = this.sessionService.Session.RetrieveSiteDirectory()
                .AvailableReferenceDataLibraries()
                .SelectMany(x => x.ParameterType)
                .DistinctBy(x => x.Iid)
                .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                .ToList();

            var owner = (thing as IOwnedThing)?.Owner;

            this.DomainOfExpertiseSelectorViewModel.CurrentIteration = iteration;
            this.DomainOfExpertiseSelectorViewModel.AvailableDomainsOfExpertise = ((EngineeringModel)iteration.Container).EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
            this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(owner is null, owner);
        }

        /// <summary>
        /// Gets the <see cref="Definition" /> of the <see cref="Thing" /> for the currently selected Basic-tab language, if any.
        /// </summary>
        /// <returns>The matching definition, or null.</returns>
        private Definition GetDefinitionForSelectedLanguage()
        {
            return this.DefinedThing.Definition.FirstOrDefault(x => string.Equals(x.LanguageCode, this.selectedLanguageCode, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the directory-wide default definition language: the first <see cref="NaturalLanguage" /> configured on the
        /// model's <see cref="SiteDirectory" />, falling back to English when the directory defines none.
        /// </summary>
        /// <returns>The default language code.</returns>
        private string GetDirectoryDefaultLanguageCode()
        {
            var directoryLanguage = this.sessionService.GetSiteDirectory().NaturalLanguage.FirstOrDefault()?.LanguageCode;
            return string.IsNullOrWhiteSpace(directoryLanguage) ? "en" : directoryLanguage;
        }

        /// <summary>
        /// Builds the distinct set of categories that permit the given <paramref name="classKind" /> across every reference
        /// data library available on the open session.
        /// </summary>
        /// <param name="classKind">The <see cref="ClassKind" /> of the edited <see cref="Thing" />.</param>
        /// <returns>The flattened, distinct list of permissible categories.</returns>
        private List<Category> GetAvailableCategories(ClassKind classKind)
        {
            var categories = new List<Category>();

            foreach (var referenceDataLibrary in this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries())
            {
                categories.AddRange(referenceDataLibrary.DefinedCategory.Where(category => category.PermissibleClass.Contains(classKind)));
            }

            return categories.Distinct().ToList();
        }
    }
}
