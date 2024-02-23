// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomDataServiceTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Services.Interoperability
{
    using Bunit;

    using CDP4Common.ReportingData;

    using COMETwebapp.Components.BookEditor;
    using COMETwebapp.Services.Interoperability;

    using Microsoft.JSInterop;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class DomDataServiceTestFixture
    {
        private TestContext context;
        private DomDataService service;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            var jsRuntime = new Mock<IJSRuntime>();

            this.context.JSInterop.SetupVoid("setDotNetHelper");
            this.context.JSInterop.SetupVoid("SubscribeToResizeEvent");
            this.context.JSInterop.Setup<float[]>("GetElementSizeAndPosition").SetResult(new float[] { 1, 2, 3, 4 });
            
            this.service = new DomDataService(jsRuntime.Object);
        }

        [Test]
        public void VerifyMethods()
        {
            var dotnet = DotNetObjectReference.Create(new BookEditorColumn<Book>());

            Assert.Multiple(() =>
            {
                Assert.That(() => this.service.LoadDotNetHelper(dotnet), Throws.Nothing);
                Assert.That(async () => await this.service.GetElementSizeAndPosition(0, "node", true), Throws.Nothing);
                Assert.That(() => this.service.SubscribeToResizeEvent("resize"), Throws.Nothing);
            });
        }
    }
}


