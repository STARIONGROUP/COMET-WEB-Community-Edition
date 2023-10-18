// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CherryPickRunnerTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Tests.Utilities.CherryPick
{
    using NUnit.Framework;

    using CDP4Common.SiteDirectoryData;

    using Moq;

    [TestFixture]
    public class CherryPickRunnerTestFixture
    {
        private Common.Utilities.CherryPick.CherryPickRunner viewModel;
        private Mock<Common.Services.SessionManagement.ISessionService> sessionService;
        private Mock<Common.Utilities.CherryPick.INeedCherryPickedData> needCherryPickedData;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<Common.Services.SessionManagement.ISessionService>();
            this.needCherryPickedData = new Mock<Common.Utilities.CherryPick.INeedCherryPickedData>();
            this.viewModel = new Common.Utilities.CherryPick.CherryPickRunner(this.sessionService.Object);
        }

        [Test]
        public async Task VerifyProperties()
        {
            Assert.That(this.viewModel.IsCherryPicking, Is.False);
            this.viewModel.InitializeProperties(new List<Common.Utilities.CherryPick.INeedCherryPickedData> { this.needCherryPickedData.Object });

            this.sessionService.Setup(x => x.Session.RetrieveSiteDirectory()).Returns(new SiteDirectory());

            var propertyInfo = typeof(Common.Utilities.CherryPick.CherryPickRunner).GetProperty("IsCherryPicking", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            propertyInfo?.SetValue(this.viewModel, true, null);
            await this.viewModel.RunCherryPick();
            this.needCherryPickedData.Verify(x => x.ProcessCherryPickedData(Moq.It.IsAny<IEnumerable<IEnumerable<CDP4Common.DTO.Thing>>>()), Times.Never);

            propertyInfo?.SetValue(this.viewModel, false, null);
            await this.viewModel.RunCherryPick();
            this.needCherryPickedData.Verify(x => x.ProcessCherryPickedData(Moq.It.IsAny<IEnumerable<IEnumerable<CDP4Common.DTO.Thing>>>()), Times.Once);
        }
    }
}
