// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabComponentTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Tabs
{
    using Bunit;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Tabs;

    using Microsoft.AspNetCore.Components.Web;

    using NUnit.Framework;

    [TestFixture]
    public class TabComponentTestFixture
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
        public void VerifyTheCurrentTabIsMarkedAsFocusableAndCurrent()
        {
            var renderer = this.context.Render<TabComponent>(parameters =>
            {
                parameters.Add(p => p.Id, "tab");
                parameters.Add(p => p.Text, "A tab");
                parameters.Add(p => p.Icon, IconName.Close);
                parameters.Add(p => p.OnClick, () => { });
                parameters.Add(p => p.IsCurrent, true);
            });

            var tab = renderer.Find("#tab");

            Assert.Multiple(() =>
            {
                Assert.That(tab.GetAttribute("tabindex"), Is.EqualTo("0"));
                Assert.That(tab.GetAttribute("aria-current"), Is.EqualTo("page"));
            });
        }

        [Test]
        public async Task VerifyTabIsKeyboardOperable()
        {
            var activations = 0;

            var renderer = this.context.Render<TabComponent>(parameters =>
            {
                parameters.Add(p => p.Id, "tab");
                parameters.Add(p => p.Text, "A tab");
                parameters.Add(p => p.Icon, IconName.Close);
                parameters.Add(p => p.OnClick, () => { activations++; });
            });

            var tab = renderer.Find("#tab");

            await renderer.InvokeAsync(() => tab.KeyDownAsync(new KeyboardEventArgs { Key = "Enter" }));
            await renderer.InvokeAsync(() => tab.KeyDownAsync(new KeyboardEventArgs { Key = " " }));
            await renderer.InvokeAsync(() => tab.KeyDownAsync(new KeyboardEventArgs { Key = "a" }));

            Assert.That(activations, Is.EqualTo(2));
        }

        [Test]
        public void VerifyNonInteractiveTabIsNotAFocusStop()
        {
            var renderer = this.context.Render<TabComponent>(parameters =>
            {
                parameters.Add(p => p.Id, "tab");
                parameters.Add(p => p.Text, "A tab");
                parameters.Add(p => p.Icon, IconName.Close);
            });

            var tab = renderer.Find("#tab");

            Assert.Multiple(() =>
            {
                Assert.That(tab.GetAttribute("tabindex"), Is.EqualTo("-1"));
                Assert.That(tab.HasAttribute("aria-current"), Is.False);
            });
        }
    }
}
