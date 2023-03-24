// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompoundParameterTypeEditorTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.Shared.ParameterTypeEditors
{
    using System;
    using System.Collections.Generic;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Components.Shared.ParameterTypeEditors;
    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Components.Viewer.PropertiesPanel;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class CompoundParameterTypeEditorTestFixture
    {
        private TestContext context;
        private IRenderedComponent<CompoundParameterTypeEditor> renderedComponent;
        private CompoundParameterTypeEditor editor;
        private Mock<IParameterEditorBaseViewModel<CompoundParameterType>> viewModelMock;
        private Mock<IParameterTypeEditorSelectorViewModel> parameterEditorSelectorViewModelMock;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var compoundValues = new List<string> { "1", "0", "3" };

            var parameterValueSet = new ParameterValueSet()
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(compoundValues),
            };

            var compoundData = new OrderedItemList<ParameterTypeComponent>(null)
            {
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "firstValue",
                    Scale = new OrdinalScale()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                },
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "secondValue",
                    Scale = new OrdinalScale()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                },
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "thirdValue",
                    Scale = new OrdinalScale()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                }
            };

            var parametertype = new CompoundParameterType()
            {
                Iid = Guid.NewGuid(),
            };

            parametertype.Component.AddRange(compoundData);

            this.viewModelMock = new Mock<IParameterEditorBaseViewModel<CompoundParameterType>>();
            this.parameterEditorSelectorViewModelMock = new Mock<IParameterTypeEditorSelectorViewModel>();
            this.viewModelMock.As<ICompoundParameterTypeEditorViewModel>();
            this.viewModelMock.As<ICompoundParameterTypeEditorViewModel>().Setup(x => x.IsOnEditMode).Returns(true);
            this.viewModelMock.As<ICompoundParameterTypeEditorViewModel>().Setup(x => x.CreateParameterTypeEditorSelectorViewModel(It.IsAny<ParameterType>(), It.IsAny<int>())).Returns(this.parameterEditorSelectorViewModelMock.Object);
            this.viewModelMock.Setup(x => x.ParameterType).Returns(parametertype);
            this.viewModelMock.Setup(x => x.ValueSet).Returns(parameterValueSet);
            this.viewModelMock.Setup(x => x.ValueArray).Returns(parameterValueSet.Manual);

            this.renderedComponent = this.context.RenderComponent<CompoundParameterTypeEditor>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });

            this.editor = this.renderedComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderedComponent, Is.Not.Null);
                Assert.That(this.editor, Is.Not.Null);
                Assert.That(this.editor.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyThatOrientationIsRendered()
        {
            var parameterType = new CompoundParameterType()
            {
                Iid = Guid.NewGuid(),
                ShortName = SceneSettings.OrientationShortName
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });

            var component = this.renderedComponent.FindComponent<OrientationComponent>();
            Assert.That(component, Is.Not.Null);
        }
    }
}
