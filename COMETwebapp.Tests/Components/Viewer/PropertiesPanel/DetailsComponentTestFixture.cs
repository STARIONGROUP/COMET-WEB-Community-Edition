// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DetailsComponentTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.Viewer.PropertiesPanel
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components.ParameterTypeEditors;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Viewer.PropertiesPanel;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class DetailsComponentTestFixture
    {
        private TestContext context;
        private DetailsComponent details;
        private IRenderedComponent<DetailsComponent> renderedComponent;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var viewModel = new Mock<IDetailsComponentViewModel>();
            viewModel.Setup(x => x.ParameterType).Returns(new TextParameterType());

            this.renderedComponent = this.context.RenderComponent<DetailsComponent>(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });
            
            this.details = this.renderedComponent.Instance;
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
                Assert.That(this.details, Is.Not.Null);
                Assert.That(this.details.ViewModel, Is.Not.Null);
            });
        }
        
        [Test]
        public void VerifyThatDetailsAreRendered()
        {
            var viewModel = new Mock<IDetailsComponentViewModel>();

            var parameter = new Parameter()
            {
                ParameterType = new EnumerationParameterType()
                {
                    ShortName = "EnumerationType",
                    ValueDefinition =
                    {
                        new EnumerationValueDefinition(){Name = "True"},
                        new EnumerationValueDefinition(){Name = "False"},
                    }
                }
            };

            viewModel.Setup(x => x.IsVisible).Returns(true);
            viewModel.Setup(x => x.ParameterType).Returns(parameter.ParameterType);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            var result = this.renderedComponent.FindComponent<ParameterTypeEditorSelector>();

            Assert.That(result, Is.Not.Null);
        }
    }
}
