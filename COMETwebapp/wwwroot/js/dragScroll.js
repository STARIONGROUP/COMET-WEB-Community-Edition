// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="dragScroll.js" company="Starion Group S.A.">
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

    const cometDragScroll = {
        /**
         * Computes the vertical auto-scroll velocity while a drag is in progress, so a node dragged towards the top or
         * bottom edge of a scroll container brings the out-of-view rows into reach. Returns a negative velocity (scroll
         * up) when the pointer is within edge px of the top, a positive one (scroll down) when within edge px of the
         * bottom, and 0 in between. Kept free of DOM globals (the rect is passed in) so it is unit testable with
         * node:test - see COMETwebapp.Tests/Javascript/dragScroll.test.js.
         * @param {DOMRect} rect  - The bounding rectangle of the scroll container.
         * @param {number} clientY - The pointer's Y coordinate (e.g. from a dragover event).
         * @param {number} edge   - The distance, in pixels, from an edge within which scrolling is triggered.
         * @param {number} speed  - The scroll step, in pixels, applied each animation frame.
         * @returns {number} The signed scroll velocity in pixels per frame.
         */
        computeScrollVelocity: function (rect, clientY, edge, speed) {
            if (clientY < rect.top + edge) {
                return -speed;
            }

            if (clientY > rect.bottom - edge) {
                return speed;
            }

            return 0;
        },

        /**
         * Wires drag auto-scroll onto a scroll container. While a drag hovers near the top or bottom edge the container
         * keeps scrolling (via requestAnimationFrame) until the pointer leaves the edge or the drag ends.
         * @param {string} containerId - The id of the scroll container element.
         * @param {number} [edge]      - Distance from an edge that triggers scrolling. Defaults to 40px.
         * @param {number} [speed]     - Scroll step per frame. Defaults to 12px.
         */
        init: function (containerId, edge, speed) {
            const container = document.getElementById(containerId);

            if (!container || container._cometDragScrollInitialised) {
                return;
            }

            container._cometDragScrollInitialised = true;
            edge = edge || 40;
            speed = speed || 12;

            let velocity = 0;
            let frame = null;

            function step() {
                if (velocity === 0) {
                    frame = null;
                    return;
                }

                container.scrollTop += velocity;
                frame = window.requestAnimationFrame(step);
            }

            function stop() {
                velocity = 0;
            }

            container.addEventListener('dragover', function (e) {
                velocity = cometDragScroll.computeScrollVelocity(container.getBoundingClientRect(), e.clientY, edge, speed);

                if (velocity !== 0 && frame === null) {
                    frame = window.requestAnimationFrame(step);
                }
            });

            container.addEventListener('dragleave', function (e) {
                if (e.target === container) {
                    stop();
                }
            });

            container.addEventListener('drop', stop);
            container.addEventListener('dragend', stop);
        }
    };

    if (typeof window !== 'undefined') {
        window.cometDragScroll = cometDragScroll;
    }

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = cometDragScroll;
    }
})();
