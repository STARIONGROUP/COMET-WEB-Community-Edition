// --------------------------------------------------------------------------------------------------------------------
//  Copyright (c) 2023-2026 Starion Group S.A.
//
//  This file is part of COMET WEB Community Edition
//  The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25
//  Annex A and Annex C.
//
//  The COMET WEB Community Edition is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Affero General Public License as published by the Free Software
//  Foundation; either version 3 of the License, or (at your option) any later version.
//
//  The COMET WEB Community Edition is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
//  PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License along with this program.
//  If not, see http://www.gnu.org/licenses/.
// --------------------------------------------------------------------------------------------------------------------

/**
 * Masonry layout helper for the .param-groups-container — packs top-level .group-card children
 * into the shortest column first so that a lone short group does not span a full new row.
 */
window.cometMasonry = (function () {

    /**
     * Per-container state keyed by the container DOM element itself.
     * Each value is { resizeObserver, mutationObserver, rafHandle }.
     */
    var _state = new WeakMap();

    var GAP = 12;
    var COL_WIDTH = 300;

    /**
     * Performs the masonry layout on all direct .group-card children of container.
     * @param {HTMLElement} container
     */
    function layout(container) {
        var children = Array.prototype.slice.call(container.querySelectorAll(':scope > .group-card'));

        if (children.length === 0) {
            container.style.height = '';
            return;
        }

        var totalWidth = container.clientWidth;

        if (totalWidth === 0) {
            return;
        }

        var cols = Math.max(1, Math.floor((totalWidth + GAP) / (COL_WIDTH + GAP)));
        var actualColWidth = (totalWidth - GAP * (cols - 1)) / cols;

        var colHeights = new Array(cols).fill(0);

        children.forEach(function (child) {
            child.style.position = 'absolute';
            child.style.width = actualColWidth + 'px';

            // Find the shortest column
            var minHeight = colHeights[0];
            var col = 0;

            for (var i = 1; i < cols; i++) {
                if (colHeights[i] < minHeight) {
                    minHeight = colHeights[i];
                    col = i;
                }
            }

            child.style.left = (col * (actualColWidth + GAP)) + 'px';
            child.style.top = colHeights[col] + 'px';

            colHeights[col] += child.offsetHeight + GAP;
        });

        var maxHeight = Math.max.apply(null, colHeights);
        container.style.height = maxHeight + 'px';
    }

    /**
     * Schedules a layout run using requestAnimationFrame to debounce rapid calls.
     * @param {HTMLElement} container
     * @param {{ rafHandle: number }} state
     */
    function scheduleLayout(container, state) {
        if (state.rafHandle) {
            cancelAnimationFrame(state.rafHandle);
        }

        state.rafHandle = requestAnimationFrame(function () {
            state.rafHandle = 0;
            layout(container);
        });
    }

    /**
     * Initialises the masonry layout for the supplied container element.
     * Attaches a ResizeObserver on the container and all direct .group-card children, plus a
     * MutationObserver on the container to detect child additions/removals.
     * @param {HTMLElement} container
     */
    function init(container) {
        if (!container || _state.has(container)) {
            return;
        }

        container.style.position = 'relative';

        var state = { rafHandle: 0, resizeObserver: null, mutationObserver: null };

        var ro = new ResizeObserver(function () {
            scheduleLayout(container, state);
        });

        ro.observe(container);

        var children = container.querySelectorAll(':scope > .group-card');
        children.forEach(function (child) {
            ro.observe(child);
        });

        var mo = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1 && node.classList && node.classList.contains('group-card')) {
                        ro.observe(node);
                    }
                });
            });

            scheduleLayout(container, state);
        });

        mo.observe(container, { childList: true });

        state.resizeObserver = ro;
        state.mutationObserver = mo;

        _state.set(container, state);

        layout(container);
    }

    /**
     * Cleans up observers and inline styles set by this module on the supplied container.
     * @param {HTMLElement} container
     */
    function dispose(container) {
        if (!container || !_state.has(container)) {
            return;
        }

        var state = _state.get(container);

        if (state.rafHandle) {
            cancelAnimationFrame(state.rafHandle);
        }

        if (state.resizeObserver) {
            state.resizeObserver.disconnect();
        }

        if (state.mutationObserver) {
            state.mutationObserver.disconnect();
        }

        _state.delete(container);

        // Remove inline styles set by the layout function on children.
        var children = container.querySelectorAll(':scope > .group-card');
        children.forEach(function (child) {
            child.style.position = '';
            child.style.width = '';
            child.style.left = '';
            child.style.top = '';
        });

        container.style.height = '';
        container.style.position = '';
    }

    return {
        init: init,
        dispose: dispose
    };

}());
