// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationsTable.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Components.SiteDirectory.EngineeringModel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="IterationsTable"/>
    /// </summary>
    public partial class IterationsTable : SelectedDataItemBase<Iteration, IterationRowViewModel>
    {
        /// <summary>
        /// The <see cref="IIterationsTableViewModel" /> for this component
        /// </summary>
        [Inject]
        public IIterationsTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EngineeringModelSetup"/>
        /// </summary>
        [Parameter]
        public EngineeringModelSetup EngineeringModelSetup { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Initialize(this.ViewModel);
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            this.ViewModel.SetEngineeringModel(this.EngineeringModelSetup);
        }
    }
}
