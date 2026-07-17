// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationSetupRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.SiteDirectory.Rows
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for <see cref="IterationSetupRowViewModel" />
    /// </summary>
    [TestFixture]
    public class IterationSetupRowViewModelTestFixture
    {
        /// <summary>
        /// The <see cref="IterationSetup" /> used for testing.
        /// </summary>
        private IterationSetup iterationSetup;

        /// <summary>
        /// Sets up the test environment.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.iterationSetup = new IterationSetup
            {
                IterationNumber = 5
            };
        }

        /// <summary>
        /// Verifies the properties and behaviour of the <see cref="IterationSetupRowViewModel" />.
        /// </summary>
        [Test]
        public void VerifyIterationSetupRowViewModel()
        {
            var rowViewModel = new IterationSetupRowViewModel(this.iterationSetup);

            Assert.Multiple(() =>
            {
                Assert.That(rowViewModel.Name, Is.EqualTo("Iteration 5"));
                Assert.That(rowViewModel.Number, Is.EqualTo("5"));
                Assert.That(rowViewModel.IsDeleted, Is.False);
            });

            rowViewModel.Number = "10";
            Assert.That(rowViewModel.Number, Is.EqualTo("10"));
        }
    }
}
