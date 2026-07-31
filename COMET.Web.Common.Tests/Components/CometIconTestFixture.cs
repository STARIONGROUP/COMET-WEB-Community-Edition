// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CometIconTestFixture.cs" company="Starion Group S.A.">
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
    using COMET.Web.Common.Test.Helpers;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CometIcon" /> component.
    /// </summary>
    [TestFixture]
    public class CometIconTestFixture
    {
        private BunitContext context;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyIconRendersTheMaskClassForTheGlyph()
        {
            var component = this.context.Render<CometIcon>(parameters => parameters.Add(p => p.Icon, IconName.Add));
            var icon = component.Find("span");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(icon.ClassList, Does.Contain("comet-icon"));
                Assert.That(icon.ClassList, Does.Contain("comet-icon-add"));
                Assert.That(icon.GetAttribute("aria-hidden"), Is.EqualTo("true"));
                Assert.That(icon.HasAttribute("style"), Is.False);
                Assert.That(icon.HasAttribute("title"), Is.False);
            }
        }

        [Test]
        public void VerifySizeRendersAnInlineStyleAndCssClassIsAppended()
        {
            var component = this.context.Render<CometIcon>(parameters => parameters
                .Add(p => p.Icon, IconName.ChevronDown)
                .Add(p => p.Size, 22)
                .Add(p => p.CssClass, "extra-class"));

            var icon = component.Find("span");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(icon.ClassList, Does.Contain("comet-icon-chevron-down"));
                Assert.That(icon.ClassList, Does.Contain("extra-class"));
                Assert.That(icon.GetAttribute("style"), Is.EqualTo("width:22px !important;height:22px !important;"));
            }
        }

        [Test]
        public void VerifyTitleRendersTheTitleAttribute()
        {
            var component = this.context.Render<CometIcon>(parameters => parameters
                .Add(p => p.Icon, IconName.Settings)
                .Add(p => p.Title, "Option dependent parameter"));

            Assert.That(component.Find("span").GetAttribute("title"), Is.EqualTo("Option dependent parameter"));
        }
    }
}
