// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PublicationsViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.ViewModels.Components.Publications
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Publications;
    using COMET.Web.Common.ViewModels.Components.Publications.Rows;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PublicationsViewModelTestFixture
    {
        private PublicationsViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Parameter parameter;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.viewModel = new PublicationsViewModel(this.sessionService.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows, Is.Empty);
                Assert.That(this.viewModel.PublishableParameters, Is.Empty);
                Assert.That(this.viewModel.CurrentIteration, Is.Null);
                Assert.That(this.viewModel.CanPublish, Is.False);
                Assert.That(this.viewModel.DataSource, Is.Null.Or.Empty);
                Assert.That(this.viewModel.PersonName, Is.Null.Or.Empty);
                Assert.That(this.viewModel.ModelName, Is.Null.Or.Empty);
                Assert.That(this.viewModel.IterationName, Is.Null.Or.Empty);
                Assert.That(this.viewModel.DomainName, Is.Null.Or.Empty);
            });
        }

        [Test]
        public void VerifyUpdateProperties()
        {
            var session = new Mock<ISession>();
            session.Setup(x => x.DataSourceUri).Returns("http://localhost:5000");

            var person = new Person
            {
                Iid = Guid.NewGuid(),
                GivenName = "Test",
                Surname = "Person"
            };

            session.Setup(x => x.ActivePerson).Returns(person);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            
            var iteration = new Iteration();

            var doe = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                Name = "Sys"
            };

            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(doe);

            var elementBase = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
            };

            this.parameter = new Parameter
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

            elementBase.Parameter.Add(this.parameter);

            iteration.Element.Add(elementBase);
            iteration.TopElement = elementBase;

            var modelSetup = new EngineeringModelSetup
            {
                Iid = Guid.NewGuid(),
                Name = "TestModel"
            };

            var model = new EngineeringModel
            {
                Iid = Guid.NewGuid(),
                EngineeringModelSetup = modelSetup
            };

            var iterationSetup = new IterationSetup
            {
                Iid = Guid.NewGuid(),
            };

            iteration.IterationSetup = iterationSetup;
            modelSetup.IterationSetup.Add(iterationSetup);
            model.Iteration.Add(iteration);
            
            this.viewModel.UpdateProperties(iteration);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows, Is.Not.Empty);
                Assert.That(this.viewModel.PublishableParameters, Is.Not.Empty);
                Assert.That(this.viewModel.CurrentIteration, Is.EqualTo(iteration));
                Assert.That(this.viewModel.DataSource, Is.EqualTo("http://localhost:5000"));
                Assert.That(this.viewModel.PersonName, Is.EqualTo("Test Person"));
                Assert.That(this.viewModel.ModelName, Is.EqualTo("TestModel"));
                Assert.That(this.viewModel.IterationName, Is.EqualTo(iterationSetup.IterationNumber.ToString()));
                Assert.That(this.viewModel.DomainName, Is.EqualTo("Sys"));
            });
        }

        [Test]
        public async Task VerifyExecutePublish()
        {
            this.VerifyUpdateProperties();
            await this.viewModel.ExecutePublish();

            this.sessionService.Verify(x=>x.CreateThing(It.IsAny<Iteration>(), It.IsAny<Publication>()), Times.Never);

            this.viewModel.CanPublish = true;
            this.viewModel.SelectedDataItems = new List<object>() { new PublicationRowViewModel(this.parameter, this.parameter.ValueSets.First()) };

            await this.viewModel.ExecutePublish();

            this.sessionService.Verify(x => x.CreateThing(It.IsAny<Iteration>(), It.IsAny<Publication>()), Times.Once);
        }
    }
}
