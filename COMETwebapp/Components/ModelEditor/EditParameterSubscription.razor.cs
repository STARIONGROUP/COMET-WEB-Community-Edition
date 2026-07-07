// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParameterSubscription.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ModelEditor
{
    using COMETwebapp.ViewModels.Components.ModelEditor.EditParameterSubscriptionViewModel;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Dialog that edits the current domain's <see cref="CDP4Common.EngineeringModelData.ParameterSubscription" />
    /// on a parameter it does not own: the subscribed parameter, owner and model code are read-only, and each
    /// subscription value set's Manual value and switch are editable. Opened from a parameter card on the element
    /// details panel.
    /// </summary>
    public partial class EditParameterSubscription
    {
        /// <summary>
        /// Gets or sets the <see cref="IEditParameterSubscriptionViewModel" /> that drives this dialog.
        /// </summary>
        [Parameter]
        public IEditParameterSubscriptionViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets a value indicating whether the Option column should be shown.
        /// </summary>
        private bool HasOptionColumn => this.ViewModel.ValueRows.Any(row => !string.IsNullOrEmpty(row.OptionName));

        /// <summary>
        /// Gets a value indicating whether the State column should be shown.
        /// </summary>
        private bool HasStateColumn => this.ViewModel.ValueRows.Any(row => !string.IsNullOrEmpty(row.StateName));

        /// <summary>
        /// Gets a value indicating whether the subscribed parameter is compound — in which case the grid shows one
        /// row per component with the Name / Parameter Type / Scale columns.
        /// </summary>
        private bool IsCompound => this.ViewModel.ValueRows.Any(row => !string.IsNullOrEmpty(row.Name));
    }
}
