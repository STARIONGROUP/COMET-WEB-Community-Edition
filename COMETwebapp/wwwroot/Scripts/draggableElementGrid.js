// --------------------------------------------------------------------------------------------------------------------
// <copyright file="draggableElementGrid.js" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

/**
 * The grid tbody selector
 * @type {string}
 */
const gridTbodySelector = " .dxbl-grid-table > tbody";

/**
 * The dotnet helper
 * @type {any}
 */
let dotNetHelper;

/**
 * Sets the dotnet helper
 * @param {any} helper
 */
function setDotNetHelper(helper) {
    dotNetHelper = helper;
}

/**
 * Initializes the draggable element grid
 * @param {string} firstGridSelector
 * @param {string} secondGridSelector
 */
function initialize(firstGridSelector, secondGridSelector) {
    const draggableElementContainer = createDraggableElementContainer();
    initializeCore(draggableElementContainer, firstGridSelector, secondGridSelector, true);
    initializeCore(draggableElementContainer, secondGridSelector, firstGridSelector, false);
}

/**
 * Initializes the draggable grouped element grid
 * @param {any} draggableElementContainer
 * @param {string} draggableGridSelector
 * @param {string} droppableGridSelector
 * @param {boolean} isFirstGridDraggable
 */
function initializeCore(draggableElementContainer, draggableGridSelector, droppableGridSelector, isFirstGridDraggable) {
    let draggableSelector = draggableGridSelector + gridTbodySelector + " > tr.dxbl-grid-group-row";
    let droppableSelector = droppableGridSelector + gridTbodySelector + " > tr.dxbl-grid-group-row";

    let draggableElementTable = draggableElementContainer.querySelector("table");
    let draggableElementTBody = draggableElementContainer.querySelector("tbody");

    $(function () {
        $(draggableSelector).draggable({
            cursor: 'move',
            helper: "clone",
            appendTo: draggableElementTBody,

            start: function (e, ui) {
                let originalRow = ui.helper.prevObject[0];
                let originalTable = originalRow.parentNode.parentNode;

                draggableElementTable.className = originalTable.className;
                draggableElementTable.style.width = originalTable.offsetWidth + "px";

                let cols = originalTable.querySelectorAll(":scope > colgroup > col");
                let row = ui.helper[0];
                for (let i = 0; i < cols.length; i++) {
                    row.cells[i].style.width = cols[i].offsetWidth + "px";
                }

                row.style.width = originalRow.offsetWidth + "px";
                row.style.height = originalRow.offsetHeight + "px";
                row.style.backgroundColor = "white";
                row.style.zIndex = "1000";
            }
        });
        $(droppableSelector).droppable({
            accept: draggableSelector,
            classes: {
                "ui-droppable-active": "ui-state-default",
                "ui-droppable-hover": "ui-state-hover"
            },
            drop: function (e, ui) {
                let droppableIndex = getRowVisibleIndex(this);
                dotNetHelper.invokeMethodAsync("MoveGridRow", droppableIndex, getRowVisibleIndex(ui.helper.prevObject[0]), isFirstGridDraggable);
            }
        });
    });
}

/**
 * Gets the visible index of the row
 * @param {any} row
 * @returns {number}
 */
function getRowVisibleIndex(row) {
    let visibleIndex = -1;
    if (row)
        visibleIndex = parseInt(row.rowIndex);
    return visibleIndex;
}

/**
 * Creates the draggable element container
 * @returns {any}
 */
function createDraggableElementContainer() {
    let container = document.createElement("DIV");
    container.innerHTML = "<table style='position: absolute; left: -10000px; top: -10000px;'><tbody></tbody></table>";
    document.body.appendChild(container);
    return container;
}
