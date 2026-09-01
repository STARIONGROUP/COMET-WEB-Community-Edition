// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SimpleParameterValuesTable.razor.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using COMETwebapp.Components.Common;
    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Inline table that manages the <see cref="SimpleParameterizableThing.ParameterValue" /> collection of the edited
    /// <see cref="Requirement" />. It mirrors the desktop IME "Simple Parameter Values" tab: a grid of the values with an
    /// add/edit panel that reuses the shared per-type editors so enumerations, dates and quantities are edited with the
    /// same widgets as elsewhere. Additions, edits and removals are staged on the in-memory requirement so they are
    /// committed together with it when the surrounding dialog is saved.
    /// </summary>
    public partial class SimpleParameterValuesTable : DisposableComponent
    {
        /// <summary>
        /// Gets or sets a value indicating whether the active user may write the parent thing this table edits part of.
        /// The rows here are parts of one aggregate saved atomically by the hosting form, so the permission is decided
        /// once by that form and passed down rather than evaluated per row. Defaults to true so a host that does not
        /// set it keeps its previous behaviour
        /// </summary>
        [Parameter]
        public bool IsAllowedToWrite { get; set; } = true;

        /// <summary>
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a session
        /// opened from an ECSS-E-TM-10-25 Annex C3 archive. Create and delete controls bind their enabled state to
        /// the inverse of this, so the data can still be inspected but never modified
        /// </summary>
        public bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// The <see cref="Requirement" /> whose <see cref="SimpleParameterizableThing.ParameterValue" /> collection is edited.
        /// </summary>
        [Parameter]
        public Requirement Requirement { get; set; }

        /// <summary>
        /// The <see cref="ParameterType" />s the user may pick from when adding a value.
        /// </summary>
        [Parameter]
        public IReadOnlyList<ParameterType> AvailableParameterTypes { get; set; } = [];

        /// <summary>
        /// The <see cref="ICDPMessageBus" /> the shared editors publish on.
        /// </summary>
        [Inject]
        public ICDPMessageBus MessageBus { get; set; }

        /// <summary>
        /// The value being edited (an existing one), or null while adding a new value.
        /// </summary>
        private SimpleParameterValue editingValue;

        /// <summary>
        /// The parameter type selected in the add/edit panel.
        /// </summary>
        private ParameterType selectedParameterType;

        /// <summary>
        /// The scale selected in the add/edit panel.
        /// </summary>
        private MeasurementScale selectedScale;

        /// <summary>
        /// The selector view model driving the shared per-type value editor, or null when the panel is closed.
        /// </summary>
        private ParameterTypeEditorSelectorViewModel editorSelectorViewModel;

        /// <summary>
        /// The value staged by the editor, or null when nothing has been edited yet.
        /// </summary>
        private ValueArray<string> stagedValue;

        /// <summary>
        /// Gets a value indicating whether the add/edit panel is open.
        /// </summary>
        public bool IsPanelOpen { get; private set; }

        /// <summary>
        /// The popup that asks the user to confirm the removal of a <see cref="SimpleParameterValue" /> before it is applied.
        /// </summary>
        public ConfirmRemovalPopup<SimpleParameterValue> RemovalPopup { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the panel is adding a new value (rather than editing an existing one).
        /// </summary>
        public bool IsAddMode => this.editingValue == null;

        /// <summary>
        /// Gets the scales the selected quantity-kind parameter type may use.
        /// </summary>
        private IEnumerable<MeasurementScale> AvailableScales => (this.selectedParameterType as QuantityKind)?.AllPossibleScale ?? [];

        /// <summary>
        /// Gets the <see cref="ParameterType" />s selectable when adding a value: every <see cref="AvailableParameterTypes" />
        /// entry except those already used by the requirement, so it keeps at most one
        /// <see cref="SimpleParameterValue" /> per <see cref="ParameterType" />.
        /// </summary>
        /// <returns>The selectable parameter types</returns>
        public IEnumerable<ParameterType> GetSelectableParameterTypes()
        {
            var usedParameterTypes = this.Requirement?.ParameterValue
                .Select(x => x.ParameterType?.Iid)
                .ToHashSet() ?? [];

            return this.AvailableParameterTypes.Where(x => !usedParameterTypes.Contains(x.Iid));
        }

        /// <summary>
        /// Gets the rows to display in the grid.
        /// </summary>
        /// <returns>The requirement's simple parameter values, ordered by parameter type name.</returns>
        private List<SimpleParameterValue> GetRows()
        {
            return this.Requirement?.ParameterValue
                .OrderBy(x => x.ParameterType?.Name, StringComparer.InvariantCultureIgnoreCase)
                .ToList() ?? [];
        }

        /// <summary>
        /// Formats the value of the given <paramref name="simpleParameterValue" /> for display.
        /// </summary>
        /// <param name="simpleParameterValue">The <see cref="SimpleParameterValue" /></param>
        /// <returns>The formatted value</returns>
        private static string FormatValue(SimpleParameterValue simpleParameterValue)
        {
            return ParameterValueFormatter.Format(simpleParameterValue.Value, simpleParameterValue.ParameterType, simpleParameterValue.Scale);
        }

        /// <summary>
        /// Opens the panel to add a new value.
        /// </summary>
        public void OpenAdd()
        {
            this.editingValue = null;
            this.selectedParameterType = null;
            this.selectedScale = null;
            this.stagedValue = null;
            this.DisposeEditor();
            this.IsPanelOpen = true;
        }

        /// <summary>
        /// Opens the panel to edit the given existing value.
        /// </summary>
        /// <param name="value">The <see cref="SimpleParameterValue" /> to edit</param>
        public void OpenEdit(SimpleParameterValue value)
        {
            this.editingValue = value;
            this.selectedParameterType = value.ParameterType;
            this.selectedScale = value.Scale;
            this.stagedValue = null;
            this.BuildEditor(value.Value);
            this.IsPanelOpen = true;
        }

        /// <summary>
        /// Handles selection of a parameter type in add mode, building the editor with default values.
        /// </summary>
        /// <param name="parameterType">The selected <see cref="ParameterType" /></param>
        public void OnParameterTypeSelected(ParameterType parameterType)
        {
            this.selectedParameterType = parameterType;
            this.selectedScale = (parameterType as QuantityKind)?.DefaultScale;
            this.stagedValue = null;

            if (parameterType == null)
            {
                this.DisposeEditor();
                return;
            }

            this.BuildEditor(new ValueArray<string>(Enumerable.Repeat("-", Math.Max(parameterType.NumberOfValues, 1))));
        }

        /// <summary>
        /// Builds the shared per-type editor around a proxy value set seeded with the given <paramref name="value" />.
        /// </summary>
        /// <param name="value">The value array to seed the editor with</param>
        private void BuildEditor(ValueArray<string> value)
        {
            this.DisposeEditor();

            var proxy = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(value),
                Computed = new ValueArray<string>(value),
                Reference = new ValueArray<string>(value),
                Formula = new ValueArray<string>(Enumerable.Repeat("-", value.Count)),
                Published = new ValueArray<string>(value),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.editorSelectorViewModel = new ParameterTypeEditorSelectorViewModel(this.selectedParameterType, proxy, false, this.MessageBus)
            {
                Scale = this.selectedScale,
                ParameterValueChanged = EventCallback.Factory.Create<(IValueSet, int)>(this, this.OnEditorValueChanged)
            };

            this.editorSelectorViewModel.UpdateSwitchKind(ParameterSwitchKind.MANUAL);
        }

        /// <summary>
        /// Stages the value emitted by the editor (the editor clones the proxy, so its Manual array is the new value).
        /// </summary>
        /// <param name="editorValue">The emitted value set and value-array index</param>
        private void OnEditorValueChanged((IValueSet ValueSet, int Index) editorValue)
        {
            if (editorValue.ValueSet is ParameterValueSetBase valueSet)
            {
                this.stagedValue = valueSet.Manual;
            }
        }

        /// <summary>
        /// Commits the add/edit panel onto the requirement's value collection.
        /// </summary>
        public void Confirm()
        {
            if (this.selectedParameterType == null)
            {
                return;
            }

            var value = this.stagedValue ?? new ValueArray<string>(Enumerable.Repeat("-", Math.Max(this.selectedParameterType.NumberOfValues, 1)));

            if (this.IsAddMode)
            {
                if (this.Requirement.ParameterValue.Any(x => x.ParameterType?.Iid == this.selectedParameterType.Iid))
                {
                    this.ClosePanel();
                    return;
                }

                this.Requirement.ParameterValue.Add(new SimpleParameterValue
                {
                    Iid = Guid.NewGuid(),
                    ParameterType = this.selectedParameterType,
                    Scale = this.selectedScale,
                    Value = value
                });
            }
            else
            {
                this.editingValue.Value = value;
                this.editingValue.Scale = this.selectedScale;
            }

            this.ClosePanel();
        }

        /// <summary>
        /// Removes the given value from the requirement, once the user has confirmed the removal in the
        /// <see cref="RemovalPopup" />.
        /// </summary>
        /// <param name="value">The <see cref="SimpleParameterValue" /> to remove</param>
        public void Remove(SimpleParameterValue value)
        {
            this.Requirement.ParameterValue.Remove(value);
        }

        /// <summary>
        /// Handles the popup being closed via its close button or the overlay by cleaning up the panel state.
        /// </summary>
        /// <param name="visible">The new visibility of the popup.</param>
        private void OnPopupVisibleChanged(bool visible)
        {
            if (!visible)
            {
                this.ClosePanel();
            }
        }

        /// <summary>
        /// Closes the add/edit panel.
        /// </summary>
        private void ClosePanel()
        {
            this.IsPanelOpen = false;
            this.editingValue = null;
            this.DisposeEditor();
        }

        /// <summary>
        /// Clears the current editor selector view model, if any.
        /// </summary>
        private void DisposeEditor()
        {
            this.editorSelectorViewModel = null;
        }
    }
}
