// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleThingApplicationTemplateViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components.Applications
{
    using CDP4Common.CommonData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Base ViewModel that will englobe all applications where only one <typeparamref name="TThing" /> needs to be selected
    /// </summary>
    /// <typeparam name="TThing">Any <see cref="Thing" /></typeparam>
    public abstract class SingleThingApplicationTemplateViewModel<TThing> : ApplicationTemplateViewModel, ISingleThingApplicationTemplateViewModel<TThing> where TThing : Thing
    {
        /// <summary>
        /// Backing field for <see cref="IsOnSelectionMode" />
        /// </summary>
        private bool isOnSelectionMode;

        /// <summary>
        /// Backing field <see cref="SelectedThing" />
        /// </summary>
        private TThing selectedThing;

        /// <summary>
        /// Initializes a new <see cref="SingleThingApplicationTemplateViewModel{TThing}" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="selectorViewModel">The <see cref="IThingSelectorViewModel{TThing}" /></param>
        protected SingleThingApplicationTemplateViewModel(ISessionService sessionService, IThingSelectorViewModel<TThing> selectorViewModel) : base(sessionService)
        {
            this.SelectorViewModel = selectorViewModel;
            this.SelectorViewModel.OnSubmit = new EventCallbackFactory().Create<TThing>(this, this.OnThingSelect);
        }

        /// <summary>
        /// Gets the <see cref="IThingSelectorViewModel{TThing}" />
        /// </summary>
        public IThingSelectorViewModel<TThing> SelectorViewModel { get; }

        /// <summary>
        /// Value asserting that the user should select an <typeparamref name="TThing" />
        /// </summary>
        public bool IsOnSelectionMode
        {
            get => this.isOnSelectionMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnSelectionMode, value);
        }

        /// <summary>
        /// Gets or sets the selected <typeparamref name="TThing" />
        /// </summary>
        public TThing SelectedThing
        {
            get => this.selectedThing;
            set => this.RaiseAndSetIfChanged(ref this.selectedThing, value);
        }

        /// <summary>
        /// Asks the user to selects the <typeparamref name="TThing" /> that he wants to works with
        /// </summary>
        public void AskToSelectThing()
        {
            this.UpdateProperties();
            this.IsOnSelectionMode = true;
        }

        /// <summary>
        /// Selects a <typeparamref name="TThing" />
        /// </summary>
        /// <param name="thing">The newly selected <typeparamref name="TThing" /></param>
        public virtual void OnThingSelect(TThing thing)
        {
            this.SelectedThing = thing;
            this.IsOnSelectionMode = false;
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        protected abstract void UpdateProperties();
    }
}
