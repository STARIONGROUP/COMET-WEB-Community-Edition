// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MatrixGrid.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.RelationshipMatrix
{
    using CDP4Common.CommonData;

    using COMET.Web.Common.Components;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;

    using ReactiveUI;

    /// <summary>
    /// Renders the Relationship Matrix grid (headers, cells, the right-click create/delete context menu and the
    /// item-details panel). It is a component of its own so that rebuilding the matrix only re-renders the grid and
    /// never the configuration panel; that keeps the category/owner multi-select dropdowns open while the user picks
    /// several values in a row.
    /// </summary>
    public partial class MatrixGrid : DisposableComponent
    {
        /// <summary>
        /// A value indicating whether the cell context menu is visible.
        /// </summary>
        private bool isContextMenuVisible;

        /// <summary>
        /// A value indicating whether the item-details panel is collapsed.
        /// </summary>
        private bool isDetailsCollapsed;

        /// <summary>
        /// The viewport X coordinate at which the context menu is shown.
        /// </summary>
        private double contextMenuX;

        /// <summary>
        /// The viewport Y coordinate at which the context menu is shown.
        /// </summary>
        private double contextMenuY;

        /// <summary>
        /// Gets or sets the <see cref="IRelationshipMatrixBodyViewModel" /> providing the matrix content.
        /// </summary>
        [Parameter]
        public IRelationshipMatrixBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Registers the single re-render subscription. Only the grid's own inputs are observed, so a matrix rebuild
        /// leaves the configuration panel untouched.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.RowThings,
                    x => x.ViewModel.ColumnThings,
                    x => x.ViewModel.SelectedCell)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Selects the cell for the given row and column <see cref="DefinedThing" />s and closes the context menu.
        /// </summary>
        /// <param name="row">The row <see cref="DefinedThing" />.</param>
        /// <param name="column">The column <see cref="DefinedThing" />.</param>
        private void OnCellClick(DefinedThing row, DefinedThing column)
        {
            this.isContextMenuVisible = false;
            this.ViewModel.SelectCell(row, column);
        }

        /// <summary>
        /// Quick-toggles a relationship on double-click, mirroring the COMET-IME gestures: a plain double-click
        /// toggles the row→column link, <c>Alt</c>+double-click toggles the column→row link, and
        /// <c>Ctrl</c>+<c>Alt</c>+double-click deletes every relationship of the cell.
        /// </summary>
        /// <param name="eventArgs">The <see cref="MouseEventArgs" /> of the double-click.</param>
        /// <param name="row">The row <see cref="DefinedThing" />.</param>
        /// <param name="column">The column <see cref="DefinedThing" />.</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnCellDoubleClick(MouseEventArgs eventArgs, DefinedThing row, DefinedThing column)
        {
            this.isContextMenuVisible = false;
            this.ViewModel.SelectCell(row, column);

            if (eventArgs is { CtrlKey: true, AltKey: true })
            {
                if (this.ViewModel.CanDelete)
                {
                    await this.ViewModel.DeleteRelationshipAsync();
                }

                return;
            }

            var direction = eventArgs.AltKey ? RelationshipDirectionKind.ColumnToRow : RelationshipDirectionKind.RowToColumn;
            var canCreate = eventArgs.AltKey ? this.ViewModel.CanCreateColumnToRow : this.ViewModel.CanCreateRowToColumn;

            if (canCreate)
            {
                await this.ViewModel.CreateRelationshipAsync(direction);
            }
            else if (this.ViewModel.CanDelete)
            {
                await this.ViewModel.DeleteRelationshipAsync();
            }
        }

        /// <summary>
        /// Selects the cell and opens the create/delete context menu at the cursor.
        /// </summary>
        /// <param name="eventArgs">The <see cref="MouseEventArgs" /> of the right-click.</param>
        /// <param name="row">The row <see cref="DefinedThing" />.</param>
        /// <param name="column">The column <see cref="DefinedThing" />.</param>
        private void OnCellContextMenu(MouseEventArgs eventArgs, DefinedThing row, DefinedThing column)
        {
            this.ViewModel.SelectCell(row, column);
            this.contextMenuX = eventArgs.ClientX;
            this.contextMenuY = eventArgs.ClientY;
            this.isContextMenuVisible = true;
        }

        /// <summary>
        /// Runs the given <paramref name="action" /> and closes the context menu.
        /// </summary>
        /// <param name="action">The action to run</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ExecuteContextMenuAction(Func<Task> action)
        {
            this.isContextMenuVisible = false;
            await action();
        }

        /// <summary>
        /// Gets the mark shown in a matrix cell when directionality is not shown: a check for any relationship,
        /// empty otherwise. (When directionality is shown, <see cref="GetCellArrow" /> renders a bent-arrow SVG instead.)
        /// </summary>
        /// <param name="direction">The <see cref="RelationshipDirectionKind" /> of the cell.</param>
        /// <returns>The mark to render</returns>
        private string GetCellContent(RelationshipDirectionKind direction)
        {
            return direction == RelationshipDirectionKind.None ? string.Empty : "✔";
        }

        /// <summary>
        /// Gets the inline bent-arrow SVG shown in a matrix cell for the given <see cref="RelationshipDirectionKind" />
        /// when directionality is shown: an elbow with a head pointing right (row→column), down (column→row) or both
        /// (bidirectional).
        /// </summary>
        /// <param name="direction">The <see cref="RelationshipDirectionKind" /> of the cell.</param>
        /// <returns>The SVG markup to render, empty for <see cref="RelationshipDirectionKind.None" />.</returns>
        private MarkupString GetCellArrow(RelationshipDirectionKind direction)
        {
            // Matches the COMET-IME arrows (ytox/xtoy/bidir): one bent shape — bottom-horizontal then up on the
            // right — with the head pointing UP for row→column, LEFT for column→row, and both for bidirectional.
            const string elbow = "<path d=\"M7 17 H16 V7\"/>";
            const string headUp = "<path d=\"M13 10 L16 7 L19 10\"/>";
            const string headLeft = "<path d=\"M10 14 L7 17 L10 20\"/>";

            var paths = direction switch
            {
                RelationshipDirectionKind.RowToColumn => elbow + headUp,
                RelationshipDirectionKind.ColumnToRow => elbow + headLeft,
                RelationshipDirectionKind.Bidirectional => elbow + headUp + headLeft,
                _ => string.Empty
            };

            if (paths.Length == 0)
            {
                return new MarkupString(string.Empty);
            }

            return new MarkupString($"<svg width=\"18\" height=\"18\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">{paths}</svg>");
        }
    }
}
