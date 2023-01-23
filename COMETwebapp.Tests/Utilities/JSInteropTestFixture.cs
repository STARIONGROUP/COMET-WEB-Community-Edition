// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JSInteropTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Tests.Utilities
{
    using System.Threading.Tasks;

    using COMETwebapp.Interoperability;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class JSInteropTestFixture
    {
        private IJSInterop JSInterop;
        private Mock<IJSInterop> jsInteropMock;

        [SetUp]
        public void SetUp()
        {
            jsInteropMock = new Mock<IJSInterop>();
            jsInteropMock.Setup(x => x.Invoke("voidMethod")).Verifiable();
            jsInteropMock.Setup(x => x.Invoke("method1")).Returns(() => Task.FromResult(true));
            jsInteropMock.Setup(x => x.Invoke("method2", It.IsAny<string>())).Returns(() => Task.FromResult(true));

            jsInteropMock.Setup(x => x.Invoke<bool>("method3")).Returns(() => Task.FromResult(true));
            jsInteropMock.Setup(x => x.Invoke<bool>("method4", It.IsAny<string>())).Returns(() => Task.FromResult(true));

            JSInterop = jsInteropMock.Object;
        }

        [Test]
        public void VerifyThatInvokeVoidMethodsCanBeCalled()
        {
            JSInterop.Invoke("voidMethod");
            jsInteropMock.Verify(x => x.Invoke("voidMethod"), Times.Once());

            var result1 = JSInterop.Invoke("method1");
            var result2 = JSInterop.Invoke("method2", "param");

            jsInteropMock.Verify(x => x.Invoke("method1"), Times.Once());
            jsInteropMock.Verify(x => x.Invoke("method2", It.IsAny<string>()), Times.Once());
        }

        [Test]
        public async Task VerifyThatInvokeMethodsCanBeCalled()
        {
            await JSInterop.Invoke("voidMethod");
            jsInteropMock.Verify(x => x.Invoke("voidMethod"), Times.Once());

            var result1 = await JSInterop.Invoke<bool>("method3");
            var result2 = await JSInterop.Invoke<bool>("method4", "param");

            jsInteropMock.Verify(x => x.Invoke<bool>("method3"), Times.Once());
            jsInteropMock.Verify(x => x.Invoke<bool>("method4", It.IsAny<string>()), Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(result1, Is.True);
                Assert.That(result2, Is.True);
            });
        }
    }
}
