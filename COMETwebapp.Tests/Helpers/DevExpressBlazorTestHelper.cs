// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DevExpressBlazorTestHelper.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Helpers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading.Tasks;

    using Bunit;

    using DevExpress.Blazor.Internal;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Helper class that configures a <see cref="TestContext" /> to be able to test DevExpress components
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class DevExpressBlazorTestHelper
    {
        /// <summary>
        /// Configure the <see cref="TestContext" /> to include all prerequisites to test components with DevExpress components
        /// </summary>
        /// <param name="context">The <see cref="TestContext" /></param>
        public static void ConfigureDevExpressBlazor(this TestContext context)
        {
            context.Services.TryAddScoped<IEnvironmentInfoFactory, MockEnvironmentInfoFactory>();
            context.Services.TryAddScoped<IEnvironmentInfo, MockEnvironmentInfo>();
            context.Services.AddOptions();
            context.Services.AddLogging();
            context.Services.TryAddComponentRequiredServices();
            context.Services.AddDevExpressBlazor(_ => ConfigureJsInterop(context.JSInterop));
            context.JSInterop.SetupVoid("DxBlazor.AdaptiveDropDown.init");
            context.JSInterop.SetupVoid("DxBlazor.Input.loadModule");
            context.JSInterop.SetupVoid("DxBlazor.UiHandlersBridge.loadModule");
        }

        /// <summary>
        /// Restore the <see cref="TestContext.JSInterop" /> and disposes the <see cref="TestContext" />
        /// </summary>
        /// <param name="context">The <see cref="TestContext" /></param>
        public static void CleanContext(this TestContext context)
        {
            context.JSInterop.Mode = JSRuntimeMode.Strict;
            context.Dispose();
        }

        /// <summary>
        /// Configure the <see cref="BunitJSInterop" /> for DevExpress
        /// </summary>
        /// <param name="interop">The <see cref="BunitJSInterop" /> to configure</param>
        private static void ConfigureJsInterop(BunitJSInterop interop)
        {
            interop.Mode = JSRuntimeMode.Loose;

            var rootModule = interop.SetupModule("./_content/DevExpress.Blazor/dx-blazor.js");
            rootModule.Mode = JSRuntimeMode.Strict;

            rootModule.Setup<DeviceInfo>("getDeviceInfo", _ => true)
                .SetResult(new DeviceInfo(false));
        }
    }

    /// <summary>
    /// Mocked class for the <see cref="EnvironmentInfoFactory" />
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class MockEnvironmentInfoFactory : IEnvironmentInfoFactory
    {
        /// <summary>
        /// The cacehd <see cref="IEnvironmentInfo" />
        /// </summary>
        private readonly IEnvironmentInfo cached;

        /// <summary>
        /// Initializes a new <see cref="MockEnvironmentInfoFactory" />
        /// </summary>
        /// <param name="isWasm">If the environment is WebAssembly</param>
        public MockEnvironmentInfoFactory(bool isWasm = false)
        {
            this.cached = new MockEnvironmentInfo(isWasm);
        }

        /// <summary>
        /// Gets the <see cref="IEnvironmentInfo" />
        /// </summary>
        /// <returns>The <see cref="IEnvironmentInfo" /></returns>
        public IEnvironmentInfo CreateEnvironmentInfo()
        {
            return this.cached;
        }
    }

    /// <summary>
    /// Mocked the <see cref="IEnvironmentInfo" />
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MockEnvironmentInfo : IEnvironmentInfo
    {
        /// <summary>
        /// The <see cref="DateTime" />
        /// </summary>
        public static readonly DateTime DateTimeNow = DateTime.Now.Date;

        /// <summary>
        /// Initializes a new <see cref="MockEnvironmentInfo" />
        /// </summary>
        /// <param name="isWasm">If the environment is WebAssembly</param>
        public MockEnvironmentInfo(bool isWasm = false)
        {
            this.IsWasm = isWasm;
            this.CurrentCulture = CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Value asserting if the environment is WebAssembly
        /// </summary>
        public bool IsWasm { get; }

        /// <summary>
        /// Gets the <see cref="CultureInfo" />
        /// </summary>
        public CultureInfo CurrentCulture { get; }

        /// <summary>
        /// Gets the <see cref="DateTime" />
        /// </summary>
        /// <returns>The now <see cref="DateTime" /></returns>
        public DateTime GetDateTimeNow()
        {
            return DateTimeNow;
        }

        /// <summary>
        /// Gets the <see cref="ApiScheme" />
        /// </summary>
        Task<ApiScheme> IEnvironmentInfo.ApiScheme => Task.FromResult(new ApiScheme(true));

        /// <summary>
        /// Gets the <see cref="DeviceInfo" />
        /// </summary>
        Task<DeviceInfo> IEnvironmentInfo.DeviceInfo => Task.FromResult(new DeviceInfo(false));
    }
}
