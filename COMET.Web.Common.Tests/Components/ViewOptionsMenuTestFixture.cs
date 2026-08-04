// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewOptionsMenuTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Test.Helpers;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for the <see cref="ViewOptionsMenu" /> shared component.
    /// </summary>
    [TestFixture]
    public class ViewOptionsMenuTestFixture
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

        /// <summary>
        /// Verifies that the trigger button is rendered with the caller-supplied id, the default "View" text and
        /// the shared settings icon.
        /// </summary>
        [Test]
        public void VerifyTriggerButtonIsRendered()
        {
            var renderer = this.context.Render<ViewOptionsMenu>(parameters => parameters
                .Add(p => p.ButtonId, "testViewMenuButton")
                .Add(p => p.ChildContent, "<span id=\"option-marker\">option</span>"));

            var button = renderer.FindComponent<DxButton>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(() => renderer.Find("#testViewMenuButton"), Throws.Nothing);
                Assert.That(button.Instance.Text, Is.EqualTo("View"));
                Assert.That(button.Instance.IconCssClass, Is.EqualTo(IconName.Settings.GetCssClass()));
            }
        }

        /// <summary>
        /// Verifies that clicking the trigger button toggles the dropdown open and closed.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyClickTogglesDropdown()
        {
            var renderer = this.context.Render<ViewOptionsMenu>(parameters => parameters
                .Add(p => p.ButtonId, "testViewMenuButton")
                .Add(p => p.ChildContent, "<span>option</span>"));

            var button = renderer.FindComponent<DxButton>();
            var dropDown = renderer.FindComponent<DxDropDown>();

            Assert.That(dropDown.Instance.IsOpen, Is.False);

            await renderer.InvokeAsync(() => button.Instance.Click.InvokeAsync(new MouseEventArgs()));
            Assert.That(dropDown.Instance.IsOpen, Is.True);

            await renderer.InvokeAsync(() => button.Instance.Click.InvokeAsync(new MouseEventArgs()));
            Assert.That(dropDown.Instance.IsOpen, Is.False);
        }
    }
}
