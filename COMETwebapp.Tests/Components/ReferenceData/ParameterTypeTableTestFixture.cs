// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeTableTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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

namespace COMETwebapp.Tests.Components.ReferenceData
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    using BlazorStrap;

    using Bunit;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using COMETwebapp.Components.ReferenceData;
    using COMETwebapp.IterationServices;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.ReferenceData;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterTypeTableTestFixture
    {
        private TestContext context;
        private IParameterTypeTableViewModel viewModel;
        private Mock<ISession> session;
        private Mock<IIterationService> iterationService;
        private Mock<IPermissionService> permissionService;
        private ISessionAnchor sessionAnchor;
        private Assembler assembler;
        private Participant participant;
        private Participant participant1;
        private Person person;
        private Person person1;
        private readonly Uri uri = new Uri("http://test.com");
        private ModelReferenceDataLibrary referenceDataLibrary;
        private EngineeringModelSetup engineeringSetup;
        private DomainOfExpertise domain;
        private Iteration iteration;
        private ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>> openIteration;
        private SiteDirectory siteDirectory;
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private SimpleQuantityKind sourceParameterType_1;
        private CompoundParameterType sourceParameterType_2;


        [SetUp]
        public void SetUp()
        {
            context = new TestContext();

            session = new Mock<ISession>();
            sessionAnchor = new SessionAnchor() { Session = session.Object };

            iterationService = new Mock<IIterationService>();

            iterationService.Setup(x => x.GetNestedElementsByOption(It.IsAny<Iteration>(), It.IsAny<Guid>())).Returns(new List<NestedElement>());

            permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            context.Services.AddBlazorStrap();
            context.Services.AddAntDesign();
            context.Services.AddSingleton(sessionAnchor);
            context.Services.AddDevExpressBlazor();
            context.ConfigureDevExpressBlazor();
            context.Services.AddSingleton<ISelectionMediator, SelectionMediator>();

            assembler = new Assembler(uri);
            domain = new DomainOfExpertise(Guid.NewGuid(), assembler.Cache, uri);

            viewModel = new ParameterTypeTableViewModel(sessionAnchor);

            context.Services.AddSingleton(viewModel);

            person = new Person(Guid.NewGuid(), assembler.Cache, uri)
            {
                GivenName = "Test",
                Surname = "Person",
                DefaultDomain = domain,
                IsActive = true,
                IsDeprecated = false
            };

            person1 = new Person(Guid.NewGuid(), assembler.Cache, uri)
            {
                GivenName = "Test1",
                Surname = "Person1",
                DefaultDomain = domain,
                IsDeprecated = true
            };

            participant = new Participant(Guid.NewGuid(), assembler.Cache, uri)
            {
                Person = person
            };

            participant1 = new Participant(Guid.NewGuid(), assembler.Cache, uri)
            {
                Person = person1
            };

            referenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), assembler.Cache, uri)
            {
                ShortName = "ARDL"
            };

            engineeringSetup = new EngineeringModelSetup(Guid.NewGuid(), assembler.Cache, uri)
            {
                Name = "TestModel",
                RequiredRdl =
                {
                    referenceDataLibrary
                },
                Participant = { participant, participant1 }
            };
            
            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), assembler.Cache, this.uri);

            iteration = new Iteration(Guid.NewGuid(), assembler.Cache, uri)
            {
                Element =
                {
                    new ElementDefinition(Guid.NewGuid(), assembler.Cache, uri)
                    {
                        Name = "TestElement",
                        Owner = domain,
                        ShortName = "TE",
                        ContainedElement = { new ElementUsage(Guid.NewGuid(), assembler.Cache, uri) {
                            Owner = domain,
                            ShortName = "TEU",
                            ElementDefinition = new ElementDefinition(Guid.NewGuid(), assembler.Cache, uri)
                            {
                                Name = "TestElementUsage",
                                Owner = domain,
                                ShortName = "TEU"
                            }
                        } }
                    }
                },
                Container = new EngineeringModel(Guid.NewGuid(), assembler.Cache, uri)
                {
                    EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), assembler.Cache, uri)
                    {
                        RequiredRdl =
                        {
                            new ModelReferenceDataLibrary(Guid.NewGuid(), assembler.Cache, uri)
                            {
                                FileType =
                                {
                                    new FileType(Guid.NewGuid(), assembler.Cache, uri) { Extension = "tar" },
                                    new FileType(Guid.NewGuid(), assembler.Cache, uri) { Extension = "gz" },
                                    new FileType(Guid.NewGuid(), assembler.Cache, uri) { Extension = "zip" }
                                }
                            }
                        },
                        Participant = { participant }
                    }
                },
                IterationSetup = new IterationSetup(Guid.NewGuid(), assembler.Cache, uri)
                {
                    Container = engineeringSetup
                },
                DomainFileStore =
                {
                    new DomainFileStore(Guid.NewGuid(), assembler.Cache, uri) { Owner = domain }
                },
                Option =
                {
                    new Option(Guid.NewGuid(), assembler.Cache, uri)
                    {
                        Name = "TestOption",
                        ShortName = "TO"
                    }
                }
            };

            var option_1 = new Option(Guid.NewGuid(), assembler.Cache, uri)
            {
                ShortName = "OPT_1",
                Name = "Option1"
            };

            iteration.Option.Add(option_1);
            iteration.DefaultOption = option_1;

            engineeringSetup.IterationSetup.Add(iteration.IterationSetup);
            openIteration = new ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>>(
               new List<KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>>()
               {
                    new KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>(iteration, new Tuple<DomainOfExpertise, Participant>(domain, participant))
               });
            siteDirectory = new SiteDirectory(Guid.NewGuid(), assembler.Cache, uri)
            {
                Model = { engineeringSetup }
            };
            siteDirectory.Person.Add(person);
            siteDirectory.Person.Add(person1);
            siteDirectory.Domain.Add(domain);
            siteDirectory.SiteReferenceDataLibrary.Add(this.siteReferenceDataLibrary);

            this.sourceParameterType_1 = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "Source 1", 
                ShortName = "source1"
            };

            this.siteReferenceDataLibrary.ParameterType.Add(this.sourceParameterType_1);

            this.sourceParameterType_2 = new CompoundParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "Source 2", 
                ShortName = "source2",
                IsDeprecated = true
            };

             this.sourceParameterType_2.Component.Add(
                new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    ShortName = "Name",
                    ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri)
                });

            this.sourceParameterType_2.Component.Add(
                new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.uri) 
                {
                    ShortName = "Value",
                    ParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri)
                });

            this.siteReferenceDataLibrary.ParameterType.Add(this.sourceParameterType_2);

            session.Setup(x => x.Assembler).Returns(assembler);
            session.Setup(x => x.OpenIterations).Returns(openIteration);
            session.Setup(x => x.Credentials).Returns(new Credentials("admin", "pass", uri));
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            session.Setup(x => x.ActivePerson).Returns(person);
        }

        [Test]
        public async Task VerifyOnInitialized()
        {
            sessionAnchor.IsSessionOpen = true;
            sessionAnchor.ReadIteration(iteration.IterationSetup);
            sessionAnchor.CurrentEngineeringModelName = "model";

            var renderer = context.RenderComponent<ParameterTypeTable>();

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.DataSource.Count, Is.EqualTo(2));
                Assert.That(renderer.Markup, Does.Contain(sourceParameterType_1.Name));
                Assert.That(renderer.Markup, Does.Contain(sourceParameterType_2.Name));
                Assert.That(viewModel.IsAllowedToWrite, Is.True);
            });

            var checkBox = renderer.FindComponents<DxCheckBox<bool>>().FirstOrDefault(x => x.Instance.Id == "hideDeprecatedItems");

            Assert.Multiple(() =>
            {
                Assert.That(checkBox.Instance.Checked, Is.False);
                Assert.That(renderer.Markup, Does.Contain(sourceParameterType_1.Name));
                Assert.That(renderer.Markup, Does.Contain(sourceParameterType_2.Name));
            });

            await renderer.InvokeAsync(() => checkBox.Instance.Checked = true);
            await renderer.InvokeAsync(() => renderer.Instance.HideOrShowDeprecatedItems(true));

            Assert.Multiple(() =>
            {
                Assert.That(checkBox.Instance.Checked, Is.True);
                Assert.That(renderer.Markup, Does.Contain(sourceParameterType_1.Name));
                Assert.That(renderer.Markup, Does.Not.Contain(sourceParameterType_2.Name));
            });
        }
    }
}
