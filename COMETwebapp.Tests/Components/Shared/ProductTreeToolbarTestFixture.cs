// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ProductTreeToolbarTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Shared
{
    using Bunit;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Shared;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for <see cref="ProductTreeToolbar" />.
    /// </summary>
    [TestFixture]
    public class ProductTreeToolbarTestFixture
    {
        private BunitContext context;
        private ViewerProductTreeViewModel viewModel;

        /// <summary>
        /// Set up the test run.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.viewModel = new ViewerProductTreeViewModel(new Mock<ISelectionMediator>().Object);
        }

        /// <summary>
        /// Tears down the test run.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        /// <summary>
        /// Verifies that the shared <see cref="COMET.Web.Common.Components.SearchBar" /> and the "View" cog
        /// trigger button, identified by the caller-supplied <see cref="ProductTreeToolbar.ButtonId" />, are rendered.
        /// </summary>
        [Test]
        public void VerifySearchBarAndViewMenuButtonAreRendered()
        {
            var component = this.context.Render<ProductTreeToolbar>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel)
                .Add(p => p.ButtonId, "testTreeViewMenuButton"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(() => component.FindComponent<COMET.Web.Common.Components.SearchBar>(), Throws.Nothing);
                Assert.That(() => component.Find("#testTreeViewMenuButton"), Throws.Nothing);
            }
        }
    }
}
