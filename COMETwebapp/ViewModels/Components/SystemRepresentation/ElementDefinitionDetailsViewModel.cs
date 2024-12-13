﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDetailsViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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
namespace COMETwebapp.ViewModels.Components.SystemRepresentation
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Components.SystemRepresentation;
    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using ReactiveUI;

    /// <summary>
    ///     View model for the <see cref="ElementDefinitionDetails" /> component
    /// </summary>
    public class ElementDefinitionDetailsViewModel : ReactiveObject, IElementDefinitionDetailsViewModel
    {
        /// <summary>
        ///     Backing field for <see cref="SelectedSystemNode" />
        /// </summary>
        private ElementBase selectedSystemNode;

        /// <summary>
        ///     The selected <see cref="SystemNode"/>
        /// </summary>
        public ElementBase SelectedSystemNode
        {
            get => this.selectedSystemNode;
            set => this.RaiseAndSetIfChanged(ref this.selectedSystemNode, value);
        }

        /// <summary>
        /// A collection of <see cref="ElementDefinitionDetailsRowViewModel" />
        /// </summary>
        public ICollection<ElementDefinitionDetailsRowViewModel> Rows { get; set; }
    }
}
