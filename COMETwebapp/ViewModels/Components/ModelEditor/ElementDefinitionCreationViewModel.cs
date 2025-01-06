// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionCreationViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.ViewModels.Components.ModelEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Components.ModelEditor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model for the <see cref="ElementDefinitionCreation" /> component
    /// </summary>
    public class ElementDefinitionCreationViewModel : IElementDefinitionCreationViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionCreationViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        public ElementDefinitionCreationViewModel(ISessionService sessionService, ICDPMessageBus messageBus)
        {
            this.sessionService = sessionService;

            this.DomainOfExpertiseSelectorViewModel = new DomainOfExpertiseSelectorViewModel(sessionService, messageBus)
            {
                OnSelectedDomainOfExpertiseChange = new EventCallbackFactory().Create<DomainOfExpertise>(this, selectedOwner =>
                {
                    this.ElementDefinition.Owner = selectedOwner;
                })
            };
        }

        /// <summary>
        /// A collection of available <see cref="Category" />s
        /// </summary>
        public IEnumerable<Category> AvailableCategories { get; set; } = new List<Category>();

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        public IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; private set; }

        /// <summary>
        /// Selected <see cref="Category" />
        /// </summary>
        public IEnumerable<Category> SelectedCategories { get; set; } = new List<Category>();

        /// <summary>
        /// An <see cref="EventCallback" /> to invoke on form submit
        /// </summary>
        public EventCallback OnValidSubmit { get; set; }

        /// <summary>
        /// Value indicating if the <see cref="ElementDefinition" /> is top element
        /// </summary>
        public bool IsTopElement { get; set; }

        /// <summary>
        /// The <see cref="ElementDefinition" /> to create or edit
        /// </summary>
        public ElementDefinition ElementDefinition { get; set; } = new();

        /// <summary>
        /// Initializes the current view model
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        public void InitializeViewModel(Iteration iteration)
        {
            ArgumentNullException.ThrowIfNull(iteration);

            this.DomainOfExpertiseSelectorViewModel.CurrentIteration = iteration;
            this.DomainOfExpertiseSelectorViewModel.AvailableDomainsOfExpertise = ((EngineeringModel)iteration.Container).EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        public void OnInitialized()
        {
            this.AvailableCategories = [];

            foreach (var referenceDataLibrary in this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries())
            {
                this.AvailableCategories = this.AvailableCategories.Concat(referenceDataLibrary.DefinedCategory).Where(category => category.PermissibleClass.Contains(ClassKind.ElementDefinition)).ToList();
            }

            this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(true);
        }
    }
}
