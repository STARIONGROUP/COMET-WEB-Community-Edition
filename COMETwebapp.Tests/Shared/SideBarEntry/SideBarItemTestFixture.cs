// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SideBarItemTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Shared.SideBarEntry
{
    using Bunit;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Shared.SideBarEntry;

    using Microsoft.AspNetCore.Components.Web;

    using NUnit.Framework;


    [TestFixture]
    public class SideBarItemTestFixture
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
        public async Task VerifySideBarItem()
        {
            var wasCliked = false;

            var renderer = this.context.Render<SideBarItem>(parameters =>
            {
                parameters.Add(p => p.Text, "txt");
                parameters.Add(p => p.Id, "id");
                parameters.Add(p => p.OnClick, () => { wasCliked = true; });
                parameters.Add(p => p.DropdownSelector, true);
            });

            Assert.That(renderer.Markup, Does.Contain("txt"));

            var idDiv = renderer.Find("#id");
            await renderer.InvokeAsync(() => idDiv.ClickAsync(new MouseEventArgs()));
            Assert.That(wasCliked, Is.EqualTo(true));

            renderer.Render(parameters => { parameters.Add(p => p.ChildContent, "childContent"); });

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Markup, Does.Contain("childContent"));
                Assert.That(renderer.Markup, Does.Contain("txt"));
            });
        }

        [Test]
        public async Task VerifySideBarItemIsKeyboardOperable()
        {
            var activations = 0;

            var renderer = this.context.Render<SideBarItem>(parameters =>
            {
                parameters.Add(p => p.Text, "txt");
                parameters.Add(p => p.Id, "id");
                parameters.Add(p => p.OnClick, () => { activations++; });
            });

            var item = renderer.Find("#id");

            Assert.Multiple(() =>
            {
                Assert.That(item.GetAttribute("tabindex"), Is.EqualTo("0"));
                Assert.That(item.GetAttribute("role"), Is.EqualTo("button"));
            });

            await renderer.InvokeAsync(() => item.KeyDownAsync(new KeyboardEventArgs { Key = "Enter" }));
            await renderer.InvokeAsync(() => item.KeyDownAsync(new KeyboardEventArgs { Key = " " }));
            await renderer.InvokeAsync(() => item.KeyDownAsync(new KeyboardEventArgs { Key = "a" }));

            Assert.That(activations, Is.EqualTo(2));
        }

        [Test]
        public void VerifySelectedSideBarItemIsMarkedAsCurrent()
        {
            var renderer = this.context.Render<SideBarItem>(parameters =>
            {
                parameters.Add(p => p.Id, "id");
                parameters.Add(p => p.Selected, true);
            });

            Assert.That(renderer.Find("#id").GetAttribute("aria-current"), Is.EqualTo("page"));

            renderer.Render(parameters => { parameters.Add(p => p.Selected, false); });

            Assert.That(renderer.Find("#id").HasAttribute("aria-current"), Is.False);
        }

        [Test]
        public void VerifyDisabledSideBarItemIsNotAFocusStop()
        {
            var renderer = this.context.Render<SideBarItem>(parameters =>
            {
                parameters.Add(p => p.Id, "id");
                parameters.Add(p => p.Enabled, false);
            });

            var item = renderer.Find("#id");

            Assert.Multiple(() =>
            {
                Assert.That(item.GetAttribute("tabindex"), Is.EqualTo("-1"));
                Assert.That(item.GetAttribute("aria-disabled"), Is.EqualTo("true"));
            });
        }

        [Test]
        public void VerifySideBarItemIconDisplay()
        {
            var renderer = this.context.Render<SideBarItem>(parameters => { parameters.Add(p => p.Icon, IconName.Check); });

            var cometIcons = renderer.FindComponents<CometIcon>();
            var cssIcons = renderer.FindAll("#side-bar-item-css-icon");

            Assert.Multiple(() =>
            {
                Assert.That(cometIcons, Has.Count.GreaterThan(0));
                Assert.That(cssIcons, Has.Count.EqualTo(0));
            });

            renderer.Render(parameters =>
            {
                parameters.Add(p => p.IconCssClass, IconName.Check.GetCssClass());
                parameters.Add(p => p.Icon, null);
            });

            cometIcons = renderer.FindComponents<CometIcon>();
            cssIcons = renderer.FindAll("#side-bar-item-css-icon");

            Assert.Multiple(() =>
            {
                Assert.That(cometIcons, Has.Count.EqualTo(0));
                Assert.That(cssIcons, Has.Count.GreaterThan(0));
            });
        }
    }
}
