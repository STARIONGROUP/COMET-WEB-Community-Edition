// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOptionsTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.Options
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    /// <summary>
    /// View model used to manage <see cref="Option" />
    /// </summary>
    public interface IOptionsTableViewModel : IDeletableDataItemTableViewModel<Option, OptionRowViewModel>
    {
        /// <summary>
        /// Creates or edits a <see cref="Option"/>
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="Option"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        Task CreateOrEditOption(bool shouldCreate);

        /// <summary>
        /// Sets the <see cref="OptionsTableViewModel.CurrentIteration"/> value
        /// </summary>
        /// <param name="iteration">The iteration to be set</param>
        void SetCurrentIteration(Iteration iteration);

        /// <summary>
        /// Gets or sets the value to check if the option to create is the default option for the <see cref="OptionsTableViewModel.CurrentIteration"/>
        /// </summary>
        bool SelectedIsDefaultValue { get; set; }

        /// <summary>
        /// Sets the current option value
        /// </summary>
        /// <param name="option">The option to be set</param>
        void SetCurrentOption(Option option);
    }
}
