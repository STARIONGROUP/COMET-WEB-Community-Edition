// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationTemplateViewModel.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.ViewModels.Components.Applications
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    /// <summary>
    /// ViewModel that will englobe all applications where only one <see cref="Iteration" /> needs to be selected
    /// </summary>
    public class SingleIterationApplicationTemplateViewModel : SingleThingApplicationTemplateViewModel<Iteration>, ISingleIterationApplicationTemplateViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleIterationApplicationTemplateViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="iterationSelectorViewModel">The <see cref="IIterationSelectorViewModel" /></param>
        public SingleIterationApplicationTemplateViewModel(ISessionService sessionService, IIterationSelectorViewModel iterationSelectorViewModel) : base(sessionService, iterationSelectorViewModel)
        {
            this.Disposables.Add(this.SessionService.OpenIterations.CountChanged.Subscribe(_ => this.OnOpenIterationCountChanged()));
        }

        /// <summary>
        /// Gets the <see cref="IIterationSelectorViewModel"/>
        /// </summary>
        public IIterationSelectorViewModel IterationSelectorViewModel => this.SelectorViewModel as IIterationSelectorViewModel;

        /// <summary>
        /// The <see cref="IterationData" /> that will be used
        /// </summary>
        public IterationData SelectedIterationData { get; set; }

        /// <summary>
        /// Selects a <see cref="Iteration" />
        /// </summary>
        /// <param name="thing">The newly selected <see cref="Iteration" /></param>
        public override void OnThingSelect(Iteration thing)
        {
            base.OnThingSelect(thing);
            this.SelectedIterationData = thing == null ? null : new IterationData(thing.IterationSetup, true);
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        protected override void UpdateProperties()
        {
            this.SelectorViewModel.UpdateProperties(this.SessionService.OpenIterations.Items);
        }

        /// <summary>
        /// Handles the change of opened <see cref="Iteration" />
        /// </summary>
        private void OnOpenIterationCountChanged()
        {
            if (this.SessionService.OpenIterations.Count is 0 or 1)
            {
                this.SelectedThing = this.SessionService.OpenIterations.Items.FirstOrDefault();
            }
        }
    }
}
