// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterBaseBaseRowViewModel.cs" company="RHEA System S.A.">
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

    /// <summary>
    /// ViewModel for the rows asociated to a <see cref="ParameterBase"/>
    /// </summary>
    public class ParameterBaseBaseRowViewModel : IParameterBaseRowViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ParameterBase"/> for this <see cref="ParameterBaseBaseRowViewModel"/>
        /// </summary>
        public ParameterBase Parameter { get; set; }

        /// <summary>
        /// Gets the <see cref="Parameter"/> name
        /// </summary>
        public string ParameterTypeName { get; }

        /// <summary>
        /// Gets the <see cref="Parameter"/> owner name
        /// </summary>
        public string ParameterOwnerName { get; }

        /// <summary>
        /// Gets the <see cref="Parameter"/> model code
        /// </summary>
        public string ParameterModelCode { get; }

        /// <summary>
        /// Gets or sets the <see cref="ElementBase"/> used for grouping this <see cref="ParameterBaseBaseRowViewModel"/>
        /// </summary>
        public string ElementBaseName { get; }

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterBaseBaseRowViewModel"/>
        /// </summary>
        /// <param name="parameterBase"></param>
        public ParameterBaseBaseRowViewModel(ParameterBase parameterBase)
        {
            this.Parameter = parameterBase ?? throw new ArgumentNullException(nameof(parameterBase));
            this.ParameterTypeName = this.Parameter.ParameterType.Name;
            this.ParameterOwnerName = this.Parameter.Owner.ShortName;
            this.ParameterModelCode = this.Parameter.ModelCode();
            this.ElementBaseName = (parameterBase.Container as ElementBase)?.ShortName;
        }
    }
}
