// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoriesTableTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.ReferenceData
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData;
    using COMETwebapp.Wrappers;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class CategoriesTableTestFixture
    {
        private TestContext context;
        private ICategoriesTableViewModel viewModel;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<ISessionService> sessionService;
        private Mock<IShowHideDeprecatedThingsService> showHideDeprecatedThingsService;
        private Assembler assembler;
        private Participant participant;
        private Participant participant1;
        private Person person;
        private Person person1;
        private readonly Uri uri = new("http://test.com");
        private ModelReferenceDataLibrary referenceDataLibrary;
        private EngineeringModelSetup engineeringSetup;
        private DomainOfExpertise domain;
        private Iteration iteration;
        private ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>> openIteration;
        private SiteDirectory siteDirectory;
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private SimpleQuantityKind sourceParameterType1;
        private CompoundParameterType sourceParameterType2;
        private Category elementDefinitionCategory1;
        private Category elementDefinitionCategory2;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            this.session = new Mock<ISession>();
            this.sessionService = new Mock<ISessionService>();
            this.showHideDeprecatedThingsService = new Mock<IShowHideDeprecatedThingsService>();
            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.showHideDeprecatedThingsService.Setup(x => x.ShowDeprecatedThings).Returns(true);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.context.Services.AddSingleton(this.sessionService);
            this.context.ConfigureDevExpressBlazor();

            this.assembler = new Assembler(this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.viewModel = new CategoriesTableViewModel(this.sessionService.Object, this.showHideDeprecatedThingsService.Object);

            this.context.Services.AddSingleton(this.viewModel);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                GivenName = "Test",
                Surname = "Person",
                DefaultDomain = this.domain,
                IsActive = true,
                IsDeprecated = false
            };

            this.person1 = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                GivenName = "Test1",
                Surname = "Person1",
                DefaultDomain = this.domain,
                IsDeprecated = true
            };

            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Person = this.person
            };

            this.participant1 = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Person = this.person1
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
                Participant = { this.participant, this.participant1 }
            };

            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.elementDefinitionCategory1 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Batteries", ShortName = "BAT" };
            this.elementDefinitionCategory1.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.elementDefinitionCategory1);
            this.elementDefinitionCategory2 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Reaction Wheels", ShortName = "RW", IsDeprecated = true };
            this.elementDefinitionCategory2.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.elementDefinitionCategory2);

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
            this.siteDirectory.Person.Add(this.person1);
            this.siteDirectory.Domain.Add(this.domain);
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteReferenceDataLibrary);

            this.sourceParameterType1 = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "Source 1",
                ShortName = "source1"
            };

            this.siteReferenceDataLibrary.ParameterType.Add(this.sourceParameterType1);

            this.sourceParameterType2 = new CompoundParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "Source 2",
                ShortName = "source2",
                IsDeprecated = true
            };

            this.sourceParameterType2.Component.Add(
                new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    ShortName = "Name",
                    ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri)
                });

            this.sourceParameterType2.Component.Add(
                new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    ShortName = "Value",
                    ParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri)
                });

            this.siteReferenceDataLibrary.ParameterType.Add(this.sourceParameterType2);

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenIterations).Returns(this.openIteration);
            this.session.Setup(x => x.Credentials).Returns(new Credentials("admin", "pass", this.uri));
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
        }

        [Test]
        public void VerifyOnInitialized()
        {
            var renderer = this.context.RenderComponent<CategoriesTable>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DataSource.Count, Is.EqualTo(2));
                Assert.That(renderer.Markup, Does.Contain(this.elementDefinitionCategory1.Name));
                Assert.That(renderer.Markup, Does.Contain(this.elementDefinitionCategory2.Name));
            });
        }

        [Test]
        public async Task VerifyDeprecatingCategory()
        {
            var renderer = this.context.RenderComponent<CategoriesTable>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DataSource.Count, Is.EqualTo(2));
                Assert.That(renderer.Markup, Does.Contain(this.elementDefinitionCategory1.Name));
                Assert.That(renderer.Markup, Does.Contain(this.elementDefinitionCategory2.Name));
            });

            var deprecateButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deprecateButton");
            var currentCategory = this.viewModel.Category;

            Assert.That(this.viewModel.IsOnDeprecationMode, Is.False);

            await renderer.InvokeAsync(deprecateButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnDeprecationMode, Is.True);
                Assert.That(this.viewModel.Category, Is.Not.EqualTo(currentCategory));
            });

            this.viewModel.Category = this.elementDefinitionCategory1;

            this.viewModel.OnConfirmButtonClick();

            Assert.That(this.viewModel.IsOnDeprecationMode, Is.False);
        }

        [Test]
        public async Task VerifyUnDeprecatingCategory()
        {
            var renderer = this.context.RenderComponent<CategoriesTable>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DataSource.Count, Is.EqualTo(2));
                Assert.That(renderer.Markup, Does.Contain(this.elementDefinitionCategory1.Name));
                Assert.That(renderer.Markup, Does.Contain(this.elementDefinitionCategory2.Name));
            });

            var deprecateButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "undeprecateButton");
            var currentCategory = this.viewModel.Category;

            Assert.That(this.viewModel.IsOnDeprecationMode, Is.False);

            await renderer.InvokeAsync(deprecateButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnDeprecationMode, Is.True);
                Assert.That(this.viewModel.Category, Is.Not.EqualTo(currentCategory));
            });

            this.viewModel.Category = this.elementDefinitionCategory2;

            this.viewModel.OnConfirmButtonClick();

            Assert.That(this.viewModel.IsOnDeprecationMode, Is.False);
        }

        [Test]
        public async Task VerifyAddingCategory()
        {
            var renderer = this.context.RenderComponent<CategoriesTable>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DataSource.Count, Is.EqualTo(2));
                Assert.That(renderer.Markup, Does.Contain(this.elementDefinitionCategory1.Name));
                Assert.That(renderer.Markup, Does.Contain(this.elementDefinitionCategory2.Name));
            });

            this.viewModel.Category = new Category
            {
                Name = "Cat1",
                ShortName = "TT",
                IsAbstract = true,
                IsDeprecated = false,
            };

            this.viewModel.SelectedReferenceDataLibrary = this.siteReferenceDataLibrary;
            this.viewModel.SelectedPermissibleClasses = new List<ClassKindWrapper>() { new (ClassKind.ElementDefinition) };

            await this.viewModel.AddingCategory();
            CDPMessageBus.Current.SendMessage(new ObjectChangedEvent(this.viewModel.Category, EventKind.Added));
            Assert.That(this.viewModel.Rows.Count, Is.EqualTo(2));
        }
    }
}
