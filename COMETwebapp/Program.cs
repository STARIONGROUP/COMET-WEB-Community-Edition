// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Shared.TopMenuEntry;

    using COMETwebapp.Extensions;
    using COMETwebapp.Model;
    using COMETwebapp.Shared.TopMenuEntry;

    /// <summary>
    /// Point of entry of the application
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Program
    {
        /// <summary>
        /// Point of entry of the application
        /// </summary>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            builder.Services.RegisterCdp4CometCommonServices(true, options =>
            {
                options.Applications = Applications.ExistingApplications;
                options.AdditionalAssemblies.Add(Assembly.GetAssembly(typeof(Program)));
                options.AdditionalMenuEntries.AddRange(new List<Type> { typeof(ApplicationMenu), typeof(ModelMenu), typeof(SessionMenu), typeof(ShowHideDeprecatedThings), typeof(AboutMenu) });
            });

            builder.Services.RegisterServices();
            builder.Services.RegisterViewModels();

            var app = builder.Build();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            await app.Services.InitializeCdp4CometCommonServices();
            await app.RunAsync();
        }
    }
}
