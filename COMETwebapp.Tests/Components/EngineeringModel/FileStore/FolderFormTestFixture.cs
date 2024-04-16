// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderFormTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.EngineeringModel.FileStore
{
    using System.Linq;

    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.EngineeringModel.FileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FolderHandler;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Forms;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class FolderFormTestFixture
    {
        private TestContext context;
        private IRenderedComponent<FolderForm> renderer;
        private Mock<IFolderHandlerViewModel> viewModel;

        [SetUp]
         public void SetUp()
         {
             this.context = new TestContext();
             this.viewModel = new Mock<IFolderHandlerViewModel>();

             this.viewModel.Setup(x => x.Folder).Returns(new Folder());
             this.context.ConfigureDevExpressBlazor();

             this.renderer = this.context.RenderComponent<FolderForm>(parameters =>
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
         public async Task VerifyOnValidSubmitAndDelete()
         {
            Assert.That(this.renderer.Instance.IsDeletePopupVisible, Is.EqualTo(false));

            var form = this.renderer.FindComponent<EditForm>();
            await this.renderer.InvokeAsync(form.Instance.OnValidSubmit.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditFolder(It.IsAny<bool>()), Times.Once);

            var deleteFolderButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deleteFolderButton");
            await this.renderer.InvokeAsync(deleteFolderButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.IsDeletePopupVisible, Is.EqualTo(true));

            var deleteFolderPopupButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deleteFolderPopupButton");
            await this.renderer.InvokeAsync(deleteFolderPopupButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsDeletePopupVisible, Is.EqualTo(false));
                this.viewModel.Verify(x => x.DeleteFolder(), Times.Once);
            });
        }
    }
}
