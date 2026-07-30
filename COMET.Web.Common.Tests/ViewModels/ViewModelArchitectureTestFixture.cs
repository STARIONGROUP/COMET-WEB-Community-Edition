// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewModelArchitectureTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;

    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using DevExpress.Blazor;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture enforcing ViewModel layer architecture purity rules for COMET.Web.Common.
    /// </summary>
    [TestFixture]
    public class ViewModelArchitectureTestFixture
    {
        /// <summary>
        /// Verifies that no type in any ViewModels namespace in the COMET.Web.Common assembly references a type from a UI component library namespace.
        /// </summary>
        [Test]
        public void VerifyViewModelLayerPurity()
        {
            var assembly = typeof(ApplicationBaseViewModel).Assembly;
            var violations = ViewModelArchitectureHelper.GetViewModelPurityViolations(assembly);

            Assert.That(violations, Is.Empty, () => $"ViewModel purity violations found:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
        }

        /// <summary>
        /// Verifies that reintroducing a UI library type into a ViewModel property is caught with a clear failure message naming the type and member.
        /// </summary>
        [Test]
        public void VerifyUiTypeDetection()
        {
            var violations = new List<string>();
            ViewModelArchitectureHelper.InspectType(typeof(MockOffendingViewModel), violations);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(violations, Has.Count.EqualTo(1));
                Assert.That(violations[0], Does.Contain(nameof(MockOffendingViewModel)));
                Assert.That(violations[0], Does.Contain(nameof(MockOffendingViewModel.OffendingProperty)));
                Assert.That(violations[0], Does.Contain(nameof(DxPopup)));
            }
        }

        /// <summary>
        /// Verifies that importing a UI library type in a ViewModel source file is caught, while string literals containing using text are ignored.
        /// </summary>
        [Test]
        public void VerifyInlineUiTypeDetection()
        {
            var violations = new List<string>();
            const string mockContent = "using DevExpress.Blazor;\npublic class SampleViewModel { public void Test() { var a = \"using DevExpress.Blazor;\"; } }";
            ViewModelArchitectureHelper.InspectSourceContent("SampleViewModel.cs", mockContent, violations);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(violations, Has.Count.EqualTo(1));
                Assert.That(violations[0], Does.Contain("SampleViewModel.cs"));
                Assert.That(violations[0], Does.Contain("using DevExpress.Blazor;"));
            }
        }
    }
}
