// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTableTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.ParameterEditor
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ParameterEditor;
    using COMETwebapp.ViewModels.Components.ParameterEditor;

    using DevExpress.Blazor;
    
    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;
    
    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ParameterTable> renderedComponent;
        private ParameterTable table;
        private Mock<ISessionService> sessionService;
        private CDPMessageBus messageBus;
        private Mock<IParameterTableViewModel> parameterTableViewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();
            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);
            this.messageBus = new CDPMessageBus();

            this.parameterTableViewModel = new Mock<IParameterTableViewModel>();

            var parametersList = new SourceList<ParameterBaseRowViewModel>();
            parametersList.Add(this.GenerateIntegerRow("n1", "group1", 1, 2));
            parametersList.Add(this.GenerateIntegerRow("n2", "group1", 1, 1));
            parametersList.Add(this.GenerateIntegerRow("n3", "group2", 1, 1));
            parametersList.Add(this.GenerateIntegerRow("n4", "group2", 1, 1));

            this.parameterTableViewModel.Setup(x => x.Rows).Returns(parametersList);

            this.renderedComponent = this.context.RenderComponent<ParameterTable>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.parameterTableViewModel.Object);
            });
            
            this.table = this.renderedComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.Dispose();
            this.context.CleanContext();
            this.messageBus.Dispose();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderedComponent, Is.Not.Null);
                Assert.That(this.table, Is.Not.Null);
                Assert.That(this.table.ViewModel, Is.Not.Null);
            });

            var grid = this.renderedComponent.FindComponent<DxGrid>();
            var highlightedGroupRows = grid.FindAll(".font-weight-bold");
            var expectedNumberOfGroups = this.parameterTableViewModel.Object.Rows.Items.DistinctBy(x => x.ElementBaseName).Count();
            var expectedNumberOfDataRows = this.parameterTableViewModel.Object.Rows.Count;

            Assert.Multiple(() =>
            {
                Assert.That(grid.Instance.GetVisibleRowCount(), Is.EqualTo(expectedNumberOfGroups));
                Assert.That(highlightedGroupRows.Count, Is.EqualTo(1));
            });
            
            grid.SetParametersAndRender(p => p.Add(x => x.AutoExpandAllGroupRows, true));
            var highlightedRows = grid.FindAll(".font-weight-bold");

            Assert.Multiple(() =>
            {
                Assert.That(grid.Instance.GetVisibleRowCount(), Is.EqualTo(expectedNumberOfDataRows + expectedNumberOfGroups));
                Assert.That(highlightedRows.Count, Is.EqualTo(2));
            });
        }

        private ParameterBaseRowViewModel GenerateIntegerRow(string parameterShortName, string elementBaseShortName, int publishedValue, int manualValue)
        {
            var parameter = new Parameter()
            {
                Container = new ElementDefinition()
                {
                    ShortName = elementBaseShortName,
                    Name = elementBaseShortName
                },
                ParameterType = new SimpleQuantityKind()
                {
                    ShortName = parameterShortName,
                    Name = parameterShortName
                }
            };

            var valueSet = new ParameterValueSet()
            {
                Published = new ValueArray<string>([publishedValue.ToString()]),
                Manual = new ValueArray<string>([manualValue.ToString()]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            return new ParameterBaseRowViewModel(this.sessionService.Object, false, parameter, valueSet, this.messageBus);
        }
    }
}
