// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabbedApplicationInformation.cs" company="Starion Group S.A.">
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
    using COMET.Web.Common.ViewModels.Components.Applications;

    /// <summary>
    /// The <see cref="TabbedApplicationInformation" /> provides required information related to an open application
    /// </summary>
    public class TabbedApplicationInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabbedApplicationInformation" /> class.
        /// </summary>
        /// <param name="applicationBaseViewModel">The <see cref="IApplicationBaseViewModel" /></param>
        /// <param name="componentType">The <see cref="Type" /> of the component to use</param>
        /// <param name="objectOfInterest">An optinal object of interest</param>
        public TabbedApplicationInformation(IApplicationBaseViewModel applicationBaseViewModel, Type componentType, object objectOfInterest)
        {
            this.ApplicationBaseViewModel = applicationBaseViewModel;
            this.ComponentType = componentType;
            this.ObjectOfInterest = objectOfInterest;
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the <see cref="IApplicationBaseViewModel" />
        /// </summary>
        public IApplicationBaseViewModel ApplicationBaseViewModel { get; }

        /// <summary>
        /// Gets the component <see cref="Type" />
        /// </summary>
        public Type ComponentType { get; }

        /// <summary>
        /// Gets the object of interest
        /// </summary>
        public object ObjectOfInterest { get; }

        /// <summary>
        /// Gets the tab's id
        /// </summary>
        public Guid Id { get; }
    }
}
