// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditorPopupViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components.BookEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the EditorPopup component
    /// </summary>
    public class EditorPopupViewModel : ReactiveObject, IEditorPopupViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="IsVisible"/> property
        /// </summary>
        private bool isVisible;

        /// <summary>
        /// Gets or sets the text of the header
        /// </summary>
        public string HeaderText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets if the popup is visible
        /// </summary>
        public bool IsVisible
        {
            get => this.isVisible;
            set => this.RaiseAndSetIfChanged(ref this.isVisible, value);
        }

        /// <summary>
        /// Gets or sets the collection of validation errors
        /// </summary>
        public SourceList<string> ValidationErrors { get; set; } = new();

        /// <summary>
        /// Gets or sets the callback for when the confirm button has been clicked
        /// </summary>
        public EventCallback OnConfirmClick { get; set; }

        /// <summary>
        /// Gets or sets the callback for when the cancel button has been clicked
        /// </summary>
        public EventCallback OnCancelClick { get; set; }

        /// <summary>
        /// Gets or sets the collection of active domains
        /// </summary>
        public IEnumerable<DomainOfExpertise> ActiveDomains { get; set; }

        /// <summary>
        /// Gets or sets the collection of available categories
        /// </summary>
        public IEnumerable<Category> AvailableCategories { get; set; } 

        /// <summary>
        /// Gets or sets the item for which this ViewModel is refering to
        /// </summary>
        public Thing Item { get; set; }
    }
}

