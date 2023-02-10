// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemRepresentation.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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

namespace COMETwebapp.Pages.SystemRepresentation
{
    using System.Threading.Tasks;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using COMETwebapp.Components.SystemRepresentation;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Model;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.ViewModels.Pages.SystemRepresentation;
    using Microsoft.AspNetCore.Components;
    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="SystemRepresentation"/>
    /// </summary>
    public partial class SystemRepresentation
    {
        /// <summary>
        ///     The <see cref="ISystemRepresentationPageViewModel" /> for this page
        /// </summary>
        [Inject]
        public ISystemRepresentationPageViewModel ViewModel { get; set; }

        /// <summary>
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        protected override Task OnInitializedAsync()
        {
            this.ViewModel.OnInitializedAsync(this.SessionAnchor);
            return base.OnInitializedAsync();
        }

        /// <summary>
        /// Name of the option selected
        /// </summary>
        public string? OptionSelected { get; set; }

        /// <summary>
        /// Name of the domain selected
        /// </summary>
        public DomainOfExpertise DomainSelected { get; set; }

        /// <summary>
        /// Injected property to get access to <see cref="ISessionAnchor"/>
        /// </summary>
        [Inject]
        public ISessionAnchor SessionAnchor { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="OnAfterRenderAsync(bool)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                this.ViewModel.OnInitializedAsync(this.SessionAnchor);
                this.StateHasChanged();
            }
        }

        /// <summary>
        /// Updates Elements list when a filter for option is selected
        /// </summary>
        /// <param name="option">Name of the Option selected</param>
        public void OnOptionFilterChange(string? option)
        {
            this.ViewModel.OnOptionFilterChange(option, this.SessionAnchor);
        }

        /// <summary>
        /// Updates Elements list when a filter for option is selected
        /// </summary>
        /// <param name="domain">Name of the Option selected</param>
        public void OnDomainFilterChange(DomainOfExpertise? domain)
        {
            this.ViewModel.OnDomainFilterChange(domain, this.SessionAnchor);
        }
    }
}
