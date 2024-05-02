// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HaveObjectChangedTrackingTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Utilities.HaveObjectChangedTracking
{
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Utilities.HaveObjectChangedTracking;

    using NUnit.Framework;

    [TestFixture]
    public class HaveObjectChangedTrackingTestFixture
    {
        private CDPMessageBus messageBus;
        private ObjectChangedTracking objectChangedTracking;

        private class ObjectChangedTracking : HaveObjectChangedTracking
        {
            /// <summary>
            /// Initializes a new instance of <see cref="HaveObjectChangedTracking" />
            /// </summary>
            /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
            public ObjectChangedTracking(ICDPMessageBus messageBus) : base(messageBus)
            {
            }

            public void Initialize(IEnumerable<Type> types)
            {
                this.InitializeSubscriptions(types);
            }

            public void Clear()
            {
                this.ClearRecordedChanges();
            }
        }

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.objectChangedTracking = new ObjectChangedTracking(this.messageBus);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyCollectionsAfterObjectChangeEvent()
        {
            var elementDefinition = new ElementDefinition();

            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Added);

            Assert.Multiple(() =>
            {
                Assert.That(this.objectChangedTracking.GetAddedThings(), Is.Empty);
                Assert.That(this.objectChangedTracking.GetDeletedThings(), Is.Empty);
                Assert.That(this.objectChangedTracking.GetUpdatedThings(), Is.Empty);
            });

            this.objectChangedTracking.Initialize(new List<Type> { typeof(ElementBase) });
            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Added);

            Assert.Multiple(() =>
            {
                Assert.That(this.objectChangedTracking.GetAddedThings(), Has.Count.EqualTo(1));
                Assert.That(this.objectChangedTracking.GetDeletedThings(), Is.Empty);
                Assert.That(this.objectChangedTracking.GetUpdatedThings(), Is.Empty);
            });

            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Updated);

            Assert.Multiple(() =>
            {
                Assert.That(this.objectChangedTracking.GetAddedThings(), Has.Count.EqualTo(1));
                Assert.That(this.objectChangedTracking.GetDeletedThings(), Is.Empty);
                Assert.That(this.objectChangedTracking.GetUpdatedThings(), Has.Count.EqualTo(1));
            });

            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Removed);

            Assert.Multiple(() =>
            {
                Assert.That(this.objectChangedTracking.GetAddedThings(), Has.Count.EqualTo(1));
                Assert.That(this.objectChangedTracking.GetDeletedThings(), Has.Count.EqualTo(1));
                Assert.That(this.objectChangedTracking.GetUpdatedThings(), Has.Count.EqualTo(1));
            });

            this.objectChangedTracking.Clear();

            Assert.Multiple(() =>
            {
                Assert.That(this.objectChangedTracking.GetAddedThings(), Is.Empty);
                Assert.That(this.objectChangedTracking.GetDeletedThings(), Is.Empty);
                Assert.That(this.objectChangedTracking.GetUpdatedThings(), Is.Empty);
            });

            this.messageBus.SendObjectChangeEvent(new Parameter(), EventKind.Removed);

            Assert.Multiple(() =>
            {
                Assert.That(this.objectChangedTracking.GetAddedThings(), Is.Empty);
                Assert.That(this.objectChangedTracking.GetDeletedThings(), Is.Empty);
                Assert.That(this.objectChangedTracking.GetUpdatedThings(), Is.Empty);
            });
        }
    }
}
