// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CometIconStylesTestFixture.cs" company="Starion Group S.A.">
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
    using COMET.Web.Common.Test.Helpers;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CometIconStyles" /> component.
    /// </summary>
    [TestFixture]
    public class CometIconStylesTestFixture
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
        public void VerifyStyleBlockHoldsMaskRulesSourcedFromTheLibrary()
        {
            var markup = this.context.Render<CometIconStyles>().Markup;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(markup, Does.Contain("<style>"));
                Assert.That(markup, Does.Contain(".comet-icon-add{"));
                Assert.That(markup, Does.Contain(".comet-icon-branch{"));
                Assert.That(markup, Does.Contain("mask-image:url(\"data:image/svg+xml,"));
            }
        }
    }
}
