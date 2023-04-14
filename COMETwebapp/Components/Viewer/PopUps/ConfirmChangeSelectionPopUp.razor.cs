// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfirmSelectionPopUp.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.Viewer.PopUps
{
    /// <summary>
    /// Partial class for the <see cref="ConfirmChangeSelectionPopUp"/>
    /// </summary>
    public partial class ConfirmChangeSelectionPopUp
    {
        /// <summary>
        /// Gets or sets if the pop up is visible
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// Gets or sets the event for the response when clicked the buttons
        /// </summary>
        public event EventHandler<bool> OnResponse;

        /// <summary> 
        /// Shows the pop up 
        /// </summary> 
        public void Show()
        {
            this.IsVisible = true;
            this.InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Hides the pop up
        /// </summary>
        public void Hide()
        {
            this.IsVisible = false;
            this.InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Event for when the continue button is clicked
        /// </summary>
        private void ContinueButtonClicked()
        {
            this.Hide();
            this.OnResponse?.Invoke(this, true);
        }

        /// <summary>
        /// Event for when the cancel button is clicked
        /// </summary>
        private void CancelButtonClicked()
        {
            this.Hide();
            this.OnResponse?.Invoke(this, false);
        }
    }
}
