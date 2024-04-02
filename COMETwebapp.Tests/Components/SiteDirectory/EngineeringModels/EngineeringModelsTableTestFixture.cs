// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelsTableTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Components.SiteDirectory.EngineeringModels
{
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.SiteDirectory.EngineeringModel;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class EngineeringModelsTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<EngineeringModelsTable> renderer;
        private Mock<IEngineeringModelsTableViewModel> viewModel;
        private EngineeringModelSetup engineeringModel1;
        private EngineeringModelSetup engineeringModel2;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            this.viewModel = new Mock<IEngineeringModelsTableViewModel>();

            this.engineeringModel1 = new EngineeringModelSetup()
            {
                Name = "A name",
                ShortName = "AName",
                Container = new SiteDirectory(){ ShortName = "siteDir" },
            };

            this.engineeringModel2 = new EngineeringModelSetup()
            {
                Name = "B name",
                ShortName = "BName",
                Container = new SiteDirectory() { ShortName = "siteDir" },
            };

            var rows = new SourceList<EngineeringModelRowViewModel>();
            rows.Add(new EngineeringModelRowViewModel(this.engineeringModel1));
            rows.Add(new EngineeringModelRowViewModel(this.engineeringModel2));

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.Thing).Returns(new EngineeringModelSetup());

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<EngineeringModelsTable>();
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
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.ViewModel, Is.Not.Null);
                Assert.That(this.renderer.Markup, Does.Contain(this.engineeringModel1.Name));
                Assert.That(this.renderer.Markup, Does.Contain(this.engineeringModel2.Name));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }

        [Test]
        public async Task VerifyDeleteEngineeringModel()
        {
            var deleteButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deleteEngineeringModelButton");
            await this.renderer.InvokeAsync(deleteButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnDeleteButtonClick(It.IsAny<EngineeringModelRowViewModel>()), Times.Once);
        }

        [Test]
        public async Task VerifyAddingOrEditingEngineeringModel()
        {
            var addEngineeringModelButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addEngineeringModelButton");
            await this.renderer.InvokeAsync(addEngineeringModelButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(true));
                Assert.That(this.viewModel.Object.Thing, Is.InstanceOf(typeof(EngineeringModelSetup)));
            });

            var editEngineeringModelButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editEngineeringModelButton");
            await this.renderer.InvokeAsync(editEngineeringModelButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(this.viewModel.Object.Thing, Is.InstanceOf(typeof(EngineeringModelSetup)));
            });

            var form = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditEngineeringModel(false), Times.Once);
        }

        [Test]
        public async Task VerifyRowClick()
        {
            Assert.That(this.renderer.Instance.IsModelSelected, Is.EqualTo(false));

            var firstRow = this.viewModel.Object.Rows.Items.First();
            var grid = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(async () => await grid.Instance.SelectedDataItemChanged.InvokeAsync(firstRow));

            Assert.That(this.renderer.Instance.IsModelSelected, Is.EqualTo(true));
        }
    }
}
