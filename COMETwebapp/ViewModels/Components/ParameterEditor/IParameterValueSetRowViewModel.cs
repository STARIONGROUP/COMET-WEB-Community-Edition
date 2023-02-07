// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterValueSetRowViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Interface for the <see cref="ParameterValueSetRowViewModel"/>
    /// </summary>
    public interface IParameterValueSetRowViewModel
    {
        /// <summary>
        /// The ParameterValueSet to show in the table
        /// </summary>
        public ParameterValueSetBase ParameterValueSet { get; set; }

        /// <summary>
        /// The associated Parameter to show
        /// </summary>
        public ParameterOrOverrideBase Parameter { get; set; }

        /// <summary>
        /// Sets if ParameterValueSet was edited
        /// </summary>
        public bool IsParameterValueSetEdited { get; set; }

        /// <summary>
        /// ParameterSwitchKind to show
        /// </summary>
        public string SelectedSwitchKind { get; set; }

        /// <summary>
        /// Initializes this <see cref="ParameterValueSetRowViewModel"/>
        /// </summary>
        void InitializeViewModel();

        /// <summary>
        /// Tells if ParameterValueSet is editable
        /// A <see cref="ParameterValueSetBase"/> is editable if it is owned by the active <see cref="DomainOfExpertise"/>
        /// </summary>
        public bool IsEditable();
    }
}
