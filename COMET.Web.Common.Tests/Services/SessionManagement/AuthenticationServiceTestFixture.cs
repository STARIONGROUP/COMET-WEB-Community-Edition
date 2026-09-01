// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AuthenticationServiceTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Services.SessionManagement
{
    using Blazored.SessionStorage;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using CDP4DalCommon.Authentication;

    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities;

    using FluentResults;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class AuthenticationServiceTestFixture
    {
        private Mock<ISession> session;
        private Mock<ISessionService> sessionService;
        private CometWebAuthStateProvider cometWebAuthStateProvider;
        private AuthenticationService authenticationService;
        private AuthenticationDto authenticationDto;
        private Mock<ISessionStorageService> sessionStorageService;
        private Mock<IProvideExternalAuthenticationService> openIdConnectService;
        private Mock<IAuthenticationRefreshService> automaticTokenRefreshService;
        private Mock<IArchiveFileService> archiveFileService;
        private Credentials credentials;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.sessionService = new Mock<ISessionService>();
            this.sessionStorageService = new Mock<ISessionStorageService>();
            this.openIdConnectService = new Mock<IProvideExternalAuthenticationService>();
            this.automaticTokenRefreshService = new Mock<IAuthenticationRefreshService>();
            this.archiveFileService = new Mock<IArchiveFileService>();

            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(false);

            this.cometWebAuthStateProvider = new CometWebAuthStateProvider(this.sessionService.Object);
            
            this.authenticationService = new AuthenticationService(this.sessionService.Object, this.cometWebAuthStateProvider, this.sessionStorageService.Object,
                this.openIdConnectService.Object, this.automaticTokenRefreshService.Object, this.archiveFileService.Object);

            this.credentials = new Credentials(new Uri("http://localhost:5000/"));
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
            
            this.authenticationDto = new AuthenticationDto
            {
                SourceAddress = "https://www.stariongroup.eu/",
                UserName = "John Doe",
                Password = "secret"
            };
        }

        [TearDown]
        public void Teardown()
        {
            this.authenticationService.Dispose();
        }

        [Test]
        public async Task VerifyLogout()
        {
            await this.authenticationService.Logout();
            this.sessionService.Verify(x => x.CloseSession(), Times.Once);
            this.archiveFileService.Verify(x => x.Remove(), Times.Once);
        }

        [Test]
        public async Task VerifyLoginFromArchive()
        {
            var authStateChanged = false;
            this.cometWebAuthStateProvider.AuthenticationStateChanged += _ => authStateChanged = true;

            var failedResult = await this.authenticationService.LoginFromArchive(null, "admin", "password");

            Assert.Multiple(() =>
            {
                Assert.That(failedResult.IsFailed, Is.EqualTo(true));
                this.sessionService.Verify(x => x.OpenArchiveSession(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            });

            failedResult = await this.authenticationService.LoginFromArchive(string.Empty, "admin", "password");

            Assert.Multiple(() =>
            {
                Assert.That(failedResult.IsFailed, Is.EqualTo(true));
                this.sessionService.Verify(x => x.OpenArchiveSession(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            });

            const string invalidArchivePath = "invalid-archive.zip";
            const string archivePath = "archive.zip";
            const string userName = "admin";
            const string password = "password";

            this.sessionService.Setup(x => x.OpenArchiveSession(invalidArchivePath, userName, password)).ReturnsAsync(Result.Fail("error"));

            failedResult = await this.authenticationService.LoginFromArchive(invalidArchivePath, userName, password);

            Assert.Multiple(() =>
            {
                Assert.That(failedResult.IsFailed, Is.EqualTo(true));
                this.sessionStorageService.Verify(x => x.SetItemAsync("cdp4-comet-username", userName, default), Times.Never);
                Assert.That(authStateChanged, Is.EqualTo(false));
            });

            this.sessionService.Setup(x => x.OpenArchiveSession(archivePath, userName, password)).ReturnsAsync(Result.Ok());

            var successResult = await this.authenticationService.LoginFromArchive(archivePath, userName, password);

            Assert.Multiple(() =>
            {
                Assert.That(successResult.IsSuccess, Is.EqualTo(true));
                this.sessionService.Verify(x => x.OpenArchiveSession(archivePath, userName, password), Times.Once);
                this.sessionStorageService.Verify(x => x.SetItemAsync("cdp4-comet-username", userName, default), Times.Once);
                Assert.That(authStateChanged, Is.EqualTo(true));
            });
        }

        [Test]
        public async Task VerifyValidLogin()
        {
            this.sessionService.Setup(x => x.OpenSession(It.IsAny<Credentials>())).ReturnsAsync(Result.Ok);

            this.authenticationDto.FullTrust = true;
            var loginResult = await this.authenticationService.Login(this.authenticationDto);

            Assert.That(loginResult.IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public async Task VerifyInvalidLogin()
        {
            this.sessionService.Setup(x => x.OpenSession(It.IsAny<Credentials>())).ReturnsAsync(Result.Fail(["error"]));

            var loginResult = await this.authenticationService.Login(new AuthenticationDto());
            Assert.That(loginResult.IsSuccess, Is.EqualTo(false));

            loginResult = await this.authenticationService.Login(this.authenticationDto);
            Assert.That(loginResult.IsSuccess, Is.EqualTo(false));
        }

        [Test]
        public void VerifyLoginDoesNotLeakExceptions()
        {
            // The SDK throws (for example InvalidOperationException "User not found." or "Session is already open.") for
            // failures it does not translate to a Result. Left unhandled these terminate the Blazor circuit, which is
            // the crash seen when logging in as another user while an archive session is still open.
            this.sessionService.Setup(x => x.OpenSession(It.IsAny<Credentials>())).ThrowsAsync(new InvalidOperationException("User not found."));
            this.sessionService.Setup(x => x.AuthenticateAndOpenSession(It.IsAny<AuthenticationSchemeKind>(), It.IsAny<AuthenticationInformation>())).ThrowsAsync(new InvalidOperationException("Session is already open."));

            Assert.Multiple(() =>
            {
                Assert.That(async () => await this.authenticationService.Login(this.authenticationDto), Throws.Nothing);
                Assert.That(async () => (await this.authenticationService.Login(this.authenticationDto)).IsFailed, Is.True);
                Assert.That(async () => await this.authenticationService.LoginAsync(AuthenticationSchemeKind.LocalJwtBearer, new AuthenticationInformation("user", "pass")), Throws.Nothing);
                Assert.That(async () => (await this.authenticationService.LoginAsync(AuthenticationSchemeKind.LocalJwtBearer, new AuthenticationInformation("user", "pass"))).IsFailed, Is.True);
            });
        }

        [Test]
        public async Task VerifyLoginWithDefinedScheme()
        {
            var authenticationInfo = new AuthenticationInformation("admin", "pass");
            this.sessionService.Setup(x => x.AuthenticateAndOpenSession(AuthenticationSchemeKind.Basic, authenticationInfo)).ReturnsAsync(Result.Fail("error"));
            
            var loginResult = await this.authenticationService.LoginAsync(AuthenticationSchemeKind.Basic, authenticationInfo);
            Assert.That(loginResult.IsSuccess, Is.EqualTo(false));
            
            this.sessionService.Setup(x => x.AuthenticateAndOpenSession(AuthenticationSchemeKind.Basic, authenticationInfo)).ReturnsAsync(Result.Ok());
            loginResult = await this.authenticationService.LoginAsync(AuthenticationSchemeKind.Basic, authenticationInfo);

            Assert.Multiple(() =>
            {
                Assert.That(loginResult.IsSuccess, Is.EqualTo(true)); 
                this.sessionStorageService.Verify(x => x.SetItemAsync("access_token", It.IsAny<string>(), default), Times.Never);
            });
            
            var tokenBasedAuthenticationInfo = new AuthenticationInformation(new AuthenticationToken("token", "refresh"));
            
            this.sessionService.Setup(x => x.AuthenticateAndOpenSession(AuthenticationSchemeKind.ExternalJwtBearer, tokenBasedAuthenticationInfo)).ReturnsAsync(Result.Ok())
                .Callback(() => this.credentials.ProvideUserToken(tokenBasedAuthenticationInfo.Token, AuthenticationSchemeKind.ExternalJwtBearer));
            
            this.sessionService.Setup(x => x.AuthenticateAndOpenSession(AuthenticationSchemeKind.LocalJwtBearer, tokenBasedAuthenticationInfo)).ReturnsAsync(Result.Ok())
                .Callback(() => this.credentials.ProvideUserToken(tokenBasedAuthenticationInfo.Token, AuthenticationSchemeKind.LocalJwtBearer));
            
            loginResult = await this.authenticationService.LoginAsync(AuthenticationSchemeKind.LocalJwtBearer, tokenBasedAuthenticationInfo);

            Assert.Multiple(() =>
            {
                Assert.That(loginResult.IsSuccess, Is.EqualTo(true)); 
                this.sessionStorageService.Verify(x => x.SetItemAsync("access_token", tokenBasedAuthenticationInfo.Token.AccessToken, default), Times.Once);
            });
            
            loginResult = await this.authenticationService.LoginAsync(AuthenticationSchemeKind.ExternalJwtBearer, tokenBasedAuthenticationInfo);

            Assert.Multiple(() =>
            {
                Assert.That(loginResult.IsSuccess, Is.EqualTo(true)); 
                this.sessionStorageService.Verify(x => x.SetItemAsync("access_token", tokenBasedAuthenticationInfo.Token.AccessToken, default), Times.Exactly(2));
            });
        }

        [Test]
        public void VerifyRetrieveLastUsedServerUrl()
        {
            this.sessionStorageService.Setup(x => x.GetItemAsync<string>("cdp4-comet-url", CancellationToken.None)).ReturnsAsync((string)null);

            Assert.That(async () => await this.authenticationService.RetrieveLastUsedServerUrlAsync(), Is.Null);
            
            const string serverUrl = "https://www.stariongroup.eu/";
            this.sessionStorageService.Setup(x => x.GetItemAsync<string>("cdp4-comet-url", CancellationToken.None)).ReturnsAsync(serverUrl);

            Assert.That(async () => await this.authenticationService.RetrieveLastUsedServerUrlAsync(), Is.EqualTo(serverUrl));
        }

        [Test]
        public void VerifyRetrieveLastUsedUserName()
        {
            this.sessionStorageService.Setup(x => x.GetItemAsync<string>("cdp4-comet-username", CancellationToken.None)).ReturnsAsync((string)null);

            Assert.That(async () => await this.authenticationService.RetrieveLastUsedUserNameAsync(), Is.Null);
            
            const string userName = "admin";
            this.sessionStorageService.Setup(x => x.GetItemAsync<string>("cdp4-comet-username", CancellationToken.None)).ReturnsAsync(userName);

            Assert.That(async () => await this.authenticationService.RetrieveLastUsedUserNameAsync(), Is.EqualTo(userName));
        }

        [Test]
        public async Task VerifyTryRestoreLastSession()
        {
            this.sessionStorageService.Setup(x => x.GetItemAsync<string>("cdp4-comet-url", default)).ReturnsAsync((string)null);
            await this.authenticationService.TryRestoreLastSessionAsync();

            this.sessionService.Verify(x => x.InitializeSessionAndRequestServerSupportedAuthenticationScheme(It.IsAny<Credentials>()), Times.Never);
            
            const string serverUrl = "https://www.stariongroup.eu/";
            this.sessionStorageService.Setup(x => x.GetItemAsync<string>("cdp4-comet-url", default)).ReturnsAsync(serverUrl);

            this.sessionService.Setup(x => x.InitializeSessionAndRequestServerSupportedAuthenticationScheme(It.IsAny<Credentials>()))
                .ReturnsAsync(Result.Fail<AuthenticationSchemeResponse>("failed"));
            
            await this.authenticationService.TryRestoreLastSessionAsync();
            this.sessionService.Verify(x => x.AuthenticateAndOpenSession(It.IsAny<AuthenticationSchemeKind>(), It.IsAny<AuthenticationInformation>()), Times.Never);
            
            var authenticationResponse = new AuthenticationSchemeResponse()
            {
                Schemes = [AuthenticationSchemeKind.Basic]
            };
            
            this.sessionService.Setup(x => x.InitializeSessionAndRequestServerSupportedAuthenticationScheme(It.IsAny<Credentials>()))
                .ReturnsAsync(Result.Ok(authenticationResponse));
            
            await this.authenticationService.TryRestoreLastSessionAsync();
            
            this.sessionService.Verify(x => x.AuthenticateAndOpenSession(It.IsAny<AuthenticationSchemeKind>(), It.IsAny<AuthenticationInformation>()), Times.Never);

            authenticationResponse.Schemes = [AuthenticationSchemeKind.LocalJwtBearer];
            this.sessionStorageService.Setup(x => x.GetItemAsync<string>("access_token", default)).ReturnsAsync((string)null);
            
            await this.authenticationService.TryRestoreLastSessionAsync();
            
            this.sessionService.Verify(x => x.AuthenticateAndOpenSession(It.IsAny<AuthenticationSchemeKind>(), It.IsAny<AuthenticationInformation>()), Times.Never);
            this.sessionStorageService.Setup(x => x.GetItemAsync<string>("access_token", default)).ReturnsAsync("token");

            this.sessionService.Setup(x => x.AuthenticateAndOpenSession(AuthenticationSchemeKind.LocalJwtBearer, It.IsAny<AuthenticationInformation>()))
                .ReturnsAsync(Result.Fail("invalid credentials"));
            
            await this.authenticationService.TryRestoreLastSessionAsync();
            this.sessionService.Verify(x => x.AuthenticateAndOpenSession(AuthenticationSchemeKind.LocalJwtBearer, It.IsAny<AuthenticationInformation>()), Times.Once);
        }
        
        [Test]
        public async Task VerifyExchangeOpenIdConnectCodeAsync()
        {
            const string code = "aRandomCode";
            const string redirect = "http://localhost/callback";
            
            var authenticationSchemeResponse = new AuthenticationSchemeResponse()
            {
                Schemes = [AuthenticationSchemeKind.Basic]
            };

            await  Assert.MultipleAsync(async () =>
            {
                await Assert.ThatAsync(() => this.authenticationService.ExchangeOpenIdConnectCodeAsync(null, authenticationSchemeResponse, redirect), Throws.Exception);
                await Assert.ThatAsync(() => this.authenticationService.ExchangeOpenIdConnectCodeAsync(code, null, redirect), Throws.Exception);
                await Assert.ThatAsync(() => this.authenticationService.ExchangeOpenIdConnectCodeAsync(code, authenticationSchemeResponse, null), Throws.Exception);
                await Assert.ThatAsync(() => this.authenticationService.ExchangeOpenIdConnectCodeAsync(code, authenticationSchemeResponse, redirect), Throws.Exception);
            });

            authenticationSchemeResponse.Schemes = [AuthenticationSchemeKind.ExternalJwtBearer];
            var openIdDto = new AuthenticationToken("access", "refresh");
            
            this.openIdConnectService.Setup(x => x.RequestAuthenticationToken(code, authenticationSchemeResponse, redirect, null)).ReturnsAsync(openIdDto);
            
            this.sessionService.Setup(x => x.AuthenticateAndOpenSession(AuthenticationSchemeKind.ExternalJwtBearer, It.IsAny<AuthenticationInformation>()))
                .ReturnsAsync(Result.Ok())
                .Callback(()=> this.credentials.ProvideUserToken(openIdDto, AuthenticationSchemeKind.ExternalJwtBearer));
            
            await this.authenticationService.ExchangeOpenIdConnectCodeAsync(code, authenticationSchemeResponse, redirect);
            this.sessionService.Verify(x => x.AuthenticateAndOpenSession(AuthenticationSchemeKind.ExternalJwtBearer, It.IsAny<AuthenticationInformation>()), Times.Once);
            
            this.openIdConnectService.Setup(x => x.RequestAuthenticationToken(code, authenticationSchemeResponse, redirect, null)).ThrowsAsync(new InvalidOperationException());
            await this.authenticationService.ExchangeOpenIdConnectCodeAsync(code, authenticationSchemeResponse, redirect);

            this.sessionStorageService.Verify(x => x.SetItemAsync(It.IsAny<string>(), string.Empty, CancellationToken.None), Times.Exactly(3));
        }

        [Test]
        public async Task VerifyBuildExternalProviderLogoutUrl()
        {
            Assert.That(this.authenticationService.BuildExternalProviderLogoutUrl("http://localhost/"), Is.Null);

            var basicSchemeResponse = new AuthenticationSchemeResponse
            {
                Schemes = [AuthenticationSchemeKind.Basic]
            };

            this.sessionService.Setup(x => x.InitializeSessionAndRequestServerSupportedAuthenticationScheme(It.IsAny<Credentials>())).ReturnsAsync(Result.Ok(basicSchemeResponse));
            await this.authenticationService.RequestAvailableAuthenticationSchemeAsync("http://localhost:5000");

            Assert.That(this.authenticationService.BuildExternalProviderLogoutUrl("http://localhost/"), Is.Null);

            var externalSchemeResponse = new AuthenticationSchemeResponse
            {
                Schemes = [AuthenticationSchemeKind.ExternalJwtBearer],
                Authority = "http://localhost:8080/realms/test",
                ClientId = "comet-web"
            };

            this.sessionService.Setup(x => x.InitializeSessionAndRequestServerSupportedAuthenticationScheme(It.IsAny<Credentials>())).ReturnsAsync(Result.Ok(externalSchemeResponse));
            await this.authenticationService.RequestAvailableAuthenticationSchemeAsync("http://localhost:5000");
            var logoutUrl = this.authenticationService.BuildExternalProviderLogoutUrl("http://localhost/Logout?confirmed=true");

            Assert.That(logoutUrl, Is.EqualTo("http://localhost:8080/realms/test/protocol/openid-connect/logout?client_id=comet-web&post_logout_redirect_uri=http%3A%2F%2Flocalhost%2FLogout%3Fconfirmed%3Dtrue"));
        }
    }
}
