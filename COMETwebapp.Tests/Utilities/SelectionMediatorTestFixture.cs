// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectionMediatorTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Utilities
{
    using System;
    using System.Collections.Concurrent;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMETwebapp.Model;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Shared;
    using COMETwebapp.ViewModels.Components.Viewer;
    
    using NUnit.Framework;

    /// <summary>
    /// Test fixture for <see cref="SelectionMediator" />.
    /// </summary>
    [TestFixture]
    public class SelectionMediatorTestFixture
    {
        private SelectionMediator selectionMediator;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri;

        /// <summary>
        /// Set up the test run.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.selectionMediator = new SelectionMediator();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.uri = new Uri("http://test.com");
        }

        /// <summary>
        /// Verifies the RaiseOnTreeSelectionChanged method.
        /// </summary>
        [Test]
        public void VerifyRaiseTreeSelection()
        {
            var elementDef = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            var sceneObject = SceneObject.Create(elementDef, null, []);
            
            var treeNode = new ViewerNodeViewModel(sceneObject)
            {
                SelectionMediator = this.selectionMediator
            };

            IBaseNodeViewModel result = null;
            this.selectionMediator.OnTreeSelectionChanged += node => { result = node; };

            this.selectionMediator.RaiseOnTreeSelectionChanged(treeNode);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(treeNode));
                Assert.That(this.selectionMediator.SelectedSceneObject, Is.EqualTo(sceneObject));
            }

            // Test case when SelectedSceneObject has changes
            this.selectionMediator.SceneObjectHasChanges = true;
            var elementDef2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            var sceneObject2 = SceneObject.Create(elementDef2, null, []);
            
            var treeNode2 = new ViewerNodeViewModel(sceneObject2)
            {
                SelectionMediator = this.selectionMediator
            };

            this.selectionMediator.RaiseOnTreeSelectionChanged(treeNode2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.selectionMediator.SceneObjectHasChanges, Is.False);
                Assert.That(this.selectionMediator.SelectedSceneObject, Is.EqualTo(sceneObject2));
            }
        }

        /// <summary>
        /// Verifies the RaiseOnModelSelectionChanged method.
        /// </summary>
        [Test]
        public void VerifyRaiseModelSelection()
        {
            var elementDef = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            var sceneObject = SceneObject.Create(elementDef, null, []);
            SceneObject result = null;
            this.selectionMediator.OnModelSelectionChanged += so => { result = so; };

            this.selectionMediator.RaiseOnModelSelectionChanged(sceneObject);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(sceneObject));
                Assert.That(this.selectionMediator.SelectedSceneObject, Is.EqualTo(sceneObject));
            }

            // Test case when SelectedSceneObject has changes
            this.selectionMediator.SceneObjectHasChanges = true;
            var elementDef2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            var sceneObject2 = SceneObject.Create(elementDef2, null, []);

            this.selectionMediator.RaiseOnModelSelectionChanged(sceneObject2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.selectionMediator.SceneObjectHasChanges, Is.False);
                Assert.That(this.selectionMediator.SelectedSceneObject, Is.EqualTo(sceneObject2));
            }
        }

        /// <summary>
        /// Verifies the RaiseOnTreeVisibilityChanged method.
        /// </summary>
        [Test]
        public void VerifyRaiseTreeVisibility()
        {
            var treeNode = new ViewerNodeViewModel(new SceneObject(null))
            {
                SelectionMediator = this.selectionMediator
            };

            IBaseNodeViewModel result = null;
            this.selectionMediator.OnTreeVisibilityChanged += node => { result = node; };

            this.selectionMediator.RaiseOnTreeVisibilityChanged(treeNode);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(treeNode));
            }
        }

        /// <summary>
        /// Verifies the RaiseOnParameterChanged method.
        /// </summary>
        [Test]
        public void VerifyRaiseOnParameterChanged()
        {
            var eventRaised = false;
            this.selectionMediator.OnParameterChanged += () => { eventRaised = true; };

            this.selectionMediator.RaiseOnParameterChanged();

            Assert.That(eventRaised, Is.True);
        }

        /// <summary>
        /// Verifies the RaiseOnParameterSubmitted method.
        /// </summary>
        [Test]
        public void VerifyRaiseOnParameterSubmitted()
        {
            var eventRaised = false;
            this.selectionMediator.OnParameterSubmitted += () => { eventRaised = true; };

            this.selectionMediator.RaiseOnParameterSubmitted();

            Assert.That(eventRaised, Is.True);
        }
    }
}
