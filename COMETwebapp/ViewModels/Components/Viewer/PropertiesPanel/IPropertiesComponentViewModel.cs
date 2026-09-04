// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IPropertiesComponentViewModel.cs" company="Starion Group S.A.">
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
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Components.Viewer.PropertiesPanel;
    using COMETwebapp.Model;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;

    using FluentResults;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View Model for the <see cref="PropertiesComponent" />
    /// </summary>
    public interface IPropertiesComponentViewModel
    {
        /// <summary>
        /// Injected property to get access to <see cref="ISessionService" />
        /// </summary>
        ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator" />
        /// </summary>
        ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Gets or sets the property used for the Interoperability
        /// </summary>
        IBabylonInterop BabylonInterop { get; set; }

        /// <summary>
        /// The collection of <see cref="ParameterBase" /> and <see cref="IValueSet" /> of the selected <see cref="SceneObject" />
        /// </summary>
        Dictionary<ParameterBase, IValueSet> ParameterValueSetRelations { get; set; }

        /// <summary>
        /// The list of parameters that the selected <see cref="SceneObject" /> uses
        /// </summary>
        List<ParameterBase> ParametersInUse { get; set; }

        /// <summary>
        /// Gets or sets if the parameters have changes
        /// </summary>
        bool ParameterHaveChanges { get; set; }

        /// <summary>
        /// Gets or sets if this component is visible
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterBase" /> to fill the details
        /// </summary>
        ParameterBase SelectedParameter { get; set; }

        /// <summary>
        /// Event callback for when a <see cref="IValueSet" /> asociated to a <see cref="ParameterBase" /> has changed
        /// </summary>
        EventCallback<(IValueSet,int)> OnParameterValueSetChanged { get; set; }

        /// <summary>
        /// When the button for submit changes is clicked. Persists every changed value set and surfaces the outcome as a
        /// toast notification.
        /// </summary>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the write</returns>
        Task<Result> OnSubmit();

        /// <summary>
        /// Reverts an unsubmitted change on the given <see cref="ParameterBase" />, restoring its original value.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /> whose change should be discarded</param>
        void RevertChange(ParameterBase parameter);

        /// <summary>
        /// Clears the dialog editor cache so the submit confirmation dialog rebuilds its editors when it is opened.
        /// </summary>
        void OnSubmitDialogOpened();

        /// <summary>
        /// Clears the editor caches when the submit confirmation dialog closes so the panel reflects dialog edits.
        /// </summary>
        void OnSubmitDialogClosed();

        /// <summary>
        /// Asserts whether the given <see cref="ParameterBase" /> has an unsubmitted change, used to highlight its label.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /> to check</param>
        /// <returns>True when the parameter has a pending change</returns>
        bool HasChanges(ParameterBase parameter);

        /// <summary>
        /// Gets the <see cref="ParameterBase" />s that have an unsubmitted change, in tracking order.
        /// </summary>
        /// <returns>The changed parameters</returns>
        IReadOnlyList<ParameterBase> GetChangedParameters();

        /// <summary>
        /// Gets the display name of the given <see cref="ParameterBase" />, including its scale short name when present.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /></param>
        /// <returns>The formatted name</returns>
        string GetParameterDisplayName(ParameterBase parameter);

        /// <summary>
        /// Gets the original (pre-edit) value of the given <see cref="ParameterBase" /> for display in the dialog.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /></param>
        /// <returns>The formatted original value</returns>
        string GetOriginalValue(ParameterBase parameter);

        /// <summary>
        /// Gets a memoized editor for the given <see cref="ParameterBase" /> to be shown inside the submit dialog.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /> to edit</param>
        /// <returns>The <see cref="IDetailsComponentViewModel" /> for the parameter</returns>
        IDetailsComponentViewModel GetDialogEditor(ParameterBase parameter);

        /// <summary>
        /// Gets the current used <see cref="IValueSet" />
        /// </summary>
        /// <returns>the <see cref="IValueSet" /></returns>
        IValueSet GetUsedValueSet();

        /// <summary>
        /// Creates a new <see cref="IDetailsComponentViewModel" />
        /// </summary>
        /// <returns>
        /// a <see cref="IDetailsComponentViewModel" /> based on this <see cref="IPropertiesComponentViewModel" />
        /// </returns>
        IDetailsComponentViewModel CreateDetailsComponentViewModel();
    }
}
