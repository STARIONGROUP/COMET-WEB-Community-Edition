// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsExportDialog.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.RequirementsEditor
{
    using COMETwebapp.Model.RequirementsEditor.Export;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Dialog that lets the user configure and trigger an Excel export of the requirements. It edits the
    /// <see cref="IRequirementsEditorBodyViewModel.ExportConfiguration" /> in place and calls
    /// <see cref="IRequirementsEditorBodyViewModel.ExportAsync" /> on confirmation.
    /// </summary>
    public partial class RequirementsExportDialog
    {
        /// <summary>
        /// The selectable <see cref="RequirementsExportNamingMode" /> values shown in the naming combo box.
        /// </summary>
        private static readonly RequirementsExportNamingMode[] NamingModes =
        [
            RequirementsExportNamingMode.ShortName,
            RequirementsExportNamingMode.Name,
            RequirementsExportNamingMode.Both
        ];

        /// <summary>
        /// The selectable <see cref="RequirementsExportSelectionMode" /> values shown in the section combo boxes.
        /// </summary>
        private static readonly RequirementsExportSelectionMode[] SelectionModes =
        [
            RequirementsExportSelectionMode.All,
            RequirementsExportSelectionMode.Selection,
            RequirementsExportSelectionMode.None
        ];

        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" /> whose export configuration is edited.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets the export configuration edited by the dialog.
        /// </summary>
        private RequirementsExportConfiguration Config => this.ViewModel.ExportConfiguration;

        /// <summary>
        /// Gets the human-readable label for the given <paramref name="mode" />.
        /// </summary>
        /// <param name="mode">The <see cref="RequirementsExportNamingMode" /></param>
        /// <returns>The label</returns>
        private static string NamingModeLabel(RequirementsExportNamingMode mode)
        {
            return mode switch
            {
                RequirementsExportNamingMode.ShortName => "Short names",
                RequirementsExportNamingMode.Name => "Names",
                _ => "Names and short names"
            };
        }
    }
}
