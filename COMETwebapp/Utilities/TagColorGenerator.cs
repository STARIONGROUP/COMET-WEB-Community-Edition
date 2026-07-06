// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TagColorGenerator.cs" company="Starion Group S.A.">
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
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Utilities
{
    /// <summary>
    /// Static helper that maps an arbitrary string key to a stable pastel background colour expressed as a
    /// CSS <c>hsl()</c> value. The colour is deterministic across runs because it is derived from a
    /// FNV-1a hash of the key's characters rather than from <see cref="string.GetHashCode()" /> (whose
    /// output is deliberately unstable between .NET process invocations).
    /// </summary>
    public static class TagColorGenerator
    {
        /// <summary>
        /// FNV-1a 32-bit offset basis.
        /// </summary>
        private const uint FnvOffsetBasis = 2166136261u;

        /// <summary>
        /// FNV-1a 32-bit prime.
        /// </summary>
        private const uint FnvPrime = 16777619u;

        /// <summary>
        /// Returns a stable pastel CSS background-colour string for the supplied <paramref name="key" />.
        /// When <paramref name="key" /> is <see langword="null" /> or empty, a neutral light-grey
        /// (<c>#e9ecef</c>) is returned so pill tags always have a visible background.
        /// </summary>
        /// <param name="key">
        /// An arbitrary string — typically a <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" />
        /// or <see cref="CDP4Common.SiteDirectoryData.Category" /> short name — whose characters are
        /// hashed to derive a hue angle.
        /// </param>
        /// <returns>
        /// A CSS colour string of the form <c>hsl({hue}, 70%, 85%)</c> where <c>{hue}</c> is a stable
        /// integer in the range 0–359, or <c>#e9ecef</c> when <paramref name="key" /> is
        /// <see langword="null" /> or empty.
        /// </returns>
        public static string GetBackgroundColor(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "#e9ecef";
            }

            var hash = ComputeFnv1A(key);
            var hue = (int)(hash % 360u);
            return $"hsl({hue}, 70%, 85%)";
        }

        /// <summary>
        /// Computes a 32-bit FNV-1a hash of the supplied string, treating each <see cref="char" /> as
        /// two bytes (low byte first) to produce a consistent result regardless of platform endianness.
        /// </summary>
        /// <param name="value">The non-null, non-empty string to hash.</param>
        /// <returns>The 32-bit FNV-1a hash of <paramref name="value" />.</returns>
        private static uint ComputeFnv1A(string value)
        {
            var hash = FnvOffsetBasis;

            foreach (var character in value)
            {
                hash ^= (byte)(character & 0xFF);
                hash *= FnvPrime;
                hash ^= (byte)(character >> 8);
                hash *= FnvPrime;
            }

            return hash;
        }
    }
}
