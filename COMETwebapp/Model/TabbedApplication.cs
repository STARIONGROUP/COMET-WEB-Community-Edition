// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabbedApplication.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Model
{
    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.Applications;

    /// <summary>
    /// The <see cref="TabbedApplication" /> is an <see cref="Application" /> that provides additional information about req
    /// </summary>
    public class TabbedApplication : Application
    {
        /// <summary>
        /// Gets or sets the <see cref="Type" /> of the associated component
        /// </summary>
        public Type ComponentType { get; set; }

        /// <summary>
        /// Gets the <see cref="Type" /> of the ViewModel that is required for the compoenet
        /// </summary>
        public Type ViewModelType { get; private set; }

        /// <summary>
        /// Gets the <see cref="Type" /> of possible thing of interest
        /// </summary>
        public Type ThingTypeOfInterest { get; private set; }

        /// <summary>
        /// Resolve the <see cref="ViewModelType" /> and <see cref="ThingTypeOfInterest" /> properties
        /// </summary>
        public void ResolveTypesProperties()
        {
            if (TypeChecker.IsSubclassOfRawGeneric(this.ComponentType, typeof(ApplicationBase<>), out var viewModelType))
            {
                this.ViewModelType = viewModelType;
            }

            if (TypeChecker.ImplementsGenericInterface(this.ViewModelType, typeof(ISingleThingApplicationBaseViewModel<>), out var thingType))
            {
                this.ThingTypeOfInterest = thingType;
            }
        }
    }
}
