// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="GenericApplicationTemplate.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
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

namespace COMET.Web.Common.Components.Applications
{
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Shared component that will englobe all applications
    /// </summary>
    /// <typeparam name="TViewModel">Any <see cref="IApplicationTemplateViewModel" /></typeparam>
    public abstract partial class GenericApplicationTemplate<TViewModel>
    {
        /// <summary>
        /// Body of the application
        /// </summary>
        [Parameter]
        public RenderFragment Body { get; set; }

        /// <summary>
        /// Gets or Sets the <typeparamref name="TViewModel" />
        /// </summary>
        [Inject]
        public TViewModel ViewModel { get; set; }

        /// <summary>
        /// The <see cref="NavigationManager" />
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IConfiguration" />
        /// </summary>
        [Inject]
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Set URL parameters based on the current context
        /// </summary>
        /// <param name="currentOptions">A <see cref="Dictionary{TKey,TValue}" /> of URL parameters</param>
        protected virtual void SetUrlParameters(Dictionary<string, string> currentOptions)
        {
            if (string.IsNullOrEmpty(this.Configuration.GetSection(ConfigurationKeys.ServerConfigurationKey).Get<ServerConfiguration>().ServerAddress))
            {
                currentOptions[QueryKeys.ServerKey] = this.ViewModel.SessionService.Session.DataSourceUri;
            }
        }
    }
}
