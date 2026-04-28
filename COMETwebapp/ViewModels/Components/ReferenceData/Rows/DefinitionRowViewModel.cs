// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DefinitionRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.Rows
{
    using CDP4Common.CommonData;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    /// <summary>
    /// Row View Model for a <see cref="Definition" /> shown in a <see cref="DefinedThing" />'s definitions grid.
    /// </summary>
    public class DefinitionRowViewModel : BaseDataItemRowViewModel<Definition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionRowViewModel" /> class.
        /// </summary>
        /// <param name="definition">The associated <see cref="Definition" /></param>
        public DefinitionRowViewModel(Definition definition) : base(definition)
        {
            this.Content = definition.Content;
            this.LanguageCode = definition.LanguageCode;
        }

        /// <summary>
        /// The textual content of the <see cref="Definition" />.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The ISO language code of the <see cref="Definition" /> (for example <c>en-GB</c>).
        /// </summary>
        public string LanguageCode { get; set; }
    }
}
