﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DeprecatableDataItemTableViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable
{
    using AntDesign;

    using CDP4Common.CommonData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.Common.Rows;

    /// <summary>
    /// View model that provides the basic functionalities for a reference data item
    /// </summary>
    public abstract class DeprecatableDataItemTableViewModel<T, TRow> : BaseDataItemTableViewModel<T, TRow>, IDeprecatableDataItemTableViewModel<T, TRow> where T : Thing, IShortNamedThing, INamedThing, IDeprecatableThing where TRow : DeprecatableDataItemRowViewModel<T>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DeprecatableDataItemTableViewModel{T,TRow}" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        /// <param name="extraTypesOfInterest">The collection of extra types of interest to subscribe</param>
        protected DeprecatableDataItemTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, IShowHideDeprecatedThingsService showHideDeprecatedThingsService,
            ILogger<DeprecatableDataItemTableViewModel<T, TRow>> logger, IEnumerable<Type> extraTypesOfInterest = null) : base(sessionService, messageBus, logger, extraTypesOfInterest)
        {
            this.ShowHideDeprecatedThingsService = showHideDeprecatedThingsService;
        }

        /// <summary>
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        public IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }
    }
}
