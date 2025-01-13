// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IElementDefinitionTreeViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.MultiModelEditor
{
    using System.Collections.ObjectModel;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.ViewModels.Components.MultiModelEditor.CopySettings;
    using COMETwebapp.ViewModels.Components.MultiModelEditor.Rows;

    /// <summary>
    /// Interface for the <see cref="ElementDefinitionTreeViewModel" />
    /// </summary>
    public interface IElementDefinitionTreeViewModel : IHaveReusableRows
    {
        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionTreeRowViewModel" />
        /// </summary>
        ObservableCollection<ElementDefinitionTreeRowViewModel> Rows { get; }

        /// <summary>
        /// The <see cref="Iteration"/> from which to build the tree
        /// </summary>
        Iteration Iteration { get; set; }

        /// <summary>
        /// Gets the Description of the selected model and iteration
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets or a collection of selectable <see cref="Iteration"/>s
        /// </summary>
        ObservableCollection<IterationData> Iterations { get; }

        /// <summary>
        /// The <see cref="Iteration"/> from which to build the tree
        /// </summary>
        IterationData SelectedIterationData { get; set; }
    }
}
