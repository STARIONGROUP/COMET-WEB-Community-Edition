// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeFormTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.ViewModels.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.Wrappers;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Forms;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterTypeFormTestFixture
    {
        private TestContext context;
        private Mock<IParameterTypeTableViewModel> viewModel;
        private IRenderedComponent<ParameterTypeForm> renderer;

        private void SetupParameterTypeAndRender(ParameterType parameterType)
        {
            this.viewModel.Setup(x => x.CurrentThing).Returns(parameterType);
            this.renderer.Render();
        }

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.viewModel = new Mock<IParameterTypeTableViewModel>();
            this.viewModel.Setup(x => x.ParameterTypes).Returns([new ClassKindWrapper(ClassKind.BooleanParameterType)]);
            this.viewModel.Setup(x => x.CurrentThing).Returns(new BooleanParameterType());

            this.renderer = this.context.RenderComponent<ParameterTypeForm>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
                parameters.Add(p => p.ShouldCreate, true);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyParameterTypeForm()
        {
            this.SetupParameterTypeAndRender(new EnumerationParameterType());
            var enumerationMultiSelect = this.renderer.FindComponents<DxFormLayoutItem>().FirstOrDefault(x => x.Instance.Id == "multiSelect");
            Assert.That(enumerationMultiSelect, Is.Not.Null);

            this.SetupParameterTypeAndRender(new CompoundParameterType());
            var componentsTable = this.renderer.FindComponent<ComponentsTable>();
            Assert.That(componentsTable.Instance, Is.Not.Null);

            this.SetupParameterTypeAndRender(new ArrayParameterType());
            var arrayIsTensor = this.renderer.FindComponents<DxFormLayoutItem>().FirstOrDefault(x => x.Instance.Id == "isTensor");
            Assert.That(arrayIsTensor, Is.Not.Null);

            this.SetupParameterTypeAndRender(new SpecializedQuantityKind { General = new SimpleQuantityKind() });
            var specializedGeneralizationScales = this.renderer.FindComponents<DxFormLayoutItem>().FirstOrDefault(x => x.Instance.Id == "generalizationScales");
            Assert.That(specializedGeneralizationScales, Is.Not.Null);

            this.SetupParameterTypeAndRender(new DerivedQuantityKind());
            var possibleScales = this.renderer.FindComponents<DxFormLayoutItem>().FirstOrDefault(x => x.Instance.Id == "possibleScales");
            Assert.That(possibleScales, Is.Not.Null);

            this.SetupParameterTypeAndRender(new SampledFunctionParameterType());
            var independentParameterTypeTable = this.renderer.FindComponent<IndependentParameterTypeTable>();
            var dependentParameterTypeTable = this.renderer.FindComponent<DependentParameterTypeTable>();

            Assert.Multiple(() =>
            {
                Assert.That(independentParameterTypeTable, Is.Not.Null);
                Assert.That(dependentParameterTypeTable, Is.Not.Null);
            });
        }

        [Test]
        public async Task VerifyParameterTypeSubmit()
        {
            var editForm = this.renderer.FindComponent<EditForm>();
            await this.renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditParameterType(true), Times.Once);
        }
    }
}
