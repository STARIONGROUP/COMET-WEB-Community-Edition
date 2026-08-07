// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ChartColors.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ModelDashboard
{
    using System.Drawing;

    /// <summary>
    /// The green/red color palette shared by the Model Dashboard charts and progress bars. Each dashboard "feature"
    /// (Published Parameters, Missing Values, Unused Elements, Unreferenced Elements) owns ONE green/red pair used
    /// consistently across all of its graphs, and the four features use four visually distinct pairs. Green means
    /// "done", red means "needs attention" (see issue #892). Colors are lowercase hex strings so they can also feed the
    /// AntDesign <c>Progress</c> bars, whose <c>StrokeColor</c> takes a <c>"#rrggbb"</c> string, each with a matching
    /// <see cref="Color" /> form for the DevExpress chart series.
    /// </summary>
    public static class ChartColors
    {
        /// <summary>
        /// The "done" color of the Published Parameters feature (published parameters). A light green.
        /// </summary>
        public const string PublishedColor = "#8bc34a";

        /// <summary>
        /// The "needs attention" color of the Published Parameters feature (publishable parameters). A light red.
        /// </summary>
        public const string PublishableColor = "#ef5350";

        /// <summary>
        /// The "done" color of the Missing Values feature (complete values). A sea green.
        /// </summary>
        public const string CompleteColor = "#2e8b57";

        /// <summary>
        /// The "needs attention" color of the Missing Values feature (missing values). An orange red.
        /// </summary>
        public const string MissingColor = "#d21f04";

        /// <summary>
        /// The "done" color of the Unused Elements feature (used elements). A vivid green.
        /// </summary>
        public const string UsedColor = "#00c853";

        /// <summary>
        /// The "needs attention" color of the Unused Elements feature (unused elements). A vivid red.
        /// </summary>
        public const string UnusedColor = "#ff1744";

        /// <summary>
        /// The "done" color of the Unreferenced Elements feature (referenced elements). A dark green.
        /// </summary>
        public const string ReferencedColor = "#1b5e20";

        /// <summary>
        /// The "needs attention" color of the Unreferenced Elements feature (unreferenced elements). A dark red.
        /// </summary>
        public const string UnreferencedColor = "#7f0000";

        /// <summary>
        /// The <see cref="Color" /> form of <see cref="PublishedColor" /> for DevExpress chart series.
        /// </summary>
        public static Color PublishedChartColor => ColorTranslator.FromHtml(PublishedColor);

        /// <summary>
        /// The <see cref="Color" /> form of <see cref="PublishableColor" /> for DevExpress chart series.
        /// </summary>
        public static Color PublishableChartColor => ColorTranslator.FromHtml(PublishableColor);

        /// <summary>
        /// The <see cref="Color" /> form of <see cref="CompleteColor" /> for DevExpress chart series.
        /// </summary>
        public static Color CompleteChartColor => ColorTranslator.FromHtml(CompleteColor);

        /// <summary>
        /// The <see cref="Color" /> form of <see cref="MissingColor" /> for DevExpress chart series.
        /// </summary>
        public static Color MissingChartColor => ColorTranslator.FromHtml(MissingColor);

        /// <summary>
        /// The <see cref="Color" /> form of <see cref="UsedColor" /> for DevExpress chart series.
        /// </summary>
        public static Color UsedChartColor => ColorTranslator.FromHtml(UsedColor);

        /// <summary>
        /// The <see cref="Color" /> form of <see cref="UnusedColor" /> for DevExpress chart series.
        /// </summary>
        public static Color UnusedChartColor => ColorTranslator.FromHtml(UnusedColor);

        /// <summary>
        /// The <see cref="Color" /> form of <see cref="ReferencedColor" /> for DevExpress chart series.
        /// </summary>
        public static Color ReferencedChartColor => ColorTranslator.FromHtml(ReferencedColor);

        /// <summary>
        /// The <see cref="Color" /> form of <see cref="UnreferencedColor" /> for DevExpress chart series.
        /// </summary>
        public static Color UnreferencedChartColor => ColorTranslator.FromHtml(UnreferencedColor);
    }
}
