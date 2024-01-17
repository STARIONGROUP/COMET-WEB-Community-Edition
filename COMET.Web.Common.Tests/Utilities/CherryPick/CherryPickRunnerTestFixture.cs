// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CherryPickRunnerTestFixture.cs" company="RHEA System S.A.">
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


namespace COMET.Web.Common.Tests.Utilities.CherryPick
{
    using NUnit.Framework;

    using SiteDirectory = CDP4Common.SiteDirectoryData.SiteDirectory;
    using CDP4Common.DTO;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.CherryPick;

    using Moq;

    [TestFixture]
    public class CherryPickRunnerTestFixture
    {
        private CherryPickRunner viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<INeedCherryPickedData> needCherryPickedData;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.needCherryPickedData = new Mock<INeedCherryPickedData>();
            this.viewModel = new CherryPickRunner(this.sessionService.Object);
        }

        [Test]
        public async Task VerifyProperties()
        {
            Assert.That(this.viewModel.IsCherryPicking, Is.False);
            this.viewModel.InitializeProperties(new List<INeedCherryPickedData> { this.needCherryPickedData.Object });

            this.sessionService.Setup(x => x.Session.RetrieveSiteDirectory()).Returns(new SiteDirectory());

            var propertyInfo = typeof(CherryPickRunner).GetProperty("IsCherryPicking", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            propertyInfo?.SetValue(this.viewModel, true, null);
            await this.viewModel.RunCherryPickAsync();
            this.needCherryPickedData.Verify(x => x.ProcessCherryPickedData(Moq.It.IsAny<IEnumerable<Thing>>()), Times.Never);

            propertyInfo?.SetValue(this.viewModel, false, null);
            await this.viewModel.RunCherryPickAsync();
            this.needCherryPickedData.Verify(x => x.ProcessCherryPickedData(Moq.It.IsAny<IEnumerable<Thing>>()), Times.Once);
            
            this.needCherryPickedData.Invocations.Clear();
            
            var engineeringModelId = Guid.NewGuid();
            var iterationId = Guid.NewGuid();
            propertyInfo?.SetValue(this.viewModel, true, null);
            await this.viewModel.RunCherryPickAsync(new[] { (engineeringModelId, iterationId)});
            this.needCherryPickedData.Verify(x => x.ProcessCherryPickedData(Moq.It.IsAny<IEnumerable<Thing>>()), Times.Never);
            
            propertyInfo?.SetValue(this.viewModel, false, null);
            await this.viewModel.RunCherryPickAsync(new[] { (engineeringModelId, iterationId)});
            this.needCherryPickedData.Verify(x => x.ProcessCherryPickedData(Moq.It.IsAny<IEnumerable<Thing>>()), Times.Once);
        }
    }
}
