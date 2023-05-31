// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemTreeTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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

namespace COMETwebapp.Tests.Components.SystemRepresentation
{
    using System.Collections.Concurrent;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.DAL;
    
    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.SystemRepresentation;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class SystemRepresentationPageTestFixture
    {
        private TestContext context;
        private ISystemRepresentationBodyViewModel viewModel;
        private Mock<ISession> session;
        private ISessionService sessionService;
        private Assembler assembler;
        private Participant participant;
        private Person person;
        private readonly Uri uri = new("http://test.com");
        private ModelReferenceDataLibrary referenceDataLibrary;
        private EngineeringModelSetup engineeringSetup;
        private DomainOfExpertise domain;
        private Iteration iteration;
        private ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>> openIteration;
        private SiteDirectory siteDirectory;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            this.session = new Mock<ISession>();
            this.sessionService = new SessionService { Session = this.session.Object };

            this.context.Services.AddSingleton(this.sessionService);
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddAntDesign();
            this.context.Services.AddSingleton<ISelectionMediator, SelectionMediator>();

            this.assembler = new Assembler(this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.viewModel = new SystemRepresentationBodyViewModel(this.sessionService);

            this.context.Services.AddSingleton(this.viewModel);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Person = this.person
            };

            this.referenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = "ARDL"
            };

            this.engineeringSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "TestModel",
                RequiredRdl =
                {
                    this.referenceDataLibrary
                },
                Participant = { this.participant }
            };

            var enumerationValues = new List<string> { "cube", "sphere", "cylinder" };

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Element =
                {
                    new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
                    {
                        Name = "TestElement",
                        Owner = this.domain,
                        ShortName = "TE",
                        ContainedElement =
                        {
                            new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri)
                            {
                                Owner = this.domain,
                                ShortName = "TEU",
                                ElementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
                                {
                                    Name = "TestElementUsage",
                                    Owner = this.domain,
                                    ShortName = "TEU"
                                }
                            }
                        },
                        Parameter =
                        {
                            new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
                            {
                                Owner = this.domain,
                                ParameterType = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri)
                                {
                                    Name = "paramType1",
                                    ShortName = "BPT"
                                },
                                ValueSet = 
                                { 
                                    new ParameterValueSet()
                                    {
                                        Published = new ValueArray<string>(enumerationValues),
                                        Manual = new ValueArray<string>(enumerationValues)
                                    }
                                },
                                Scale = new OrdinalScale
                                {
                                    Iid = Guid.NewGuid(),
                                    ShortName = "m"
                                }
                            }
                        }
                    }
                },
                TopElement = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    Name = "TestElement",
                    Owner = this.domain,
                    ShortName = "TE",
                    ContainedElement =
                        {
                            new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri)
                            {
                                Owner = this.domain,
                                ShortName = "TEU",
                                ElementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
                                {
                                    Name = "TestElementUsage",
                                    Owner = this.domain,
                                    ShortName = "TEU"
                                }
                            }
                        }
                },
                Container = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
                    {
                        RequiredRdl =
                        {
                            new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri)
                            {
                                FileType =
                                {
                                    new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Extension = "tar" },
                                    new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Extension = "gz" },
                                    new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Extension = "zip" }
                                }
                            }
                        },
                        Participant = { this.participant }
                    }
                },
                IterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    Container = this.engineeringSetup
                },
                DomainFileStore =
                {
                    new DomainFileStore(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.domain }
                },
                Option =
                {
                    new Option(Guid.NewGuid(), this.assembler.Cache, this.uri)
                    {
                        Name = "TestOption",
                        ShortName = "TO"
                    }
                }
            };

            var option1 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = "OPT_1",
                Name = "Option1"
            };

            this.iteration.Option.Add(option1);
            this.iteration.DefaultOption = option1;

            this.engineeringSetup.IterationSetup.Add(this.iteration.IterationSetup);

            this.openIteration = new ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>>(
                new List<KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>>
                {
                    new(this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant))
                });

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Model = { this.engineeringSetup }
            };

            this.siteDirectory.Person.Add(this.person);
            this.siteDirectory.Domain.Add(this.domain);

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenIterations).Returns(this.openIteration);
            this.session.Setup(x => x.Credentials).Returns(new Credentials("admin", "pass", this.uri));
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyOnInitialized()
        {
            var renderer = this.context.RenderComponent<SystemRepresentationBody>(parameters =>
            {
                parameters.Add(p => p.CurrentIteration, this.iteration);
            });

            var option1 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = "OPT_1",
                Name = "Option1"
            };

            var optionFilterCombo = renderer.FindComponent<OptionSelector>();

            Assert.That(optionFilterCombo, Is.Not.Null);

            this.viewModel.OptionSelector.SelectedOption = option1;
        }

        [Test]
        public async Task VerifySelectNode()
        {
            var renderer = this.context.RenderComponent<SystemRepresentationBody>(parameters =>
            {
                parameters.Add(p => p.CurrentIteration, this.iteration);
            });

            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.Elements.Clear();
            this.viewModel.Elements.Add(this.iteration.Element.First());

            this.viewModel.SelectElement(this.viewModel.RootNode);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ElementDefinitionDetailsViewModel.SelectedSystemNode, Is.Not.Null);
                Assert.That(this.viewModel.ElementDefinitionDetailsViewModel.Rows.Count, Is.EqualTo(1));
                Assert.That(this.viewModel.ElementDefinitionDetailsViewModel.Rows.First().ParameterTypeName, Is.EqualTo(this.iteration.Element.First().Parameter.First().ParameterType.Name));
                Assert.That(this.viewModel.ElementDefinitionDetailsViewModel.Rows.First().ShortName, Is.EqualTo(this.iteration.Element.First().Parameter.First().ParameterType.ShortName));
                Assert.That(this.viewModel.ElementDefinitionDetailsViewModel.Rows.First().Owner, Is.EqualTo(this.iteration.Element.First().Parameter.First().Owner.ShortName));
                Assert.That(this.viewModel.ElementDefinitionDetailsViewModel.Rows.First().PublishedValue, Is.Not.Null);
                Assert.That(this.viewModel.ElementDefinitionDetailsViewModel.Rows.First().ActualValue, Is.Not.Null);
                Assert.That(this.viewModel.ElementDefinitionDetailsViewModel.Rows.First().SwitchValue, Is.Not.Null);
            });
        }
    }
}
