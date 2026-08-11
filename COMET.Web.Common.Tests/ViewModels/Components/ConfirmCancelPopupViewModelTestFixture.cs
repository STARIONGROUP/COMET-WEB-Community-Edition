// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfirmCancelPopupViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.ViewModels.Components
{
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.ViewModels.Components;

    using NUnit.Framework;

#pragma warning disable CS0618

    /// <summary>
    /// Suite of tests for the <see cref="ConfirmCancelPopupViewModel" /> class.
    /// </summary>
    [TestFixture]
    public class ConfirmCancelPopupViewModelTestFixture
    {
        private ConfirmCancelPopupViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            this.viewModel = new ConfirmCancelPopupViewModel();
        }

        [Test]
        public void VerifyProperties()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.CancelStyle, Is.EqualTo(CometButtonStyle.Secondary));
                Assert.That(this.viewModel.ConfirmStyle, Is.EqualTo(CometButtonStyle.Primary));
                Assert.That(this.viewModel.CancelRenderStyle, Is.EqualTo(CometButtonStyle.Secondary));
                Assert.That(this.viewModel.ConfirmRenderStyle, Is.EqualTo(CometButtonStyle.Primary));
                Assert.That(this.viewModel.HeaderText, Is.EqualTo("Please confirm"));
                Assert.That(this.viewModel.IsVisible, Is.False);
                Assert.That(this.viewModel.ShowCloseButton, Is.True);
            }

            this.viewModel.CancelStyle = CometButtonStyle.Danger;
            this.viewModel.ConfirmStyle = CometButtonStyle.Success;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.CancelStyle, Is.EqualTo(CometButtonStyle.Danger));
                Assert.That(this.viewModel.ConfirmStyle, Is.EqualTo(CometButtonStyle.Success));
                Assert.That(this.viewModel.CancelRenderStyle, Is.EqualTo(CometButtonStyle.Danger));
                Assert.That(this.viewModel.ConfirmRenderStyle, Is.EqualTo(CometButtonStyle.Success));
            }

            this.viewModel.CancelRenderStyle = CometButtonStyle.Warning;
            this.viewModel.ConfirmRenderStyle = CometButtonStyle.Info;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.CancelStyle, Is.EqualTo(CometButtonStyle.Warning));
                Assert.That(this.viewModel.ConfirmStyle, Is.EqualTo(CometButtonStyle.Info));
            }
        }
    }
}
