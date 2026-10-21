// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ArchiveLoginTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;

    using FluentResults;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ArchiveLoginTestFixture
    {
        private BunitContext context;
        private Mock<IArchiveLoginViewModel> viewModel;

        [SetUp]
        public void Setup()
        {
            this.viewModel = new Mock<IArchiveLoginViewModel>();
            this.viewModel.SetupGet(x => x.AuthenticationDto).Returns(new AuthenticationDto());
            this.viewModel.SetupGet(x => x.AuthenticationResult).Returns(new Result());
            this.viewModel.SetupGet(x => x.IsLoading).Returns(false);
            this.viewModel.Setup(x => x.ExecuteLogin()).Returns(Task.CompletedTask);

            this.context = new BunitContext();
            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyArchiveLoginFormIsRendered()
        {
            var renderer = this.context.Render<ArchiveLogin>();

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Find("#archive-login-form"), Is.Not.Null);
                Assert.That(renderer.Find("#archive-username"), Is.Not.Null);
                Assert.That(renderer.Find("#archive-password"), Is.Not.Null);
                Assert.That(renderer.Find("#archive-connectbtn"), Is.Not.Null);
            });
        }

        [Test]
        public async Task VerifySubmittingTheFormExecutesLogin()
        {
            var renderer = this.context.Render<ArchiveLogin>();
            var editForm = renderer.FindComponent<EditForm>();

            await renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            this.viewModel.Verify(x => x.ExecuteLogin(), Times.Once);
        }

        [Test]
        public void VerifyAuthenticationErrorIsRendered()
        {
            this.viewModel.SetupGet(x => x.AuthenticationResult).Returns(Result.Fail("Select an Annex C3 archive to open"));

            var renderer = this.context.Render<ArchiveLogin>();

            Assert.That(renderer.Markup, Does.Contain("Select an Annex C3 archive to open"));
        }
    }
}
