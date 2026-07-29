// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditElementDefinitionViewModel.cs" company="Starion Group S.A.">
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

    using CDP4Dal;

    using COMETwebapp.Extensions;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View model driving the edit-Element-Definition popup. Holds a working clone of the target
    /// <see cref="ElementDefinition" /> so the form can mutate state without leaking changes back into
    /// the cached domain graph until the surrounding commit succeeds. The containing
    /// <see cref="Iteration" /> is held uncloned so the domain-of-expertise selector can resolve it
    /// through the open <see cref="ISession" />; it is cloned at submit time when a
    /// <see cref="Iteration.TopElement" /> change must be persisted.
    /// </summary>
    public class EditElementDefinitionViewModel : DisposableObject, IEditElementDefinitionViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" /> queried for the available
        /// <see cref="DefinedThing.Definition" /> categories.
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Backing field for the <see cref="SelectedCategories" /> property
        /// </summary>
        private IEnumerable<Category> selectedCategories = new List<Category>();

        /// <summary>
        /// Backing field for the <see cref="IsTopElement" /> property
        /// </summary>
        private bool isTopElement;

        /// <summary>
        /// Backing field for the <see cref="OnValidSubmit" /> property
        /// </summary>
        private EventCallback onValidSubmit;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditElementDefinitionViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /> used to look up reference data libraries.</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /> used by the domain selector.</param>
        public EditElementDefinitionViewModel(ISessionService sessionService, ICDPMessageBus messageBus)
        {
            this.sessionService = sessionService;

            this.DomainOfExpertiseSelectorViewModel = new DomainOfExpertiseSelectorViewModel(sessionService, messageBus)
            {
                OnSelectedDomainOfExpertiseChange = new EventCallbackFactory().Create<DomainOfExpertise>(this, selectedOwner =>
                {
                    if (this.ElementDefinition is not null)
                    {
                        this.ElementDefinition.Owner = selectedOwner;
                    }
                })
            };

            this.Disposables.Add(this.DomainOfExpertiseSelectorViewModel);
        }

        /// <summary>
        /// Gets the working clone of the <see cref="ElementDefinition" /> bound to the form. Set by
        /// <see cref="InitializeViewModel" />.
        /// </summary>
        public ElementDefinition ElementDefinition { get; private set; }

        /// <summary>
        /// Gets the original <see cref="Iteration" /> containing <see cref="ElementDefinition" />. Held
        /// uncloned so that the domain-of-expertise selector — which queries the session for the active
        /// domain — can resolve the iteration in <see cref="ISession.OpenIterations" />. The iteration is
        /// cloned at submit time when an <see cref="Iteration.TopElement" /> change must be persisted.
        /// </summary>
        public Iteration Iteration { get; private set; }

        /// <summary>
        /// Gets or sets the categories selected by the user on the form.
        /// </summary>
        public IEnumerable<Category> SelectedCategories
        {
            get => this.selectedCategories;
            set => this.RaiseAndSetIfChanged(ref this.selectedCategories, value);
        }

        /// <summary>
        /// Gets the categories permitted on an <see cref="ElementDefinition" /> across all open
        /// reference data libraries. Recomputed on every <see cref="InitializeViewModel" /> call so the
        /// list reflects the currently-open RDLs.
        /// </summary>
        public IEnumerable<Category> AvailableCategories { get; private set; } = new List<Category>();

        /// <summary>
        /// Gets the <see cref="NaturalLanguage" />s available for selection in definition fields.
        /// </summary>
        public IEnumerable<NaturalLanguage> AvailableLanguages { get; private set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether the edited <see cref="ElementDefinition" /> should be
        /// promoted to <see cref="Iteration.TopElement" /> on save. Initialized to the iteration's current
        /// top-element identity.
        /// </summary>
        public bool IsTopElement
        {
            get => this.isTopElement;
            set => this.RaiseAndSetIfChanged(ref this.isTopElement, value);
        }

        /// <summary>
        /// Gets the selector view model used by the form to pick the owning
        /// <see cref="DomainOfExpertise" />.
        /// </summary>
        public IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Gets or sets the callback invoked by the form when the user submits a valid edit. Wired by the
        /// owning <see cref="ModelEditorViewModel" /> to the actual save handler.
        /// </summary>
        public EventCallback OnValidSubmit
        {
            get => this.onValidSubmit;
            set => this.RaiseAndSetIfChanged(ref this.onValidSubmit, value);
        }

        /// <summary>
        /// Initializes the view model with a clone of the <see cref="ElementDefinition" /> to edit and the
        /// original <see cref="Iteration" /> that contains it. The iteration must be the original — the
        /// one registered in <see cref="ISession.OpenIterations" /> — because
        /// <see cref="DomainOfExpertiseSelectorViewModel" /> resolves the active domain through the
        /// session, which only knows about open iterations. The iteration is cloned later, only when the
        /// edit is committed (so an optional <see cref="Iteration.TopElement" /> change can be persisted).
        /// </summary>
        /// <param name="elementDefinition">A clone of the <see cref="ElementDefinition" /> to edit.</param>
        /// <param name="iteration">The original <see cref="Iteration" /> containing the element definition.</param>
        public void InitializeViewModel(ElementDefinition elementDefinition, Iteration iteration)
        {
            ArgumentNullException.ThrowIfNull(elementDefinition);
            ArgumentNullException.ThrowIfNull(iteration);

            this.ElementDefinition = elementDefinition;
            this.Iteration = iteration;
            this.SelectedCategories = elementDefinition.Category.ToList();
            this.IsTopElement = iteration.TopElement?.Iid == elementDefinition.Iid;

            this.DomainOfExpertiseSelectorViewModel.CurrentIteration = iteration;
            this.DomainOfExpertiseSelectorViewModel.AvailableDomainsOfExpertise = ((EngineeringModel)iteration.Container).EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
            this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(elementDefinition.Owner is null, elementDefinition.Owner);

            this.AvailableCategories = this.GetAvailableCategories();
            this.AvailableLanguages = this.sessionService.GetAvailableNaturalLanguages();
        }

        /// <summary>
        /// Builds the union of categories that permit <see cref="ClassKind.ElementDefinition" /> across
        /// every reference data library currently available on the open <see cref="ISession" />.
        /// </summary>
        /// <returns>The flattened, distinct list of permissible categories.</returns>
        private List<Category> GetAvailableCategories()
        {
            var categories = new List<Category>();

            foreach (var referenceDataLibrary in this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries())
            {
                categories.AddRange(referenceDataLibrary.DefinedCategory.Where(category => category.PermissibleClass.Contains(ClassKind.ElementDefinition)));
            }

            return categories.Distinct().ToList();
        }
    }
}
