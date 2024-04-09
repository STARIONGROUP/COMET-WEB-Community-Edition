// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EngineeringModelBodyViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.ViewModels.Components.EngineeringModel
{
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.EngineeringModel;
    using COMETwebapp.ViewModels.Components.EngineeringModel.CommonFileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Options;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Publications;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EngineeringModelBodyViewModelTestFixture
    {
        private EngineeringModelBodyViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private CDPMessageBus messageBus;
        private Mock<IOptionsTableViewModel> optionsTableViewModel;
        private Mock<IPublicationsTableViewModel> publicationsTableViewModel;
        private Mock<ICommonFileStoreTableViewModel> commonFileStoreTableViewModel;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.optionsTableViewModel = new Mock<IOptionsTableViewModel>();
            this.publicationsTableViewModel = new Mock<IPublicationsTableViewModel>();
            this.commonFileStoreTableViewModel = new Mock<ICommonFileStoreTableViewModel>();
            this.messageBus = new CDPMessageBus();

            this.viewModel = new EngineeringModelBodyViewModel(this.sessionService.Object, this.messageBus, this.optionsTableViewModel.Object, this.publicationsTableViewModel.Object, 
                this.commonFileStoreTableViewModel.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyThingChanged()
        {
            var newIteration = new Iteration();
            this.viewModel.CurrentThing = newIteration;

            this.optionsTableViewModel.Verify(x => x.SetCurrentIteration(newIteration), Times.Once);
        }
    }
}
