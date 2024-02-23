// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IModelMenuViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Shared.TopMenuEntry
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components;

    /// <summary>
    /// View model that handles the menu entry related to the opened <see cref="EngineeringModel" />
    /// </summary>
    public interface IModelMenuViewModel : IDisposableObject
    {
        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        ISessionService SessionService { get; }

        /// <summary>
        /// The <see cref="IConfirmCancelPopupViewModel" /> for closing an <see cref="Iteration" />
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
        /// Value asserting that the user is asked to open a new <see cref="Iteration" />
        /// </summary>
        bool IsOnOpenIterationMode { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="ModelMenuRowViewModel" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> for the <see cref="ModelMenuRowViewModel" /></param>
        /// <returns>The newly created <see cref="ModelMenuRowViewModel" /></returns>
        ModelMenuRowViewModel CreateRowViewModel(Iteration iteration);

        /// <summary>
        /// Asks the user to open a new <see cref="Iteration" />
        /// </summary>
        void AskToOpenIteration();
    }
}
