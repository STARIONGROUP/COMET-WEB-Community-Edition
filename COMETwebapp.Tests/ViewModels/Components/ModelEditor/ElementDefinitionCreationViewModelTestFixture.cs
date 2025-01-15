// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionCreationViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.ModelEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMETwebapp.ViewModels.Components.ModelEditor.ElementDefinitionCreationViewModel;
    using DynamicData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ElementDefinitionCreationViewModelTestFixture
    {
        private ElementDefinitionCreationViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();
            this.messageBus = new CDPMessageBus();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            this.domain = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                ShortName = "SYS"
            };

            var siteDirectory = new SiteDirectory { Domain = { this.domain } };
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            var topElement = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Container"
            };

            var elementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Box"
            };

            var usage1 = new ElementUsage
            {
                Name = "Box1",
                Iid = Guid.NewGuid(),
                ElementDefinition = elementDefinition
            };

            topElement.ContainedElement.Add(usage1);

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                Element = { topElement, elementDefinition }
            };

            var iterations = new SourceList<Iteration>();
            iterations.Add(this.iteration);

            this.sessionService.Setup(x => x.OpenIterations).Returns(iterations);
            this.viewModel = new ElementDefinitionCreationViewModel(this.sessionService.Object, this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.Dispose();
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.OnInitialized();

            Assert.Multiple(() => { Assert.That(this.viewModel.AvailableCategories, Is.Not.Null); });
        }
    }
}
