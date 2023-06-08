// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PublicationsTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.Components
{
    using System.Reflection;

    using Bunit;
    using Bunit.TestDoubles;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.RegistrationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.VersionService;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Publications;
    using COMET.Web.Common.ViewModels.Components.Publications.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class PublicationsTestFixture
    {
        private TestContext context;
        private IRenderedComponent<Publications> renderer;
        private Mock<IPublicationsViewModel> viewModel;
        

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();
            
            this.viewModel = new Mock<IPublicationsViewModel>();
            this.viewModel.Setup(x => x.ModelName).Returns("Test");
            this.viewModel.Setup(x => x.DataSource).Returns("http://localhost:5000");
            this.viewModel.Setup(x => x.IterationName).Returns("Iteration1");
            this.viewModel.Setup(x => x.PersonName).Returns("TestPerson");
            this.viewModel.Setup(x => x.DomainName).Returns("Sys");

            this.viewModel.Setup(x => x.Rows).Returns(new SourceList<PublicationRowViewModel>());
            this.viewModel.Setup(x => x.PublishableParameters).Returns(new List<ParameterOrOverrideBase>());

            this.renderer = this.context.RenderComponent<Publications>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });
        }

        [Test]
        public void VerifyButtonToPublish()
        {
            var button = this.renderer.FindComponent<DxButton>();

            Assert.Multiple(() =>
            {
                Assert.That(button, Is.Not.Null);
                Assert.That(button.Instance.Enabled, Is.False);
            });

            this.viewModel.Setup(x => x.CanPublish).Returns(true);

            this.renderer.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });

            button = this.renderer.FindComponent<DxButton>();

            Assert.Multiple(() =>
            {
                Assert.That(button, Is.Not.Null);
                Assert.That(button.Instance.Enabled, Is.True);
            });
        }

        [Test]
        public void VerifyInformation()
        {
            var dataContainer = this.renderer.Find("#publication-data-container");

            Assert.Multiple(() =>
            {
                Assert.That(dataContainer, Is.Not.Null);
                Assert.That(dataContainer.ChildElementCount, Is.EqualTo(5));
            });

            var childs = dataContainer.Children;
            
            Assert.Multiple(() =>
            {
                Assert.That(childs.First(x => x.InnerHtml == "Model: Test"), Is.Not.Null);
                Assert.That(childs.First(x => x.InnerHtml == "Data Source: http://localhost:5000"), Is.Not.Null);
                Assert.That(childs.First(x => x.InnerHtml == "Iteration: Iteration1"), Is.Not.Null);
                Assert.That(childs.First(x => x.InnerHtml == "Person: TestPerson"), Is.Not.Null);
                Assert.That(childs.First(x => x.InnerHtml == "Domain Of Expertise: Sys"), Is.Not.Null);
            });
        }

        [Test]
        public void VerifyContent()
        {
            var content = this.renderer.Find("#empty-content");

            Assert.That(content.InnerHtml, Is.EqualTo("No new values available to publish"));

            var doe = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                Name = "Sys"
            };

            var elementBase = new ElementDefinition
            {
                Iid = Guid.NewGuid()
            };

            var parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = doe,
                ValueSet =
                {
                    new ParameterValueSet
                    {
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        Manual = new ValueArray<string>(new []{"-"})
                    }
                },
                ParameterType = new BooleanParameterType
                {
                    Iid = Guid.NewGuid(),
                    Name = "IsDeprecated"
                }
            };

            elementBase.Parameter.Add(parameter);
            
            var publishableParameters = new List<ParameterOrOverrideBase>
            {
                parameter,
            };

            var publicationRows = new SourceList<PublicationRowViewModel>();
            publicationRows.Add(new PublicationRowViewModel(parameter, parameter.ValueSet.First()));
            
            this.viewModel.Setup(x => x.PublishableParameters).Returns(publishableParameters);
            this.viewModel.Setup(x => x.Rows).Returns(publicationRows);

            this.renderer.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });

            var grid = this.renderer.FindComponent<DxGrid>();

            Assert.Multiple(() =>
            {
                Assert.That(grid.Instance, Is.Not.Null);
                Assert.That(grid.Instance.GetVisibleRowCount(), Is.EqualTo(1));
            });
        }
    }
}
