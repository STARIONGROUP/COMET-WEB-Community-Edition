// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HaveObjectChangedTrackingTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Utilities.HaveObjectChangedTracking;

    using NUnit.Framework;

    [TestFixture]
    public class HaveObjectChangedTrackingTestFixture
    {
        private class ObjectChangedTracking: HaveObjectChangedTracking
        {
            public IReadOnlyList<Thing> AddedThingsReadOnlyList => this.AddedThings.AsReadOnly();

            public IReadOnlyList<Thing> DeletedThingsReadOnlyList => this.DeletedThings.AsReadOnly();

            public IReadOnlyList<Thing> UpdatedThingsReadOnlyList => this.UpdatedThings.AsReadOnly();

            public void Initialize(IEnumerable<Type> types)
            {
                this.InitializeSubscriptions(types);
            }

            public void Clear()
            {
                this.ClearRecordedChanges();
            }
        }

        [Test]
        public void VerifyCollectionsAfterObjectChangeEvent()
        {
            var objectChangedTracking = new ObjectChangedTracking();
            var elementDefinition = new ElementDefinition();

            CDPMessageBus.Current.SendObjectChangeEvent(elementDefinition, EventKind.Added);

            Assert.Multiple(() =>
            {
                Assert.That(objectChangedTracking.AddedThingsReadOnlyList, Is.Empty);
                Assert.That(objectChangedTracking.DeletedThingsReadOnlyList, Is.Empty);
                Assert.That(objectChangedTracking.UpdatedThingsReadOnlyList, Is.Empty);
            });

            objectChangedTracking.Initialize(new List<Type> { typeof(ElementBase)});
            CDPMessageBus.Current.SendObjectChangeEvent(elementDefinition, EventKind.Added);

            Assert.Multiple(() =>
            {
                Assert.That(objectChangedTracking.AddedThingsReadOnlyList, Has.Count.EqualTo(1));
                Assert.That(objectChangedTracking.DeletedThingsReadOnlyList, Is.Empty);
                Assert.That(objectChangedTracking.UpdatedThingsReadOnlyList, Is.Empty);
            });

            CDPMessageBus.Current.SendObjectChangeEvent(elementDefinition, EventKind.Updated);

            Assert.Multiple(() =>
            {
                Assert.That(objectChangedTracking.AddedThingsReadOnlyList, Has.Count.EqualTo(1));
                Assert.That(objectChangedTracking.DeletedThingsReadOnlyList, Is.Empty);
                Assert.That(objectChangedTracking.UpdatedThingsReadOnlyList, Has.Count.EqualTo(1));
            });

            CDPMessageBus.Current.SendObjectChangeEvent(elementDefinition, EventKind.Removed);

            Assert.Multiple(() =>
            {
                Assert.That(objectChangedTracking.AddedThingsReadOnlyList, Has.Count.EqualTo(1));
                Assert.That(objectChangedTracking.DeletedThingsReadOnlyList, Has.Count.EqualTo(1));
                Assert.That(objectChangedTracking.UpdatedThingsReadOnlyList, Has.Count.EqualTo(1));
            });

            objectChangedTracking.Clear();

            Assert.Multiple(() =>
            {
                Assert.That(objectChangedTracking.AddedThingsReadOnlyList, Is.Empty);
                Assert.That(objectChangedTracking.DeletedThingsReadOnlyList, Is.Empty);
                Assert.That(objectChangedTracking.UpdatedThingsReadOnlyList, Is.Empty);
            });

            CDPMessageBus.Current.SendObjectChangeEvent(new Parameter(), EventKind.Removed);

            Assert.Multiple(() =>
            {
                Assert.That(objectChangedTracking.AddedThingsReadOnlyList, Is.Empty);
                Assert.That(objectChangedTracking.DeletedThingsReadOnlyList, Is.Empty);
                Assert.That(objectChangedTracking.UpdatedThingsReadOnlyList, Is.Empty);
            });
        }
    }
}
