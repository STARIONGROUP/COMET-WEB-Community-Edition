// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionServiceExtensionsTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SessionServiceExtensionsTestFixture
    {
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void Setup()
        {
            var session = new Mock<ISession>();
            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
        }

        [Test]
        public void VerifyAddParameter()
        {
            var model = new EngineeringModel();
            var iterationSetup = new IterationSetup();

            var iteration = new Iteration
            {
                IterationSetup = iterationSetup,
                Container = model
            };

            model.Iteration.Add(iteration);

            var elementDefinition = new ElementDefinition();
            iteration.Element.Add(elementDefinition);

            var textParameterType = new TextParameterType();

            var doe = new DomainOfExpertise
            {
                Name = "doe",
                ShortName = "doe"
            };

            Assert.Multiple(() =>
            {
                Assert.That(() => this.sessionService.Object.AddParameter(null, null, null, null, null), Throws.ArgumentNullException);
                Assert.That(() => this.sessionService.Object.AddParameter(elementDefinition, null, null, null, null), Throws.ArgumentNullException);
                Assert.That(() => this.sessionService.Object.AddParameter(elementDefinition, null, textParameterType, null, null), Throws.ArgumentNullException);
                Assert.That(() => this.sessionService.Object.AddParameter(elementDefinition, null, textParameterType, null, doe), Throws.Nothing);
            });
        }
    }
}
