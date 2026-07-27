// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParameterValueSetGroupViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor.EditParameterViewModel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using ReactiveUI;

    /// <summary>
    /// Groups the editable rows for a single <see cref="ParameterValueSetBase" />. A scalar parameter type produces
    /// one row; a <see cref="CompoundParameterType" /> produces one row per component (each with its own parameter
    /// type and scale, mirroring the desktop IME). The switch and the staged clone are shared by all the rows of the
    /// value set, so edits to different components accumulate onto the same clone and commit as a single value set.
    /// An orientation parameter is the one exception: although it is a <see cref="CompoundParameterType" />,
    /// it produces a single row backed by the dedicated <c>OrientationComponent</c> editor, which owns the whole
    /// array, instead of nine flattened scalar rows.
    /// </summary>
    public class EditParameterValueSetGroupViewModel : DisposableObject
    {
        /// <summary>
        /// The pending clone accumulating the user's edits, or <c>null</c> while the value set is untouched.
        /// </summary>
        private ParameterValueSetBase pendingClone;

        /// <summary>
        /// Whether the edited parameter type is a <see cref="CompoundParameterType" />. A compound type has one editor
        /// per component (each emitting the whole array with only its own index changed) so edits must be merged by
        /// index; a non-compound type has a single editor that owns the whole array (e.g. a
        /// <see cref="SampledFunctionParameterType" />'s table) so its emitted array replaces the pending one wholesale.
        /// </summary>
        private readonly bool isCompound;

        /// <summary>
        /// Creates a new instance of the <see cref="EditParameterValueSetGroupViewModel" /> class.
        /// </summary>
        /// <param name="parameterType">The <see cref="ParameterType" /> of the edited parameter.</param>
        /// <param name="valueSet">The original <see cref="ParameterValueSetBase" /> this group edits.</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /> passed to the value editors.</param>
        public EditParameterValueSetGroupViewModel(ParameterType parameterType, ParameterValueSetBase valueSet, ICDPMessageBus messageBus)
        {
            this.OriginalValueSet = valueSet;

            // An orientation parameter (issue #811) is a CompoundParameterType, but it must not be flattened into a
            // scalar row per matrix/euler component: it has its own dedicated OrientationComponent editor that owns
            // the whole array, exactly like a SampledFunctionParameterType's table.
            var isOrientation = parameterType is CompoundParameterType { ShortName: var shortName }
                                 && string.Equals(shortName, ConstantValues.OrientationShortName, StringComparison.InvariantCultureIgnoreCase);

            this.isCompound = parameterType is CompoundParameterType && !isOrientation;
            this.ParameterSwitchKindSelectorViewModel = new ParameterSwitchKindSelectorViewModel(valueSet.ValueSwitch, false);

            var rows = new List<EditParameterValueSetRowViewModel>();

            if (parameterType is CompoundParameterType compoundParameterType && !isOrientation)
            {
                var index = 0;

                foreach (ParameterTypeComponent component in compoundParameterType.Component)
                {
                    rows.Add(new EditParameterValueSetRowViewModel(this, index, component.ParameterType, component.ShortName, component.Scale?.ShortName ?? string.Empty, messageBus));
                    index++;
                }
            }
            else
            {
                rows.Add(new EditParameterValueSetRowViewModel(this, 0, parameterType, string.Empty, string.Empty, messageBus));
            }

            this.Rows = rows;

            foreach (var row in rows)
            {
                this.Disposables.Add(row);
            }

            this.Disposables.Add(this.WhenAnyValue(x => x.ParameterSwitchKindSelectorViewModel.SwitchValue)
                .Subscribe(_ => this.OnSwitchValueChanged()));
        }

        /// <summary>
        /// Gets the original, unmodified <see cref="ParameterValueSetBase" /> this group edits.
        /// </summary>
        public ParameterValueSetBase OriginalValueSet { get; }

        /// <summary>
        /// Gets the switch selector shared by every row of this value set.
        /// </summary>
        public IParameterSwitchKindSelectorViewModel ParameterSwitchKindSelectorViewModel { get; }

        /// <summary>
        /// Gets the per-component (or single scalar) rows of this value set.
        /// </summary>
        public IReadOnlyList<EditParameterValueSetRowViewModel> Rows { get; }

        /// <summary>
        /// Gets the value set currently reflecting the staged edits (the pending clone if any, else the original).
        /// </summary>
        public ParameterValueSetBase Current => this.pendingClone ?? this.OriginalValueSet;

        /// <summary>
        /// Gets the pending clone carrying the user's edits, or <c>null</c> when the value set was not modified.
        /// </summary>
        /// <returns>The modified <see cref="ParameterValueSetBase" /> clone, or <c>null</c>.</returns>
        public ParameterValueSetBase GetPendingValueSet()
        {
            return this.pendingClone;
        }

        /// <summary>
        /// Merges a Manual edit for one component onto the pending clone.
        /// </summary>
        /// <param name="index">The component (value-array) index.</param>
        /// <param name="editedArray">The value array emitted by the editor (with the component's new value at <paramref name="index" />).</param>
        public void ApplyManual(int index, ValueArray<string> editedArray)
        {
            this.EnsurePending();

            this.pendingClone.Manual = this.isCompound
                ? new ValueArray<string>(this.pendingClone.Manual) { [index] = editedArray[index] }
                : new ValueArray<string>(editedArray);
        }

        /// <summary>
        /// Merges a Reference edit for one component onto the pending clone.
        /// </summary>
        /// <param name="index">The component (value-array) index.</param>
        /// <param name="editedArray">The value array emitted by the editor (with the component's new value at <paramref name="index" />).</param>
        public void ApplyReference(int index, ValueArray<string> editedArray)
        {
            this.EnsurePending();

            this.pendingClone.Reference = this.isCompound
                ? new ValueArray<string>(this.pendingClone.Reference) { [index] = editedArray[index] }
                : new ValueArray<string>(editedArray);
        }

        /// <summary>
        /// Ensures <see cref="pendingClone" /> exists, cloning the original value set (with its contained things)
        /// the first time the value set is edited.
        /// </summary>
        private void EnsurePending()
        {
            this.pendingClone ??= (ParameterValueSetBase)this.OriginalValueSet.Clone(true);
        }

        /// <summary>
        /// Stages a switch change and refreshes the read-only columns of every row. The reactive subscription fires
        /// once on construction with the initial value, so a pending clone is created only when the switch differs.
        /// </summary>
        private void OnSwitchValueChanged()
        {
            var switchValue = this.ParameterSwitchKindSelectorViewModel.SwitchValue;

            if (this.pendingClone is not null)
            {
                this.pendingClone.ValueSwitch = switchValue;
            }
            else if (switchValue != this.OriginalValueSet.ValueSwitch)
            {
                this.EnsurePending();
                this.pendingClone.ValueSwitch = switchValue;
            }

            foreach (var row in this.Rows)
            {
                row.RefreshReadOnlyValues();
            }
        }
    }
}
