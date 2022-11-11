// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesBase.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.PropertiesPanel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMETwebapp.Primitives;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// The component used for showing the details of the <see cref="PrimitiveSelected"/>
    /// </summary>
    public class DetailsComponentBase : ComponentBase
    {
        /// <summary>
        /// The collection of <see cref="ParameterBase"/> and <see cref="IValueSet"/> of the <see cref="PrimitiveSelected"/> property
        /// </summary>
        private Dictionary<ParameterBase, IValueSet> ValueSetsCollection { get; set; }

        /// <summary>
        /// Backing field for the <see cref="parameterSelected"/> property
        /// </summary>
        private ParameterBase parameterSelected;

        /// <summary>
        /// Backing field for the <see cref="PrimitiveSelected"/> property
        /// </summary>
        private Primitive primitiveSelected;

        /// <summary>
        /// Gets or sets the selected primitive used for the details
        /// </summary>
        [Parameter]
        public Primitive PrimitiveSelected
        {
            get => this.primitiveSelected;
            set
            {
                if (this.primitiveSelected != value)
                {
                    this.primitiveSelected = value;
                    this.InitValueSet();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected parameter used for the details
        /// </summary>
        [Parameter]
        public ParameterBase ParameterSelected
        {
            get => this.parameterSelected;
            set
            {
                if (this.parameterSelected != value)
                {
                    this.parameterSelected = value;
                    this.InitValueSet();
                }
            }
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.InitValueSet();
        }

        /// <summary>
        /// Inits the <see cref="ValueSetsCollection"/>
        /// </summary>
        private void InitValueSet()
        {
            if (this.PrimitiveSelected is not null)
            {
                this.ValueSetsCollection = this.PrimitiveSelected.GetValueSets();
            }
        }

        /// <summary>
        /// Gets the <see cref="IValueSet"/> asociated to the <see cref="ParameterSelected"/> property
        /// </summary>
        /// <returns>the set</returns>
        public IValueSet GetValueSet()
        {
            return this.ValueSetsCollection[this.ParameterSelected];
        }

        /// <summary>
        /// Gets the <see cref="IValueSet"/> asociated to the <paramref name="parameterBase"/>
        /// </summary>
        /// <param name="parameterBase">The parameter for the set wants to be retrieved</param>
        /// <returns>the set</returns>
        public IValueSet GetValueSet(ParameterBase parameterBase)
        {
            return this.ValueSetsCollection[parameterBase];
        }

        /// <summary>
        /// Event for when the value of a parameter has changed
        /// </summary>
        /// <param name="changedIndex">The index of the changed value for the <see cref="ValueArray{T}"/></param>
        /// <param name="e">Supplies information about an change event that is being raised.</param>
        public void OnParameterValueChange(int changedIndex, ChangeEventArgs e)
        {
            var valueSet = this.ValueSetsCollection[this.ParameterSelected];

            List<string> newValueValues = new List<string>();

            for (int i = 0; i < valueSet.ActualValue.Count; i++)
            {
                if (i == changedIndex)
                {
                    newValueValues.Add((string)e.Value);
                }
                else
                {
                    newValueValues.Add(valueSet.ActualValue[i]);
                }
            }

            ValueArray<string> newValueArray = new ValueArray<string>(newValueValues);

            if (valueSet is ParameterValueSetBase parameterValueSetBase)
            {
                var clonedValueSetBase = parameterValueSetBase.Clone(false);
                clonedValueSetBase.Manual = newValueArray;
                this.ValueSetsCollection[this.ParameterSelected] = clonedValueSetBase;
                this.PrimitiveSelected.UpdatePropertyWithParameterData(this.ParameterSelected.ParameterType.ShortName, clonedValueSetBase);
            }
        }
    }
}
