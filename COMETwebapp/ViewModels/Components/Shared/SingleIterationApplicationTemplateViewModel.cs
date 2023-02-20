// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationTemplateViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.ViewModels.Components.Shared
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Utilities.DisposableObject;
    using COMETwebapp.ViewModels.Components.Shared.Selectors;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel that will englobe all applications where only one <see cref="Iteration" /> needs to be selected
    /// </summary>
    public class SingleIterationApplicationTemplateViewModel : DisposableObject, ISingleIterationApplicationTemplateViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsOnIterationSelectionMode" />
        /// </summary>
        private bool isOnIterationSelectionMode;

        /// <summary>
        /// Backing field for <see cref="SelectedIteration" />
        /// </summary>
        private Iteration selectedIteration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleIterationApplicationTemplateViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="iterationSelectorViewModel">The <see cref="IIterationSelectorViewModel" /></param>
        public SingleIterationApplicationTemplateViewModel(ISessionService sessionService, IIterationSelectorViewModel iterationSelectorViewModel)
        {
            this.SessionService = sessionService;
            this.IterationSelectorViewModel = iterationSelectorViewModel;
            this.IterationSelectorViewModel.OnSubmit = new EventCallbackFactory().Create<Iteration>(this, this.SelectIteration);
            this.Disposables.Add(this.SessionService.OpenIterations.CountChanged.Subscribe(_ => this.OnOpenIterationCountChanged()));
        }

        /// <summary>
        /// Selects an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The selected <see cref="Iteration" /></param>
        public void SelectIteration(Iteration iteration)
        {
            this.SelectedIteration = iteration;
            this.IsOnIterationSelectionMode = false;
        }

        /// <summary>
        /// The <see cref="Iteration" /> that will be used
        /// </summary>
        public Iteration SelectedIteration
        {
            get => this.selectedIteration;
            set => this.RaiseAndSetIfChanged(ref this.selectedIteration, value);
        }

        /// <summary>
        /// Asks the user to selects the <see cref="Iteration" /> that he wants to works with
        /// </summary>
        public void AskToSelectIteration()
        {
            this.IterationSelectorViewModel.UpdateProperties(this.SessionService.OpenIterations.Items);
            this.IsOnIterationSelectionMode = true;
        }

        /// <summary>
        /// Gets the <see cref="ISessionService" />
        /// </summary>
        public ISessionService SessionService { get; }

        /// <summary>
        /// Gets the <see cref="IIterationSelectorViewModel" />
        /// </summary>
        public IIterationSelectorViewModel IterationSelectorViewModel { get; }

        /// <summary>
        /// Value asserting that the user should select an <see cref="Iteration" />
        /// </summary>
        public bool IsOnIterationSelectionMode
        {
            get => this.isOnIterationSelectionMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnIterationSelectionMode, value);
        }

        /// <summary>
        /// Handles the change of opened <see cref="Iteration" />
        /// </summary>
        private void OnOpenIterationCountChanged()
        {
            if (this.SessionService.OpenIterations.Count is 0 or 1)
            {
                this.SelectedIteration = this.SessionService.OpenIterations.Items.FirstOrDefault();
            }
        }
    }
}
