// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleThingApplicationBaseViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

    using CDP4Dal;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;

    using DynamicData.Binding;

    using ReactiveUI;

    /// <summary>
    /// Base view model for any application that will need only one <typeparamref name="TThing" />
    /// </summary>
    /// <typeparam name="TThing">Any <see cref="Thing" /></typeparam>
    public abstract class SingleThingApplicationBaseViewModel<TThing> : ApplicationBaseViewModel, ISingleThingApplicationBaseViewModel<TThing> where TThing : Thing
    {
        /// <summary>
        /// Backing field for <see cref="CurrentThing" />
        /// </summary>
        private TThing currentThing;

        /// <summary>
        /// Initializes a new <see cref="SingleThingApplicationBaseViewModel{TThing}" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        protected SingleThingApplicationBaseViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.Disposables.Add(this.WhenAnyPropertyChanged(nameof(this.CurrentThing))
                .SubscribeAsync(_ => this.OnThingChanged()));
        }

        /// <summary>
        /// The current <typeparamref name="TThing" />  to work with
        /// </summary>
        public TThing CurrentThing
        {
            get => this.currentThing;
            set => this.RaiseAndSetIfChanged(ref this.currentThing, value);
        }

        /// <summary>
        /// Update this view model properties when the <see cref="CurrentThing" /> has changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected abstract Task OnThingChanged();
    }
}
