// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IIterationsTableViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage <see cref="IterationSetup" />
    /// </summary>
    public interface IIterationsTableViewModel : IDeletableDataItemTableViewModel<IterationSetup, IterationSetupRowViewModel>
    {
        /// <summary>
        /// Gets a value indicating whether a new iteration can be created.
        /// </summary>
        bool CanCreateIteration { get; }

        /// <summary>
        /// Gets a collection of all the available <see cref="IterationSetup" />s to be used as a source
        /// </summary>
        IEnumerable<IterationSetup> SourceIterations { get; }

        /// <summary>
        /// Initializes the view model
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup" /> to get its iterations</param>
        void InitializeViewModel(EngineeringModelSetup model);

        /// <summary>
        /// Creates or edits the current iteration
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="IterationSetup" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        Task CreateOrEditIteration(bool shouldCreate);
    }
}
