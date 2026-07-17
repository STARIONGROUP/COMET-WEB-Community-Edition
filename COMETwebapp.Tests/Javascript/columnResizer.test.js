// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="columnResizer.test.js" company="Starion Group S.A.">
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

// Unit tests for the pure width-clamp logic of columnResizer.js. Uses only Node's built-in test runner - no packages.
// Run from the repository root:  node --test COMETwebapp.Tests/Javascript/columnResizer.test.js

'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');

const cometResizer = require(path.join(__dirname, '..', '..', 'COMETwebapp', 'wwwroot', 'js', 'columnResizer.js'));

// --- minimal fake DOM ------------------------------------------------------------------------------------------------
// Only the members computeMaxWidth actually reads are modelled, so no jsdom (or any dependency) is needed.

function makeChild(spec) {
    return {
        style: { maxWidth: spec.maxWidth || '' },
        _style: {
            flexGrow: String(spec.flexGrow || 0),
            minWidth: (spec.minWidth || 0) + 'px',
            paddingLeft: (spec.padLeft || 0) + 'px',
            paddingRight: (spec.padRight || 0) + 'px',
            borderLeftWidth: (spec.borderLeft || 0) + 'px',
            borderRightWidth: (spec.borderRight || 0) + 'px'
        },
        _width: spec.width || 0,
        _visible: spec.visible !== false,
        getClientRects() { return this._visible ? [{}] : []; },
        getBoundingClientRect() { return { width: this._width, left: 0 }; },
        nextElementSibling: null
    };
}

function makeRow(spec, children) {
    for (let i = 0; i < children.length - 1; i++) {
        children[i].nextElementSibling = children[i + 1];
    }

    return {
        firstElementChild: children[0] || null,
        clientWidth: spec.clientWidth,
        _style: {
            columnGap: (spec.gap || 0) + 'px',
            paddingLeft: (spec.padLeft || 0) + 'px',
            paddingRight: (spec.padRight || 0) + 'px'
        }
    };
}

const getStyle = node => node._style;

function maxWidth(row, leftEl, maxPx) {
    return cometResizer.computeMaxWidth(row, leftEl, maxPx, getStyle);
}

// --- tests -----------------------------------------------------------------------------------------------------------

test('reserves every sibling, the column-gap and the padding so the row fits exactly (no overflow)', () => {
    const source = makeChild({ flexGrow: 1, minWidth: 0, padLeft: 15, padRight: 15, borderLeft: 1, borderRight: 1 });
    const sourceResizer = makeChild({ flexGrow: 0, width: 6 });
    const target = makeChild({ flexGrow: 1, minWidth: 0, padLeft: 15, padRight: 15, borderLeft: 1, borderRight: 1 });
    const targetResizer = makeChild({ flexGrow: 0, width: 6 });
    const details = makeChild({ flexGrow: 1, minWidth: 350 });
    const row = makeRow({ clientWidth: 1200, gap: 10 }, [source, sourceResizer, target, targetResizer, details]);

    const max = maxWidth(row, target);

    // reserve = source(32 padding+border) + sourceResizer(6) + targetResizer(6) + details(350) = 394; gaps = 4 * 10.
    assert.equal(max, 766);
    assert.equal(max + 394 + 4 * 10, row.clientWidth, 'the widened panel plus reservations must fill the row exactly');
});

test('discounts the column-gap (ignoring it overflowed the row by the total gap width)', () => {
    const build = gap => {
        const source = makeChild({ flexGrow: 1, minWidth: 0 });
        const target = makeChild({ flexGrow: 1, minWidth: 0 });
        const details = makeChild({ flexGrow: 1, minWidth: 350 });
        return { row: makeRow({ clientWidth: 1000, gap }, [source, target, details]), target };
    };

    const withGap = build(10);
    const noGap = build(0);

    // three children => two gaps between them.
    assert.equal(maxWidth(noGap.row, noGap.target) - maxWidth(withGap.row, withGap.target), 2 * 10);
});

test('a min-width:0 flex panel still reserves its own padding and border (box-sizing floor)', () => {
    const target = makeChild({ flexGrow: 1, minWidth: 0 });
    const sibling = makeChild({ flexGrow: 1, minWidth: 0, padLeft: 20, padRight: 20, borderLeft: 1, borderRight: 1 });
    const row = makeRow({ clientWidth: 1000 }, [target, sibling]);

    assert.equal(maxWidth(row, target), 1000 - 42, 'padding+border is reserved even though min-width is 0');
});

test('a panel already pinned to a fixed width reserves its current width, not its min-width', () => {
    const target = makeChild({ flexGrow: 1, minWidth: 0 });
    const pinned = makeChild({ flexGrow: 0, minWidth: 0, width: 500, maxWidth: '500px' });
    const row = makeRow({ clientWidth: 1000 }, [target, pinned]);

    assert.equal(maxWidth(row, target), 500);
});

test('a collapsed (display:none) sibling is skipped entirely - no reserve and no gap', () => {
    const target = makeChild({ flexGrow: 1, minWidth: 0 });
    const hidden = makeChild({ flexGrow: 1, minWidth: 350, visible: false });
    const row = makeRow({ clientWidth: 1000, gap: 10 }, [target, hidden]);

    assert.equal(maxWidth(row, target), 1000);
});

test('honours an explicit maxPx ceiling only when it is the smaller limit', () => {
    const target = makeChild({ flexGrow: 1, minWidth: 0 });
    const sibling = makeChild({ flexGrow: 1, minWidth: 100 });
    const row = makeRow({ clientWidth: 1000 }, [target, sibling]);

    assert.equal(maxWidth(row, target, 500), 500, 'the room would allow 900, the ceiling caps it at 500');
    assert.equal(maxWidth(row, target, 5000), 900, 'a ceiling above the available room lets the room govern');
});

test('with no maxPx (omitted or 0) the only limit is the room left in the row', () => {
    const target = makeChild({ flexGrow: 1, minWidth: 0 });
    const sibling = makeChild({ flexGrow: 1, minWidth: 100 });
    const row = makeRow({ clientWidth: 1000 }, [target, sibling]);

    assert.equal(maxWidth(row, target, 0), 900);
    assert.equal(maxWidth(row, target), 900);
});
