// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IconNameExtensionsTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Extensions
{
    using System;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="IconNameExtensions" /> class.
    /// </summary>
    [TestFixture]
    public class IconNameExtensionsTestFixture
    {
        [Test]
        public void VerifyToCssSuffixConvertsToKebabCase()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(IconName.Add.ToCssSuffix(), Is.EqualTo("add"));
                Assert.That(IconName.AddCircle.ToCssSuffix(), Is.EqualTo("add-circle"));
                Assert.That(IconName.EyeOff.ToCssSuffix(), Is.EqualTo("eye-off"));
                Assert.That(IconName.PieChart.ToCssSuffix(), Is.EqualTo("pie-chart"));
                Assert.That(IconName.UploadCloud.ToCssSuffix(), Is.EqualTo("upload-cloud"));
            }
        }

        [Test]
        public void VerifyGetCssClass()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(IconName.Add.GetCssClass(), Is.EqualTo("comet-icon comet-icon-add"));
                Assert.That(IconName.ChevronDown.GetCssClass(), Is.EqualTo("comet-icon comet-icon-chevron-down"));
                Assert.That(IconName.LogOut.GetCssClass(), Is.EqualTo("comet-icon comet-icon-log-out"));
            }
        }

        [Test]
        public void VerifyEveryIconNameProducesADistinctClass()
        {
            var values = Enum.GetValues<IconName>();
            var classes = new HashSet<string>();

            using (Assert.EnterMultipleScope())
            {
                foreach (var value in values)
                {
                    var cssClass = value.GetCssClass();
                    Assert.That(cssClass, Does.StartWith("comet-icon comet-icon-"));
                    Assert.That(classes.Add(cssClass), Is.True, $"Duplicate CSS class for {value}");
                }
            }
        }

        [Test]
        public void VerifyToFeatherNameMapsEveryIconToaNonEmptyName()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(IconName.Add.ToFeatherName(), Is.EqualTo("plus"));
                Assert.That(IconName.Edit.ToFeatherName(), Is.EqualTo("edit-2"));
                Assert.That(IconName.Branch.ToFeatherName(), Is.EqualTo("git-branch"));

                foreach (var value in Enum.GetValues<IconName>())
                {
                    Assert.That(value.ToFeatherName(), Is.Not.Null.Or.Empty, $"No Feather name for {value}");
                }
            }
        }
    }
}
