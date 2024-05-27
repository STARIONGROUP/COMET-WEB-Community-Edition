// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBaseDataItemTableViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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

namespace COMETwebapp.ViewModels.Components.Common.BaseDataItemTable
{
    using COMET.Web.Common.ViewModels.Components.Applications;

    using DynamicData;

    /// <summary>
    /// View model that provides the basic functionalities for a reference data item
    /// </summary>
    public interface IBaseDataItemTableViewModel<T, TRow> : IApplicationBaseViewModel, IHaveReusableRows
    {
        /// <summary>
        /// A reactive collection of things
        /// </summary>
        SourceList<TRow> Rows { get; }

        /// <summary>
        /// The thing to create or edit
        /// </summary>
        T CurrentThing { get; set; }

        /// <summary>
        /// Initializes the <see cref="BaseDataItemTableViewModel{T,TRow}" />
        /// </summary>
        void InitializeViewModel();
    }
}
