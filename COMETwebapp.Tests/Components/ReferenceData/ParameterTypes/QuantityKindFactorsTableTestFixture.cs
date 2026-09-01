// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="QuantityKindFactorsTableTestFixture.cs" company="Starion Group S.A.">
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
    public class QuantityKindFactorsTableTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<QuantityKindFactorsTable> renderer;
        private DerivedQuantityKind parameterType;
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

            this.parameterType = new DerivedQuantityKind
            {
                QuantityKindFactor =
                {
                    new QuantityKindFactor { Exponent = "exp1", QuantityKind = new SimpleQuantityKind() },
                    new QuantityKindFactor { Exponent = "exp2", QuantityKind = new SimpleQuantityKind() }
                }
            };

            this.renderer = this.context.Render<QuantityKindFactorsTable>(parameters =>
            {
                parameters.Add(p => p.QuantityKindParameterTypes, [new SimpleQuantityKind(), new SpecializedQuantityKind()]);
                parameters.Add(p => p.Thing, this.parameterType);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyComponentsEdit()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.Item, Is.Null);
                Assert.That(this.parameterType.QuantityKindFactor, Has.Count.EqualTo(2));
            });

            this.renderer.Instance.StartEdit(null);
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);

            var editQuantityKindFactorButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editQuantityKindFactorButton");
            await this.renderer.InvokeAsync(editQuantityKindFactorButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.Item, Is.Not.Null);
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.True);
                Assert.That(this.renderer.Instance.ShouldCreate, Is.False);
            });

            var form = this.renderer.FindComponent<EditForm>();
            await this.renderer.InvokeAsync(form.Instance.OnValidSubmit.InvokeAsync);

            Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);

            var addQuantityKindFactorButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addQuantityKindFactorButton");
            await this.renderer.InvokeAsync(addQuantityKindFactorButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.True);
                Assert.That(this.renderer.Instance.ShouldCreate, Is.True);
            });

            form = this.renderer.FindComponent<EditForm>();
            await this.renderer.InvokeAsync(form.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);
                Assert.That(this.parameterType.QuantityKindFactor, Has.Count.EqualTo(3));
            });

            await this.renderer.InvokeAsync(addQuantityKindFactorButton.Instance.Click.InvokeAsync);
            var cancelButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "cancelItemButton");
            await this.renderer.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);

            Assert.That(this.renderer.Instance.IsOnEditMode, Is.False);
        }

        [Test]
        public async Task VerifyReorderAndRemove()
        {
            var firstFactor = this.parameterType.QuantityKindFactor[0];

            var moveUpButton = this.renderer.FindComponents<DxButton>().Last(x => x.Instance.Id == "moveUpButton");
            await this.renderer.InvokeAsync(moveUpButton.Instance.Click.InvokeAsync);
            Assert.That(this.parameterType.QuantityKindFactor[0], Is.Not.EqualTo(firstFactor));

            var moveDownButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "moveDownButton");
            await this.renderer.InvokeAsync(moveDownButton.Instance.Click.InvokeAsync);
            Assert.That(this.parameterType.QuantityKindFactor[0], Is.EqualTo(firstFactor));

            var removeQuantityKindFactorButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeQuantityKindFactorButton");
            await this.renderer.InvokeAsync(removeQuantityKindFactorButton.Instance.Click.InvokeAsync);
            Assert.That(this.parameterType.QuantityKindFactor, Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.QuantityKindParameterTypes.Count(), Is.EqualTo(2));
                Assert.That(this.renderer.Instance.Thing, Is.Not.Null);
                Assert.That(this.renderer.Instance.OrderedItemsList, Is.EqualTo(this.parameterType.QuantityKindFactor));
                Assert.That(this.renderer.Markup, Does.Contain(this.parameterType.QuantityKindFactor.First().Exponent));
            });
        }
    }
}
