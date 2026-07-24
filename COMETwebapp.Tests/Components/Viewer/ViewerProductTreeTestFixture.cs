// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewerProductTreeTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.Viewer
{
    using Bunit;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for <see cref="ViewerProductTree" />.
    /// </summary>
    [TestFixture]
    public class ViewerProductTreeTestFixture
    {
        private BunitContext context;
        private ViewerProductTreeViewModel viewModel;
        private Mock<ISelectionMediator> selectionMediator;

        /// <summary>
        /// Set up the test run.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.selectionMediator = new Mock<ISelectionMediator>();
            this.viewModel = new ViewerProductTreeViewModel(this.selectionMediator.Object);
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
        /// Verifies that the shared <see cref="COMET.Web.Common.Components.SearchBar" /> and the "View" display-options
        /// cog trigger button are rendered, matching the System Representation tree's layout.
        /// </summary>
        [Test]
        public void VerifySearchBarAndViewMenuButtonAreRendered()
        {
            var component = this.context.Render<ViewerProductTree>(parameters => parameters.Add(p => p.ViewModel, this.viewModel));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(() => component.FindComponent<COMET.Web.Common.Components.SearchBar>(), Throws.Nothing);
                Assert.That(() => component.Find("#viewerTreeViewMenuButton"), Throws.Nothing);
            }
        }
    }
}
