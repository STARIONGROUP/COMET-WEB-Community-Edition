// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DateParameterTypeEditorTestFixture.cs" company="RHEA System S.A.">
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
    using COMET.Web.Common.Tests.Helpers;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class DateParameterTypeEditorTestFixture
    {
        private TestContext context;
        private IRenderedComponent<DateParameterTypeEditor> renderedComponent;
        private DateParameterTypeEditor editor;
        private Mock<IParameterEditorBaseViewModel<DateParameterType>> viewModelMock;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new List<string> { "0", "0", "0" })
            };

            this.viewModelMock = new Mock<IParameterEditorBaseViewModel<DateParameterType>>();
            this.viewModelMock.Setup(x => x.ValueSet).Returns(parameterValueSet);
            this.viewModelMock.Setup(x => x.ValueArray).Returns(parameterValueSet.Manual);

            this.renderedComponent = this.context.RenderComponent<DateParameterTypeEditor>(parameters => { parameters.Add(p => p.ViewModel, this.viewModelMock.Object); });

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
    }
}
