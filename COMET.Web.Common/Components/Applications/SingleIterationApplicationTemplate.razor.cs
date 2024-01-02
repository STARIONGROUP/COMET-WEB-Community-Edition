// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationTemplate.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMET.Web.Common.Components.Applications
{
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Utilities;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Shared component that will englobe all applications where only one <see cref="Iteration" /> needs to be selected
    /// </summary>
    public partial class SingleIterationApplicationTemplate
    {
        /// <summary>
        /// The <see cref="Guid" /> of selected <see cref="Iteration" />
        /// </summary>
        [Parameter]
        public Guid IterationId { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            this.UpdateProperties();
            base.OnInitialized();

            this.Disposables.Add(CDPMessageBus.Current.Listen<DomainChangedEvent>()
                .Subscribe(_ => this.InvokeAsync(this.SetCorrectUrl)));
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            this.UpdateProperties();
        }

        /// <summary>
        /// Update properties of the viewmodel based on provided parameters
        /// </summary>
        private void UpdateProperties()
        {
            if (this.IterationId == Guid.Empty)
            {
                switch (this.ViewModel.SessionService.OpenIterations.Count)
                {
                    case 1:
                        this.ViewModel.OnThingSelect(this.ViewModel.SessionService.OpenIterations.Items.First());
                        break;
                    case > 1:
                        this.ViewModel.AskToSelectThing();
                        break;
                }
            }
            else if (this.IterationId != Guid.Empty && this.ViewModel.SelectedThing == null)
            {
                this.ViewModel.OnThingSelect(this.ViewModel.SessionService.OpenIterations.Items.FirstOrDefault(x => x.Iid == this.IterationId));
            }

            this.IterationId = this.ViewModel.SelectedThing?.Iid ?? Guid.Empty;
        }

        /// <summary>
        /// Set URL parameters based on the current context
        /// </summary>
        /// <param name="currentOptions">A <see cref="Dictionary{TKey,TValue}" /> of URL parameters</param>
        protected override void SetUrlParameters(Dictionary<string, string> currentOptions)
        {
            base.SetUrlParameters(currentOptions);

            currentOptions[QueryKeys.IterationKey] = this.ViewModel.SelectedThing.Iid.ToShortGuid();
            currentOptions[QueryKeys.ModelKey] = this.ViewModel.SelectedThing.IterationSetup.Container.Iid.ToShortGuid();
            currentOptions[QueryKeys.DomainKey] = this.ViewModel.SessionService.GetDomainOfExpertise(this.ViewModel.SelectedThing).Iid.ToShortGuid();
        }
    }
}
