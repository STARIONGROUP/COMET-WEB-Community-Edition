// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeTableTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.ReferenceData
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.ViewModels.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.Wrappers;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterTypeTableTestFixture
    {
        private TestContext context;
        private Mock<IParameterTypeTableViewModel> viewModel;
        private ParameterType parameterType;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.parameterType = new BooleanParameterType
            {
                Name = "name"
            };

            this.viewModel = new Mock<IParameterTypeTableViewModel>();

            var rows = new SourceList<ParameterTypeRowViewModel>();
            rows.Add(new ParameterTypeRowViewModel(this.parameterType));

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.ParameterTypes).Returns([new ClassKindWrapper(ClassKind.BooleanParameterType)]);
            this.viewModel.Setup(x => x.Thing).Returns(new BooleanParameterType());
            this.context.Services.AddSingleton(this.viewModel.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyOnInitialized()
        {
            var renderer = this.context.RenderComponent<ParameterTypeTable>();

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
                Assert.That(renderer.Markup, Does.Contain(this.parameterType.Name));
                Assert.That(renderer.Instance, Is.Not.Null);
            });
        }

        [Test]
        public async Task VerifyParameterTypeGridActions()
        {
            var renderer = this.context.RenderComponent<ParameterTypeTable>();
            var grid = renderer.FindComponent<DxGrid>();

            await renderer.InvokeAsync(grid.Instance.EditModelSaving.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditParameterType(false), Times.Once);

            await renderer.InvokeAsync(() => grid.Instance.StartEditNewRowAsync());
            this.viewModel.Verify(x => x.SelectParameterType(It.IsAny<ParameterType>()), Times.Once);

            await renderer.InvokeAsync(() => grid.Instance.SelectedDataItemChanged.InvokeAsync(this.viewModel.Object.Rows.Items.First()));
            this.viewModel.Verify(x => x.SelectParameterType(It.IsAny<ParameterType>()), Times.Exactly(2));
        }
    }
}
