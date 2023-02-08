// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
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

namespace COMETwebapp
{
    using BlazorStrap;

    using CDP4Dal;
    using COMETwebapp.Components.Canvas;
    using COMETwebapp.Interoperability;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Pages.SystemRepresentation;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

    /// <summary>
    /// Point of entry of the application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Point of entry of the application
        /// </summary>
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient());

            builder.Services.AddSingleton<ISessionAnchor, SessionAnchor>();
            builder.Services.AddSingleton<ISession, Session>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddSingleton<AuthenticationStateProvider, CometWebAuthStateProvider>();
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            builder.Services.AddSingleton<IIterationService, IterationService>();
            builder.Services.AddSingleton<IAutoRefreshService, AutoRefreshService>();
            builder.Services.AddSingleton<IVersionService, VersionService>();
            builder.Services.AddSingleton<ISceneSettings, SceneSettings>();
            builder.Services.AddSingleton<IJSInterop, JSInterop>();
            builder.Services.AddSingleton<ISelectionMediator, SelectionMediator>();


            builder.Services.AddTransient<ISystemRepresentationPageViewModel, SystemRepresentationPageViewModel>();
            builder.Services.AddTransient<ISystemTreeViewModel, SystemTreeViewModel>();


            builder.Services.AddDevExpressBlazor();
            builder.Services.AddBlazorStrap();
            builder.Services.AddAntDesign();

            await builder.Build().RunAsync();
        }
    }
}
