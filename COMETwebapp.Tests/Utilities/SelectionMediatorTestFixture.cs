﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectionMediatorTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>

namespace COMETwebapp.Tests.Utilities
{
    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Shared;
    using COMETwebapp.ViewModels.Components.Viewer;
    
    using NUnit.Framework;

    [TestFixture]
    public class SelectionMediatorTestFixture
    {
        private SelectionMediator selectionMediator;

        [SetUp]
        public void SetUp()
        {
            this.selectionMediator = new SelectionMediator();
        }

        [Test]
        public void VerifyRaiseTreeSelection()
        {
            var treeNode = new ViewerNodeViewModel(new SceneObject(null));
            treeNode.SelectionMediator = this.selectionMediator;
            IBaseNodeViewModel result = null;
            this.selectionMediator.OnTreeSelectionChanged += (node) => { result = node; };

            this.selectionMediator.RaiseOnTreeSelectionChanged(treeNode);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(treeNode));
        }

        [Test]
        public void VerifyRaiseModelSelection()
        {
            var sceneObject = new SceneObject(null);
            SceneObject result = null;
            this.selectionMediator.OnModelSelectionChanged += (so) => { result = so;  };

            this.selectionMediator.RaiseOnModelSelectionChanged(sceneObject);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(sceneObject));
        }

        [Test]
        public void VerifyRaiseTreeVisibility()
        {
            var treeNode = new ViewerNodeViewModel(new SceneObject(null));
            treeNode.SelectionMediator = this.selectionMediator;
            IBaseNodeViewModel result = null;
            this.selectionMediator.OnTreeVisibilityChanged += (node) => { result = node; };

            this.selectionMediator.RaiseOnTreeVisibilityChanged(treeNode);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(treeNode));
        }
    }
}
