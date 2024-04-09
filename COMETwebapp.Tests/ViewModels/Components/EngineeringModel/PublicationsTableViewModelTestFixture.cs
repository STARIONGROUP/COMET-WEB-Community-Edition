// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PublicationsTableViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common.Rows;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Publications;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PublicationsTableViewModelTestFixture
    {
        private PublicationsTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private Mock<ILogger<PublicationsTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private Iteration iteration;
        private Publication publication;
        private Parameter parameter;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<PublicationsTableViewModel>>();

            this.parameter = new Parameter()
            {
                ParameterType = new TextParameterType()
                {
                    ShortName = "txt",
                    Name = "Text"
                },
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Published = new ValueArray<string>(["old"]),
                        Manual = new ValueArray<string>(["new"]),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var element = new ElementDefinition();
            element.Parameter.Add(this.parameter);

            this.publication = new Publication()
            {
                Domain =
                {
                    new DomainOfExpertise()
                    {
                        ShortName = "doe",
                        Name = "Domain of Expertise"
                    }
                },
                PublishedParameter = { this.parameter }
            };

            this.iteration = new Iteration()
            {
                Container = new EngineeringModel(),
                TopElement = element
            };

            this.iteration.Publication.Add(this.publication);
            this.iteration.Element.Add(element);

            this.assembler = new Assembler(new Uri("http://localhost:5000/"), this.messageBus);
            var lazyPublication = new Lazy<Thing>(this.publication);
            this.assembler.Cache.TryAdd(new CacheKey(), lazyPublication);

            this.permissionService.Setup(x => x.CanWrite(this.publication.ClassKind, this.publication.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.Assembler).Returns(this.assembler);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            this.viewModel = new PublicationsTableViewModel(this.sessionService.Object, this.messageBus, this.loggerMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel();
            this.viewModel.SetCurrentIteration(this.iteration);

            var firstRow = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(1));
                Assert.That(firstRow.Thing, Is.EqualTo(this.publication));
                Assert.That(firstRow.CreatedOn, Is.EqualTo(this.publication.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss")));
                Assert.That(firstRow.Domains, Does.Contain(this.publication.Domain.First().ShortName));
            });
        }

        [Test]
        public async Task VerifyPublicationCreation()
        {
            var listOfSelectedRowsToPublish = new List<object>([new OwnedParameterOrOverrideBaseRowViewModel(this.parameter)]);

            this.viewModel.InitializeViewModel();
            this.viewModel.SetCurrentIteration(this.iteration);
            await this.viewModel.CreatePublication();
            this.sessionService.Verify(x => x.RefreshSession(), Times.Never);

            this.viewModel.SelectedParameterRowsToPublish = listOfSelectedRowsToPublish;
            await this.viewModel.CreatePublication();

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.UpdateThings(It.IsAny<Iteration>(), It.IsAny<IEnumerable<Thing>>()), Times.Once);
                this.sessionService.Verify(x => x.RefreshSession(), Times.Once);
                Assert.That(this.viewModel.SelectedParameterRowsToPublish, Is.Empty);
            });

            this.sessionService.Setup(x => x.UpdateThings(It.IsAny<Iteration>(), It.IsAny<IEnumerable<Thing>>())).Throws(new Exception());
            this.viewModel.SelectedParameterRowsToPublish = listOfSelectedRowsToPublish;
            await this.viewModel.CreatePublication();
            this.sessionService.Verify(x => x.RefreshSession(), Times.Once);
        }

        [Test]
        public void VerifyQueries()
        {
            this.viewModel.InitializeViewModel();
            this.viewModel.SetCurrentIteration(this.iteration);

            var publishedParameters = this.viewModel.GetPublishedParametersRows(this.publication);
            Assert.That(publishedParameters.Count(), Is.EqualTo(this.publication.PublishedParameter.Count));

            var parametersThatCanBePublished = this.viewModel.GetParametersThatCanBePublished();
            Assert.That(parametersThatCanBePublished.Count(), Is.EqualTo(1));
        }
    }
}
