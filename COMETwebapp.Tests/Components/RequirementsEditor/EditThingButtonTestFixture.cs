// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditThingButtonTestFixture.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using DevExpress.Blazor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EditThingButtonTestFixture
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

        private IRenderedComponent<EditThingButton> Render(Thing thing)
        {
            return this.context.Render<EditThingButton>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Thing, thing));
        }

        [Test]
        public void VerifyTooltipReflectsTheThingKind()
        {
            var requirement = new Requirement { Iid = Guid.NewGuid() };
            var specification = new RequirementsSpecification { Iid = Guid.NewGuid() };
            var group = new RequirementsGroup { Iid = Guid.NewGuid() };

            Assert.Multiple(() =>
            {
                Assert.That(this.Render(requirement).FindComponent<DxButton>().Instance.Attributes["title"], Is.EqualTo("Edit requirement"));
                Assert.That(this.Render(specification).FindComponent<DxButton>().Instance.Attributes["title"], Is.EqualTo("Edit specification"));
                Assert.That(this.Render(group).FindComponent<DxButton>().Instance.Attributes["title"], Is.EqualTo("Edit group"));
            });
        }

        [Test]
        public async Task VerifyClickOpensTheEditForm()
        {
            var requirement = new Requirement { Iid = Guid.NewGuid() };
            var component = this.Render(requirement);

            await component.InvokeAsync(component.FindComponent<DxButton>().Instance.Click.InvokeAsync);

            this.viewModel.Verify(x => x.OpenEdit(requirement), Times.Once);
        }
    }
}
