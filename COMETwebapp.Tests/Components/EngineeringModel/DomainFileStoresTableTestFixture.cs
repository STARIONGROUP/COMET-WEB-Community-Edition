// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoresTableTestFixture.cs" company="Starion Group S.A.">
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
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.Components.EngineeringModel.DomainFileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.DomainFileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components.Forms;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class DomainFileStoresTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<DomainFileStoresTable> renderer;
        private Mock<IDomainFileStoreTableViewModel> viewModel;
        private DomainFileStore domainFileStore;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IDomainFileStoreTableViewModel>();

            this.domainFileStore = new DomainFileStore()
            {
                Name = "DFS Name",
                Owner = new DomainOfExpertise(){ Name = "Owner" },
                Container = new EngineeringModel()
            };

            var rows = new SourceList<DomainFileStoreRowViewModel>();
            rows.Add(new DomainFileStoreRowViewModel(this.domainFileStore));
            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.CurrentThing).Returns(new DomainFileStore());
            this.viewModel.Setup(x => x.DomainOfExpertiseSelectorViewModel).Returns(new Mock<IDomainOfExpertiseSelectorViewModel>().Object);
            
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<DomainFileStoresTable>(parameters =>
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
                Assert.That(this.renderer.Markup, Does.Contain(this.domainFileStore.Name));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }

        [Test]
        public async Task VerifyDeleteDomainFileStore()
        {
            var grid = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(() => grid.Instance.SelectedDataItemChanged.InvokeAsync(this.viewModel.Object.Rows.Items.First()));

            var deleteButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deleteItemButton");
            await this.renderer.InvokeAsync(deleteButton.Instance.Click.InvokeAsync);
            this.viewModel.VerifySet(x => x.IsOnDeletionMode = true, Times.Once);
            this.viewModel.Setup(x => x.IsOnDeletionMode).Returns(true);

            this.renderer.Render();

            var deletionPopup = this.renderer.FindComponents<DxPopup>().First(x => x.Instance.Visible);
            var confirmDeletionButton = deletionPopup.FindComponents<DxButton>().ElementAt(1);
            await deletionPopup.InvokeAsync(confirmDeletionButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnConfirmPopupButtonClick(), Times.Once);
        }

        [Test]
        public async Task VerifyCreateDomainFileStore()
        {
            var addOptionButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "dataItemDetailsButton");
            await this.renderer.InvokeAsync(addOptionButton.Instance.Click.InvokeAsync);
            var form = this.renderer.FindComponent<DomainFileStoresForm>();

            Assert.Multiple(() =>
            {
                Assert.That(form, Is.Not.Null);
                Assert.That(form.Instance.IsVisible, Is.EqualTo(true));
                Assert.That(form.Instance.ShouldCreate, Is.EqualTo(true));
            });

            var editForm = form.FindComponent<EditForm>();
            await form.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditDomainFileStore(true), Times.Once);
        }

        [Test]
        public async Task VerifyEditDomainFileStore()
        {
            var grid = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(() => grid.Instance.SelectedDataItemChanged.InvokeAsync(this.viewModel.Object.Rows.Items.First()));
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(true));

            var form = this.renderer.FindComponent<DomainFileStoresForm>();

            Assert.Multiple(() =>
            {
                Assert.That(form, Is.Not.Null);
                Assert.That(form.Instance.IsVisible, Is.EqualTo(true));
                Assert.That(form.Instance.ShouldCreate, Is.EqualTo(false));
            });

            var editForm = form.FindComponent<EditForm>();
            await form.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditDomainFileStore(false), Times.Once);

            var cancelButton = editForm.FindComponents<DxButton>().First(x => x.Instance.Id == "cancelItemButton");
            await editForm.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(false));
        }
    }
}
