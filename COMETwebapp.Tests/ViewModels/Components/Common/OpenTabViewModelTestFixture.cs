// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenTabViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Tests.ViewModels.Components.Common
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Components.Common.OpenTab;
    using COMETwebapp.ViewModels.Pages;

    using DynamicData;

    using FluentResults;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class OpenTabViewModelTestFixture
    {
        private OpenTabViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IConfigurationService> configurationService;
        private Mock<ITabsViewModel> tabsViewModel;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.configurationService = new Mock<IConfigurationService>();
            this.tabsViewModel = new Mock<ITabsViewModel>();

            this.sessionService.Setup(x => x.ReadEngineeringModels(It.IsAny<IEnumerable<EngineeringModelSetup>>())).Returns(Task.FromResult(new Result()));
            this.sessionService.Setup(x => x.ReadIteration(It.IsAny<IterationSetup>(), It.IsAny<DomainOfExpertise>())).Returns(Task.FromResult(new Result<Iteration>()));
            this.sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());
            this.sessionService.Setup(x => x.OpenEngineeringModels).Returns(new List<EngineeringModel>());

            this.viewModel = new OpenTabViewModel(this.sessionService.Object, this.configurationService.Object, this.tabsViewModel.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public async Task VerifyOpenIterationAndModel()
        {
            var openIterationResult = await this.viewModel.OpenSession();

            Assert.Multiple(() =>
            {
                Assert.That(openIterationResult.IsSuccess, Is.False);
                this.tabsViewModel.VerifySet(x => x.SelectedApplication = this.viewModel.SelectedApplication, Times.Never);
                this.sessionService.Verify(x => x.ReadIteration(It.IsAny<IterationSetup>(), It.IsAny<DomainOfExpertise>()), Times.Never);
            });

            var iterationSetup = new IterationSetup { Iid = Guid.NewGuid() };
            this.viewModel.SelectedApplication = new TabbedApplication();
            this.viewModel.SelectedEngineeringModel = new EngineeringModelSetup { IterationSetup = { iterationSetup } };
            this.viewModel.SelectedIterationSetup = new IterationData(iterationSetup);
            this.viewModel.SelectedDomainOfExpertise = new DomainOfExpertise();
            openIterationResult = await this.viewModel.OpenSession();

            Assert.Multiple(() =>
            {
                Assert.That(openIterationResult.IsSuccess, Is.True);
                this.tabsViewModel.VerifySet(x => x.SelectedApplication = this.viewModel.SelectedApplication, Times.Once);
                this.sessionService.Verify(x => x.ReadIteration(It.IsAny<IterationSetup>(), It.IsAny<DomainOfExpertise>()), Times.Once);
            });

            var newEngineeringModel = new EngineeringModel { EngineeringModelSetup = this.viewModel.SelectedEngineeringModel };
            this.sessionService.Setup(x => x.OpenEngineeringModels).Returns([newEngineeringModel]);

            await this.viewModel.OpenSession();
            this.sessionService.Verify(x => x.SwitchDomain(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>()), Times.Once);
        }
    }
}
