// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementValueCell.razor.cs" company="Starion Group S.A.">
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;

    /// <summary>
    /// One DOORS-style value cell on a requirement row: it shows the requirement's <see cref="SimpleParameterValue" />
    /// for a single <see cref="ParameterType" /> (or an "add" affordance when the requirement has no such value yet),
    /// and turns into the shared per-type editor on click so enumerations, dates and quantities are edited with the
    /// same widgets as the Edit Parameter dialog. The commit is explicit (Enter / the tick) and the original value is
    /// never mutated, so a rejected write simply leaves the displayed value unchanged.
    /// </summary>
    public partial class RequirementValueCell
    {
        /// <summary>
        /// The <see cref="SimpleParameterValue" /> being edited, captured when edit mode began.
        /// </summary>
        private SimpleParameterValue editingValue;

        /// <summary>
        /// The selector view model driving the shared per-type editor while <see cref="IsEditing" /> is true.
        /// </summary>
        private ParameterTypeEditorSelectorViewModel editorSelectorViewModel;

        /// <summary>
        /// The value staged by the editor since editing began, or null when nothing has been edited yet.
        /// </summary>
        private ValueArray<string> stagedValue;

        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Requirement" /> this cell belongs to.
        /// </summary>
        [Parameter]
        public Requirement Requirement { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType" /> this cell shows the value for.
        /// </summary>
        [Parameter]
        public ParameterType ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ICDPMessageBus" /> the shared editors publish on.
        /// </summary>
        [Inject]
        public ICDPMessageBus MessageBus { get; set; }

        /// <summary>
        /// Gets a value indicating whether the cell is currently in edit mode.
        /// </summary>
        public bool IsEditing => this.editorSelectorViewModel != null;

        /// <summary>
        /// Gets the <see cref="SimpleParameterValue" /> this cell currently shows, resolved fresh from the view model
        /// so a just-added or just-saved value is reflected without a parent re-render.
        /// </summary>
        private SimpleParameterValue Value => this.ViewModel.GetSimpleParameterValue(this.Requirement, this.ParameterType);

        /// <summary>
        /// Enters edit mode by building a proxy <see cref="ParameterValueSet" /> around the given value and wiring the
        /// shared per-type editor to it, pinned to the Manual switch.
        /// </summary>
        /// <param name="value">The <see cref="SimpleParameterValue" /> to edit</param>
        private void BeginEdit(SimpleParameterValue value)
        {
            this.editingValue = value;
            this.stagedValue = null;

            var proxy = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(value.Value),
                Computed = new ValueArray<string>(value.Value),
                Reference = new ValueArray<string>(value.Value),
                Formula = new ValueArray<string>(Enumerable.Repeat("-", value.Value.Count)),
                Published = new ValueArray<string>(value.Value),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.editorSelectorViewModel = new ParameterTypeEditorSelectorViewModel(this.ParameterType, proxy, false, this.MessageBus)
            {
                Scale = value.Scale,
                ParameterValueChanged = new EventCallbackFactory().Create<(IValueSet, int)>(this, this.OnEditorValueChanged)
            };

            this.editorSelectorViewModel.UpdateSwitchKind(ParameterSwitchKind.MANUAL);
        }

        /// <summary>
        /// Stages the value emitted by the editor (the editor clones the proxy, so the Manual array is the new value).
        /// </summary>
        /// <param name="editorValue">The emitted value set clone and value-array index</param>
        private void OnEditorValueChanged((IValueSet ValueSet, int Index) editorValue)
        {
            if (editorValue.ValueSet is ParameterValueSetBase emitted)
            {
                this.stagedValue = emitted.Manual;
            }
        }

        /// <summary>
        /// Commits the staged value to the server through the view model, then leaves edit mode.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task CommitAsync()
        {
            if (this.editingValue == null)
            {
                return;
            }

            var valueToWrite = this.editingValue;
            var newValue = this.stagedValue ?? valueToWrite.Value;
            this.Cancel();
            await this.ViewModel.UpdateSimpleParameterValue(valueToWrite, newValue);
        }

        /// <summary>
        /// Leaves edit mode without writing anything.
        /// </summary>
        private void Cancel()
        {
            this.editorSelectorViewModel = null;
            this.editingValue = null;
            this.stagedValue = null;
        }

        /// <summary>
        /// Creates the missing <see cref="SimpleParameterValue" /> on the requirement so it becomes editable.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task AddAsync()
        {
            await this.ViewModel.CreateSimpleParameterValue(this.Requirement, this.ParameterType);
        }

        /// <summary>
        /// Commits on Enter and cancels on Escape while editing.
        /// </summary>
        /// <param name="eventArgs">The <see cref="KeyboardEventArgs" /></param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnEditorKeyDownAsync(KeyboardEventArgs eventArgs)
        {
            switch (eventArgs.Key)
            {
                // only commit once the editor has actually staged a change, so an Enter that merely picks a value
                // from an open dropdown is not swallowed as a no-op that closes the editor
                case "Enter" when this.stagedValue != null:
                    await this.CommitAsync();
                    break;

                case "Escape":
                    this.Cancel();
                    break;
            }
        }
    }
}
