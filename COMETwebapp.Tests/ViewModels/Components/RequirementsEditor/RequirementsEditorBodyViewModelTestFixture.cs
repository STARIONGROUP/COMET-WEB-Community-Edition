// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsEditorBodyViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsEditorBodyViewModelTestFixture
    {
        private RequirementsEditorBodyViewModel viewModel;
        private CDPMessageBus messageBus;
        private Mock<ISession> session;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private RequirementsGroup operateGroup;
        private RequirementsGroup c4iGroup;
        private Requirement topRequirement;
        private Requirement operateRequirement;
        private Requirement c4iRequirement;
        private DomainOfExpertise systemDomain;
        private DomainOfExpertise thermalDomain;
        private Category keyUserCategory;
        private RequirementsSpecification deprecatedSpecification;
        private Requirement deprecatedRequirement;
        private ShowHideDeprecatedThingsService showHideService;

        [SetUp]
        public void SetUp()
        {
            this.systemDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System Engineering" };
            this.thermalDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "THE", Name = "Thermal" };
            var parentCategory = new Category { Iid = Guid.NewGuid(), ShortName = "REQ", Name = "Requirement" };
            this.keyUserCategory = new Category { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements", SuperCategory = { parentCategory } };

            this.c4iGroup = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "C4I", Name = "C4I", Owner = this.systemDomain };
            this.operateGroup = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "OPERATE", Name = "Operate", Owner = this.systemDomain };
            this.operateGroup.Group.Add(this.c4iGroup);

            this.topRequirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R01", Name = "Top level", Owner = this.thermalDomain,
                Definition = { new Definition { LanguageCode = "en", Content = "The system shall exist." } }
            };

            this.operateRequirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R25", Name = "Minimize user cognitive load", Owner = this.systemDomain,
                Definition = { new Definition { LanguageCode = "en", Content = "The USV SHALL have minimal user cognitive load." } }
            };

            this.c4iRequirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R24", Name = "Provide user with sensor information", Owner = this.systemDomain,
                Definition = { new Definition { LanguageCode = "en", Content = "The USV SHALL provide the required sensor information." } },
                Category = { this.keyUserCategory }
            };

            this.operateRequirement.Group = this.operateGroup;
            this.c4iRequirement.Group = this.c4iGroup;

            this.deprecatedRequirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R00", Name = "Retired requirement", Owner = this.systemDomain, IsDeprecated = true,
                Definition = { new Definition { LanguageCode = "en", Content = "This requirement is retired." } }
            };

            this.specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements", Owner = this.systemDomain };
            this.specification.Group.Add(this.operateGroup);
            this.specification.Requirement.AddRange([this.topRequirement, this.operateRequirement, this.c4iRequirement, this.deprecatedRequirement]);

            this.deprecatedSpecification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "OLD", Name = "Deprecated", Owner = this.systemDomain, IsDeprecated = true };

            this.iteration = new Iteration { Iid = Guid.NewGuid() };
            this.iteration.RequirementsSpecification.AddRange([this.specification, this.deprecatedSpecification]);

            var sessionService = new Mock<ISessionService>();
            this.session = new Mock<ISession>();
            sessionService.Setup(x => x.Session).Returns(this.session.Object);
            sessionService.Setup(x => x.GetDomainOfExpertise(this.iteration)).Returns(this.systemDomain);
            this.messageBus = new CDPMessageBus();
            this.showHideService = new ShowHideDeprecatedThingsService();

            this.viewModel = new RequirementsEditorBodyViewModel(sessionService.Object, this.messageBus, this.showHideService, new Mock<ILogger<RequirementsEditorBodyViewModel>>().Object)
            {
                CurrentThing = this.iteration
            };
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
        }

        [Test]
        public async Task VerifyInitialisation()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableSpecifications, Has.Exactly(1).Items);
                Assert.That(this.viewModel.AvailableSpecifications, Does.Contain(this.specification));
                Assert.That(this.viewModel.SelectedSpecification, Is.EqualTo(this.specification));
                Assert.That(this.viewModel.AvailableOwners, Is.EquivalentTo(new[] { this.systemDomain, this.thermalDomain }));
                Assert.That(this.viewModel.AvailableCategories, Is.EquivalentTo(new[] { this.keyUserCategory }));
                Assert.That(this.viewModel.DisplayMode, Is.EqualTo(RequirementRowDisplayMode.ShortNameNameAndDefinition));
                Assert.That(this.viewModel.IsTocCollapsed, Is.False);
            });
        }

        [Test]
        public async Task VerifyDocumentStructure()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetGroups(this.specification), Is.EqualTo(new[] { this.operateGroup }));
                Assert.That(this.viewModel.GetGroups(this.operateGroup), Is.EqualTo(new[] { this.c4iGroup }));
                Assert.That(this.viewModel.GetRequirements(this.specification), Is.EqualTo(new[] { this.topRequirement }));
                Assert.That(this.viewModel.GetRequirements(this.operateGroup), Is.EqualTo(new[] { this.operateRequirement }));
                Assert.That(this.viewModel.GetRequirements(this.c4iGroup), Is.EqualTo(new[] { this.c4iRequirement }));
            });
        }

        [Test]
        public async Task VerifySearchFiltersOnDefinition()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.SearchText = "cognitive";

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetRequirements(this.operateGroup), Is.EqualTo(new[] { this.operateRequirement }));
                Assert.That(this.viewModel.GetRequirements(this.c4iGroup), Is.Empty);
                Assert.That(this.viewModel.GetRequirements(this.specification), Is.Empty);
                Assert.That(this.viewModel.ShouldDisplayGroup(this.c4iGroup), Is.False);
                Assert.That(this.viewModel.GetGroups(this.operateGroup), Is.Empty);
            });

            this.viewModel.SearchText = string.Empty;
            Assert.That(this.viewModel.GetRequirements(this.c4iGroup), Is.EqualTo(new[] { this.c4iRequirement }));
        }

        [Test]
        public async Task VerifyOwnerAndCategoryFilters()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.SelectedOwners = [this.thermalDomain];

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetRequirements(this.specification), Is.EqualTo(new[] { this.topRequirement }));
                Assert.That(this.viewModel.GetRequirements(this.c4iGroup), Is.Empty);
            });

            this.viewModel.SelectedOwners = [];
            this.viewModel.SelectedCategories = [this.keyUserCategory];

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetRequirements(this.c4iGroup), Is.EqualTo(new[] { this.c4iRequirement }));
                Assert.That(this.viewModel.GetRequirements(this.operateGroup), Is.Empty);
                Assert.That(this.viewModel.ShouldDisplayGroup(this.operateGroup), Is.True);
            });
        }

        [Test]
        public async Task VerifyCollapseToggle()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.viewModel.IsTocCollapsed, Is.False);
            this.viewModel.IsTocCollapsed = true;
            Assert.That(this.viewModel.IsTocCollapsed, Is.True);
        }

        [Test]
        public async Task VerifyDirectCategoriesOnly()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            // KUR has super-category REQ; only the directly-assigned KUR must surface, not REQ.
            Assert.That(this.viewModel.AvailableCategories.Select(x => x.ShortName), Is.EqualTo(new[] { "KUR" }));
        }

        [Test]
        public async Task VerifyTogglesAndCollapseState()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ShowOwner, Is.True);
                Assert.That(this.viewModel.ShowCategory, Is.True);
                Assert.That(this.viewModel.IsTreeNodeCollapsed(this.operateGroup.Iid), Is.False);
                Assert.That(this.viewModel.IsDocumentGroupCollapsed(this.operateGroup.Iid), Is.False);
            });

            this.viewModel.ToggleTreeNode(this.operateGroup.Iid);
            this.viewModel.ToggleDocumentGroup(this.operateGroup.Iid);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsTreeNodeCollapsed(this.operateGroup.Iid), Is.True);
                Assert.That(this.viewModel.IsDocumentGroupCollapsed(this.operateGroup.Iid), Is.True);
            });

            this.viewModel.ToggleTreeNode(this.operateGroup.Iid);
            this.viewModel.ToggleDocumentGroup(this.operateGroup.Iid);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsTreeNodeCollapsed(this.operateGroup.Iid), Is.False);
                Assert.That(this.viewModel.IsDocumentGroupCollapsed(this.operateGroup.Iid), Is.False);
            });
        }

        [Test]
        public async Task VerifySessionRefreshPreservesOrResetsSelection()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.session.Object);
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.viewModel.SelectedSpecification, Is.EqualTo(this.specification));

            this.viewModel.SelectedSpecification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "FOREIGN" };
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.session.Object);
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.viewModel.SelectedSpecification, Is.EqualTo(this.specification));
        }

        [Test]
        public async Task VerifyIndentGroupsToggle()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.viewModel.IndentGroups, Is.True);
            this.viewModel.IndentGroups = false;
            Assert.That(this.viewModel.IndentGroups, Is.False);
        }

        [Test]
        public async Task VerifyDeprecatedThingsRespectGlobalToggle()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableSpecifications, Does.Not.Contain(this.deprecatedSpecification));
                Assert.That(this.viewModel.GetRequirements(this.specification), Does.Not.Contain(this.deprecatedRequirement));
            });

            this.showHideService.ShowDeprecatedThings = true;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableSpecifications, Does.Contain(this.deprecatedSpecification));
                Assert.That(this.viewModel.GetRequirements(this.specification), Does.Contain(this.deprecatedRequirement));
            });
        }
    }
}
