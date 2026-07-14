// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterValueFormatter.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    /// <summary>
    /// Formatting helpers shared by the parameter/subscription row view models for turning a value array into a
    /// display string.
    /// </summary>
    public static class ParameterValueFormatter
    {
        /// <summary>
        /// Formats a value array as a single display string: comma-separates the entries, wraps them in braces when
        /// there is more than one, and appends a <c>[shortName]</c> scale suffix when <paramref name="scale" /> is set.
        /// </summary>
        /// <param name="values">The values to format.</param>
        /// <param name="scale">The <see cref="MeasurementScale" /> whose short name is appended as a unit suffix, or <c>null</c>.</param>
        /// <returns>The formatted display string.</returns>
        public static string Format(IEnumerable<string> values, MeasurementScale scale = null)
        {
            var list = values.ToList();
            var joined = string.Join(", ", list);
            var display = list.Count > 1 ? "{" + joined + "}" : joined;

            return scale != null ? display + " [" + scale.ShortName + "]" : display;
        }

        /// <summary>
        /// Formats a value array for display, grouping a <see cref="SampledFunctionParameterType" />'s flat array into
        /// one <c>{…}</c> group per sampled row (e.g. a 2×3 function reads <c>{3, 4, 1}, {4, 8, 2}</c> instead of the
        /// undelimited <c>{3, 4, 1, 4, 8, 2}</c>). Any other parameter type falls back to <see cref="Format(IEnumerable{string}, MeasurementScale)" />.
        /// </summary>
        /// <param name="values">The value array to format.</param>
        /// <param name="parameterType">The <see cref="ParameterType" /> the value array belongs to.</param>
        /// <param name="scale">The <see cref="MeasurementScale" /> appended as a unit suffix, or <c>null</c>.</param>
        /// <returns>The formatted display string.</returns>
        public static string Format(ValueArray<string> values, ParameterType parameterType, MeasurementScale scale = null)
        {
            if (parameterType is not SampledFunctionParameterType { NumberOfValues: > 0 } sampledFunctionParameterType)
            {
                return Format(values, scale);
            }

            var columns = sampledFunctionParameterType.NumberOfValues;
            var rows = new List<string>();

            for (var start = 0; start < values.Count; start += columns)
            {
                var row = new List<string>();

                for (var column = start; column < start + columns && column < values.Count; column++)
                {
                    row.Add(values[column]);
                }

                rows.Add("{" + string.Join(", ", row) + "}");
            }

            return string.Join(", ", rows);
        }

        /// <summary>
        /// Returns the value at <paramref name="index" /> of the supplied array, or an empty string when out of range.
        /// </summary>
        /// <param name="values">The value array.</param>
        /// <param name="index">The index to read.</param>
        /// <returns>The value at the index, or an empty string.</returns>
        public static string ValueAt(ValueArray<string> values, int index)
        {
            return index >= 0 && index < values.Count ? values[index] : string.Empty;
        }
    }
}
