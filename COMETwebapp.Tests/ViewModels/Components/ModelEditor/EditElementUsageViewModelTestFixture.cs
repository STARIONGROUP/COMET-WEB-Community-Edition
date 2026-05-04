// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditElementUsageViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.ModelEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ModelEditor.EditElementUsageViewModel;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EditElementUsageViewModelTestFixture
    {
        private EditElementUsageViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private CDPMessageBus messageBus;
        private DomainOfExpertise domain;
        private DomainOfExpertise otherDomain;
        private Iteration iteration;
        private ElementDefinition referencedDefinition;
        private ElementUsage elementUsage;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();

            var session = new Mock<ISession>();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());

            this.domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.otherDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "PWR", Name = "Power" };

            var rdl = new SiteReferenceDataLibrary { ShortName = "siteRdl", Name = "Site RDL" };
            var siteDirectory = new SiteDirectory { Domain = { this.domain, this.otherDomain }, SiteReferenceDataLibrary = { rdl } };

            var modelSetup = new EngineeringModelSetup
            {
                Name = "ModelName",
                ShortName = "ModelShortName",
                ActiveDomain = { this.domain, this.otherDomain },
                RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = rdl } }
            };

            siteDirectory.Model.Add(modelSetup);
            var iterationSetup = new IterationSetup { IterationNumber = 1 };
            modelSetup.IterationSetup.Add(iterationSetup);

            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            var topElement = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Container", ShortName = "CONT", Owner = this.domain };
            this.referencedDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Box", ShortName = "BOX", Owner = this.domain };

            this.elementUsage = new ElementUsage
            {
                Iid = Guid.NewGuid(),
                Name = "Box1",
                ShortName = "BOX1",
                ElementDefinition = this.referencedDefinition,
                Owner = this.domain
            };

            topElement.ContainedElement.Add(this.elementUsage);

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                Element = { topElement, this.referencedDefinition },
                TopElement = topElement,
                IterationSetup = iterationSetup,
                Container = new EngineeringModel { EngineeringModelSetup = modelSetup }
            };

            this.viewModel = new EditElementUsageViewModel(this.sessionService.Object, this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyInitializeViewModelSeedsForm()
        {
            this.viewModel.InitializeViewModel(this.elementUsage, this.iteration);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ElementUsage, Is.SameAs(this.elementUsage));
                Assert.That(this.viewModel.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise, Is.EqualTo(this.domain));
                Assert.That(this.viewModel.DomainOfExpertiseSelectorViewModel.AvailableDomainsOfExpertise, Is.EquivalentTo(new[] { this.domain, this.otherDomain }));
            });
        }

        [Test]
        public void VerifyInitializeViewModelRejectsNullArguments()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => this.viewModel.InitializeViewModel(null, this.iteration), Throws.ArgumentNullException);
                Assert.That(() => this.viewModel.InitializeViewModel(this.elementUsage, null), Throws.ArgumentNullException);
            });
        }

        [Test]
        public void VerifyOwnerAssignmentWiresThroughSelectorChange()
        {
            this.viewModel.InitializeViewModel(this.elementUsage, this.iteration);

            this.viewModel.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise = this.otherDomain;

            Assert.That(this.viewModel.ElementUsage.Owner, Is.EqualTo(this.otherDomain));
        }
    }
}
