// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SearchBarTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
//
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Test.Helpers;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using NUnit.Framework;

    [TestFixture]
    public class SearchBarTestFixture
    {
        private BunitContext context;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifySearchBar()
        {
            var captured = string.Empty;

            var renderer = this.context.Render<SearchBar>(parameters => parameters
                .Add(p => p.Placeholder, "Find things...")
                .Add(p => p.CssClass, "extra-class")
                .Add(p => p.Text, "initial")
                .Add(p => p.TextChanged, EventCallback.Factory.Create<string>(this, value => captured = value)));

            var textBox = renderer.FindComponent<DxTextBox>();

            Assert.Multiple(() =>
            {
                Assert.That(textBox.Instance.NullText, Is.EqualTo("Find things..."));
                Assert.That(textBox.Instance.CssClass, Does.Contain("inline-search-icon"));
                Assert.That(textBox.Instance.CssClass, Does.Contain("extra-class"));
                Assert.That(textBox.Instance.Text, Is.EqualTo("initial"));
            });

            await renderer.InvokeAsync(() => textBox.Instance.TextChanged.InvokeAsync("hello"));

            Assert.That(captured, Is.EqualTo("hello"));
        }
    }
}
