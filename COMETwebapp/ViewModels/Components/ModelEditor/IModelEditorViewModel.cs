// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IModelEditorViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.ViewModels.Components.ModelEditor
{
    using CDP4Common.EngineeringModelData;

    using CDP4DalCommon.Protocol.Operations;

    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.Common;
    using COMETwebapp.ViewModels.Components.ModelEditor.CopySettings;

    /// <summary>
    /// Interface for the <see cref="ModelEditorViewModel" />
    /// </summary>
    public interface IModelEditorViewModel : ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Gets the <see cref="IElementDetailsPanelViewModel" /> managing the editable element details panel.
        /// </summary>
        IElementDetailsPanelViewModel DetailsPanelViewModel { get; }

        /// <summary>
        /// Gets the target <see cref="Iteration"/> />
        /// </summary>
        Iteration TargetIteration { get; set; }

        /// <summary>
        /// Gets the source <see cref="Iteration" />
        /// </summary>
        Iteration SourceIteration { get; set; }

        /// <summary>
        /// Value indicating the user is currently setting the Copy settings that apply when a node is dropped
        /// </summary>
        bool IsOnCopySettingsMode { get; set; }

        /// <summary>
        /// Gets a value indicating that source model and target model are based on the same <see cref="Iteration"/>
        /// </summary>
        bool IsSourceModelSameAsTargetModel { get; }

        /// <summary>
        /// Gets the <see cref="ICopySettingsViewModel" />
        /// </summary>
        ICopySettingsViewModel CopySettingsViewModel { get; set; }

        /// <summary>
        /// Opens the <see cref="COMETwebapp.Components.ModelEditor.CopySettings" /> popup
        /// </summary>
        void OpenCopySettingsPopup();

        /// <summary>
        /// Add a new <see cref="ElementDefinition"/> based on an existing <see cref="ElementBase"/>
        /// </summary>
        /// <param name="elementDefinitionTree">The <see cref="ElementDefinitionTree"/> to copy the node to</param>
        /// <param name="elementBase">The <see cref="ElementBase"/> to copy</param>
        /// <param name="operationKind">
        /// The <see cref="OperationKind"/> requested by the modifier keys that were held during the drop, or null to use the
        /// copy mode that the user selected in the copy settings
        /// </param>
        Task CopyAndAddNewElementAsync(ElementDefinitionTree elementDefinitionTree, ElementBase elementBase, OperationKind? operationKind = null);

        /// <summary>
        /// Add a new <see cref="ElementUsage"/> based on an existing <see cref="ElementBase"/>
        /// </summary>
        /// <param name="fromElementBase">The <see cref="ElementBase"/> to be added as <see cref="ElementUsage"/></param>
        /// <param name="toElementBase">The <see cref="ElementBase"/> where to add the new <see cref="ElementUsage"/> to</param>
        Task AddNewElementUsageAsync(ElementBase fromElementBase, ElementBase toElementBase);
    }
}
