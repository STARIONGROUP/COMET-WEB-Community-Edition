// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
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

namespace COMETwebapp.Tests.Pages
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.ViewModels.Pages;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class TabsViewModelTestFixture
    {
        private TabsViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IServiceProvider> serviceProvider;

        [SetUp]
        public void Setup()
        {
            this.serviceProvider = new Mock<IServiceProvider>();
            this.sessionService = new Mock<ISessionService>();
            var iterations = new SourceList<Iteration>();
            iterations.Add(new Iteration());
            var engineeringModels = new List<EngineeringModel> { new () };
            this.sessionService.Setup(x => x.OpenIterations).Returns(iterations);
            this.sessionService.Setup(x => x.OpenEngineeringModels).Returns(engineeringModels);
            this.serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(new Mock<IApplicationBaseViewModel>().Object);
            this.viewModel = new TabsViewModel(this.sessionService.Object, this.serviceProvider.Object);
        }

        [Test]
        public void VerifyViewModelProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableApplications, Is.Not.Empty);
                Assert.That(this.viewModel.SelectedApplication, Is.Null);
                Assert.That(this.viewModel.OpenTabs, Has.Count.EqualTo(0));
            });
        }

        [Test]
        public void VerifyOnSelectedApplication()
        {
            this.viewModel.SelectedApplication = this.viewModel.AvailableApplications.FirstOrDefault(x => x.ThingTypeOfInterest == typeof(Iteration));

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.OpenTabs, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.OpenTabs.Items.First().ObjectOfInterest, Is.EqualTo(this.sessionService.Object.OpenIterations.Items.First()));
            });

            this.viewModel.SelectedApplication = this.viewModel.AvailableApplications.FirstOrDefault(x => x.ThingTypeOfInterest == null);
            Assert.That(this.viewModel.OpenTabs, Is.Empty);
        }
    }
}
