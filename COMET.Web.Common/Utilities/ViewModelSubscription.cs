// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewModelSubscription.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMET.Web.Common.Utilities
{
    /// <summary>
    /// Holds one subscription to a view model and replaces it when the view model changes.
    /// </summary>
    /// <remarks>
    /// A component that receives its view model as a parameter can get a new instance on every render of its parent.
    /// Calling <see cref="Update{TViewModel}" /> from <c>OnParametersSet</c> subscribes to a new view model and disposes the
    /// subscription to the previous one, and does nothing when the view model is the same instance.
    /// </remarks>
    public sealed class ViewModelSubscription : IDisposable
    {
        /// <summary>
        /// The view model that <see cref="subscription" /> observes
        /// </summary>
        private object viewModel;

        /// <summary>
        /// The current subscription
        /// </summary>
        private IDisposable subscription;

        /// <summary>
        /// Gets a value indicating whether a subscription is active
        /// </summary>
        public bool IsSubscribed => this.subscription != null;

        /// <summary>
        /// Subscribes to <paramref name="newViewModel" /> unless it is the view model already observed
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model</typeparam>
        /// <param name="newViewModel">The view model to observe; <c>null</c> removes the current subscription</param>
        /// <param name="subscribe">Creates the subscription to the given view model</param>
        public void Update<TViewModel>(TViewModel newViewModel, Func<TViewModel, IDisposable> subscribe) where TViewModel : class
        {
            ArgumentNullException.ThrowIfNull(subscribe);

            if (ReferenceEquals(newViewModel, this.viewModel))
            {
                return;
            }

            this.subscription?.Dispose();
            this.subscription = null;
            this.viewModel = null;

            if (newViewModel == null)
            {
                return;
            }

            // Only record the view model once the subscription exists, so that a subscribe that throws is retried on the
            // next call with the same view model
            this.subscription = subscribe(newViewModel);
            this.viewModel = newViewModel;
        }

        /// <summary>
        /// Disposes the current subscription
        /// </summary>
        public void Dispose()
        {
            this.subscription?.Dispose();
            this.subscription = null;
            this.viewModel = null;
        }
    }
}
