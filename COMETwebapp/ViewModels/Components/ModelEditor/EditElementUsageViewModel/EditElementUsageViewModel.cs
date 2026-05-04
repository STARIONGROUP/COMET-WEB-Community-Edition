// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditElementUsageViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model driving the edit-Element-Usage popup. Holds a working clone of the target
    /// <see cref="ElementUsage" /> so the form can mutate state without leaking changes back into the
    /// cached domain graph until the surrounding commit succeeds.
    /// </summary>
    public class EditElementUsageViewModel : IEditElementUsageViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditElementUsageViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /> used by the domain selector.</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /> used by the domain selector.</param>
        public EditElementUsageViewModel(ISessionService sessionService, ICDPMessageBus messageBus)
        {
            this.DomainOfExpertiseSelectorViewModel = new DomainOfExpertiseSelectorViewModel(sessionService, messageBus)
            {
                OnSelectedDomainOfExpertiseChange = new EventCallbackFactory().Create<DomainOfExpertise>(this, selectedOwner =>
                {
                    if (this.ElementUsage is not null)
                    {
                        this.ElementUsage.Owner = selectedOwner;
                    }
                })
            };
        }

        /// <summary>
        /// Gets the working clone of the <see cref="ElementUsage" /> bound to the form. Set by
        /// <see cref="InitializeViewModel" />.
        /// </summary>
        public ElementUsage ElementUsage { get; private set; }

        /// <summary>
        /// Gets the selector view model used by the form to pick the owning
        /// <see cref="DomainOfExpertise" />.
        /// </summary>
        public IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Gets or sets the callback invoked by the form when the user submits a valid edit. Wired by the
        /// owning <see cref="ModelEditorViewModel" /> to the actual save handler.
        /// </summary>
        public EventCallback OnValidSubmit { get; set; }

        /// <summary>
        /// Initializes the view model with the supplied clone of the <see cref="ElementUsage" /> and
        /// refreshes the owning-domain selector so the form starts from a coherent baseline.
        /// </summary>
        /// <param name="elementUsage">A clone of the <see cref="ElementUsage" /> to edit.</param>
        /// <param name="iteration">The <see cref="Iteration" /> containing the usage's element definition.</param>
        public void InitializeViewModel(ElementUsage elementUsage, Iteration iteration)
        {
            ArgumentNullException.ThrowIfNull(elementUsage);
            ArgumentNullException.ThrowIfNull(iteration);

            this.ElementUsage = elementUsage;

            this.DomainOfExpertiseSelectorViewModel.CurrentIteration = iteration;
            this.DomainOfExpertiseSelectorViewModel.AvailableDomainsOfExpertise = ((EngineeringModel)iteration.Container).EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
            this.DomainOfExpertiseSelectorViewModel.SetSelectedDomainOfExpertiseOrReset(elementUsage.Owner is null, elementUsage.Owner);
        }
    }
}
