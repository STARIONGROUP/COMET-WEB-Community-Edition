// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemNodeTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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

namespace COMETwebapp.Tests.Model
{
    using COMETwebapp.Model;
    using NUnit.Framework;
    using System.Linq;


    [TestFixture]
    public class SystemNodeTestFixture
    {
        private SystemNode rootNode;
        private SystemNode node1;
        private SystemNode node2;
        private SystemNode node3;

        [SetUp]
        public void SetUp()
        {
            this.rootNode = new SystemNode("Root");
            node1 = new SystemNode("first");
            node2 = new SystemNode("second");
            node3 = new SystemNode("third");

            this.rootNode.AddChild(node1);
            this.rootNode.AddChild(node2);
            node2.AddChild(node3);
        }

        [Test]
        public void VerifyGetParentAndChildren()
        {
            var result1 = this.rootNode.GetChildren();
            var result2 = this.node3.GetParentNode();

            Assert.Multiple(() =>
            {
                Assert.That(result1.Count, Is.EqualTo(2));
                Assert.That(result1.Contains(node1));
                Assert.That(result1.Contains(node2));
                Assert.That(result2, Is.EqualTo(node2));
            });
        }
    }
}
