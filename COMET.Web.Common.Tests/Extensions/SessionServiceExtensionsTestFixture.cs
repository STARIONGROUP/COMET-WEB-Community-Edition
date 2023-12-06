// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionServiceExtensionsTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.Extensions
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SessionServiceExtensionsTestFixture
    {
        private Mock<ISession> session;
        private ISessionService sessionService;

        [SetUp]
        public void Setup()
        {
            var logger = new Mock<ILogger<SessionService>>();

            this.session = new Mock<ISession>();
            this.sessionService = new SessionService(logger.Object)
            {
                Session = this.session.Object
            };
        }

        [Test]
        public void VerifyAddParameter()
        {
            var model = new EngineeringModel();
            var iterationSetup = new IterationSetup();
            var iteration = new Iteration()
            {
                IterationSetup = iterationSetup,
                Container = model
            };
            model.Iteration.Add(iteration);

            var elementDefinition = new ElementDefinition();
            iteration.Element.Add(elementDefinition);

            var textParameterType = new TextParameterType();

            var doe = new DomainOfExpertise()
            {
                Name = "doe",
                ShortName = "doe"
            };

            Assert.Multiple(() =>
            {
                Assert.That(() => this.sessionService.AddParameter(null, null, null, null, null), Throws.ArgumentNullException);
                Assert.That(() => this.sessionService.AddParameter(elementDefinition, null, null, null, null), Throws.ArgumentNullException);
                Assert.That(() => this.sessionService.AddParameter(elementDefinition, null, textParameterType, null, null), Throws.ArgumentNullException);
                Assert.That(() => this.sessionService.AddParameter(elementDefinition, null, textParameterType, null, doe), Throws.Nothing);
            });

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once);
        }
    }
}
