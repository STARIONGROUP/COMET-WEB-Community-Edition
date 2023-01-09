// --------------------------------------------------------------------------------------------------------
// <copyright file="DevExpressBlazorTestHelper.cs" company="RHEA System S.A.">
//  Copyright (c) 2022 RHEA System S.A.
// 
//  Author: Antoine Théate, Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
// 
//  This file is part of UI-DSM.
//  The UI-DSM web application is used to review an ECSS-E-TM-10-25 model.
// 
//  The UI-DSM application is provided to the community under the Apache License 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Utilities
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
    ///     Helper class that configures a <see cref="TestContext" /> to be able to test DevExpress components
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class DevExpressBlazorTestHelper
    {
        /// <summary>
        ///     Configure the <see cref="TestContext" /> to include all prerequisites to test components with DevExpress components
        /// </summary>
        /// <param name="context">The <see cref="TestContext" /></param>
        public static void ConfigureDevExpressBlazor(this TestContext context)
        {
            context.Services.AddDevExpressBlazor(_ => ConfigureJSInterop(context.JSInterop));
        }

        /// <summary>
        ///     Restore the <see cref="TestContext.JSInterop" /> and disposes the <see cref="TestContext" />
        /// </summary>
        /// <param name="context">The <see cref="TestContext" /></param>
        public static void CleanContext(this TestContext context)
        {
            context.JSInterop.Mode = JSRuntimeMode.Strict;
            context.Dispose();
        }

        /// <summary>
        ///     Configure the <see cref="BunitJSInterop" /> for DevExpress
        /// </summary>
        /// <param name="interop">The <see cref="BunitJSInterop" /> to configure</param>
        private static void ConfigureJSInterop(BunitJSInterop interop)
        {
            interop.Mode = JSRuntimeMode.Loose;
            
            var rootModule = interop.SetupModule("./_content/DevExpress.Blazor/dx-blazor.js");
            rootModule.Mode = JSRuntimeMode.Strict;

            rootModule.Setup<DeviceInfo>("getDeviceInfo", _ => true)
                .SetResult(new DeviceInfo(false));
        }

        /// <summary>
        /// Adds the services for DevExpress testing
        /// </summary>
        /// <param name="testContext">the <see cref="TestContext"/> to add the services</param>
        public static void AddDevExpressBlazorTesting(this TestContext testContext)
        {
            testContext.Services.TryAddScoped<IEnvironmentInfoFactory, MockEnvironmentInfoFactory>();
            testContext.Services.TryAddScoped<IEnvironmentInfo, MockEnvironmentInfo>();

            testContext.Services.AddOptions();
            testContext.Services.AddLogging();
            testContext.Services.TryAddComponentRequiredServices();
        }
    }

    /// <summary>
    /// Class for mocking the environment info factory of DevExpress
    /// </summary>
    sealed class MockEnvironmentInfoFactory : IEnvironmentInfoFactory
    {
        /// <summary>
        /// Backing field for the <see cref="CreateEnvironmentInfo"/>
        /// </summary>
        readonly IEnvironmentInfo cached;

        /// <summary>
        /// Creates a new instance of type <see cref="MockEnvironmentInfoFactory"/>
        /// </summary>
        /// <param name="isWasm">if the app is a Blazor WASM application</param>
        public MockEnvironmentInfoFactory(bool isWasm = false)
        {
            this.cached = new MockEnvironmentInfo(isWasm);
        }

        /// <summary>
        /// Gets the environment info
        /// </summary>
        /// <returns>the environment info</returns>
        public IEnvironmentInfo CreateEnvironmentInfo() => cached;
    }

    /// <summary>
    /// Class for mocking the environment of DevExpress
    /// </summary>
    public class MockEnvironmentInfo : IEnvironmentInfo
    {
        /// <summary>
        /// Gets or sets if its a Blazor WASM application
        /// </summary>
        public bool IsWasm { get; }

        /// <summary>
        /// Backing field for the method <see cref="GetDateTimeNow"/>
        /// </summary>
        public static readonly DateTime DateTimeNow = DateTime.Now.Date;

        /// <summary>
        /// Gets the current culture
        /// </summary>
        public CultureInfo CurrentCulture => CultureInfo.InvariantCulture;

        /// <summary>
        /// Gets the api scheme
        /// </summary>
        Task<ApiScheme> IEnvironmentInfo.ApiScheme
        {
            get { return Task.FromResult(new ApiScheme(true)); }
        }

        /// <summary>
        /// Gets the device info
        /// </summary>
        Task<DeviceInfo> IEnvironmentInfo.DeviceInfo
        {
            get { return Task.FromResult(new DeviceInfo(false)); }
        }

        /// <summary>
        /// Creates a new instance of type <see cref="MockEnvironmentInfo"/>
        /// </summary>
        /// <param name="isWasm">if the app is a Blazor WASM application</param>
        public MockEnvironmentInfo(bool isWasm = false)
        {
            IsWasm = isWasm;
        }

        /// <summary>
        /// Gets the current date time
        /// </summary>
        /// <returns>the date time</returns>
        public DateTime GetDateTimeNow()
        {
            return DateTimeNow;
        }
    }
}
