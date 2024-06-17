// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabBody.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Components.Tabs
{
    using COMET.Web.Common.Components;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Core component for the System Representation application
    /// </summary>
    public partial class TabBody : DisposableComponent
    {
        /// <summary>
        /// Gets or sets the text to be displayed in the tab title
        /// </summary>
        [Parameter]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the icon to be displayed
        /// </summary>
        [Parameter]
        public Type Icon { get; set; }

        /// <summary>
        /// Gets the icon configuration to display
        /// </summary>
        private static Dictionary<string, object> IconConfiguration => new()
        {
            { "Size", 20 },
            { "Color", "currentColor" },
            { "StrokeWidth", 1.8f }
        };
    }
}
