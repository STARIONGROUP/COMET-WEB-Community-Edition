// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewModelArchitectureTestFixture.cs" company="Starion Group S.A.">
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
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.ViewModels
{
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.ViewModels.Components.BookEditor;

    using DevExpress.Blazor;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture enforcing ViewModel layer architecture purity rules for COMETwebapp.
    /// </summary>
    [TestFixture]
    public class ViewModelArchitectureTestFixture
    {
        /// <summary>
        /// Verifies that reintroducing a UI library type into a ViewModel is caught with a clear failure message naming the type
        /// and member.
        /// </summary>
        [Test]
        public void VerifyUiTypeDetection()
        {
            var violations = new List<string>();
            ViewModelArchitectureHelper.InspectType(typeof(MockOffendingViewModel), violations);

            Assert.Multiple(() =>
            {
                Assert.That(violations, Has.Count.EqualTo(1));
                Assert.That(violations[0], Does.Contain(nameof(MockOffendingViewModel)));
                Assert.That(violations[0], Does.Contain(nameof(MockOffendingViewModel.OffendingProperty)));
                Assert.That(violations[0], Does.Contain(nameof(DxPopup)));
            });
        }

        /// <summary>
        /// Verifies that no type in any ViewModels namespace in the COMETwebapp assembly references a type from a UI component
        /// library namespace.
        /// </summary>
        [Test]
        public void VerifyViewModelLayerPurity()
        {
            var assembly = typeof(BookEditorBodyViewModel).Assembly;
            var violations = ViewModelArchitectureHelper.GetViewModelPurityViolations(assembly);

            Assert.That(violations, Is.Empty);
        }
    }
}
