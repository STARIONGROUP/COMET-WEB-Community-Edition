// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IModelMenuViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Shared.TopMenuEntry
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.ViewModels.Components.Shared;

    /// <summary>
    /// Interface definition for <see cref="ModelMenuViewModel" />
    /// </summary>
    public interface IModelMenuViewModel : IDisposableViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        ISessionService SessionService { get; }

        /// <summary>
        /// The <see cref="IConfirmCancelPopupViewModel"/> for closing an <see cref="Iteration"/>
        /// </summary>
        IConfirmCancelPopupViewModel ConfirmCancelViewModel { get; set; }

        /// <summary>
        /// Value asserting that the user is asked to select a <see cref="DomainOfExpertise" />
        /// </summary>
        bool IsOnSwitchDomainMode { get; set; }

        /// <summary>
        /// The <see cref="ISwitchDomainViewModel" />
        /// </summary>
        ISwitchDomainViewModel SwitchDomainViewModel { get; }

        /// <summary>
        /// Creates a new instance of <see cref="ModelMenuRowViewModel" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> for the <see cref="ModelMenuRowViewModel" /></param>
        /// <returns>The newly created <see cref="ModelMenuRowViewModel" /></returns>
        ModelMenuRowViewModel CreateRowViewModel(Iteration iteration);
    }
}
