// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Pages
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.EngineeringModel;
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
        private Mock<ICacheService> cacheService;
        private SourceList<Iteration> openIterations;

        [SetUp]
        public void Setup()
        {
            this.serviceProvider = new Mock<IServiceProvider>();
            this.sessionService = new Mock<ISessionService>();
            this.cacheService = new Mock<ICacheService>();
            this.openIterations = new SourceList<Iteration>();
            this.openIterations.Add(new Iteration());

            var engineeringModels = new List<EngineeringModel> { new() };
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.openIterations);
            this.sessionService.Setup(x => x.OpenEngineeringModels).Returns(engineeringModels);
            this.serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(new Mock<IApplicationBaseViewModel>().Object);

            this.viewModel = new TabsViewModel(this.sessionService.Object, this.serviceProvider.Object, this.cacheService.Object);
        }

        [Test]
        public void VerifyOnSelectedApplication()
        {
            Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Null);

            this.viewModel.MainPanel.OpenTabs.Add(new TabbedApplicationInformation(new Mock<IEngineeringModelBodyViewModel>().Object, typeof(EngineeringModelBody), new Iteration()));
            this.viewModel.SelectedApplication = this.viewModel.AvailableApplications.FirstOrDefault(x => x.Url == WebAppConstantValues.EngineeringModelPage);

            Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.Null);
        }

        [Test]
        public void VerifyTabCreation()
        {
            var engineeringModelApplication = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            this.viewModel.CreateNewTab(engineeringModelApplication, Guid.Empty, this.viewModel.MainPanel);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.Null);
                Assert.That(this.viewModel.MainPanel.CurrentTab.ObjectOfInterest, Is.TypeOf<Iteration>());
                Assert.That(this.viewModel.SelectedApplication, Is.Not.Null);
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(1));
            });

            var bookEditorApplication = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.BookEditorPage);
            this.viewModel.CreateNewTab(bookEditorApplication, Guid.Empty, this.viewModel.MainPanel);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.MainPanel.CurrentTab.ObjectOfInterest, Is.TypeOf<EngineeringModel>());
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(2));
            });
        }

        [Test]
        public void VerifyTabRemoval()
        {
            var engineeringModelApplication1 = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            var engineeringModelApplication2 = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            this.viewModel.CreateNewTab(engineeringModelApplication1, Guid.Empty, this.viewModel.MainPanel);
            this.viewModel.CreateNewTab(engineeringModelApplication2, Guid.Empty, this.viewModel.MainPanel);

            var removedTab = this.viewModel.MainPanel.OpenTabs.Items.ElementAt(1);

            Assert.That(this.viewModel.MainPanel.CurrentTab, Is.EqualTo(removedTab));
            this.viewModel.MainPanel.OpenTabs.Remove(removedTab);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.EqualTo(removedTab));
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.Null);
            });

            this.viewModel.CreateNewTab(engineeringModelApplication2, Guid.Empty, this.viewModel.MainPanel);
            this.viewModel.CreateNewTab(engineeringModelApplication2, Guid.Empty, this.viewModel.MainPanel);
            Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(3));

            this.viewModel.MainPanel.OpenTabs.RemoveRange(1, 2);
            Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifyTabsOnSessionChanges()
        {
            var engineeringModelApplication = this.viewModel.AvailableApplications.First(x => x.Url == WebAppConstantValues.EngineeringModelPage);
            var iteration = new Iteration();

            this.viewModel.CreateNewTab(engineeringModelApplication, iteration.Iid, this.viewModel.MainPanel);
            this.openIterations.Add(iteration);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Not.Null);
            });

            this.openIterations.Clear();
            Assert.That(this.viewModel.MainPanel.CurrentTab, Is.Null);
        }

        [Test]
        public void VerifyViewModelProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableApplications, Is.Not.Empty);
                Assert.That(this.viewModel.SelectedApplication, Is.Null);
                Assert.That(this.viewModel.MainPanel.OpenTabs, Has.Count.EqualTo(0));
            });
        }
    }
}
