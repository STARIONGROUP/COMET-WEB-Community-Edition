// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeEditorSelectorTestFixture.cs" company="RHEA System S.A.">
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

    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Shared.ParameterTypeEditors;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterTypeEditorSelectorTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ParameterTypeEditorSelector> renderedComponent;
        private ParameterTypeEditorSelector editorSelector;
        private Mock<IParameterTypeEditorSelectorViewModel<ParameterType>> viewModelMock;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.viewModelMock = new Mock<IParameterTypeEditorSelectorViewModel<ParameterType>>();
            
            this.renderedComponent = this.context.RenderComponent<ParameterTypeEditorSelector>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });
            
            this.editorSelector = this.renderedComponent.Instance;
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderedComponent, Is.Not.Null);
                Assert.That(this.editorSelector, Is.Not.Null);
                Assert.That(this.editorSelector.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyThatBooleanParameterTypeEditorIsRendered()
        {
            var parameterType = new BooleanParameterType()
            {
                Iid = Guid.NewGuid(),
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });

            var editor = this.renderedComponent.FindComponent<BooleanParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatCompoundParameterTypeEditorIsRendered()
        {
            var parameterType = new CompoundParameterType()
            {
                Iid = Guid.NewGuid(),
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });

            var editor = this.renderedComponent.FindComponent<CompoundParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatDateParameterTypeEditorIsRendered()
        {
            var parameterType = new DateParameterType()
            {
                Iid = Guid.NewGuid(),
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });

            var editor = this.renderedComponent.FindComponent<DateParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatDateTimeParameterTypeEditorIsRendered()
        {
            var parameterType = new DateTimeParameterType()
            {
                Iid = Guid.NewGuid(),
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });

            var editor = this.renderedComponent.FindComponent<DateTimeParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatEnumerationParameterTypeEditorIsRendered()
        {
            var parameterType = new EnumerationParameterType()
            {
                Iid = Guid.NewGuid(),
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });

            var editor = this.renderedComponent.FindComponent<EnumerationParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatQuantityKindParameterTypeEditorIsRendered()
        {
            var parameterType = new SimpleQuantityKind()
            {
                Iid = Guid.NewGuid(),
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });

            var editor = this.renderedComponent.FindComponent<QuantityKindParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatTextParameterTypeEditorIsRendered()
        {
            var parameterType = new TextParameterType()
            {
                Iid = Guid.NewGuid(),
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });

            var editor = this.renderedComponent.FindComponent<TextParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatTimeOfDayParameterTypeEditorIsRendered()
        {
            var parameterType = new TimeOfDayParameterType()
            {
                Iid = Guid.NewGuid(),
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });

            var editor = this.renderedComponent.FindComponent<TimeOfDayParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }
    }
}
