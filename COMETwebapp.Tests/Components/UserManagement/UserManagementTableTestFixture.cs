// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserManagementTableTestFixture.cs" company="RHEA System S.A.">
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

#pragma warning disable BL0005
namespace COMETwebapp.Tests.Components.UserManagement
{
    using System.Collections.Concurrent;

    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.UserManagement;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.UserManagement;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class UserManagementTableTestFixture
    {
        private TestContext context;
        private IUserManagementTableViewModel viewModel;
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
        private ICDPMessageBus messageBus;

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
            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());
            this.context.Services.AddSingleton(configuration.Object);
            this.messageBus = new CDPMessageBus();

            this.assembler = new Assembler(this.uri, this.messageBus);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.viewModel = new UserManagementTableViewModel(this.sessionService.Object, this.showHideDeprecatedThingsService.Object, this.messageBus);

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

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenIterations).Returns(this.openIteration);
            this.session.Setup(x => x.Credentials).Returns(new Credentials("admin", "pass", this.uri));
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyActivatingPerson()
        {
            var renderer = this.context.RenderComponent<UserManagementTable>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DataSource.Count, Is.EqualTo(2));
                Assert.That(renderer.Markup, Does.Contain(this.person.Name));
                Assert.That(renderer.Markup, Does.Contain(this.person1.Name));
            });

            var checkBox = renderer.FindComponents<DxCheckBox<bool>>()
                .First(x => x.Instance.Id == "activatePerson");

            Assert.Multiple(() =>
            {
                Assert.That(checkBox.Instance.Checked, Is.True);
                Assert.That(renderer.Markup, Does.Contain(this.person.Name));
                Assert.That(renderer.Markup, Does.Contain(this.person1.Name));
            });

            await renderer.InvokeAsync(() => checkBox.Instance.Checked = false);

            Assert.Multiple(() => { Assert.That(checkBox.Instance.Checked, Is.False); });
        }

        [Test]
        public async Task VerifyAddingPerson()
        {
            var renderer = this.context.RenderComponent<UserManagementTable>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DataSource.Count, Is.EqualTo(2));
                Assert.That(renderer.Markup, Does.Contain(this.person.Name));
                Assert.That(renderer.Markup, Does.Contain(this.person1.Name));
            });

            this.viewModel.Person = new Person
            {
                GivenName = "Test",
                Surname = "Test",
                ShortName = "TT",
                IsActive = true,
                IsDeprecated = false
            };

            await this.viewModel.AddingPerson();
            this.messageBus.SendMessage(new ObjectChangedEvent(this.viewModel.Person, EventKind.Added));

            Assert.Multiple(() => { Assert.That(this.viewModel.Rows.Count, Is.EqualTo(2)); });
        }

        [Test]
        public async Task VerifyDeprecatingPerson()
        { 
            var renderer = this.context.RenderComponent<UserManagementTable>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DataSource.Count, Is.EqualTo(2));
                Assert.That(renderer.Markup, Does.Contain(this.person.Name));
                Assert.That(renderer.Markup, Does.Contain(this.person1.Name));
            });

            var deprecateButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deprecateButton");
            var currentPerson = this.viewModel.Person;

            Assert.That(this.viewModel.IsOnDeprecationMode, Is.False);

            await renderer.InvokeAsync(deprecateButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnDeprecationMode, Is.True);
                Assert.That(this.viewModel.Person, Is.Not.EqualTo(currentPerson));
            });

            this.viewModel.Person = this.person;

            await this.viewModel.OnConfirmButtonClick();

            Assert.That(this.viewModel.IsOnDeprecationMode, Is.False);
        }

        [Test]
        public void VerifyOnInitialized()
        {
            var renderer = this.context.RenderComponent<UserManagementTable>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DataSource.Count, Is.EqualTo(2));
                Assert.That(renderer.Markup, Does.Contain(this.person.Name));
                Assert.That(renderer.Markup, Does.Contain(this.person1.Name));
            });
        }

        [Test]
        public async Task VerifyUnDeprecatingPerson()
        {
            var renderer = this.context.RenderComponent<UserManagementTable>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DataSource.Count, Is.EqualTo(2));
                Assert.That(renderer.Markup, Does.Contain(this.person.Name));
                Assert.That(renderer.Markup, Does.Contain(this.person1.Name));
            });

            var undeprecateButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "undeprecateButton");
            var currentPerson = this.viewModel.Person;

            Assert.That(this.viewModel.IsOnDeprecationMode, Is.False);

            await renderer.InvokeAsync(undeprecateButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnDeprecationMode, Is.True);
                Assert.That(this.viewModel.Person, Is.Not.EqualTo(currentPerson));
            });

            this.viewModel.Person = this.person1;

            await this.viewModel.OnConfirmButtonClick();

            Assert.That(this.viewModel.IsOnDeprecationMode, Is.False);
        }

        [Test]
        public void VerifyRecordChange()
        {
            this.context.RenderComponent<UserManagementTable>();

            var personTest = new Person()
            {
                Iid = Guid.NewGuid()
            };

            this.messageBus.SendObjectChangeEvent(personTest, EventKind.Added);
            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate));

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Person, EventKind.Removed);
            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate));

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Person, EventKind.Updated);
            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate));

            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(2));
        }
    }
}
