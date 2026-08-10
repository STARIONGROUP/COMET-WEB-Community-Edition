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

// Keyboard-accessibility helpers for the application shell: global landmark hotkeys (Alt+Shift+1/2/3) and a fold/unfold
// hotkey (Alt+Shift+4), plus Arrow/Home/End roving navigation inside the side bar and the open-tabs strip.
window.cometKeyboard = (function () {
    'use strict';

    // The PHYSICAL key of each Alt+Shift shortcut, matched against event.code. Two reasons for physical-key matching:
    // (1) event.key is layout- and modifier-dependent, so on macOS the Option (Alt) modifier rewrites the produced
    // character; (2) digits are used rather than letters because Chrome reserves several Shift+Alt+letter combinations
    // (Shift+Alt+T focuses the toolbar, and experimental builds bind others), which a page cannot override. Rebind here
    // (and in docs/keyboard-shortcuts.md) if a combination clashes; values are KeyboardEvent.code identifiers.
    const shortcutCodes = { focusSidebar: 'Digit1', focusTabs: 'Digit2', focusMainContent: 'Digit3', toggleSidebar: 'Digit4' };

    const sidebarSelector = 'nav.main-side-bar';
    const itemSelector = '.application-item-container';

    // The keyboard-navigable groups: the vertical side bar (Up/Down) and the horizontal open-tabs strip (Left/Right).
    const navigationGroups = [
        { container: sidebarSelector, item: itemSelector, previousKey: 'ArrowUp', nextKey: 'ArrowDown' },
        { container: '.tabs-container', item: '.tab-component', previousKey: 'ArrowLeft', nextKey: 'ArrowRight' }
    ];

    // The roles whose Space activation must not also scroll the page.
    const activatableSelector = '.tab-component, .application-item-container';

    function getSidebar() {
        return document.querySelector(sidebarSelector);
    }

    // The focusable (enabled and visible) descendants of a container matching the given selector.
    function getFocusableItems(container, itemSelector) {
        return Array.prototype.slice
            .call(container.querySelectorAll(itemSelector))
            .filter(function (element) {
                return element.tabIndex >= 0 && !element.disabled && element.offsetParent !== null;
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

        if (!active?.closest) {
            return;
        }

        for (const group of navigationGroups) {
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
            switch (event.code) {
                case shortcutCodes.focusSidebar: event.preventDefault(); focusSidebar(); return;
                case shortcutCodes.focusTabs: event.preventDefault(); focusTabs(); return;
                case shortcutCodes.focusMainContent: event.preventDefault(); focusMainContent(); return;
                case shortcutCodes.toggleSidebar: event.preventDefault(); toggleSidebar(); return;
            }
        }

        // A side bar entry or tab is activated on Space by its own C# handler; suppress the browser default here so the
        // nearest scrollable ancestor does not scroll at the same time. Doing it centrally keeps the C# handlers simple
        // and avoids an unconditional @onkeydown:preventDefault (which would also swallow Tab).
        if (event.key === ' ' && event.target.closest?.(activatableSelector)) {
            event.preventDefault();
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
        },

        focusSidebar: focusSidebar,
        focusTabs: focusTabs,
        focusMainContent: focusMainContent,
        toggleSidebar: toggleSidebar,

        // Moves focus to the first focusable element inside the container matched by the selector. Used to move keyboard
        // focus into a drop-down (the side bar Model/Session menus) when it opens, since it renders in a body-level
        // portal that Tab order would otherwise never reach. The DevExpress inner DOM can render a frame or two after the
        // drop-down's Shown event, so it retries across a few animation frames, then falls back to the container itself
        // (which carries tabindex="-1") so focus always ends up inside the drop-down.
        focusFirstIn: function (selector) {
            const focusableSelector = 'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';
            let remainingTries = 5;

            const attempt = function () {
                const container = document.querySelector(selector);

                if (!container) {
                    return;
                }

                const focusable = container.querySelector(focusableSelector);

                if (focusable) {
                    focusable.focus();
                } else if (remainingTries-- > 0) {
                    requestAnimationFrame(attempt);
                } else {
                    container.focus();
                }
            };

            attempt();
        },

        // Returns focus to a trigger element (by selector) when its drop-down closes, so keyboard focus is not lost at
        // the end of the document.
        focusElement: function (selector) {
            const element = document.querySelector(selector);

            if (element) {
                element.focus();
            }
        }
    };
})();
