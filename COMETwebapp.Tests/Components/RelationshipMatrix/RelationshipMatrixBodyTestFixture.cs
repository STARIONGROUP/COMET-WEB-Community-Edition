// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RelationshipMatrixBodyTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.RelationshipMatrix
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using RelationshipMatrixBodyComponent = COMETwebapp.Components.RelationshipMatrix.RelationshipMatrixBody;

    [TestFixture]
    public class RelationshipMatrixBodyTestFixture
    {
        private BunitContext context;
        private Mock<IRelationshipMatrixBodyViewModel> viewModel;
        private BinaryRelationshipRule rule;
        private ElementDefinition rowElement;
        private ElementDefinition columnElement;
        private MatrixCellViewModel cell;
        private SourceConfigurationViewModel rowConfiguration;
        private SourceConfigurationViewModel columnConfiguration;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());
            this.context.Services.AddSingleton(configuration.Object);

            this.rule = new BinaryRelationshipRule { Iid = Guid.NewGuid(), ShortName = "rule1", Name = "rule one", ForwardRelationshipName = "relates to" };

            this.rowElement = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "ROW1", Name = "Row Element 1" };
            this.columnElement = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "COL1", Name = "Column Element 1" };
            this.cell = new MatrixCellViewModel(this.rowElement, this.columnElement, [], this.rule);

            this.rowConfiguration = new SourceConfigurationViewModel(() => { });
            this.columnConfiguration = new SourceConfigurationViewModel(() => { });

            this.viewModel = new Mock<IRelationshipMatrixBodyViewModel>();
            this.viewModel.Setup(x => x.IsLoading).Returns(false);
            this.viewModel.Setup(x => x.RowConfiguration).Returns(this.rowConfiguration);
            this.viewModel.Setup(x => x.ColumnConfiguration).Returns(this.columnConfiguration);
            this.viewModel.Setup(x => x.AvailableRules).Returns([this.rule]);
            this.viewModel.Setup(x => x.SelectedRule).Returns(this.rule);
            this.viewModel.Setup(x => x.RowThings).Returns([this.rowElement]);
            this.viewModel.Setup(x => x.ColumnThings).Returns([this.columnElement]);
            this.viewModel.Setup(x => x.GetCell(It.IsAny<DefinedThing>(), It.IsAny<DefinedThing>())).Returns(this.cell);
            this.viewModel.Setup(x => x.SelectCell(It.IsAny<DefinedThing>(), It.IsAny<DefinedThing>()));
            this.viewModel.Setup(x => x.CreateRelationshipAsync(It.IsAny<RelationshipDirectionKind>())).Returns(Task.CompletedTask);
            this.viewModel.Setup(x => x.DeleteRelationshipAsync()).Returns(Task.CompletedTask);
            this.viewModel.Setup(x => x.SwapAxes());
            this.viewModel.Setup(x => x.ExportAsync()).Returns(Task.CompletedTask);
            this.context.Services.AddSingleton(this.viewModel.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.rowConfiguration.Dispose();
            this.columnConfiguration.Dispose();
            this.context.CleanContext();
        }

        private IRenderedComponent<RelationshipMatrixBodyComponent> RenderMatrix()
        {
            return this.context.Render<RelationshipMatrixBodyComponent>(parameters =>
                parameters.Add(p => p.ParameterizedViewModel, this.viewModel.Object));
        }

        [Test]
        public void VerifyMatrixRenders()
        {
            var renderedComponent = this.RenderMatrix();

            var table = renderedComponent.Find("table.matrix-table");
            var headerCells = renderedComponent.FindAll("thead th");
            var rows = renderedComponent.FindAll("tbody tr");
            var cells = renderedComponent.FindAll("td.matrix-cell");

            Assert.Multiple(() =>
            {
                Assert.That(table, Is.Not.Null);

                // One empty corner header plus one per column.
                Assert.That(headerCells, Has.Count.EqualTo(2));
                Assert.That(rows, Has.Count.EqualTo(1));
                Assert.That(cells, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public async Task VerifyCellClickSelects()
        {
            var renderedComponent = this.RenderMatrix();

            var cellElement = renderedComponent.Find("td.matrix-cell");
            await renderedComponent.InvokeAsync(() => cellElement.ClickAsync(new MouseEventArgs()));

            this.viewModel.Verify(x => x.SelectCell(It.IsAny<DefinedThing>(), It.IsAny<DefinedThing>()), Times.Once);
        }

        [Test]
        public async Task VerifyContextMenuCreate()
        {
            this.viewModel.Setup(x => x.SelectedCell).Returns(this.cell);
            this.viewModel.Setup(x => x.CanCreateRowToColumn).Returns(true);

            var renderedComponent = this.RenderMatrix();

            var cellElement = renderedComponent.Find("td.matrix-cell");
            await renderedComponent.InvokeAsync(() => cellElement.TriggerEventAsync("oncontextmenu", new MouseEventArgs()));

            var createRowToColumnButton = renderedComponent.FindAll("button.matrix-context-item")[0];

            await renderedComponent.InvokeAsync(() => createRowToColumnButton.ClickAsync(new MouseEventArgs()));

            this.viewModel.Verify(x => x.CreateRelationshipAsync(RelationshipDirectionKind.RowToColumn), Times.Once);
        }

        [Test]
        public async Task VerifyContextMenuDelete()
        {
            this.viewModel.Setup(x => x.SelectedCell).Returns(this.cell);
            this.viewModel.Setup(x => x.CanDelete).Returns(true);

            var renderedComponent = this.RenderMatrix();

            var cellElement = renderedComponent.Find("td.matrix-cell");
            await renderedComponent.InvokeAsync(() => cellElement.TriggerEventAsync("oncontextmenu", new MouseEventArgs()));

            var deleteButton = renderedComponent.FindAll("button.matrix-context-item")
                .Single(x => x.TextContent.Contains("Delete"));

            await renderedComponent.InvokeAsync(() => deleteButton.ClickAsync(new MouseEventArgs()));

            this.viewModel.Verify(x => x.DeleteRelationshipAsync(), Times.Once);
        }

        [Test]
        public async Task VerifyContextMenuCreateColumnToRow()
        {
            this.viewModel.Setup(x => x.SelectedCell).Returns(this.cell);
            this.viewModel.Setup(x => x.CanCreateColumnToRow).Returns(true);

            var renderedComponent = this.RenderMatrix();

            var cellElement = renderedComponent.Find("td.matrix-cell");
            await renderedComponent.InvokeAsync(() => cellElement.TriggerEventAsync("oncontextmenu", new MouseEventArgs()));

            var createColumnToRowButton = renderedComponent.FindAll("button.matrix-context-item")[1];

            await renderedComponent.InvokeAsync(() => createColumnToRowButton.ClickAsync(new MouseEventArgs()));

            this.viewModel.Verify(x => x.CreateRelationshipAsync(RelationshipDirectionKind.ColumnToRow), Times.Once);
        }

        [Test]
        public async Task VerifyCellDoubleClickCreates()
        {
            this.viewModel.Setup(x => x.SelectedCell).Returns(this.cell);
            this.viewModel.Setup(x => x.CanCreateRowToColumn).Returns(true);

            var renderedComponent = this.RenderMatrix();

            var cellElement = renderedComponent.Find("td.matrix-cell");
            await renderedComponent.InvokeAsync(() => cellElement.TriggerEventAsync("ondblclick", new MouseEventArgs()));

            this.viewModel.Verify(x => x.CreateRelationshipAsync(RelationshipDirectionKind.RowToColumn), Times.Once);
        }

        [Test]
        public async Task VerifyAltDoubleClickCreatesColumnToRow()
        {
            this.viewModel.Setup(x => x.SelectedCell).Returns(this.cell);
            this.viewModel.Setup(x => x.CanCreateColumnToRow).Returns(true);

            var renderedComponent = this.RenderMatrix();

            var cellElement = renderedComponent.Find("td.matrix-cell");
            await renderedComponent.InvokeAsync(() => cellElement.TriggerEventAsync("ondblclick", new MouseEventArgs { AltKey = true }));

            this.viewModel.Verify(x => x.CreateRelationshipAsync(RelationshipDirectionKind.ColumnToRow), Times.Once);
        }

        [Test]
        public async Task VerifyCtrlAltDoubleClickDeletes()
        {
            this.viewModel.Setup(x => x.SelectedCell).Returns(this.cell);
            this.viewModel.Setup(x => x.CanDelete).Returns(true);

            var renderedComponent = this.RenderMatrix();

            var cellElement = renderedComponent.Find("td.matrix-cell");
            await renderedComponent.InvokeAsync(() => cellElement.TriggerEventAsync("ondblclick", new MouseEventArgs { AltKey = true, CtrlKey = true }));

            this.viewModel.Verify(x => x.DeleteRelationshipAsync(), Times.Once);
        }

        [Test]
        public async Task VerifyItemDetailsPanelCollapses()
        {
            this.viewModel.Setup(x => x.SelectedCell).Returns(this.cell);

            var renderedComponent = this.RenderMatrix();

            Assert.That(renderedComponent.FindAll(".matrix-details-column"), Has.Count.EqualTo(3));

            await renderedComponent.InvokeAsync(() => renderedComponent.Find(".matrix-details-header").ClickAsync(new MouseEventArgs()));

            Assert.That(renderedComponent.FindAll(".matrix-details-column"), Is.Empty);
        }

        [Test]
        public void VerifyItemDetailsPanelRenders()
        {
            this.viewModel.Setup(x => x.SelectedCell).Returns(this.cell);

            var renderedComponent = this.RenderMatrix();

            var detailColumns = renderedComponent.FindAll(".matrix-details-column");
            var headings = renderedComponent.FindAll(".matrix-details-column h6").Select(x => x.TextContent).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(detailColumns, Has.Count.EqualTo(3));
                Assert.That(headings, Is.EqualTo(["Row", "Relations", "Column"]));
            });
        }

        [Test]
        public void VerifyUnrelatedHeaderHighlight()
        {
            this.viewModel.Setup(x => x.ShowNonRelatedBackgroundColor).Returns(true);
            this.viewModel.Setup(x => x.IsRowUnrelated(It.IsAny<DefinedThing>())).Returns(true);

            var renderedComponent = this.RenderMatrix();

            Assert.Multiple(() =>
            {
                Assert.That(renderedComponent.FindAll("th.unrelated"), Is.Not.Empty);
                Assert.That(renderedComponent.FindAll("td.matrix-cell.unrelated"), Is.Empty);
            });
        }

        [Test]
        public async Task VerifyConfigurationActionsInvokeViewModel()
        {
            var renderedComponent = this.RenderMatrix();

            await renderedComponent.InvokeAsync(() => renderedComponent.FindAll("button").Single(x => x.TextContent.Contains("Swap")).ClickAsync(new MouseEventArgs()));
            await renderedComponent.InvokeAsync(() => renderedComponent.FindAll("button").Single(x => x.TextContent.Contains("Export to Excel")).ClickAsync(new MouseEventArgs()));
            await renderedComponent.InvokeAsync(() => renderedComponent.FindAll("button").Single(x => x.TextContent.Contains("Configuration")).ClickAsync(new MouseEventArgs()));
            await renderedComponent.InvokeAsync(() => renderedComponent.Find(".matrix-config-panel-header button").ClickAsync(new MouseEventArgs()));

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.SwapAxes(), Times.Once);
                this.viewModel.Verify(x => x.ExportAsync(), Times.Once);
                this.viewModel.VerifySet(x => x.IsConfigurationDialogVisible = true);
                this.viewModel.VerifySet(x => x.IsConfigurationPanelCollapsed = true);
            });
        }

        [Test]
        public async Task VerifyCollapsedPanelCanExpand()
        {
            this.viewModel.Setup(x => x.IsConfigurationPanelCollapsed).Returns(true);

            var renderedComponent = this.RenderMatrix();

            await renderedComponent.InvokeAsync(() => renderedComponent.Find("#expandMatrixConfigPanel").ClickAsync(new MouseEventArgs()));

            this.viewModel.VerifySet(x => x.IsConfigurationPanelCollapsed = false);
        }

        [Test]
        public void VerifyPlaceholderShownWithoutRule()
        {
            this.viewModel.Setup(x => x.SelectedRule).Returns((BinaryRelationshipRule)null);

            var renderedComponent = this.RenderMatrix();

            Assert.Multiple(() =>
            {
                Assert.That(() => renderedComponent.Find("table.matrix-table"), Throws.TypeOf<ElementNotFoundException>());
                Assert.That(renderedComponent.Markup, Does.Contain("Select row/column categories and a relationship rule to build the matrix."));
            });
        }
    }
}
