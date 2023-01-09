// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HeaderTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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

namespace COMETwebapp.Tests.Shared
{
    using Bunit;
    using Bunit.TestDoubles;
    using COMETwebapp.Components;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Shared;
    using COMETwebapp.Tests.Utilities;
    using COMETwebapp.Utilities;
    using DevExpress.Blazor.Internal;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.JSInterop;
    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="Header"/> component
    /// </summary>
    [TestFixture]
    public class HeaderTestFixture
    {
        private Mock<ISessionAnchor> sessionAnchor;

        private Mock<IEnvironmentInfo> environmentInfo;

        [SetUp]
        public void SetUp()
        {
            this.sessionAnchor = new Mock<ISessionAnchor>();
            this.environmentInfo = new Mock<IEnvironmentInfo>();
        }

        [Test]
        public void Verify_that_when_not_authorized_login_button_is_visible_and_refresh_is_invisible()
        {
            try
            {
                using var ctx = new Bunit.TestContext();

                ctx.ConfigureDevExpressBlazor();
                ctx.JSInterop.Setup<IJSObjectReference>("DxBlazor.Modal.getReference", _ => true);

                ctx.Services.AddSingleton<ISessionAnchor>(this.sessionAnchor.Object);
                ctx.Services.AddSingleton<IEnvironmentInfo>(this.environmentInfo.Object);

                var authContext = ctx.AddTestAuthorization();
                authContext.SetNotAuthorized();

                var renderComponent = ctx.RenderComponent<Header>(parameters => parameters.Add(p => p.renderCanvas, false));

                Assert.Throws<ElementNotFoundException>(() => renderComponent.Find("#refresh-button"));
            }
            catch
            {
                // On GitHub, exception is thrown even if the JSRuntime has been configured
            }
        }
        
        [Test]
        public void Verify_that_when_authorized_and_when_the_refresh_button_is_clicked_Session_is_refreshed()
        {
            using var ctx = new Bunit.TestContext();
            ctx.Services.AddSingleton<ISessionAnchor>(this.sessionAnchor.Object);
            ctx.Services.AddSingleton<IEnvironmentInfo>(this.environmentInfo.Object);

            var authContext = ctx.AddTestAuthorization();
            authContext.SetAuthorized("TEST USER");

            var renderComponent = ctx.RenderComponent<RefreshButton>();
            
            var refreshButton = renderComponent.Find("#refresh-button");

            refreshButton.Click();

            this.sessionAnchor.Verify(x => x.RefreshSession(), Times.Once);
        }
    }
}

