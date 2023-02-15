// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Pages.ParameterEditor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.ParameterEditor;
    using COMETwebapp.Pages.ParameterEditor;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.ViewModels.Components.ParameterEditor;
    using COMETwebapp.ViewModels.Pages.ParameterEditor;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterEditorTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ParameterEditor> renderedComponent;
        private ParameterEditor editor;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();
            
            var parameterEditorViewModel = new Mock<IParameterEditorViewModel>();
            
            var elements = new SourceList<ElementBase>();
            elements.Add(new ElementDefinition() { Name = "Element1" });
            elements.Add(new ElementDefinition() { Name = "Element2" });
            elements.Add(new ElementDefinition() { Name = "Element3" });

            parameterEditorViewModel.Setup(x => x.Elements).Returns(elements.Items.ToList());
            parameterEditorViewModel.Setup(x => x.FilteredElements).Returns(elements);

            parameterEditorViewModel.Setup(x => x.ParameterTypes).Returns(new List<ParameterType>()
            {
                new ArrayParameterType(),
                new BooleanParameterType(),
                new CompoundParameterType(),
            });

            this.context.Services.AddSingleton(parameterEditorViewModel.Object);

            var parameterTableViewModelMock = new Mock<IParameterTableViewModel>();

            var rowViewModels = new SourceList<ParameterBaseRowViewModel>();

            var parameter = new Parameter()
            {
                ParameterType = new ArrayParameterType(){Name = "Orientation", ShortName = "orient"},
                Owner = new DomainOfExpertise(){Name = "DoE1", ShortName = "doe1"},
                Container = elements.Items.First(),
            };
            
            var actualFiniteStateList = new ActualFiniteStateList()
            {
                Iid = Guid.NewGuid()
            };

            var possibleFiniteStateList = new List<PossibleFiniteState> { new() { Iid = Guid.NewGuid(), Name = "State1" }, new() { Iid = Guid.NewGuid(), Name = "State2" } };

            actualFiniteStateList.PossibleFiniteStateList.Add(new PossibleFiniteStateList()
            {
                PossibleState = { possibleFiniteStateList[0], possibleFiniteStateList[1] }
            });

            actualFiniteStateList.ActualState.Add(new ActualFiniteState() { Iid = Guid.NewGuid(), PossibleState = { possibleFiniteStateList[0] } });
            actualFiniteStateList.ActualState.Add(new ActualFiniteState() { Iid = Guid.NewGuid(), PossibleState = { possibleFiniteStateList[1] } });

            var iteration = new Iteration();
            iteration.Option.Add(new Option());
            iteration.ActualFiniteStateList.Add(actualFiniteStateList);

            parameterEditorViewModel.Setup(x => x.SessionService.DefaultIteration).Returns(iteration);

            var valueSet = new Mock<IValueSet>();
            valueSet.Setup(x => x.ValueSwitch).Returns(ParameterSwitchKind.MANUAL);
            valueSet.Setup(x => x.ActualState).Returns(actualFiniteStateList.ActualState[0]);
            valueSet.Setup(x => x.ActualOption).Returns(new Option() { Name = "option" });

            rowViewModels.Add(new ParameterBaseRowViewModel(parameterEditorViewModel.Object.SessionService, parameter, valueSet.Object));

            parameterTableViewModelMock.Setup(x => x.Rows).Returns(rowViewModels);
            this.context.Services.AddSingleton(parameterTableViewModelMock.Object);
            
            this.renderedComponent = this.context.RenderComponent<ParameterEditor>();
            this.editor = this.renderedComponent.Instance;
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.editor, Is.Not.Null);
                Assert.That(this.editor.ViewModel, Is.Not.Null);
                Assert.That(this.renderedComponent, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyComponentUI()
        {
            var elementFilterCombo = this.renderedComponent.Find(".element-filter");
            var parameterFilterCombo = this.renderedComponent.Find(".parameter-filter");
            var stateFilterCombo = this.renderedComponent.Find(".state-filter");
            var optionFilterCombo = this.renderedComponent.Find(".option-filter");

            var isOwnedCheckbox = this.renderedComponent.FindComponent<DxCheckBox<bool>>();
            var parameterTable = this.renderedComponent.FindComponent<ParameterTable>();
            
            Assert.Multiple(() =>
            {
                Assert.That(elementFilterCombo, Is.Not.Null);
                Assert.That(parameterFilterCombo, Is.Not.Null);
                Assert.That(stateFilterCombo, Is.Not.Null);
                Assert.That(optionFilterCombo, Is.Not.Null);
                Assert.That(isOwnedCheckbox, Is.Not.Null);
                Assert.That(parameterTable, Is.Not.Null);
            });
        }
    }
}
