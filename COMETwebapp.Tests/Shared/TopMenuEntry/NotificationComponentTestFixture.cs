// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NotificationComponentTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Shared.TopMenuEntry
{
    using Bunit;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.VersionService;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Shared.TopMenuEntry;

    using DevExpress.Blazor;

    using DynamicData;

    using FluentResults;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using INotificationService = COMET.Web.Common.Services.NotificationService.INotificationService;
    using Result = FluentResults.Result;

    [TestFixture]
    public class NotificationComponentTestFixture
    {
        private BunitContext context;
        private Mock<IVersionService> versionService;
        private Mock<INotificationService> notificationService;
        private Mock<IToastNotificationService> toastNotificationService;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.toastNotificationService = new Mock<IToastNotificationService>();
            this.notificationService = new Mock<INotificationService>();
            this.notificationService.Setup(x => x.Results).Returns(new SourceList<ResultNotification>());
            this.versionService = new Mock<IVersionService>();
            this.versionService.Setup(x => x.GetVersion()).Returns("1.1.2");
            this.context.Services.AddSingleton(this.versionService.Object);
            this.context.Services.AddSingleton(this.notificationService.Object);
            this.context.Services.AddSingleton(this.toastNotificationService.Object);
            this.context.Services.AddSingleton(new Mock<IHttpClientFactory>().Object);
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyAboutEntry()
        {
            var renderer = this.context.Render<DxMenu>(parameters =>
            {
                parameters.Add(p => p.Items, builder =>
                {
                    builder.OpenComponent(0, typeof(NotificationComponent));
                    builder.CloseComponent();
                });
            });

            var notificationComponent = renderer.FindComponent<NotificationComponent>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(notificationComponent.Instance, Is.Not.Null);
                Assert.That(notificationComponent.Instance.ToastNotificationService, Is.Not.Null);
                Assert.That(notificationComponent.Instance.NotificationService, Is.Not.Null);
            }

            this.notificationService.Object.Results.Add(new ResultNotification(new Result(), new NotificationDescription()));
            this.toastNotificationService.Verify(x => x.ShowToast(It.IsAny<ToastOptions>()), Times.Once);

            var resultNotification = new ResultNotification(new Result
            {
                Reasons = { new Error("err"), new ExceptionalError(new Exception("exception")) }
            }, new NotificationDescription());

            this.notificationService.Object.Results.Add(resultNotification);
            this.toastNotificationService.Verify(x => x.ShowToast(It.IsAny<ToastOptions>(), It.IsAny<RenderFragment>()), Times.Once);
        }

        [Test]
        public void VerifyNullNotificationDescriptionIsIgnored()
        {
            var renderer = this.context.Render<DxMenu>(parameters =>
            {
                parameters.Add(p => p.Items, builder =>
                {
                    builder.OpenComponent(0, typeof(NotificationComponent));
                    builder.CloseComponent();
                });
            });

            Assert.That(renderer.FindComponent<NotificationComponent>().Instance, Is.Not.Null);

            // A result carrying a null NotificationDescription (a silent sub-operation) must not throw
            // nor raise a toast — regression for the NullReferenceException that aborted a parameter-group
            // delete before its second (delete) transaction ran.
            Assert.That(() => this.notificationService.Object.Results.Add(new ResultNotification(new Result(), null)), Throws.Nothing);

            using (Assert.EnterMultipleScope())
            {
                this.toastNotificationService.Verify(x => x.ShowToast(It.IsAny<ToastOptions>()), Times.Never);
                this.toastNotificationService.Verify(x => x.ShowToast(It.IsAny<ToastOptions>(), It.IsAny<RenderFragment>()), Times.Never);
            }
        }
    }
}
