// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DraggableElementServiceTestFixture.cs" company="RHEA System S.A.">
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

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.Services.Interoperability;

    using Microsoft.JSInterop;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class DraggableElementServiceTestFixture
    {
        private TestContext context;
        private Mock<IJSRuntime> jsRuntimeMock;
        private DraggableElementService draggableElementService;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.jsRuntimeMock = new Mock<IJSRuntime>();

            this.draggableElementService = new DraggableElementService(jsRuntimeMock.Object);

            this.context.JSInterop.SetupVoid("setDotNetHelper");
            this.context.JSInterop.SetupVoid("initialize");
        }

        [Test]
        public void LoadDotNetHelper()
        {
            Assert.That(() => this.draggableElementService.LoadDotNetHelper(It.IsAny<DotNetObjectReference<ElementDefinitionTable>>()), Throws.Nothing);
        }

        [Test]
        public void InitDraggableGrids()
        {
            Assert.That(() => this.draggableElementService.InitDraggableGrids(It.IsAny<string>(), It.IsAny<string>()), Throws.Nothing);
        }
    }
}
