// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SideDataItemTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Shared.SideBarEntry;

    using Microsoft.AspNetCore.Components.Web;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class SideDataItemTestFixture
    {
        private TestContext context;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
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

            var renderer = this.context.RenderComponent<SideBarItem>(parameters =>
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

            renderer.SetParametersAndRender(parameters => { parameters.Add(p => p.ChildContent, "childContent"); });

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Markup, Does.Contain("childContent"));
                Assert.That(renderer.Markup, Does.Not.Contain("txt"));
            });
        }
    }
}
