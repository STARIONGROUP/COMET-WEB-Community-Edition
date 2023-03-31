// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AutoRefreshServiceTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    using COMET.Web.Common.Services.SessionManagement;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class AutoRefreshServiceTestFixture
    {
		private Mock<ISessionService> sessionService;
        private AutoRefreshService autoRefreshService;
        
        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();

            this.autoRefreshService = new AutoRefreshService(this.sessionService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.autoRefreshService.Dispose();
        }

        [Test]
        public void VerifySetTimer()
        {
            this.autoRefreshService.IsAutoRefreshEnabled = true;
            this.autoRefreshService.AutoRefreshInterval = 1;
            this.autoRefreshService.SetTimer();
            Thread.Sleep(5000);
            this.sessionService.Verify(x => x.RefreshSession(), Times.AtLeastOnce);

            this.sessionService.Invocations.Clear();

            this.autoRefreshService.IsAutoRefreshEnabled = false;
            this.autoRefreshService.SetTimer();
            Thread.Sleep(5000);
            this.sessionService.Verify(x => x.RefreshSession(), Times.Never);
        }
    }
}
