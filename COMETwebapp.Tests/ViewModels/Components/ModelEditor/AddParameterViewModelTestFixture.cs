// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AddParameterViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class AddParameterViewModelTestFixture
    {
        private AddParameterViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ElementDefinition elementDefinition;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            this.domain = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                ShortName = "SYS"
            };

            session.Setup(x => x.ActivePerson).Returns(new Person
            {
                DefaultDomain = this.domain
            });

            this.elementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                Parameter =
                {
                    new Parameter
                    {
                        ParameterType = new TextParameterType()
                    }
                },
                ParameterGroup =
                {
                    new ParameterGroup()
                }
            };

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                Element = { this.elementDefinition },
                Container = new EngineeringModel
                {
                    EngineeringModelSetup = new EngineeringModelSetup
                    {
                        ActiveDomain = { this.domain }
                    }
                }
            };

            this.viewModel = new AddParameterViewModel(this.sessionService.Object);
        }

        [Test]
        public async Task VerifyAddParameter()
        {
            this.viewModel.InitializeViewModel(this.iteration);
            await this.viewModel.AddParameterToElementDefinition();

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<ElementDefinition>(), It.Is<IReadOnlyCollection<Thing>>(c => c.Count == 2)), Times.Once);
                this.sessionService.Verify(x => x.RefreshSession(), Times.Once);
                Assert.That(this.viewModel.MeasurementScales, Is.Empty);
            });

            this.viewModel.ParameterTypeSelectorViewModel.SelectedParameterType = new SimpleQuantityKind
            {
                PossibleScale = { new OrdinalScale() }
            };

            Assert.That(this.viewModel.MeasurementScales, Is.Not.Empty);
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel(this.iteration);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DomainsOfExpertise, Is.Null);
                Assert.That(this.viewModel.ParameterGroups, Is.Empty);
                Assert.That(this.viewModel.ParameterTypeSelectorViewModel.CurrentIteration, Is.EqualTo(this.iteration));
                Assert.That(this.viewModel.Parameter.Owner, Is.EqualTo(this.domain));
            });

            this.viewModel.SetSelectedElementDefinition(this.elementDefinition);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DomainsOfExpertise, Is.Not.Null);
                Assert.That(this.viewModel.ParameterGroups, Is.Not.Null);
                Assert.That(this.viewModel.SelectedElementDefinition, Is.EqualTo(this.elementDefinition));
            });
        }
    }
}
