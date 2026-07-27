// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="dragScroll.test.js" company="Starion Group S.A.">
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

// Unit tests for the pure edge-detection logic of dragScroll.js. Uses only Node's built-in test runner - no packages.
// Run from the repository root:  node --test COMETwebapp.Tests/Javascript/dragScroll.test.js

'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');

const cometDragScroll = require(path.join(__dirname, '..', '..', 'COMETwebapp', 'wwwroot', 'js', 'dragScroll.js'));

const rect = { top: 100, bottom: 500 };
const edge = 40;
const speed = 12;

function velocity(clientY) {
    return cometDragScroll.computeScrollVelocity(rect, clientY, edge, speed);
}

test('scrolls up (negative velocity) when the pointer is within the top edge band', () => {
    assert.equal(velocity(rect.top), -speed);
    assert.equal(velocity(rect.top + edge - 1), -speed);
});

test('scrolls down (positive velocity) when the pointer is within the bottom edge band', () => {
    assert.equal(velocity(rect.bottom), speed);
    assert.equal(velocity(rect.bottom - edge + 1), speed);
});

test('does not scroll when the pointer is in the middle, outside both edge bands', () => {
    assert.equal(velocity(rect.top + edge), 0);
    assert.equal(velocity(rect.bottom - edge), 0);
    assert.equal(velocity((rect.top + rect.bottom) / 2), 0);
});
