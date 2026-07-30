// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditElementDefinitionViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ModelEditor.EditElementDefinitionViewModel;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EditElementDefinitionViewModelTestFixture
    {
        private EditElementDefinitionViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private CDPMessageBus messageBus;
        private DomainOfExpertise domain;
        private DomainOfExpertise otherDomain;
        private SiteReferenceDataLibrary rdl;
        private Iteration iteration;
        private ElementDefinition topElement;
        private ElementDefinition elementDefinition;
        private Category structureCategory;

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

            this.structureCategory = new Category
            {
                Iid = Guid.NewGuid(),
                Name = "Structure",
                ShortName = "STR",
                PermissibleClass = { ClassKind.ElementDefinition }
            };

            this.rdl = new SiteReferenceDataLibrary
            {
                ShortName = "siteRdl",
                Name = "Site RDL",
                DefinedCategory = { this.structureCategory }
            };

            var siteDirectory = new SiteDirectory
            {
                Domain = { this.domain, this.otherDomain },
                SiteReferenceDataLibrary = { this.rdl }
            };

            var modelSetup = new EngineeringModelSetup
            {
                Name = "ModelName",
                ShortName = "ModelShortName",
                ActiveDomain = { this.domain, this.otherDomain },
                RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = this.rdl } }
            };

            siteDirectory.Model.Add(modelSetup);
            var iterationSetup = new IterationSetup { IterationNumber = 1 };
            modelSetup.IterationSetup.Add(iterationSetup);

            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            this.topElement = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Container",
                ShortName = "CONT",
                Owner = this.domain
            };

            this.elementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                ShortName = "BOX",
                Owner = this.domain,
                Category = { this.structureCategory }
            };

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                Element = { this.topElement, this.elementDefinition },
                TopElement = this.topElement,
                IterationSetup = iterationSetup,
                Container = new EngineeringModel { EngineeringModelSetup = modelSetup }
            };

            this.viewModel = new EditElementDefinitionViewModel(this.sessionService.Object, this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyInitializeViewModelSeedsFormFromTarget()
        {
            this.viewModel.InitializeViewModel(this.elementDefinition, this.iteration);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ElementDefinition, Is.SameAs(this.elementDefinition));
                Assert.That(this.viewModel.Iteration, Is.SameAs(this.iteration));
                Assert.That(this.viewModel.SelectedCategories, Is.EquivalentTo(new[] { this.structureCategory }));
                Assert.That(this.viewModel.IsTopElement, Is.False, "The seeded ED is not the iteration's top element.");
                Assert.That(this.viewModel.AvailableCategories, Does.Contain(this.structureCategory));
                Assert.That(this.viewModel.AvailableLanguages, Is.Not.Empty);
                Assert.That(this.viewModel.DomainOfExpertiseSelectorViewModel.AvailableDomainsOfExpertise, Is.EquivalentTo(new[] { this.domain, this.otherDomain }));
                Assert.That(this.viewModel.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise, Is.EqualTo(this.domain));
            });
        }

        [Test]
        public void VerifyInitializeViewModelFlagsTopElement()
        {
            this.viewModel.InitializeViewModel(this.topElement, this.iteration);

            Assert.That(this.viewModel.IsTopElement, Is.True);
        }

        [Test]
        public void VerifyInitializeViewModelRejectsNullArguments()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => this.viewModel.InitializeViewModel(null, this.iteration), Throws.ArgumentNullException);
                Assert.That(() => this.viewModel.InitializeViewModel(this.elementDefinition, null), Throws.ArgumentNullException);
            });
        }

        [Test]
        public void VerifyAvailableCategoriesFiltersByPermissibleClass()
        {
            var unrelatedCategory = new Category
            {
                Iid = Guid.NewGuid(),
                Name = "Sensor",
                ShortName = "SEN",
                PermissibleClass = { ClassKind.Parameter }
            };

            this.rdl.DefinedCategory.Add(unrelatedCategory);

            this.viewModel.InitializeViewModel(this.elementDefinition, this.iteration);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableCategories, Does.Contain(this.structureCategory));
                Assert.That(this.viewModel.AvailableCategories, Does.Not.Contain(unrelatedCategory),
                    "Categories whose PermissibleClass excludes ElementDefinition must not be offered.");
            });
        }

        [Test]
        public void VerifyOwnerAssignmentWiresThroughSelectorChange()
        {
            this.viewModel.InitializeViewModel(this.elementDefinition, this.iteration);

            this.viewModel.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise = this.otherDomain;

            Assert.That(this.viewModel.ElementDefinition.Owner, Is.EqualTo(this.otherDomain));
        }
    }
}
