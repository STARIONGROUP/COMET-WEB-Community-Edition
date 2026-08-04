/**
 * Initializes Blazor Server with custom SignalR client options.
 * @see {@link https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/startup?view=aspnetcore-10.0}
 * @see {@link https://learn.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-10.0&tabs=dotnet}
 * @param {number} [serverTimeoutMs=30000] The server timeout interval in milliseconds.
 * @param {number} [keepAliveMs=15000] The keep-alive interval in milliseconds.
 */
function initBlazor(serverTimeoutMs, keepAliveMs) {
    Blazor.start({
        configureSignalR: function (builder) {
            builder.withServerTimeout(serverTimeoutMs || 30000)
                   .withKeepAliveInterval(keepAliveMs || 15000);
        }
    });
}
