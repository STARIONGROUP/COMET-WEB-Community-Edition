// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditRequirementThingTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.RequirementsEditor
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EditRequirementThingTestFixture
    {
        private BunitContext context;
        private CDPMessageBus messageBus;
        private EditRequirementThingViewModel viewModel;
        private Iteration iteration;
        private DomainOfExpertise domain;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            this.messageBus = new CDPMessageBus();
            var sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();

            this.domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            var requirementCategory = new Category { Iid = Guid.NewGuid(), ShortName = "REQ", Name = "Requirement", PermissibleClass = { ClassKind.Requirement } };
            var textParameterType = new TextParameterType { Iid = Guid.NewGuid(), ShortName = "txt", Name = "Text" };
            var rdl = new SiteReferenceDataLibrary { ShortName = "siteRdl", Name = "Site RDL", DefinedCategory = { requirementCategory }, ParameterType = { textParameterType } };

            var siteDirectory = new SiteDirectory
            {
                Domain = { this.domain },
                SiteReferenceDataLibrary = { rdl },
                NaturalLanguage = { new NaturalLanguage { LanguageCode = "en-GB", Name = "English" } }
            };

            var modelSetup = new EngineeringModelSetup { Name = "Model", ShortName = "MOD", ActiveDomain = { this.domain }, RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = rdl } } };
            siteDirectory.Model.Add(modelSetup);

            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            sessionService.Setup(x => x.Session).Returns(session.Object);
            sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domain);

            this.iteration = new Iteration { Iid = Guid.NewGuid(), Container = new EngineeringModel { EngineeringModelSetup = modelSetup } };
            this.viewModel = new EditRequirementThingViewModel(sessionService.Object, this.messageBus);
            this.context.Services.AddSingleton(sessionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
            this.context.CleanContext();
        }

        [Test]
        public void VerifyThingIsNullRendersNothing()
        {
            var renderer = this.context.Render<EditRequirementThing>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel));

            Assert.That(renderer.Markup, Is.Empty);
        }

        [Test]
        public async Task VerifyRequirementIsRenderedWithAllTabsAndSubmits()
        {
            var requirement = new Requirement { Iid = Guid.NewGuid(), Owner = this.domain };
            this.viewModel.InitializeViewModel(requirement, this.iteration, []);

            var submitted = false;
            this.viewModel.OnValidSubmit = EventCallback.Factory.Create(this, () => submitted = true);

            var renderer = this.context.Render<EditRequirementThing>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel));

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Markup, Does.Contain("Group:"), "A requirement offers the group selector.");
                Assert.That(renderer.Markup, Does.Contain("Deprecated:"), "A requirement is deprecatable.");
                Assert.That(renderer.Markup, Does.Contain("Simple Parameter Values"));
                Assert.That(renderer.Markup, Does.Contain("Parametric Constraints"));
            });

            var editForm = renderer.FindComponent<EditForm>();
            await renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            Assert.That(submitted, Is.True);
        }

        /// <summary>
        /// Regression test for the "Definition language:" selector used to be laid out on a 4-column span while
        /// every other captioned editor on the Basic tab spans 10, which left the combo box too narrow to read. The
        /// three edited thing kinds (requirement, group and specification) all share this single form.
        /// </summary>
        [Test]
        public void VerifyDefinitionLanguageSelectorIsAsWideAsTheOtherEditors()
        {
            Thing[] editableThings =
            [
                new Requirement { Iid = Guid.NewGuid(), Owner = this.domain },
                new RequirementsGroup { Iid = Guid.NewGuid(), Owner = this.domain },
                new RequirementsSpecification { Iid = Guid.NewGuid(), Owner = this.domain }
            ];

            foreach (var thing in editableThings)
            {
                this.viewModel.InitializeViewModel(thing, this.iteration, []);

                var renderer = this.context.Render<EditRequirementThing>(parameters => parameters
                    .Add(p => p.ViewModel, this.viewModel));

                var layoutItems = renderer.FindComponents<DxFormLayoutItem>()
                    .Select(x => x.Instance)
                    .ToList();

                var languageItem = layoutItems.Single(x => x.Caption == "Definition language:");
                var nameItem = layoutItems.Single(x => x.Caption == "Name:");

                Assert.That(languageItem.ColSpanMd, Is.EqualTo(nameItem.ColSpanMd),
                    $"The definition language selector of a {thing.ClassKind} must span as wide as the other editors.");
            }
        }

        [Test]
        public void VerifyRequirementsGroupIsRenderedWithoutRequirementTabs()
        {
            var group = new RequirementsGroup { Iid = Guid.NewGuid(), Owner = this.domain };
            this.viewModel.InitializeViewModel(group, this.iteration, []);

            var renderer = this.context.Render<EditRequirementThing>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel));

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Markup, Does.Not.Contain("Group:"), "A group has no placement to select.");
                Assert.That(renderer.Markup, Does.Not.Contain("Deprecated:"), "RequirementsGroup is not deprecatable.");
                Assert.That(renderer.Markup, Does.Not.Contain("Simple Parameter Values"));
                Assert.That(renderer.Markup, Does.Not.Contain("Parametric Constraints"));
                Assert.That(renderer.Markup, Does.Contain("Category"));
                Assert.That(renderer.Markup, Does.Contain("Definition"));
            });
        }
    }
}
