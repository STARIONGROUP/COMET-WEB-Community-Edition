// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionsTableTestFixture.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Options;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components.Forms;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class OptionsTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<OptionsTable> renderer;
        private Mock<IOptionsTableViewModel> viewModel;
        private Option option;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IOptionsTableViewModel>();

            this.option = new Option()
            {
                Name = "A name",
                ShortName = "AName",
                Container = new Iteration(),
            };

            var rows = new SourceList<OptionRowViewModel>();
            rows.Add(new OptionRowViewModel(this.option){ IsDefault = true});
            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.CurrentThing).Returns(new Option());
            
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<OptionsTable>(parameters =>
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
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.ViewModel, Is.EqualTo(this.viewModel.Object));
                Assert.That(this.renderer.Markup, Does.Contain(this.option.Name));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }

        [Test]
        public async Task VerifyDeleteOption()
        {
            var deleteButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deleteButton");
            await this.renderer.InvokeAsync(deleteButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnDeleteButtonClick(It.IsAny<OptionRowViewModel>()), Times.Once);
            this.viewModel.Setup(x => x.IsOnDeletionMode).Returns(true);

            this.renderer.Render();

            var deletionPopup = this.renderer.FindComponent<DxPopup>();
            var confirmDeletionButton = deletionPopup.FindComponents<DxButton>().ElementAt(1);
            await deletionPopup.InvokeAsync(confirmDeletionButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnConfirmPopupButtonClick(), Times.Once);
        }

        [Test]
        public async Task VerifyCreateOption()
        {
            var addOptionButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addOptionButton");
            await this.renderer.InvokeAsync(addOptionButton.Instance.Click.InvokeAsync);
            var form = this.renderer.FindComponent<OptionsForm>();

            Assert.Multiple(() =>
            {
                Assert.That(form, Is.Not.Null);
                Assert.That(form.Instance.IsVisible, Is.EqualTo(true));
                Assert.That(form.Instance.ShouldCreate, Is.EqualTo(true));
            });

            var editForm = form.FindComponent<EditForm>();
            await form.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditOption(true), Times.Once);
        }

        [Test]
        public async Task VerifyEditOption()
        {
            var grid = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(() => grid.Instance.SelectedDataItemChanged.InvokeAsync(this.viewModel.Object.Rows.Items.First()));

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.SetCurrentOption(It.IsAny<Option>()));
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(true));
            });

            var form = this.renderer.FindComponent<OptionsForm>();

            Assert.Multiple(() =>
            {
                Assert.That(form, Is.Not.Null);
                Assert.That(form.Instance.IsVisible, Is.EqualTo(true));
                Assert.That(form.Instance.ShouldCreate, Is.EqualTo(false));
            });

            var editForm = form.FindComponent<EditForm>();
            await form.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditOption(false), Times.Once);

            var cancelButton = editForm.FindComponents<DxButton>().First(x => x.Instance.Id == "cancelItemButton");
            await editForm.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(false));
        }
    }
}
