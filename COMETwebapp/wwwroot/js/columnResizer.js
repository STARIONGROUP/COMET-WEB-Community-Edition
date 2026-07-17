// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="columnResizer.js" company="Starion Group S.A.">
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
//     Affero General Public License for more details.
//
//     You should have received a copy of the GNU Affero General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
// --------------------------------------------------------------------------------------------------------------------

window.cometResizer = {
    /**
     * Initialises a drag-to-resize handle between two flex columns.
     * @param {string} resizerId - The id of the resizer element.
     * @param {string} leftId    - The id of the left (resizable) column element.
     * @param {number} minPx     - Minimum width in pixels.
     * @param {number} maxPx     - Maximum width in pixels.
     */
    init: function (resizerId, leftId, minPx, maxPx) {
        var resizerEl = document.getElementById(resizerId);
        var leftEl = document.getElementById(leftId);

        if (!resizerEl || !leftEl) {
            return;
        }

        // Guard against double-init
        if (resizerEl._cometResizerInitialised) {
            return;
        }

        resizerEl._cometResizerInitialised = true;

        resizerEl.addEventListener('mousedown', function (e) {
            e.preventDefault();
            document.body.style.userSelect = 'none';
            
            var row = leftEl.parentElement;
            var rowStyle = window.getComputedStyle(row);
            var reserve = 0;
            var visibleCount = 0;

            for (var child = row.firstElementChild; child; child = child.nextElementSibling) {
                if (child.getClientRects().length === 0) {
                    continue;                      
                }

                if (child !== leftEl) {
                    var childStyle = window.getComputedStyle(child);
                    var canShrink = parseFloat(childStyle.flexGrow) > 0 && child.style.maxWidth === '';

                    if (canShrink) {
                        var incompressible = (parseFloat(childStyle.paddingLeft) || 0) + (parseFloat(childStyle.paddingRight) || 0)
                            + (parseFloat(childStyle.borderLeftWidth) || 0) + (parseFloat(childStyle.borderRightWidth) || 0);
                        reserve += Math.max(parseFloat(childStyle.minWidth) || 0, incompressible);
                    } else {
                        reserve += child.getBoundingClientRect().width;
                    }
                }

                visibleCount++;
            }

            var gap = parseFloat(rowStyle.columnGap) || 0;
            var padding = (parseFloat(rowStyle.paddingLeft) || 0) + (parseFloat(rowStyle.paddingRight) || 0);
            var available = row.clientWidth - padding - gap * Math.max(0, visibleCount - 1);
            var effectiveMax = Math.min(maxPx, available - reserve);

            function onMouseMove(moveEvent) {
                var w = Math.min(effectiveMax, Math.max(minPx, moveEvent.clientX - leftEl.getBoundingClientRect().left));
                leftEl.style.flex = '0 0 ' + w + 'px';
                leftEl.style.maxWidth = w + 'px';
            }

            function onMouseUp() {
                document.removeEventListener('mousemove', onMouseMove);
                document.removeEventListener('mouseup', onMouseUp);
                document.body.style.userSelect = '';
            }

            document.addEventListener('mousemove', onMouseMove);
            document.addEventListener('mouseup', onMouseUp);
        });
    }
};
