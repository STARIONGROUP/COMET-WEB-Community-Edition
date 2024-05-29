// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SelectorsBaseWrapper.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Model.Selectors
{
    using CDP4Common.CommonData;

    using COMET.Web.Common.Extensions;

    /// <summary>
    /// The wrapper to be used to display and select data in the <see cref="DefinedThing" />
    /// </summary>
    public class SelectorModelBaseWrapper<T> where T : DefinedThing
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SelectorModelBaseWrapper{T}" />
        /// </summary>
        /// <param name="thing">The thing to be wrapped</param>
        public SelectorModelBaseWrapper(T thing)
        {
            this.WrappedThing = thing;
        }

        /// <summary>
        /// The thing to be selected
        /// </summary>
        public T WrappedThing { get; set; }

        /// <summary>
        /// The text to display for thing selection
        /// </summary>
        public string DisplayText => this.WrappedThing?.GetSelectorNameAndShortname();
    }
}
