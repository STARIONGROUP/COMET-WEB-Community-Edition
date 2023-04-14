// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscriptionServiceTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Services.SubscriptionService
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
   
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.NotificationService;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Model;
    using COMETwebapp.Services.SubscriptionService;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionServiceTestFixture
    {
        private SubscriptionService subscriptionService;
        private Mock<ISessionService> sessionService;
        private SourceList<Iteration> openIterations;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.openIterations = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.openIterations);
            this.subscriptionService = new SubscriptionService(this.sessionService.Object, new Mock<INotificationService>().Object);
        }

        [Test]
        public void VerifyTrackedSubscriptionsAndUpdateCount()
        {
            Assert.That(this.subscriptionService.TrackedSubscriptions, Is.Empty);

            var thermalOwner = new DomainOfExpertise()
            {
                Iid   = Guid.NewGuid(),
                Name = "THERMAL"
            };

            var systemOwner = new DomainOfExpertise()
            {
                Iid = Guid.NewGuid(),
                Name = "SYSTEM"
            };

            var iteration = new Iteration()
            {
                Iid = Guid.NewGuid()
            };

            var massParameterType = new SimpleQuantityKind()
            {
                Iid = Guid.NewGuid(),
                Name = "mass"
            };

            var costParameterType = new SimpleQuantityKind()
            {
                Iid = Guid.NewGuid(),
                Name = "cost"
            };

            var topElement = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "TopElement",
                Owner = systemOwner,
                Parameter = 
                {
                    new Parameter()
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = massParameterType,
                        Owner = systemOwner,
                        ValueSet = { new ParameterValueSet(){ Iid = Guid.NewGuid() } }, 
                        ParameterSubscription = { new ParameterSubscription(){Iid = Guid.NewGuid(), Owner = thermalOwner}}
                    }
                }
            };

            var accelerometerBox = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Owner = thermalOwner,
                Name = "ACCELEROMETER BOX",
                Parameter =
                {
                    new Parameter()
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = massParameterType,
                        Owner = thermalOwner,
                        ValueSet = { new ParameterValueSet() { Iid = Guid.NewGuid()} },
                        ParameterSubscription = { new ParameterSubscription() { Iid = Guid.NewGuid(), Owner = systemOwner } }
                    },
                    new Parameter()
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = costParameterType,
                        Owner = thermalOwner,
                        ValueSet = { new ParameterValueSet() { Iid = Guid.NewGuid() } }
                    }
                }
            };

            var subscribed = new ParameterOverrideValueSet { Iid = Guid.NewGuid() };
            var valueSet = new ParameterSubscriptionValueSet() { Iid = Guid.NewGuid() , SubscribedValueSet = subscribed };

            var accelerometerUsage = new ElementUsage()
            {
                Iid = Guid.NewGuid(),
                Owner = thermalOwner,
                ElementDefinition = accelerometerBox,
                ParameterOverride =
                {
                    new ParameterOverride()
                    {
                        Iid = Guid.NewGuid(),
                        Parameter = accelerometerBox.Parameter[1],
                        Owner = thermalOwner,
                        ValueSet = {subscribed},
                        ParameterSubscription = {  new ParameterSubscription() { Iid = Guid.NewGuid(), Owner = systemOwner, ValueSet = { valueSet }} }
                    }
                }
            };

            topElement.ContainedElement.Add(accelerometerUsage);
            iteration.Element.Add(topElement);
            iteration.Element.Add(accelerometerBox);
            iteration.TopElement = topElement;
            this.sessionService.Setup(x => x.GetDomainOfExpertise(iteration)).Returns(thermalOwner);
            this.openIterations.Add(iteration);
            
            Assert.Multiple(() =>
            {
                Assert.That(this.subscriptionService.TrackedSubscriptions, Is.Not.Empty);
                Assert.That(this.subscriptionService.TrackedSubscriptions[iteration.Iid], Has.Count.EqualTo(1));
            });

            this.sessionService.Setup(x => x.GetDomainOfExpertise(iteration)).Returns(systemOwner);
            CDPMessageBus.Current.SendMessage(new DomainChangedEvent(iteration, systemOwner));

            Assert.Multiple(() =>
            {
                Assert.That(this.subscriptionService.TrackedSubscriptions, Is.Not.Empty);
                Assert.That(this.subscriptionService.TrackedSubscriptions[iteration.Iid], Has.Count.EqualTo(2));
                Assert.That(this.subscriptionService.SubscriptionUpdateCount, Is.EqualTo(0));
            });

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.sessionService.Object.Session, SessionStatus.EndUpdate));
            Assert.That(this.subscriptionService.SubscriptionUpdateCount, Is.EqualTo(0));

            var nameProperty = typeof(ParameterValueSet).GetProperty(nameof(ParameterValueSet.RevisionNumber))!;
            nameProperty.SetValue(subscribed, 4);
            subscribed.Revisions[0] = new ParameterValueSet() { Published = new ValueArray<string>(new List<string>(){"-"})};
            subscribed.Published = new ValueArray<string>(new List<string>(){"45"});

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.sessionService.Object.Session, SessionStatus.EndUpdate));

            Assert.Multiple(() =>
            {
                Assert.That(this.subscriptionService.SubscriptionUpdateCount, Is.EqualTo(1));
                Assert.That(() => CDPMessageBus.Current.SendMessage(SessionStateKind.Refreshing), Throws.Nothing);

                Assert.That(() =>this.subscriptionService.TrackedSubscriptions[iteration.Iid][0].QueryChangedValueSet(new TrackedParameterSubscription(new ParameterSubscription())),
                    Throws.ArgumentException);
            });

            this.openIterations.Clear();

            Assert.That(this.subscriptionService.TrackedSubscriptions, Is.Empty);
        }
    }
}
