// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DeprecatableDataItemRowViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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

namespace COMETwebapp.ViewModels.Components.Common.Rows
{
    using CDP4Common.CommonData;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for a thing
    /// </summary>
    public abstract class DeprecatableDataItemRowViewModel<T> : BaseDataItemRowViewModel<T> where T : Thing, IDeprecatableThing
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated" />
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeprecatableDataItemRowViewModel{T}" /> class.
        /// </summary>
        /// <param name="thing">The associated thing</param>
        protected DeprecatableDataItemRowViewModel(T thing) : base(thing)
        {
            this.IsDeprecated = thing.IsDeprecated;
        }

        /// <summary>
        /// Value indicating if the thing is deprecated
        /// </summary>
        public bool IsDeprecated
        {
            get => this.isDeprecated;
            set => this.RaiseAndSetIfChanged(ref this.isDeprecated, value);
        }

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="thingRow">The <see cref="DeprecatableDataItemRowViewModel{T}" /> to use for updating</param>
        public void UpdateProperties(DeprecatableDataItemRowViewModel<T> thingRow)
        {
            base.UpdateProperties(thingRow);
            this.IsDeprecated = thingRow.IsDeprecated;
        }
    }
}
