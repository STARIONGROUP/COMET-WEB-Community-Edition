// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionDetailsRowViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.ViewModels.Components.SystemRepresentation.Rows
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using NUnit.Framework;

    [TestFixture]
    public class ElementDefinitionDetailsRowViewModelTestFixture
    {
        /// <summary>
        /// The currently logged-in <see cref="DomainOfExpertise" /> used to evaluate subscription state.
        /// </summary>
        private DomainOfExpertise currentDomain;

        /// <summary>
        /// A foreign <see cref="DomainOfExpertise" /> — owner of <see cref="parameter" />.
        /// </summary>
        private DomainOfExpertise foreignDomain;

        /// <summary>
        /// A <see cref="Parameter" /> owned by <see cref="foreignDomain" /> with one
        /// <see cref="ParameterValueSet" />, used by the row VM under test.
        /// </summary>
        private Parameter parameter;

        /// <summary>
        /// The <see cref="ElementDefinition" /> containing <see cref="parameter" />. Promoted to a field so
        /// override-aware tests can hang an <see cref="ElementUsage" /> off it.
        /// </summary>
        private ElementDefinition containingDefinition;

        [SetUp]
        public void SetUp()
        {
            this.currentDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.foreignDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "PWR", Name = "Power" };

            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };

            this.parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = this.foreignDomain,
                ParameterType = parameterType
            };

            this.parameter.ValueSet.Add(new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["1"]),
                Computed = new ValueArray<string>(["1"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"]),
                Published = new ValueArray<string>(["1"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            // Parameter.ModelCode() requires the parameter to live inside an ElementDefinition; satisfy that
            // contract once at fixture level so individual tests don't have to.
            this.containingDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Container",
                ShortName = "CONT",
                Owner = this.foreignDomain
            };

            this.containingDefinition.Parameter.Add(this.parameter);
        }

        /// <summary>
        /// Builds an <see cref="ElementUsage" /> typed by <see cref="containingDefinition" /> so it can host
        /// a <see cref="ParameterOverride" /> on <see cref="parameter" />.
        /// </summary>
        /// <param name="usageOwner">The <see cref="DomainOfExpertise" /> that owns the usage.</param>
        /// <returns>The built <see cref="ElementUsage" />.</returns>
        private ElementUsage BuildHostUsage(DomainOfExpertise usageOwner)
        {
            return new ElementUsage
            {
                Iid = Guid.NewGuid(),
                Name = "Usage",
                ShortName = "USG",
                Owner = usageOwner,
                ElementDefinition = this.containingDefinition
            };
        }

        [Test]
        public void VerifyCanSubscribeWhenForeignAndUnsubscribed()
        {
            var row = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.Multiple(() =>
            {
                Assert.That(row.IsOwnedByCurrentDomain, Is.False);
                Assert.That(row.HasCurrentDomainSubscription, Is.False);
                Assert.That(row.CurrentDomainSubscription, Is.Null);
                Assert.That(row.CanSubscribe, Is.True);
                Assert.That(row.Owner, Is.EqualTo(this.foreignDomain.ShortName),
                    "Owner pill must show only the parameter owner when no subscription exists.");
            });
        }

        [Test]
        public void VerifyCannotSubscribeWhenOwnedByCurrentDomain()
        {
            this.parameter.Owner = this.currentDomain;

            var row = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.Multiple(() =>
            {
                Assert.That(row.IsOwnedByCurrentDomain, Is.True);
                Assert.That(row.CanSubscribe, Is.False);
            });
        }

        [Test]
        public void VerifySubscriptionStateWhenAlreadySubscribed()
        {
            var sourceValueSet = this.parameter.ValueSet[0];

            var subscription = new ParameterSubscription
            {
                Iid = Guid.NewGuid(),
                Owner = this.currentDomain
            };

            subscription.ValueSet.Add(new ParameterSubscriptionValueSet
            {
                Iid = Guid.NewGuid(),
                SubscribedValueSet = sourceValueSet,
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["42"])
            });

            this.parameter.ParameterSubscription.Add(subscription);

            var row = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.Multiple(() =>
            {
                Assert.That(row.HasCurrentDomainSubscription, Is.True);
                Assert.That(row.CurrentDomainSubscription, Is.SameAs(subscription));
                Assert.That(row.CanSubscribe, Is.False);
                Assert.That(row.Owner, Is.EqualTo(this.foreignDomain.ShortName),
                    "Owner pill must stay compact even when subscribed; the subscription state is conveyed by the unsubscribe button, not extra text in the Owner pill (longer text wraps and breaks the fixed-size CardView slot).");
                Assert.That(row.ActualValue, Does.Contain("42"),
                    "ActualValue must come from the subscription value set when the current domain is subscribed.");
            });
        }

        [Test]
        public void VerifyNullCurrentDomainKeepsBackwardsCompatibleDisplay()
        {
            var row = new ElementDefinitionDetailsRowViewModel(this.parameter);

            Assert.Multiple(() =>
            {
                Assert.That(row.IsOwnedByCurrentDomain, Is.False);
                Assert.That(row.HasCurrentDomainSubscription, Is.False);
                Assert.That(row.CanSubscribe, Is.False,
                    "Without a known current domain the row must not invite a subscribe action.");
                Assert.That(row.Owner, Is.EqualTo(this.foreignDomain.ShortName));
                Assert.That(row.HostElementUsage, Is.Null);
                Assert.That(row.HasOverride, Is.False);
                Assert.That(row.CanCreateOverride, Is.False);
                Assert.That(row.CurrentOverride, Is.Null);
            });
        }

        [Test]
        public void VerifyTwoArgCtorYieldsAllOverrideFlagsFalse()
        {
            var row = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.Multiple(() =>
            {
                Assert.That(row.HostElementUsage, Is.Null);
                Assert.That(row.HasOverride, Is.False);
                Assert.That(row.CanCreateOverride, Is.False,
                    "Without a host ElementUsage the row must not invite an override action.");
                Assert.That(row.CurrentOverride, Is.Null);
            });
        }

        [Test]
        public void VerifyCanCreateOverrideWhenOwnedByCurrentDomain()
        {
            this.parameter.Owner = this.currentDomain;
            var hostUsage = this.BuildHostUsage(this.currentDomain);

            var row = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain, hostUsage);

            Assert.Multiple(() =>
            {
                Assert.That(row.HostElementUsage, Is.SameAs(hostUsage));
                Assert.That(row.HasOverride, Is.False);
                Assert.That(row.CanCreateOverride, Is.True,
                    "When the current domain owns the parameter, override on the host usage is permitted.");
                Assert.That(row.CurrentOverride, Is.Null);
            });
        }

        [Test]
        public void VerifyCanCreateOverrideWhenAllowDifferentOwnerOfOverrideIsTrue()
        {
            this.parameter.AllowDifferentOwnerOfOverride = true;
            var hostUsage = this.BuildHostUsage(this.currentDomain);

            var row = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain, hostUsage);

            Assert.Multiple(() =>
            {
                Assert.That(row.IsOwnedByCurrentDomain, Is.False);
                Assert.That(row.CanCreateOverride, Is.True,
                    "Foreign-owned parameters with AllowDifferentOwnerOfOverride may still be overridden by another domain.");
            });
        }

        [Test]
        public void VerifyCannotCreateOverrideWhenForeignAndAllowDifferentOwnerIsFalse()
        {
            this.parameter.AllowDifferentOwnerOfOverride = false;
            var hostUsage = this.BuildHostUsage(this.currentDomain);

            var row = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain, hostUsage);

            Assert.Multiple(() =>
            {
                Assert.That(row.IsOwnedByCurrentDomain, Is.False);
                Assert.That(row.CanCreateOverride, Is.False,
                    "Foreign-owned parameters that don't allow different owner of override must not invite override creation.");
            });
        }

        [Test]
        public void VerifyOverrideStateWhenAlreadyOverridden()
        {
            this.parameter.Owner = this.currentDomain;

            var hostUsage = this.BuildHostUsage(this.currentDomain);

            var sourceValueSet = this.parameter.ValueSet[0];

            var parameterOverride = new ParameterOverride
            {
                Iid = Guid.NewGuid(),
                Parameter = this.parameter,
                Owner = this.currentDomain
            };

            parameterOverride.ValueSet.Add(new ParameterOverrideValueSet
            {
                Iid = Guid.NewGuid(),
                ParameterValueSet = sourceValueSet,
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["99"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"])
            });

            hostUsage.ParameterOverride.Add(parameterOverride);

            var row = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain, hostUsage);

            Assert.Multiple(() =>
            {
                Assert.That(row.HasOverride, Is.True);
                Assert.That(row.CurrentOverride, Is.SameAs(parameterOverride));
                Assert.That(row.CanCreateOverride, Is.False,
                    "When an override already exists, the row must not invite another create action.");
                Assert.That(row.ActualValue, Does.Contain("99"),
                    "ActualValue must come from the override value set when the host usage already has an override.");
                Assert.That(row.Owner, Is.EqualTo(this.currentDomain.ShortName),
                    "Owner pill must reflect the override owner when an override exists.");
            });
        }
    }
}
