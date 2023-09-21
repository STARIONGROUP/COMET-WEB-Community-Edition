// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomDataServiceTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.Tests.Services.Interoperability
{
    using CDP4Common.ReportingData;

    using COMETwebapp.Components.BookEditor;
    using COMETwebapp.Services.Interoperability;

    using Microsoft.JSInterop;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DomDataServiceTestFixture
    {
        private Mock<IDomDataService> service;

        [SetUp]
        public void Setup()
        {
            this.service = new Mock<IDomDataService>();
            
            this.service.Setup(x => x.GetElementSizeAndPosition(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new float[] { 1, 2, 3, 4 });
        }

        [Test]
        public async Task VerifyMethods()
        {
            var dotnet = DotNetObjectReference.Create(new BookEditorColumn<Book>());

            Assert.Multiple(() =>
            {
                Assert.That(() => this.service.Object.LoadDotNetHelper(dotnet), Throws.Nothing);
                Assert.That(async () => await this.service.Object.GetElementSizeAndPosition(0, "node", true), Throws.Nothing);
            });

            var sizeAndPosition = await this.service.Object.GetElementSizeAndPosition(0, "node", true);

            Assert.Multiple(() =>
            {
                Assert.That(sizeAndPosition, Is.EquivalentTo(new float[] { 1, 2, 3, 4 }));
                Assert.That(() => this.service.Object.SubscribeToResizeEvent("resize"), Throws.Nothing);
            });
        }
    }
}


