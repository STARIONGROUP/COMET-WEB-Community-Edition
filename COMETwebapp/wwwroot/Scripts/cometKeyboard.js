// --------------------------------------------------------------------------------------------------------------------
// <copyright file="cometKeyboard.js" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// Keyboard-accessibility helpers for the application shell: global landmark hotkeys
// (Alt+Shift+N/M) and a fold/unfold hotkey (Alt+Shift+B), plus Arrow/Home/End roving navigation inside the side bar.
window.cometKeyboard = (function () {
    'use strict';

    // The letters combined with Alt+Shift for each global shortcut. Rebind here if a combination clashes.
    const shortcutKeys = { focusSidebar: 'n', focusTabs: 't', focusMainContent: 'm', toggleSidebar: 'b' };

    const sidebarSelector = 'nav.main-side-bar';
    const itemSelector = '.application-item-container';

    // The keyboard-navigable groups: the vertical side bar (Up/Down) and the horizontal open-tabs strip (Left/Right).
    const navigationGroups = [
        { container: sidebarSelector, item: itemSelector, previousKey: 'ArrowUp', nextKey: 'ArrowDown' },
        { container: '.tabs-container', item: '.tab-component', previousKey: 'ArrowLeft', nextKey: 'ArrowRight' }
    ];

    function getSidebar() {
        return document.querySelector(sidebarSelector);
    }

    // The focusable (enabled and visible) descendants of a container matching the given selector.
    function getFocusableItems(container, itemSelector) {
        return Array.prototype.slice
            .call(container.querySelectorAll(itemSelector))
            .filter(function (element) {
                return element.tabIndex >= 0 && element.offsetParent !== null;
            });
    }

    function focusSidebar() {
        const sidebar = getSidebar();

        if (!sidebar) {
            return;
        }

        const items = getFocusableItems(sidebar, itemSelector);
        (items.length ? items[0] : sidebar).focus();
    }

    function focusTabs() {
        const target = document.querySelector('.tab-component.current-tab') || document.querySelector('.tab-component');

        if (target) {
            target.focus();
        }
    }

    function focusMainContent() {
        const main = document.getElementById('main-content');

        if (main) {
            main.focus();
        }
    }

    function toggleSidebar() {
        const button = document.getElementById('side-bar-collapse-button');

        if (button) {
            button.click();
        }
    }

    // Arrow/Home/End movement inside whichever navigable group currently holds focus (side bar or tab strip), active
    // only while focus is inside one so it never steals arrow keys from grids, combo boxes or text fields elsewhere.
    function handleArrowNavigation(event) {
        const active = document.activeElement;

        if (!active || !active.closest) {
            return;
        }

        for (let i = 0; i < navigationGroups.length; i++) {
            const group = navigationGroups[i];
            const container = active.closest(group.container);

            if (!container) {
                continue;
            }

            const items = getFocusableItems(container, group.item);
            const current = items.indexOf(active);
            let next;

            switch (event.key) {
                case group.nextKey: next = current < 0 ? 0 : Math.min(current + 1, items.length - 1); break;
                case group.previousKey: next = current < 0 ? items.length - 1 : Math.max(current - 1, 0); break;
                case 'Home': next = 0; break;
                case 'End': next = items.length - 1; break;
                default: return;
            }

            if (items.length) {
                event.preventDefault();
                items[next].focus();
            }

            return;
        }
    }

    function onKeyDown(event) {
        if (event.altKey && event.shiftKey && !event.ctrlKey && !event.metaKey) {
            const key = event.key.toLowerCase();

            if (key === shortcutKeys.focusSidebar) { event.preventDefault(); focusSidebar(); return; }
            if (key === shortcutKeys.focusTabs) { event.preventDefault(); focusTabs(); return; }
            if (key === shortcutKeys.focusMainContent) { event.preventDefault(); focusMainContent(); return; }
            if (key === shortcutKeys.toggleSidebar) { event.preventDefault(); toggleSidebar(); return; }
        }

        handleArrowNavigation(event);
    }

    return {
        // Attaches the single document-level key handler. Safe to call on every render; it only wires up once.
        init: function () {
            if (window.cometKeyboardInitialized) {
                return;
            }

            window.cometKeyboardInitialized = true;
            document.addEventListener('keydown', onKeyDown, true);
        }
    };
})();
