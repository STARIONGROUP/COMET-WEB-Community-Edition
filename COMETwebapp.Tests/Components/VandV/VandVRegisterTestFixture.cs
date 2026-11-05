// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVRegisterTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.VandV
{
    using AngleSharp.Dom;

    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.Components.VandV;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Tests.Utilities.VandV;
    using COMETwebapp.Tests.ViewModels.Components.VandV;
    using COMETwebapp.Utilities.VandV;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.VandV;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class VandVRegisterTestFixture
    {
        private BunitContext context;
        private Mock<IDomDataService> domDataService;
        private VandVRegisterScenario scenario;
        private VandVRegisterViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.domDataService = new Mock<IDomDataService>();
            this.context.Services.AddSingleton(this.domDataService.Object);
            this.context.Services.AddSingleton(new Mock<ISessionService>().Object);

            this.scenario = new VandVRegisterScenario();
            this.viewModel = new VandVRegisterViewModel(new Mock<ILogger>().Object);
            this.viewModel.SetModel(this.scenario.BuildModel());
            this.viewModel.SelectedSpecification = null;
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyRowsRender()
        {
            var component = this.Render();
            var passedItem = ItemRow(component, this.scenario.PassedItem);
            var performedItem = ItemRow(component, this.scenario.PerformedItem);
            var firstRequirement = component.Find($"#{component.Instance.RowId(this.scenario.FirstRequirement.Iid)}");

            Assert.Multiple(() =>
            {
                Assert.That(component.FindAll(".vandv-row"), Has.Count.EqualTo(this.viewModel.Rows.Count));
                Assert.That(firstRequirement.QuerySelector(".vandv-pill").TextContent, Is.EqualTo("2 item(s): 1 passed, 1 open"));
                Assert.That(component.Find($"#{component.Instance.RowId(this.scenario.ThirdRequirement.Iid)} .vandv-pill").ClassList, Does.Contain("vandv-pill-uncovered"));
                Assert.That(passedItem.QuerySelector(".vandv-pill").TextContent, Is.EqualTo("Passed"));
                Assert.That(passedItem.QuerySelectorAll(".vandv-field-label").Select(x => x.TextContent), Does.Contain("Close-out").And.Contain("Procedure").And.Contain("Method").And.Not.Contain("Owner"));
                Assert.That(passedItem.TextContent, Does.Contain("Closed: Accepted at CDR"));
                Assert.That(passedItem.QuerySelectorAll(".vandv-unset"), Is.Not.Empty, "unset fields are shown, muted");
                Assert.That(performedItem.QuerySelector(".vandv-inherited").GetAttribute("title"), Is.EqualTo("Inherited from activity ACT_1"));
                Assert.That(component.FindAll("[id^=vandv-row-]"), Has.Count.EqualTo(this.viewModel.Rows.Count(x => x.Kind is VandVRegisterRowKind.Requirement or VandVRegisterRowKind.Group)), "an item can cover several requirements, so an item row has no id");
            });
        }

        [Test]
        public void VerifyChevronTogglesSteps()
        {
            var component = this.Render();
            ItemRow(component, this.scenario.PerformedItem).QuerySelector(".vandv-chevron").Click();

            var stepRows = component.FindAll(".vandv-row-step");

            Assert.Multiple(() =>
            {
                Assert.That(stepRows, Has.Count.EqualTo(2));
                Assert.That(stepRows[0].TextContent, Does.Contain("1. Weigh the unit").And.Contain("activity"));
                Assert.That(stepRows[1].QuerySelector(".vandv-pill").ClassList, Does.Contain("vandv-pill-failed"));
            });
        }

        [Test]
        public void VerifyTableOfContents()
        {
            var component = this.Render();
            var specifications = component.FindAll(".req-tree-spec").Select(x => x.TextContent.Trim()).ToList();
            var groupsWithoutScope = component.FindAll(".req-tree-group").Count;

            component.FindAll(".req-tree-spec")[0].Click();
            var scope = this.viewModel.SelectedSpecification;
            var groups = component.FindAll(".req-tree-group").Select(x => x.TextContent).ToList();
            var draggable = component.FindAll(".req-tree-group-row")[0].GetAttribute("draggable");

            component.FindAll(".req-tree-chevron")[0].Click();
            var groupsWhileCollapsed = component.FindAll(".req-tree-group").Select(x => x.TextContent).ToList();
            component.FindAll(".req-tree-chevron")[0].Click();

            component.FindAll(".req-tree-group")[1].Click();

            Assert.Multiple(() =>
            {
                Assert.That(specifications[0], Does.StartWith("SPEC"));
                Assert.That(specifications, Has.Count.EqualTo(2));
                Assert.That(groupsWithoutScope, Is.Zero);
                Assert.That(scope, Is.SameAs(this.scenario.Specification));
                Assert.That(groups, Is.EqualTo(new[] { "G1", "G11" }));
                Assert.That(groupsWhileCollapsed, Is.EqualTo(new[] { "G1" }), "the tree collapses without the rows being rebuilt");
                Assert.That(draggable, Is.EqualTo("false"), "the register only reads the structure");
            });

            component.WaitForAssertion(() => this.domDataService.Verify(x => x.ScrollElementIntoView(component.Instance.RowId(this.scenario.SubGroup.Iid)), Times.Once));
        }

        [Test]
        public async Task VerifyTocCollapse()
        {
            var component = this.Render();
            var expanded = component.FindAll(".toc-collapsed").Count;
            var toggle = component.FindComponents<DxButton>().Single(x => x.Instance.CssClass == "req-collapse-toggle");

            await component.InvokeAsync(toggle.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(expanded, Is.Zero);
                Assert.That(component.FindAll(".req-editor-layout.toc-collapsed"), Has.Count.EqualTo(1));
            });
        }

        [Test]
        public async Task VerifyFilters()
        {
            var component = this.Render();
            var filters = component.FindComponents<FilterTagBox<string>>();

            await component.InvokeAsync(() => filters[0].Instance.ValuesChanged.InvokeAsync(["CDR", "PDR"]));
            var boundStages = this.viewModel.SelectedStages.ToList();

            await component.InvokeAsync(() => filters[1].Instance.ValuesChanged.InvokeAsync(["Status"]));
            var labels = ItemRow(component, this.scenario.PassedItem).QuerySelectorAll(".vandv-field-label").Select(x => x.TextContent).ToList();

            await component.InvokeAsync(() => component.FindComponent<SearchBar>().Instance.TextChanged.InvokeAsync("nothing matches"));
            var noMatch = component.Find(".vandv-no-rows").TextContent;

            await component.InvokeAsync(() => this.viewModel.SetModel(VandVModel.Build(null, null)));
            var emptyModel = component.Find(".vandv-no-rows").TextContent;

            Assert.Multiple(() =>
            {
                Assert.That(filters[0].Instance.Data, Is.EqualTo(this.viewModel.Model.StageGates));
                Assert.That(filters[1].Instance.Data, Is.EqualTo(VandVRegisterFields.AllFields));
                Assert.That(boundStages, Is.EqualTo(new[] { "CDR", "PDR" }));
                Assert.That(labels, Is.EqualTo(new[] { "Status" }));
                Assert.That(noMatch, Is.EqualTo("No requirements match."));
                Assert.That(emptyModel, Is.EqualTo("This model has no requirements to verify."), "an empty model is not a search without a match");
            });
        }

        [Test]
        public async Task VerifyBuildError()
        {
            var component = this.Render();
            var broken = new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.scenario.Data.ParameterTypes[VandVConstants.Level], Value = null };
            this.scenario.PassedItem.ParameterValue.Add(broken);

            await component.InvokeAsync(() => this.viewModel.SetModel(this.scenario.BuildModel()));

            Assert.That(component.Find(".vandv-no-rows").TextContent, Does.Contain("could not be built"), "a failing build shows instead of ending the circuit");
        }

        [Test]
        public async Task VerifyDisplayOptions()
        {
            var component = this.Render();
            var requirementId = $"#{component.Instance.RowId(this.scenario.ThirdRequirement.Iid)}";
            var both = component.Find(requirementId).QuerySelector(".vandv-row-header").TextContent;
            var indented = component.Find(requirementId).GetAttribute("style");
            var itemIndented = ItemRow(component, this.scenario.PassedItem).GetAttribute("style");
            var ownerPills = component.Find(requirementId).QuerySelectorAll(".starion-pill").Select(x => x.TextContent).ToList();
            var itemPills = ItemRow(component, this.scenario.PassedItem).QuerySelectorAll(".starion-pill").Select(x => x.TextContent).ToList();

            await component.InvokeAsync(() => this.viewModel.DisplayMode = RequirementRowDisplayMode.ShortNameAndDefinition);
            var idOnly = (component.Find(requirementId).QuerySelector(".vandv-id")?.TextContent, component.Find(requirementId).QuerySelector(".vandv-name"));

            await component.InvokeAsync(() => this.viewModel.DisplayMode = RequirementRowDisplayMode.NameAndDefinition);
            var nameOnly = (component.Find(requirementId).QuerySelector(".vandv-id"), component.Find(requirementId).QuerySelector(".vandv-name")?.TextContent);

            await component.InvokeAsync(() => this.viewModel.IndentGroups = false);
            var flat = component.Find(requirementId).GetAttribute("style");
            var itemFlat = ItemRow(component, this.scenario.PassedItem, this.scenario.PassedItem.Name).GetAttribute("style");

            await component.InvokeAsync(() => this.viewModel.ShowOwner = false);
            var withoutOwner = component.Find(requirementId).QuerySelectorAll(".starion-pill").Select(x => x.TextContent).ToList();

            await component.InvokeAsync(() => this.viewModel.ShowCategory = false);
            var withoutCategory = component.Find(requirementId).QuerySelectorAll(".starion-pill").Count;

await component.InvokeAsync(() => this.viewModel.TreeUsesShortName = false);

            Assert.Multiple(() =>
            {
                Assert.That(both, Does.Contain("R3"));
                Assert.That(indented, Does.Contain("58px"), "the depth of R3");
                Assert.That(itemIndented, Does.Contain("40px"));
                Assert.That(ownerPills, Does.Contain("SYS"), "the owner is a badge");
                Assert.That(ownerPills, Does.Contain(VandVTestData.RequirementCategoryShortName));
                Assert.That(itemPills, Is.EqualTo(new[] { "SYS" }), "an item's category only says it is a V&V item");
                Assert.That(withoutCategory, Is.Zero);
                Assert.That(idOnly.Item1, Is.EqualTo("R3"));
                Assert.That(idOnly.Item2, Is.Null);
                Assert.That(nameOnly.Item1, Is.Null);
                Assert.That(nameOnly.Item2, Is.EqualTo(this.scenario.ThirdRequirement.Name));
                Assert.That(flat, Does.Contain("padding-left: 4px"), "flush left whatever its depth");
                Assert.That(itemFlat, Does.Contain("22px"), "an item stays below its requirement");
                Assert.That(withoutOwner, Does.Not.Contain("SYS"));
                Assert.That(component.FindAll(".req-tree-label")[0].TextContent, Is.EqualTo(this.scenario.Specification.Name), "full names label the table of contents");
            });
        }

        [Test]
        public async Task VerifyTraceability()
        {
            var component = this.Render();
            var off = component.FindAll(".req-link-row").Count;

            await component.InvokeAsync(() => this.viewModel.ShowTraceability = true);
            var firstRequirement = component.Find($"#{component.Instance.RowId(this.scenario.FirstRequirement.Iid)}");
            var links = firstRequirement.QuerySelectorAll(".req-link-req").Select(x => x.TextContent.Trim()).ToList();

            ItemRow(component, this.scenario.PerformedItem).QuerySelector(".req-link-req").Click();

            Assert.Multiple(() =>
            {
                Assert.That(off, Is.Zero);
                Assert.That(links, Has.Some.Contains("VNV_R1_1"));
                Assert.That(this.viewModel.SelectedSpecification, Is.SameAs(this.scenario.Specification), "the link of an item goes to the requirement");
            });

            component.WaitForAssertion(() => this.domDataService.Verify(x => x.ScrollElementIntoView(component.Instance.RowId(this.scenario.SecondRequirement.Iid)), Times.Once));

            this.scenario.Data.Link(this.scenario.PerformedItem, this.scenario.FirstRequirement, VandVConstants.VerifiesCategory);
            await component.InvokeAsync(() => this.viewModel.SetModel(this.scenario.BuildModel()));
            this.domDataService.Invocations.Clear();

            // the item now covers R1 first and R2 second: clicked on the row of R2, it stays on R2
            component.Find($"#{component.Instance.RowId(this.scenario.SecondRequirement.Iid)}").QuerySelectorAll(".req-link-req").Single(x => x.TextContent.Contains("VNV_R2_1")).Click();

            component.WaitForAssertion(() => this.domDataService.Verify(x => x.ScrollElementIntoView(component.Instance.RowId(this.scenario.SecondRequirement.Iid)), Times.Once));
        }

        [Test]
        public void VerifyFocusScrollsToTheRequirement()
        {
            var component = this.Render();

            component.InvokeAsync(() => this.viewModel.Focus(this.scenario.SecondRequirement));

            component.WaitForAssertion(() =>
            {
                this.domDataService.Verify(x => x.ScrollElementIntoView(component.Instance.RowId(this.scenario.SecondRequirement.Iid)), Times.Once);
                Assert.That(this.viewModel.ScrollTarget, Is.Null, "the target is cleared once scrolled");
            });
        }

        private static IElement ItemRow(IRenderedComponent<VandVRegister> component, Requirement item, string label = null)
        {
            return component.FindAll(".vandv-row-item").Single(x => x.QuerySelector("strong").TextContent == (label ?? item.ShortName));
        }

        private IRenderedComponent<VandVRegister> Render()
        {
            return this.context.Render<VandVRegister>(parameters => parameters.Add(p => p.ViewModel, this.viewModel));
        }
    }
}
