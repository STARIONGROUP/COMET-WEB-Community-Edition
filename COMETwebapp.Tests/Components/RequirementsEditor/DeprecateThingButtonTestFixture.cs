// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DeprecateThingButtonTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.RequirementsEditor
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using DevExpress.Blazor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DeprecateThingButtonTestFixture
    {
        private BunitContext context;
        private Mock<IRequirementsEditorBodyViewModel> viewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.viewModel = new Mock<IRequirementsEditorBodyViewModel>();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        private IRenderedComponent<DeprecateThingButton> Render(Thing thing)
        {
            return this.context.Render<DeprecateThingButton>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Thing, thing));
        }

        [Test]
        public void VerifyIconAndTooltipReflectTheDeprecationState()
        {
            var active = new Requirement { Iid = Guid.NewGuid() };
            var deprecated = new RequirementsSpecification { Iid = Guid.NewGuid(), IsDeprecated = true };

            var activeButton = this.Render(active).FindComponent<DxButton>().Instance;
            var deprecatedButton = this.Render(deprecated).FindComponent<DxButton>().Instance;

            Assert.Multiple(() =>
            {
                Assert.That(activeButton.IconCssClass, Is.EqualTo(IconName.Ban.GetCssClass()));
                Assert.That(activeButton.Attributes["title"], Is.EqualTo("Deprecate requirement"));
                Assert.That(deprecatedButton.IconCssClass, Is.EqualTo(IconName.Undo.GetCssClass()));
                Assert.That(deprecatedButton.Attributes["title"], Is.EqualTo("Restore specification"));
            });
        }

        [Test]
        public async Task VerifyClickConfirmsDeprecation()
        {
            var requirement = new Requirement { Iid = Guid.NewGuid() };
            var component = this.Render(requirement);

            await component.InvokeAsync(component.FindComponent<DxButton>().Instance.Click.InvokeAsync);

            this.viewModel.Verify(x => x.ConfirmDeprecation(requirement), Times.Once);
        }
    }
}
