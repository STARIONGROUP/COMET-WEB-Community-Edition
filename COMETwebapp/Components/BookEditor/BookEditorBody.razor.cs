// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BookEditorBody.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.BookEditor
{
    /// <summary>
    /// Core component for the Book Editor application
    /// </summary>
    public partial class BookEditorBody
    {
        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
        }

        /// <summary>
        /// Gets the text of the header depending on the state of the ViewModel
        /// </summary>
        /// <returns></returns>
        private string GetHeaderText()
        {
            if (this.ViewModel.IsOnBookCreation)
            {
                return "Create a new Book";
            }
            else if (this.ViewModel.IsOnSectionCreation)
            {
                return "Create a new Section";
            }
            else if (this.ViewModel.IsOnPageCreation)
            {
                return "Create a new Page";
            }
            else if (this.ViewModel.IsOnNoteCreation)
            {
                return "Create a new Node";
            }

            return string.Empty;
        }
    }
}
