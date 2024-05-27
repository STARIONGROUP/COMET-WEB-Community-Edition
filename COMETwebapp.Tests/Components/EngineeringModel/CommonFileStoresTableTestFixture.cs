// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoresTableTestFixture.cs" company="Starion Group S.A.">
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
    using COMETwebapp.ViewModels.Components.EngineeringModel.CommonFileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components.Forms;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class CommonFileStoresTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<CommonFileStoresTable> renderer;
        private Mock<ICommonFileStoreTableViewModel> viewModel;
        private CommonFileStore commonFileStore;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<ICommonFileStoreTableViewModel>();

            this.commonFileStore = new CommonFileStore()
            {
                Name = "CFS Name",
                Owner = new DomainOfExpertise(){ Name = "Owner" },
                Container = new EngineeringModel()
            };

            var rows = new SourceList<CommonFileStoreRowViewModel>();
            rows.Add(new CommonFileStoreRowViewModel(this.commonFileStore));
            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.CurrentThing).Returns(new CommonFileStore());
            
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<CommonFileStoresTable>(parameters =>
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
                Assert.That(this.renderer.Markup, Does.Contain(this.commonFileStore.Name));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }

        [Test]
        public async Task VerifyDeleteCommonFileStore()
        {
            var deleteButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deleteButton");
            await this.renderer.InvokeAsync(deleteButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnDeleteButtonClick(It.IsAny<CommonFileStoreRowViewModel>()), Times.Once);
            this.viewModel.Setup(x => x.IsOnDeletionMode).Returns(true);

            this.renderer.Render();

            var deletionPopup = this.renderer.FindComponent<DxPopup>();
            var confirmDeletionButton = deletionPopup.FindComponents<DxButton>().ElementAt(1);
            await deletionPopup.InvokeAsync(confirmDeletionButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnConfirmPopupButtonClick(), Times.Once);
        }

        [Test]
        public async Task VerifyCreateCommonFileStore()
        {
            var addOptionButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addCommonFileStoreButton");
            await this.renderer.InvokeAsync(addOptionButton.Instance.Click.InvokeAsync);
            var form = this.renderer.FindComponent<CommonFileStoresForm>();

            Assert.Multiple(() =>
            {
                Assert.That(form, Is.Not.Null);
                Assert.That(form.Instance.IsVisible, Is.EqualTo(true));
                Assert.That(form.Instance.ShouldCreate, Is.EqualTo(true));
            });

            var editForm = form.FindComponent<EditForm>();
            await form.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditCommonFileStore(true), Times.Once);
        }

        [Test]
        public async Task VerifyEditCommonFileStore()
        {
            var grid = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(() => grid.Instance.SelectedDataItemChanged.InvokeAsync(this.viewModel.Object.Rows.Items.First()));
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(true));

            var form = this.renderer.FindComponent<CommonFileStoresForm>();

            Assert.Multiple(() =>
            {
                Assert.That(form, Is.Not.Null);
                Assert.That(form.Instance.IsVisible, Is.EqualTo(true));
                Assert.That(form.Instance.ShouldCreate, Is.EqualTo(false));
            });

            var editForm = form.FindComponent<EditForm>();
            await form.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditCommonFileStore(false), Times.Once);

            var cancelButton = editForm.FindComponents<DxButton>().First(x => x.Instance.Id == "cancelItemButton");
            await editForm.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(false));
        }
    }
}
