// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfirmRemovalPopupTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Common
{
    using Bunit;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Common;

    using NUnit.Framework;

    [TestFixture]
    public class ConfirmRemovalPopupTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<ConfirmRemovalPopup<string>> renderer;
        private readonly List<string> removed = [];

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.removed.Clear();

            this.renderer = this.context.Render<ConfirmRemovalPopup<string>>(parameters => parameters
                .Add(p => p.HeaderText, "Delete thing")
                .Add(p => p.ContentText, "Are you sure?")
                .Add(p => p.OnRemove, item => this.removed.Add(item)));
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyRequestRequiresConfirmationBeforeRemoving()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsVisible, Is.False, "The popup is hidden until a removal is requested.");
                Assert.That(this.renderer.Instance.ViewModel.HeaderText, Is.EqualTo("Delete thing"));
                Assert.That(this.renderer.Instance.ViewModel.ContentText, Is.EqualTo("Are you sure?"));
            });

            await this.renderer.InvokeAsync(() => this.renderer.Instance.Request("item"));

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsVisible, Is.True, "Requesting a removal shows the confirmation.");
                Assert.That(this.removed, Is.Empty, "Nothing is removed before the user confirms.");
            });

            await this.renderer.InvokeAsync(this.renderer.Instance.Cancel);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsVisible, Is.False);
                Assert.That(this.removed, Is.Empty, "Cancelling removes nothing.");
            });

            // a confirm after a cancel must not resurrect the previously requested item
            await this.renderer.InvokeAsync(this.renderer.Instance.Confirm);

            Assert.That(this.removed, Is.Empty, "A confirm without a pending request removes nothing.");

            await this.renderer.InvokeAsync(() => this.renderer.Instance.Request("item"));
            await this.renderer.InvokeAsync(this.renderer.Instance.Confirm);

            Assert.Multiple(() =>
            {
                Assert.That(this.removed, Is.EqualTo(new[] { "item" }), "Confirming hands the requested item to OnRemove.");
                Assert.That(this.renderer.Instance.IsVisible, Is.False, "The popup closes after confirming.");
            });
        }
    }
}
