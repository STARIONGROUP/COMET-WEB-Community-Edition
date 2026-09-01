// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EnumerationValueDefinitionsTableTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.ReferenceData.ParameterTypes
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData.ParameterTypes;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EnumerationValueDefinitionsTableTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<EnumerationValueDefinitionsTable> renderer;
        private EnumerationParameterType parameterType;
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            this.parameterType = new EnumerationParameterType
            {
                ValueDefinition =
                {
                    new EnumerationValueDefinition { Name = "Val1", ShortName = "v1" },
                    new EnumerationValueDefinition { Name = "Val2", ShortName = "v2" }
                }
            };

            this.renderer = this.context.Render<EnumerationValueDefinitionsTable>(parameters =>
            {
                parameters.Add(p => p.Thing, this.parameterType);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyEnumerationValueDefinitionsEdit()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.Item, Is.Null);
                Assert.That(this.parameterType.ValueDefinition, Has.Count.EqualTo(2));
            });

            this.renderer.Instance.StartEdit(null);
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);

            var editScaleValueDefinitionButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editScaleValueDefinitionButton");
            await this.renderer.InvokeAsync(editScaleValueDefinitionButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.Item, Is.Not.Null);
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.True);
                Assert.That(this.renderer.Instance.ShouldCreate, Is.False);
            });

            this.renderer.Instance.Item.Name = "Val1_updated";
            var form = this.renderer.FindComponent<EditForm>();
            await this.renderer.InvokeAsync(form.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);
                Assert.That(this.parameterType.ValueDefinition.First().Name, Is.EqualTo("Val1_updated"));
            });

            var addEnumerationValueDefinitionButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addEnumerationValueDefinitionButton");
            await this.renderer.InvokeAsync(addEnumerationValueDefinitionButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.True);
                Assert.That(this.renderer.Instance.ShouldCreate, Is.True);
            });

            this.renderer.Instance.Item.Name = "Val3";
            this.renderer.Instance.Item.ShortName = "v3";
            form = this.renderer.FindComponent<EditForm>();
            await this.renderer.InvokeAsync(form.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);
                Assert.That(this.parameterType.ValueDefinition, Has.Count.EqualTo(3));
            });

            await this.renderer.InvokeAsync(addEnumerationValueDefinitionButton.Instance.Click.InvokeAsync);
            var cancelButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "cancelItemButton");
            await this.renderer.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);

            Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.Thing, Is.Not.Null);
                Assert.That(this.renderer.Instance.OrderedItemsList, Is.EqualTo(this.parameterType.ValueDefinition));
                Assert.That(this.renderer.Markup, Does.Contain(this.parameterType.ValueDefinition.First().Name));
            });
        }
    }
}
