// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EngineeringModelRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.SiteDirectory.Rows
{
    using COMETwebapp.ViewModels.Components.Common.Rows;

    using ReactiveUI;

    using EngineeringModelSetup = CDP4Common.SiteDirectoryData.EngineeringModelSetup;

    /// <summary>
    /// Row View Model for <see cref="CDP4Common.SiteDirectoryData.EngineeringModelSetup" />s
    /// </summary>
    public class EngineeringModelRowViewModel : BaseDataItemRowViewModel<EngineeringModelSetup>
    {
        /// <summary>
        /// Backing field for <see cref="Phase" />
        /// </summary>
        private string phase;

        /// <summary>
        /// Backing field for <see cref="Kind" />
        /// </summary>
        private string kind;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelRowViewModel" /> class.
        /// </summary>
        /// <param name="engineeringModel">The associated <see cref="EngineeringModelSetup" /></param>
        public EngineeringModelRowViewModel(EngineeringModelSetup engineeringModel) : base(engineeringModel)
        {
            this.Phase = engineeringModel.StudyPhase.ToString();
            this.Kind = engineeringModel.Kind.ToString();
        }

        /// <summary>
        /// The study phase value for the current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public string Phase
        {
            get => this.phase;
            set => this.RaiseAndSetIfChanged(ref this.phase, value);
        }

        /// <summary>
        /// The kind value for the current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public string Kind
        {
            get => this.kind;
            set => this.RaiseAndSetIfChanged(ref this.kind, value);
        }
    }
}
