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

(function () {
    'use strict';

    const cometResizer = {
        /**
         * Computes the maximum width, in pixels, that the resized column may take without pushing the flex row past its
         * content box (which would add a horizontal scrollbar). Every other visible child is reserved at the smallest
         * footprint it can occupy: a flexible panel yields down to its min-width but never below its own padding + border;
         * a resizer, a collapsed-panel strip or a panel already pinned to a fixed width keeps its current width. The
         * row's column-gap and horizontal padding are discounted as well. When maxPx is falsy the only limit is the room
         * left in the row. Kept free of event wiring and DOM globals (the style accessor is injectable) so it is unit
         * testable with node:test - see COMETwebapp.Tests/Javascript/columnResizer.test.js.
         * @param {Element} row        - The flex row that holds the columns.
         * @param {Element} leftEl     - The column being resized.
         * @param {number} [maxPx]     - Optional upper ceiling in pixels; when falsy only the row's room limits the drag.
         * @param {function} [getStyle] - Computed-style accessor; defaults to window.getComputedStyle.
         * @returns {number} The maximum width, in pixels, for leftEl.
         */
        computeMaxWidth: function (row, leftEl, maxPx, getStyle) {
            getStyle = getStyle || function (element) { return window.getComputedStyle(element); };

            const rowStyle = getStyle(row);
            let reserve = 0;
            let visibleCount = 0;

            for (let child = row.firstElementChild; child; child = child.nextElementSibling) {
                if (child.getClientRects().length === 0) {
                    continue;                      
                }

                if (child !== leftEl) {
                    const childStyle = getStyle(child);
                    const canShrink = Number.parseFloat(childStyle.flexGrow) > 0 && child.style.maxWidth === '';

                    if (canShrink) {
                        const incompressible = (Number.parseFloat(childStyle.paddingLeft) || 0) + (Number.parseFloat(childStyle.paddingRight) || 0)
                            + (Number.parseFloat(childStyle.borderLeftWidth) || 0) + (Number.parseFloat(childStyle.borderRightWidth) || 0);
                        reserve += Math.max(Number.parseFloat(childStyle.minWidth) || 0, incompressible);
                    } else {
                        reserve += child.getBoundingClientRect().width;
                    }
                }

                visibleCount++;
            }

            const gap = Number.parseFloat(rowStyle.columnGap) || 0;
            const padding = (Number.parseFloat(rowStyle.paddingLeft) || 0) + (Number.parseFloat(rowStyle.paddingRight) || 0);
            const available = row.clientWidth - padding - gap * Math.max(0, visibleCount - 1);

            return maxPx ? Math.min(maxPx, available - reserve) : available - reserve;
        },

        /**
         * Initialises a drag-to-resize handle between two flex columns.
         * @param {string} resizerId - The id of the resizer element.
         * @param {string} leftId    - The id of the left (resizable) column element.
         * @param {number} minPx     - Minimum width in pixels.
         * @param {number} [maxPx]   - Optional maximum width in pixels. When omitted (or 0), the only limit is the room
         *                             left in the flex row, so the panel can be widened up to (nearly) the full row.
         */
        init: function (resizerId, leftId, minPx, maxPx) {
            const resizerEl = document.getElementById(resizerId);
            const leftEl = document.getElementById(leftId);

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

                const effectiveMax = cometResizer.computeMaxWidth(leftEl.parentElement, leftEl, maxPx);

                function onMouseMove(moveEvent) {
                    const w = Math.min(effectiveMax, Math.max(minPx, moveEvent.clientX - leftEl.getBoundingClientRect().left));
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

    if (typeof window !== 'undefined') {
        window.cometResizer = cometResizer;
    }

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = cometResizer;     
    }
})();
