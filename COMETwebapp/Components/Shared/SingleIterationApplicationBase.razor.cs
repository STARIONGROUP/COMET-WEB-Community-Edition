// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationBase.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.Shared
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Extensions;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Shared;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.WebUtilities;

    using ReactiveUI;

    /// <summary>
    /// Base component for any application that will need only one <see cref="Iteration" />
    /// </summary>
    /// <typeparam name="TViewModel">An <see cref="ISingleIterationApplicationBaseViewModel" /></typeparam>
    public abstract partial class SingleIterationApplicationBase<TViewModel>
    {
        /// <summary>
        /// The <see cref="TViewModel" />
        /// </summary>
        [Inject]
        public TViewModel ViewModel { get; set; }

        /// <summary>
        /// The <see cref="NavigationManager" />
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// The <see cref="Iteration" />
        /// </summary>
        [CascadingParameter]
        public Iteration CurrentIteration { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.ViewModel);

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.CurrentIteration)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.ViewModel.CurrentIteration = this.CurrentIteration;

            if (!this.ViewModel.HasSetInitialValuesOnce)
            {
                this.InitializeValues(this.NavigationManager.Uri.GetParametersFromUrl());
                this.ViewModel.HasSetInitialValuesOnce = true;
            }
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected abstract void InitializeValues(Dictionary<string, string> parameters);

        /// <summary>
        /// Updates the url of the <see cref="NavigationManager" /> with the
        /// <param name="additionalParameters"></param>
        /// </summary>
        /// <param name="additionalParameters">A <see cref="Dictionary{TKey,TValue}" /> of additional parameters</param>
        /// <param name="pageName">The name of the ucrrent page</param>
        protected void UpdateUrlWithParameters(Dictionary<string, string> additionalParameters, string pageName)
        {
            var currentOptions = this.NavigationManager.Uri.GetParametersFromUrl();
            var requiredOptions = new Dictionary<string, string>();

            if (currentOptions.TryGetValue(QueryKeys.IterationKey, out var iterationValue))
            {
                requiredOptions[QueryKeys.IterationKey] = iterationValue;
            }

            if (currentOptions.TryGetValue(QueryKeys.DomainKey, out var domainValue))
            {
                requiredOptions[QueryKeys.DomainKey] = domainValue;
            }

            if (currentOptions.TryGetValue(QueryKeys.ServerKey, out var serverValue))
            {
                requiredOptions[QueryKeys.ServerKey] = serverValue;
            }

            if (currentOptions.TryGetValue(QueryKeys.ModelKey, out var modelValue))
            {
                requiredOptions[QueryKeys.ModelKey] = modelValue;
            }

            if (requiredOptions.Count != 4)
            {
                return;
            }

            foreach (var additionalParameter in additionalParameters)
            {
                requiredOptions.Add(additionalParameter.Key, additionalParameter.Value);
            }

            this.NavigationManager.NavigateTo(QueryHelpers.AddQueryString($"/{pageName}", requiredOptions));
        }
    }
}
