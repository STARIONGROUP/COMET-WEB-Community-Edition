﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileForm.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.EngineeringModel.FileStore
{
    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileHandler;

    using Microsoft.AspNetCore.Components;

    using System.ComponentModel.DataAnnotations;

    using FluentResults;

    /// <summary>
    /// Support class for the <see cref="FileForm"/>
    /// </summary>
    public partial class FileForm : SelectedDataItemForm
    {
        /// <summary>
        /// The <see cref="IFileHandlerViewModel" /> for this component
        /// </summary>
        [Parameter, Required]
        public IFileHandlerViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets the value to check if the deletion popup is visible
        /// </summary>
        public bool IsDeletePopupVisible { get; private set; }

        /// <summary>
        /// Gets the error message from the <see cref="IFileHandlerViewModel.CreateOrEditFile"/>, if any
        /// </summary>
        public string ErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Method that is executed when there is a valid submit
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        protected override async Task OnValidSubmit()
        {
            this.ErrorMessage = string.Empty;
            var result = await this.ViewModel.CreateOrEditFile(this.ShouldCreate);
            this.ErrorMessage = string.Join(", ", result.Reasons.OfType<IExceptionalError>().Select(x => x.Exception.Message));

            if (string.IsNullOrWhiteSpace(this.ErrorMessage))
            {
                await base.OnValidSubmit();
            }
        }

        /// <summary>
        /// Method that is executed when a deletion is done
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task OnDelete()
        {
            this.IsDeletePopupVisible = false;
            await this.ViewModel.DeleteFile();
            await base.OnCancel();
        }
    }
}
