// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicationsTableTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.EngineeringModel
{
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.ViewModels.Components.Common.Rows;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Publications;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class PublicationsTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<PublicationsTable> renderer;
        private Mock<IPublicationsTableViewModel> viewModel;
        private Publication publication;
        private Parameter parameter;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IPublicationsTableViewModel>();

            var domain = new DomainOfExpertise()
            {
                ShortName = "owner",
                Name = "Owner"
            };

            this.parameter = new Parameter()
            {
                Owner = domain,
                ParameterType = new TextParameterType()
                {
                    ShortName = "txt",
                    Name = "Text"
                },
                Container = new ElementDefinition()
                {
                    ShortName = "element"
                }
            };

            this.publication = new Publication()
            {
                Container = new Iteration(),
                Domain = { domain },
                CreatedOn = DateTime.Now,
                PublishedParameter = { this.parameter }
            };

            var rows = new SourceList<PublicationRowViewModel>();
            rows.Add(new PublicationRowViewModel(this.publication));
            var parametersRows = new List<OwnedParameterOrOverrideBaseRowViewModel>{ new(this.parameter) };

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.GetParametersThatCanBePublished()).Returns(parametersRows);
            this.viewModel.Setup(x => x.GetPublishedParametersRows(It.IsAny<Publication>())).Returns(parametersRows);
            
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<PublicationsTable>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ViewModel, Is.EqualTo(this.viewModel.Object));
                Assert.That(this.renderer.Markup, Does.Contain(this.viewModel.Object.Rows.Items.First().Domains));
                Assert.That(this.renderer.Markup, Does.Contain(this.parameter.ParameterType.ShortName));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }

        [Test]
        public async Task VerifyCreatePublication()
        {
            var addPublicationButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addPublicationButton");
            await this.renderer.InvokeAsync(addPublicationButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.CreatePublication(), Times.Once);
        }

        [Test]
        public async Task VerifyPublicationDetails()
        {
            // the publication history grid
            var grid = this.renderer.FindComponents<DxGrid>().ElementAt(1);
            await this.renderer.InvokeAsync(() => grid.Instance.SelectedDataItemChanged.InvokeAsync(this.viewModel.Object.Rows.Items.First()));

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(true));
                Assert.That(this.renderer.Instance.SelectedRow, Is.EqualTo(this.viewModel.Object.Rows.Items.First()));
            });
        }
    }
}
