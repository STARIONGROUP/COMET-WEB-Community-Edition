// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompoundParameterTypeEditorTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Components.ParameterTypeEditors
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Components.ParameterTypeEditors;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using DevExpress.Blazor;

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

            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(compoundValues)
            };

            var compoundData = new OrderedItemList<ParameterTypeComponent>(null)
            {
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "firstValue",
                    Scale = new OrdinalScale
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                },
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "secondValue",
                    Scale = new OrdinalScale
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                },
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "thirdValue",
                    Scale = new OrdinalScale
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                }
            };

            var parametertype = new CompoundParameterType
            {
                Iid = Guid.NewGuid()
            };

            parametertype.Component.AddRange(compoundData);

            this.viewModelMock = new Mock<IParameterEditorBaseViewModel<CompoundParameterType>>();
            this.parameterEditorSelectorViewModelMock = new Mock<IParameterTypeEditorSelectorViewModel>();
            this.viewModelMock.As<ICompoundParameterTypeEditorViewModel>().Setup(x => x.CreateParameterTypeEditorSelectorViewModel(It.IsAny<ParameterType>(), It.IsAny<int>(), It.IsAny<MeasurementScale>(), It.IsAny<EventCallback<(IValueSet, int)>>())).Returns(this.parameterEditorSelectorViewModelMock.Object);
            this.viewModelMock.Setup(x => x.ParameterType).Returns(parametertype);
            this.viewModelMock.Setup(x => x.ValueSet).Returns(parameterValueSet);
            this.viewModelMock.Setup(x => x.ValueArray).Returns(parameterValueSet.Manual);

            this.renderedComponent = this.context.RenderComponent<CompoundParameterTypeEditor>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
                parameters.Add(p => p.BindValueMode, BindValueMode.OnInput);
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
            var parameterType = new CompoundParameterType
            {
                Iid = Guid.NewGuid(),
                ShortName = ConstantValues.OrientationShortName
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.renderedComponent.SetParametersAndRender(parameters => { parameters.Add(p => p.ViewModel, this.viewModelMock.Object); });

            var component = this.renderedComponent.FindComponent<OrientationComponent>();
            Assert.That(component, Is.Not.Null);
        }
    }
}
