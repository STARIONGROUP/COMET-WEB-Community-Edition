// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditRequirementThingViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.ViewModels.Components.RequirementsEditor
{
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EditRequirementThingViewModelTestFixture
    {
        private EditRequirementThingViewModel viewModel;
        private CDPMessageBus messageBus;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private DomainOfExpertise otherDomain;
        private Category requirementCategory;
        private Category groupCategory;
        private RequirementsGroup group;
        private NaturalLanguage english;
        private NaturalLanguage french;
        private TextParameterType textParameterType;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            var sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();

            this.domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.otherDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "THE", Name = "Thermal" };
            this.requirementCategory = new Category { Iid = Guid.NewGuid(), ShortName = "REQ", Name = "Requirement", PermissibleClass = { ClassKind.Requirement } };
            this.groupCategory = new Category { Iid = Guid.NewGuid(), ShortName = "GRP", Name = "Group", PermissibleClass = { ClassKind.RequirementsGroup } };

            this.textParameterType = new TextParameterType { Iid = Guid.NewGuid(), ShortName = "txt", Name = "Text" };
            var rdl = new SiteReferenceDataLibrary { ShortName = "siteRdl", Name = "Site RDL", DefinedCategory = { this.requirementCategory, this.groupCategory }, ParameterType = { this.textParameterType } };

            this.english = new NaturalLanguage { LanguageCode = "en-GB", Name = "English" };
            this.french = new NaturalLanguage { LanguageCode = "fr", Name = "French" };

            var siteDirectory = new SiteDirectory
            {
                Domain = { this.domain, this.otherDomain },
                SiteReferenceDataLibrary = { rdl },
                NaturalLanguage = { this.english, this.french }
            };

            var modelSetup = new EngineeringModelSetup
            {
                Name = "Model", ShortName = "MOD", ActiveDomain = { this.domain, this.otherDomain },
                RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = rdl } }
            };

            siteDirectory.Model.Add(modelSetup);

            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            sessionService.Setup(x => x.Session).Returns(session.Object);
            sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domain);

            this.group = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "GRP", Name = "Group", Owner = this.domain };
            this.iteration = new Iteration { Iid = Guid.NewGuid(), Container = new EngineeringModel { EngineeringModelSetup = modelSetup } };

            this.viewModel = new EditRequirementThingViewModel(sessionService.Object, this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
        }

        [Test]
        public void VerifyInitializeViewModelRejectsNullArguments()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => this.viewModel.InitializeViewModel(null, this.iteration, []), Throws.ArgumentNullException);
                Assert.That(() => this.viewModel.InitializeViewModel(new Requirement(), null, []), Throws.ArgumentNullException);
            });
        }

        [Test]
        public void VerifyInitializeViewModelForRequirement()
        {
            var requirement = new Requirement { Iid = Guid.NewGuid(), Owner = this.otherDomain };
            this.viewModel.InitializeViewModel(requirement, this.iteration, [this.group]);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Thing, Is.SameAs(requirement));
                Assert.That(this.viewModel.ShowGroupSelector, Is.True);
                Assert.That(this.viewModel.IsDeprecatable, Is.True);
                Assert.That(this.viewModel.AvailableGroups, Is.EqualTo(new[] { this.group }));
                Assert.That(this.viewModel.AvailableCategories, Is.EqualTo(new[] { this.requirementCategory }));
                Assert.That(this.viewModel.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise, Is.EqualTo(this.otherDomain));
            });
        }

        [Test]
        public void VerifyRequirementExposesParameterTypesAndItself()
        {
            var requirement = new Requirement { Iid = Guid.NewGuid(), Owner = this.domain };
            this.viewModel.InitializeViewModel(requirement, this.iteration, []);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsRequirement, Is.True);
                Assert.That(this.viewModel.RequirementThing, Is.SameAs(requirement));
                Assert.That(this.viewModel.AvailableParameterTypes, Does.Contain(this.textParameterType));
            });

            this.viewModel.InitializeViewModel(this.group, this.iteration, []);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsRequirement, Is.False);
                Assert.That(this.viewModel.RequirementThing, Is.Null);
            });
        }

        [Test]
        public void VerifyInitializeViewModelForGroup()
        {
            this.viewModel.InitializeViewModel(this.group, this.iteration, []);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ShowGroupSelector, Is.False);
                Assert.That(this.viewModel.IsDeprecatable, Is.False, "RequirementsGroup is not IDeprecatableThing.");
                Assert.That(this.viewModel.AvailableCategories, Is.EqualTo(new[] { this.groupCategory }));
                Assert.That(this.viewModel.CategorizableThing, Is.SameAs(this.group));
            });
        }

        [Test]
        public void VerifyDeprecatedAndGroupSettersAreTypeGuarded()
        {
            this.viewModel.InitializeViewModel(this.group, this.iteration, []);

            // A group is not deprecatable and has no requirement-group placement: both setters are no-ops.
            this.viewModel.IsDeprecated = true;
            this.viewModel.SelectedGroup = new RequirementsGroup();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsDeprecated, Is.False);
                Assert.That(this.viewModel.SelectedGroup, Is.Null);
            });
        }

        [Test]
        public void VerifyPrimaryDefinitionCreatesAndUpdatesFirstDefinition()
        {
            var requirement = new Requirement { Iid = Guid.NewGuid(), Owner = this.domain };
            this.viewModel.InitializeViewModel(requirement, this.iteration, []);

            Assert.That(this.viewModel.PrimaryDefinitionContent, Is.Empty, "No definition exists yet.");

            this.viewModel.PrimaryDefinitionLanguageCode = "fr";
            this.viewModel.PrimaryDefinitionContent = "Le systeme doit exister.";

            Assert.Multiple(() =>
            {
                Assert.That(requirement.Definition, Has.Count.EqualTo(1));
                Assert.That(requirement.Definition[0].LanguageCode, Is.EqualTo("fr"));
                Assert.That(requirement.Definition[0].Content, Is.EqualTo("Le systeme doit exister."));
            });

            this.viewModel.PrimaryDefinitionContent = "Updated.";

            Assert.Multiple(() =>
            {
                Assert.That(requirement.Definition, Has.Count.EqualTo(1), "Editing must not add a second definition.");
                Assert.That(this.viewModel.PrimaryDefinitionContent, Is.EqualTo("Updated."));
            });
        }

        [Test]
        public void VerifyAvailableLanguagesMergeModelAndDefaults()
        {
            this.viewModel.InitializeViewModel(this.group, this.iteration, []);

            Assert.Multiple(() =>
            {
                // The model languages are present (taking precedence) alongside the bundled default IME languages.
                Assert.That(this.viewModel.AvailableLanguages, Does.Contain(this.english));
                Assert.That(this.viewModel.AvailableLanguages, Does.Contain(this.french));
                Assert.That(this.viewModel.AvailableLanguages.Select(x => x.LanguageCode), Does.Contain("de"), "A default IME language must be offered even when the model defines none.");
                Assert.That(this.viewModel.AvailableLanguages, Has.Count.GreaterThan(100));
            });
        }

        [Test]
        public void VerifyBasicDefinitionEditingIsLanguageKeyed()
        {
            var requirement = new Requirement { Iid = Guid.NewGuid(), Owner = this.domain };
            requirement.Definition.Add(new Definition { Iid = Guid.NewGuid(), LanguageCode = "en", Content = "English text" });
            requirement.Definition.Add(new Definition { Iid = Guid.NewGuid(), LanguageCode = "nl", Content = "Nederlandse tekst" });

            this.viewModel.InitializeViewModel(requirement, this.iteration, []);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.PrimaryDefinitionLanguageCode, Is.EqualTo("en"), "The first definition's language is selected initially.");
                Assert.That(this.viewModel.PrimaryDefinitionContent, Is.EqualTo("English text"));
            });

            // Switching the language shows that language's definition without renaming the first one.
            this.viewModel.PrimaryDefinitionLanguageCode = "nl";

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.PrimaryDefinitionContent, Is.EqualTo("Nederlandse tekst"));
                Assert.That(requirement.Definition, Has.Count.EqualTo(2));
                Assert.That(requirement.Definition.Single(x => x.LanguageCode == "en").Content, Is.EqualTo("English text"), "The English definition must be untouched.");
            });

            // Selecting a new language and typing creates a new definition for that language.
            this.viewModel.PrimaryDefinitionLanguageCode = "de";
            this.viewModel.PrimaryDefinitionContent = "Deutscher Text";

            Assert.Multiple(() =>
            {
                Assert.That(requirement.Definition, Has.Count.EqualTo(3));
                Assert.That(requirement.Definition.Single(x => x.LanguageCode == "de").Content, Is.EqualTo("Deutscher Text"));
                Assert.That(requirement.Definition.Select(x => x.LanguageCode), Is.Unique);
            });
        }

        [Test]
        public void VerifyDefaultLanguageIsTheDirectoryDefault()
        {
            // The site directory's first NaturalLanguage (English) is used as the default for a new definition.
            var requirement = new Requirement { Iid = Guid.NewGuid(), Owner = this.domain };
            this.viewModel.InitializeViewModel(requirement, this.iteration, []);

            Assert.That(this.viewModel.PrimaryDefinitionLanguageCode, Is.EqualTo(this.english.LanguageCode));
        }

        [Test]
        public void VerifyPrimaryDefinitionEmptyContentAddsNothing()
        {
            var group = new RequirementsGroup { Iid = Guid.NewGuid(), Owner = this.domain };
            this.viewModel.InitializeViewModel(group, this.iteration, []);

            this.viewModel.PrimaryDefinitionContent = string.Empty;

            Assert.That(group.Definition, Is.Empty);
        }

        [Test]
        public void VerifyOwnerWiresThroughSelectorChange()
        {
            var requirement = new Requirement { Iid = Guid.NewGuid(), Owner = this.domain };
            this.viewModel.InitializeViewModel(requirement, this.iteration, []);

            this.viewModel.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise = this.otherDomain;

            Assert.That(requirement.Owner, Is.EqualTo(this.otherDomain));
        }
    }
}
