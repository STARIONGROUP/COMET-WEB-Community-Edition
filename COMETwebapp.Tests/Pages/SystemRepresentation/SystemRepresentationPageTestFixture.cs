// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemTreeTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Page.SystemRepresentation
{
    using BlazorStrap;
    using Bunit;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Model;
    using COMETwebapp.Pages.SystemRepresentation;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Pages.SystemRepresentation;
    using DevExpress.Blazor;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using TestContext = Bunit.TestContext;


    [TestFixture]
    public class SystemRepresentationPageTestFixture
    {
        private TestContext context;
        private ISystemRepresentationPageViewModel viewModel;
        private ISystemTreeViewModel systemTreeViewModel;
        private Mock<ISession> session;
        private Mock<IIterationService> iterationService;
        private ISessionAnchor sessionAnchor;
        private Assembler assembler;
        private Participant participant;
        private Person person;
        private readonly Uri uri = new Uri("http://test.com");
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
            this.sessionAnchor = new SessionAnchor() { Session = this.session.Object };

            this.iterationService = new Mock<IIterationService>();

            this.iterationService.Setup(x => x.GetNestedElementsByOption(It.IsAny<Iteration>(), It.IsAny<Guid>())).Returns(new List<NestedElement>());

            this.context.Services.AddBlazorStrap();
            this.context.Services.AddAntDesign();
            this.context.Services.AddSingleton(this.sessionAnchor);
            this.context.Services.AddDevExpressBlazor();
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton<ISelectionMediator, SelectionMediator>();

            this.assembler = new Assembler(this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.systemTreeViewModel = new SystemTreeViewModel()
            {
                SystemNodes = new List<SystemNode>()
            };

            this.viewModel = new SystemRepresentationPageViewModel(this.systemTreeViewModel, null, iterationService.Object, sessionAnchor);

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

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Element =
                {
                    new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
                    {
                        Name = "TestElement",
                        Owner = this.domain,
                        ShortName = "TE",
                        ContainedElement = { new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri) {
                            Owner = this.domain,
                            ShortName = "TEU",
                            ElementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
                            {
                                Name = "TestElementUsage",
                                Owner = this.domain,
                                ShortName = "TEU"
                            }
                        } }
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

            var option_1 = new Option(Guid.NewGuid(), this.assembler.Cache, uri)
            {
                ShortName = "OPT_1",
                Name = "Option1"
            };

            this.iteration.Option.Add(option_1);
            this.iteration.DefaultOption = option_1;

            this.engineeringSetup.IterationSetup.Add(this.iteration.IterationSetup);
            this.openIteration = new ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>>(
               new List<KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>>()
               {
                    new KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>(this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant))
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

        [Test]
        public void VerifyOnInitialized()
        {
            var renderer = this.context.RenderComponent<SystemRepresentation>();

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance, Is.Not.Null);
                Assert.That(renderer.Markup, Does.Contain("You have to open a model first"));
            });

            this.sessionAnchor.IsSessionOpen = true;
            this.sessionAnchor.ReadIteration(this.iteration.IterationSetup);
            this.sessionAnchor.CurrentEngineeringModelName = "model";

            renderer.Render();

            var filterAndDomainComboBox = renderer.FindComponents<DxComboBox<string, string>>();

            Assert.Multiple(() =>
            {
                Assert.That(filterAndDomainComboBox, Is.Not.Null);
                Assert.That(filterAndDomainComboBox.Count, Is.EqualTo(2));
            });

            this.viewModel.OnOptionFilterChange("Option1");

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SystemTreeViewModel.SystemNodes, Is.Not.Null);
                Assert.That(this.viewModel.SystemTreeViewModel.SystemNodes.Count, Is.EqualTo(1));
            });

            this.viewModel.OnDomainFilterChange(this.domain.Name);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SystemTreeViewModel.SystemNodes, Is.Not.Null);
                Assert.That(this.viewModel.SystemTreeViewModel.SystemNodes.Count, Is.EqualTo(1));
            });
        }
    }
}
