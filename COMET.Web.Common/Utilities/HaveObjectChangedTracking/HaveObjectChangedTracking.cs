// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HaveObjectChangedTracking.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Utilities.HaveObjectChangedTracking
{
    using System.Reactive.Linq;

    using CDP4Common.CommonData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Utilities.DisposableObject;

    /// <summary>
    /// Base class for any class that needs to track <see cref="ObjectChangedEvent" /> for one or multiple types
    /// </summary>
    public abstract class HaveObjectChangedTracking : DisposableObject, IHaveObjectChangedTracking
    {
        /// <summary>
        /// A collection of added <see cref="Thing" />s
        /// </summary>
        protected readonly List<Thing> AddedThings = new();

        /// <summary>
        /// A collection of deleted <see cref="Thing" />s
        /// </summary>
        protected readonly List<Thing> DeletedThings = new();

        /// <summary>
        /// A collection of updated <see cref="Thing" />s
        /// </summary>
        protected readonly List<Thing> UpdatedThings = new();

        /// <summary>
        /// Initializes a new instance of <see cref="HaveObjectChangedTracking" />
        /// </summary>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        protected HaveObjectChangedTracking(ICDPMessageBus messageBus)
        {
            this.MessageBus = messageBus;
        }

        /// <summary>
        /// Gets the injected <see cref="ICDPMessageBus" />
        /// </summary>
        protected ICDPMessageBus MessageBus { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}" /> of added <see cref="Thing" />s
        /// </summary>
        public IReadOnlyCollection<Thing> GetAddedThings()
        {
            return this.AddedThings;
        }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}" /> of deleted <see cref="Thing" />s
        /// </summary>
        public IReadOnlyCollection<Thing> GetDeletedThings()
        {
            return this.DeletedThings;
        }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}" /> of updated <see cref="Thing" />s
        /// </summary>
        public IReadOnlyCollection<Thing> GetUpdatedThings()
        {
            return this.UpdatedThings;
        }

        /// <summary>
        /// Clears all recorded changes
        /// </summary>
        public void ClearRecordedChanges()
        {
            this.DeletedThings.Clear();
            this.UpdatedThings.Clear();
            this.AddedThings.Clear();
        }

        /// <summary>
        /// Initializes all <see cref="ObjectChangedEvent" /> subscription
        /// </summary>
        /// <param name="typesOfInterest">
        /// A collection of <see cref="Type" /> to create <see cref="ObjectChangedEvent" />
        /// subscriptions
        /// </param>
        protected void InitializeSubscriptions(IEnumerable<Type> typesOfInterest)
        {
            var observables = typesOfInterest.Select(objectChangedTypeTarget => this.MessageBus.Listen<ObjectChangedEvent>(objectChangedTypeTarget)).ToList();
            this.Disposables.Add(observables.Merge().Subscribe(this.RecordChange));
        }

        /// <summary>
        /// The logic used to check if a change should be recorded an <see cref="ObjectChangedEvent" />
        /// </summary>
        /// <param name="objectChangedEvent">The <see cref="ObjectChangedEvent" /></param>
        /// <returns>true if the change should be recorded, false otherwise</returns>
        protected virtual bool ShouldRecordChange(ObjectChangedEvent objectChangedEvent)
        {
            return true;
        }

        /// <summary>
        /// Records an <see cref="ObjectChangedEvent" />
        /// </summary>
        /// <param name="objectChangedEvent">The <see cref="ObjectChangedEvent" /></param>
        private void RecordChange(ObjectChangedEvent objectChangedEvent)
        {
            if (!this.ShouldRecordChange(objectChangedEvent))
            {
                return;
            }

            switch (objectChangedEvent.EventKind)
            {
                case EventKind.Added:
                    this.AddedThings.Add(objectChangedEvent.ChangedThing);
                    break;
                case EventKind.Removed:
                    this.DeletedThings.Add(objectChangedEvent.ChangedThing);
                    break;
                case EventKind.Updated:
                    this.UpdatedThings.Add(objectChangedEvent.ChangedThing);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(objectChangedEvent), "Unrecognised value EventKind value");
            }
        }
    }
}
