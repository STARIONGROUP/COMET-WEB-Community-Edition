// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PersonEditFormTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Components.Shared.PersonEdit
{
    using Bunit;

    using COMET.Web.Common.Components.Shared.PersonEdit;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry.PersonEdit;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="PersonEditForm" /> component.
    /// </summary>
    [TestFixture]
    public class PersonEditFormTestFixture
    {
        /// <summary>
        /// The <see cref="BunitContext" /> context.
        /// </summary>
        private BunitContext context;

        /// <summary>
        /// The mocked <see cref="IPersonEditViewModel" />.
        /// </summary>
        private Mock<IPersonEditViewModel> viewModel;

        /// <summary>
        /// The <see cref="ConfirmCancelPopupViewModel" />.
        /// </summary>
        private ConfirmCancelPopupViewModel confirmCancelViewModel;

        /// <summary>
        /// Setup method executed before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.confirmCancelViewModel = new ConfirmCancelPopupViewModel();

            this.viewModel = new Mock<IPersonEditViewModel>();
            this.viewModel.Setup(x => x.EmailAddresses).Returns([]);
            this.viewModel.Setup(x => x.TelephoneNumbers).Returns([]);
            this.viewModel.Setup(x => x.ConfirmCancelViewModel).Returns(this.confirmCancelViewModel);
        }

        /// <summary>
        /// Teardown method executed after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        /// <summary>
        /// Verifies that invoking <see cref="PersonEditForm.OnAskToResetPreferences" /> opens the confirmation popup.
        /// </summary>
        [Test]
        public async Task VerifyOnAskToResetPreferencesShowsConfirmationPopup()
        {
            var component = this.context.Render<PersonEditForm>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object));

            Assert.That(this.confirmCancelViewModel.IsVisible, Is.False);

            await component.InvokeAsync(() => component.Instance.OnAskToResetPreferences());

            Assert.That(this.confirmCancelViewModel.IsVisible, Is.True);
        }
    }
}
