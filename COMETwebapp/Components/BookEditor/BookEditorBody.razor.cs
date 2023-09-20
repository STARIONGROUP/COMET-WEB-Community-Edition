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
    using ReactiveUI;

    /// <summary>
    /// Core component for the Book Editor application
    /// </summary>
    public partial class BookEditorBody
    {
        /// <summary>
        /// Gets or sets if the books column is collapsed
        /// </summary>
        private bool IsBooksColumnCollapsed { get; set; }

        /// <summary>
        /// Gets or sets if the sections column is collapsed
        /// </summary>
        private bool IsSectionColumnCollapsed { get; set; }

        /// <summary>
        /// Gets or sets if the page column is collapsed
        /// </summary>
        private bool IsPageColumnCollapsed { get; set; }
        
        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.EditorPopupViewModel.IsVisible,
                    x => x.ViewModel.ConfirmCancelPopupViewModel.IsVisible)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
