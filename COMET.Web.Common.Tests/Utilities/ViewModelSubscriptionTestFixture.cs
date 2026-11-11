// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewModelSubscriptionTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Utilities
{
    using COMET.Web.Common.Utilities;

    using NUnit.Framework;

    [TestFixture]
    public class ViewModelSubscriptionTestFixture
    {
        private ViewModelSubscription viewModelSubscription;
        private List<object> subscribedViewModels;
        private List<object> disposedViewModels;

        [SetUp]
        public void SetUp()
        {
            this.viewModelSubscription = new ViewModelSubscription();
            this.subscribedViewModels = [];
            this.disposedViewModels = [];
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModelSubscription.Dispose();
        }

        [Test]
        public void VerifyTheSameViewModelIsSubscribedOnce()
        {
            var viewModel = new object();

            this.viewModelSubscription.Update(viewModel, this.Subscribe);
            this.viewModelSubscription.Update(viewModel, this.Subscribe);
            this.viewModelSubscription.Update(viewModel, this.Subscribe);

            Assert.Multiple(() =>
            {
                Assert.That(this.subscribedViewModels, Is.EqualTo(new[] { viewModel }));
                Assert.That(this.disposedViewModels, Is.Empty);
                Assert.That(this.viewModelSubscription.IsSubscribed, Is.True);
            });
        }

        [Test]
        public void VerifyANewViewModelReplacesThePreviousSubscription()
        {
            var first = new object();
            var second = new object();

            this.viewModelSubscription.Update(first, this.Subscribe);
            this.viewModelSubscription.Update(second, this.Subscribe);

            Assert.Multiple(() =>
            {
                Assert.That(this.subscribedViewModels, Is.EqualTo(new[] { first, second }));
                Assert.That(this.disposedViewModels, Is.EqualTo(new[] { first }));
                Assert.That(this.viewModelSubscription.IsSubscribed, Is.True);
            });
        }

        [Test]
        public void VerifyANullViewModelRemovesTheSubscription()
        {
            var viewModel = new object();

            this.viewModelSubscription.Update<object>(null, this.Subscribe);

            Assert.That(this.subscribedViewModels, Is.Empty);

            this.viewModelSubscription.Update(viewModel, this.Subscribe);
            this.viewModelSubscription.Update<object>(null, this.Subscribe);

            Assert.Multiple(() =>
            {
                Assert.That(this.subscribedViewModels, Is.EqualTo(new[] { viewModel }));
                Assert.That(this.disposedViewModels, Is.EqualTo(new[] { viewModel }));
                Assert.That(this.viewModelSubscription.IsSubscribed, Is.False);
            });

            this.viewModelSubscription.Update(viewModel, this.Subscribe);

            Assert.That(this.subscribedViewModels, Is.EqualTo(new[] { viewModel, viewModel }));
        }

        [Test]
        public void VerifyDisposeRemovesTheSubscription()
        {
            var viewModel = new object();
            this.viewModelSubscription.Update(viewModel, this.Subscribe);

            this.viewModelSubscription.Dispose();
            this.viewModelSubscription.Dispose();

            Assert.Multiple(() =>
            {
                Assert.That(this.disposedViewModels, Is.EqualTo(new[] { viewModel }));
                Assert.That(this.viewModelSubscription.IsSubscribed, Is.False);
            });

            this.viewModelSubscription.Update(viewModel, this.Subscribe);

            Assert.That(this.subscribedViewModels, Is.EqualTo(new[] { viewModel, viewModel }));
        }

        [Test]
        public void VerifyUpdateRequiresASubscribeFunction()
        {
            Assert.That(() => this.viewModelSubscription.Update<object>(new object(), null), Throws.ArgumentNullException);
        }

        private IDisposable Subscribe(object viewModel)
        {
            this.subscribedViewModels.Add(viewModel);
            return new DisposeAction(() => this.disposedViewModels.Add(viewModel));
        }

        private sealed class DisposeAction(Action action) : IDisposable
        {
            public void Dispose()
            {
                action();
            }
        }
    }
}
