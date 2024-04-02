// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationRowViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.SiteDirectory.Rows
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for <see cref="Iteration" />s
    /// </summary>
    public class IterationRowViewModel : BaseDataItemRowViewModel<Iteration>
    {
        /// <summary>
        /// Backing field for <see cref="Number" />
        /// </summary>
        private string number;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationRowViewModel" /> class.
        /// </summary>
        /// <param name="iteration">The associated <see cref="Iteration" /></param>
        public IterationRowViewModel(Iteration iteration) : base(iteration)
        {
            this.Name = iteration.GetName();
            this.Number = iteration.IterationSetup.IterationNumber.ToString();
        }

        /// <summary>
        /// The <see cref="Iteration"/> number
        /// </summary>
        public string Number
        {
            get => this.number;
            set => this.RaiseAndSetIfChanged(ref this.number, value);
        }
    }
}
