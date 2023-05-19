var gridTbodySelector = " .dxbl-grid-table > tbody";

var dotNetHelper;
function setDotNetHelper(helper) {
    dotNetHelper = helper;
}
function initialize(firstGridSelector, secondGridSelector) {
    var draggableElementContainer = createDraggableElementContainer();
    initializeCore(draggableElementContainer, firstGridSelector, secondGridSelector, true);
    initializeCore(draggableElementContainer, secondGridSelector, firstGridSelector, false);
}
function initializeCore(draggableElementContainer, draggableGridSelector, droppableGridSelector, isFirstGridDraggable) {
    var draggableSelector = draggableGridSelector + gridTbodySelector + " > tr.dxbl-grid-group-row";
    var droppableSelector = droppableGridSelector + gridTbodySelector + " > tr.dxbl-grid-group-row";

    var draggableElementTable = draggableElementContainer.querySelector("table");
    var draggableElementTBody = draggableElementContainer.querySelector("tbody");

    $(function () {
        $(draggableSelector).draggable({
            cursor: 'move',
            helper: "clone",
            appendTo: draggableElementTBody,

            start: function (e, ui) {
                var originalRow = ui.helper.prevObject[0];
                var originalTable = originalRow.parentNode.parentNode;

                draggableElementTable.className = originalTable.className;
                draggableElementTable.style.width = originalTable.offsetWidth + "px";

                var cols = originalTable.querySelectorAll(":scope > colgroup > col");
                var row = ui.helper[0];
                for (var i = 0; i < cols.length; i++) {
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
                var droppableIndex = getRowVisibleIndex(this);
                dotNetHelper.invokeMethodAsync("MoveGridRow", droppableIndex, getRowVisibleIndex(ui.helper.prevObject[0]), isFirstGridDraggable);
            }
        });
    });
}
function getRowVisibleIndex(row) {
    var visibleIndex = -1;
    if (row)
        visibleIndex = parseInt(row.rowIndex);
    return visibleIndex;
}
function createDraggableElementContainer() {
    var container = document.createElement("DIV");
    container.innerHTML = "<table style='position: absolute; left: -10000px; top: -10000px;'><tbody></tbody></table>";
    document.body.appendChild(container);
    return container;
}

export { setDotNetHelper, initialize }