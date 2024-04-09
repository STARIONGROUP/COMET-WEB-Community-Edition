// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPublicationsTableViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.Publications
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.Common.Rows;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    /// <summary>
    /// View model used to manage <see cref="Publication" />
    /// </summary>
    public interface IPublicationsTableViewModel : IBaseDataItemTableViewModel<Publication, PublicationRowViewModel>
    {
        /// <summary>
        /// Gets the published parameters rows for a given publication
        /// </summary>
        /// <param name="publication">The publication that contains the parameters to be retrieved</param>
        /// <returns>A collection of <see cref="OwnedParameterOrOverrideBaseRowViewModel"/> - the published parameters</returns>
        IEnumerable<OwnedParameterOrOverrideBaseRowViewModel> GetPublishedParametersRows(Publication publication);

        /// <summary>
        /// Sets the <see cref="PublicationsTableViewModel.CurrentIteration"/> value
        /// </summary>
        /// <param name="iteration">The iteration to be set</param>
        void SetCurrentIteration(Iteration iteration);

        /// <summary>
        /// Gets the existing parameters that can be published
        /// </summary>
        /// <returns>A collection of <see cref="OwnedParameterOrOverrideBaseRowViewModel"/> - the parameters that can be published</returns>
        IEnumerable<OwnedParameterOrOverrideBaseRowViewModel> GetParametersThatCanBePublished();

        /// <summary>
        /// Gets or sets the collection of selected rows
        /// </summary>
        IReadOnlyList<object> SelectedParameterRowsToPublish { get; set; }

        /// <summary>
        /// Creates a new <see cref="Publication"/> with the parameters from <see cref="PublicationsTableViewModel.SelectedParameterRowsToPublish"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        Task CreatePublication();
    }
}
