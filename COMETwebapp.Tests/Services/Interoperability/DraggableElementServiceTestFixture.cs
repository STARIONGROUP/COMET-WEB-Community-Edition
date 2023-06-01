// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DraggableElementServiceTestFixture.cs" company="RHEA System S.A.">
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
    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.Services.Interoperability;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DraggableElementServiceTestFixture
    {
        private ElementReference canvasReference;
        private Mock<IDraggableElementService> draggableElementServiceMock;

        [SetUp]
        public void SetUp()
        {
            this.canvasReference = new ElementReference();
            this.draggableElementServiceMock = new Mock<IDraggableElementService>();
            this.draggableElementServiceMock.Setup(x => x.LoadDotNetHelper(It.IsAny<DotNetObjectReference<ElementDefinitionTable>>()));
            this.draggableElementServiceMock.Setup(x => x.InitDraggableGrids(It.IsAny<string>(), It.IsAny<string>()));
        }

        [Test]
        public void LoadDotNetHelper()
        {
            Assert.DoesNotThrow(() => this.draggableElementServiceMock.Object.LoadDotNetHelper(It.IsAny<DotNetObjectReference<ElementDefinitionTable>>()));
            this.draggableElementServiceMock.Verify(x => x.LoadDotNetHelper(It.IsAny<DotNetObjectReference<ElementDefinitionTable>>()), Times.Once());
        }

        [Test]
        public void InitDraggableGrids()
        {
            Assert.DoesNotThrow(() => this.draggableElementServiceMock.Object.InitDraggableGrids(It.IsAny<string>(), It.IsAny<string>()));
            this.draggableElementServiceMock.Verify(x => x.InitDraggableGrids(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }
    }
}
