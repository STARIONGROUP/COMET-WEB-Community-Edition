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

        [Test]
        public void VerifyCardShowsCurrentDomainSubscriptionValueNotOwnerValue()
        {
            // The current domain (SYS) subscribes to the foreign-owned parameter with its own MANUAL value.
            var subscription = new ParameterSubscription { Iid = Guid.NewGuid(), Owner = this.currentDomain };

            subscription.ValueSet.Add(new ParameterSubscriptionValueSet
            {
                Iid = Guid.NewGuid(),
                SubscribedValueSet = this.parameter.ValueSet[0],
                Manual = new ValueArray<string>(["99"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            this.parameter.ParameterSubscription.Add(subscription);

            var row = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.Multiple(() =>
            {
                Assert.That(row.HasCurrentDomainSubscription, Is.True);
                Assert.That(row.ActualValue, Is.EqualTo("99"), "The card must show the subscriber's value, not the owner's.");
                Assert.That(row.SwitchValue, Is.EqualTo(nameof(ParameterSwitchKind.MANUAL)));
            });
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
        public void VerifyHasOtherDomainSubscriptions()
        {
            // No subscriptions → false
            var rowNoSubs = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.That(rowNoSubs.HasOtherDomainSubscriptions, Is.False,
                "A parameter with no subscriptions at all must report HasOtherDomainSubscriptions = false.");

            // Only a current-domain subscription → false (current domain is excluded by definition)
            var ownSubscription = new ParameterSubscription
            {
                Iid = Guid.NewGuid(),
                Owner = this.currentDomain
            };

            this.parameter.ParameterSubscription.Add(ownSubscription);

            var rowOwnSub = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.That(rowOwnSub.HasOtherDomainSubscriptions, Is.False,
                "A subscription owned by the current domain must not count as an 'other domain' subscription.");

            // Add a foreign-domain subscription → true
            var foreignSubscription = new ParameterSubscription
            {
                Iid = Guid.NewGuid(),
                Owner = this.foreignDomain
            };

            this.parameter.ParameterSubscription.Add(foreignSubscription);

            var rowForeignSub = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.That(rowForeignSub.HasOtherDomainSubscriptions, Is.True,
                "A subscription owned by a different domain must cause HasOtherDomainSubscriptions to be true.");
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

        [Test]
        public void VerifyOtherDomainSubscriptionOwners()
        {
            // No foreign subscriptions → empty string
            var rowNoSubs = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.That(rowNoSubs.OtherDomainSubscriptionOwners, Is.Empty,
                "OtherDomainSubscriptionOwners must be an empty string when the parameter has no foreign-domain subscriptions.");

            // Only a current-domain subscription → empty string (own domain excluded)
            var ownSubscription = new ParameterSubscription
            {
                Iid = Guid.NewGuid(),
                Owner = this.currentDomain
            };

            this.parameter.ParameterSubscription.Add(ownSubscription);

            var rowOwnSub = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.That(rowOwnSub.OtherDomainSubscriptionOwners, Is.Empty,
                "OtherDomainSubscriptionOwners must be empty when only the current domain has subscribed.");

            // Add a foreign-domain subscription → short name of that domain
            var foreignSubscription = new ParameterSubscription
            {
                Iid = Guid.NewGuid(),
                Owner = this.foreignDomain
            };

            this.parameter.ParameterSubscription.Add(foreignSubscription);

            var rowForeignSub = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.That(rowForeignSub.OtherDomainSubscriptionOwners, Is.EqualTo(this.foreignDomain.ShortName),
                "OtherDomainSubscriptionOwners must equal the foreign domain's ShortName when it has subscribed.");
        }

        [Test]
        public void VerifyGroupName()
        {
            // Parameter without a group → GroupName is empty, Group is null
            var rowNoGroup = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.Multiple(() =>
            {
                Assert.That(rowNoGroup.Group, Is.Null,
                    "A parameter not assigned to any group must yield a null Group on the row.");
                Assert.That(rowNoGroup.GroupName, Is.EqualTo(string.Empty),
                    "GroupName must be an empty string when the parameter has no group.");
            });

            // Assign a ParameterGroup and rebuild the row
            var group = new ParameterGroup { Iid = Guid.NewGuid(), Name = "Thermal" };
            this.parameter.Group = group;

            var rowWithGroup = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.Multiple(() =>
            {
                Assert.That(rowWithGroup.Group, Is.SameAs(group),
                    "Group must be the ParameterGroup that was assigned to the parameter.");
                Assert.That(rowWithGroup.GroupName, Is.EqualTo("Thermal"),
                    "GroupName must equal the group's Name when one is assigned.");
            });
        }

        [Test]
        public void VerifyOptionDependentValueSelection()
        {
            // Arrange: mark the parameter as option-dependent and add a second value set for a different option.
            this.parameter.IsOptionDependent = true;

            var optionA = new Option { Iid = Guid.NewGuid(), Name = "Option A", ShortName = "OA" };
            var optionB = new Option { Iid = Guid.NewGuid(), Name = "Option B", ShortName = "OB" };

            // The existing value set is re-used for optionA.
            var valueSetA = this.parameter.ValueSet[0];
            valueSetA.ActualOption = optionA;
            valueSetA.Manual = new ValueArray<string>(["10"]);

            // Add a second value set for optionB.
            var valueSetB = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ActualOption = optionB,
                Manual = new ValueArray<string>(["20"]),
                Computed = new ValueArray<string>(["20"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"]),
                Published = new ValueArray<string>(["20"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.parameter.ValueSet.Add(valueSetB);

            // Act: build the row with optionB selected.
            var row = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain, null, optionB);

            // Assert: ActualValue must come from valueSetB (value "20"), not valueSetA (value "10").
            Assert.Multiple(() =>
            {
                Assert.That(row.ActualValue, Does.Contain("20"),
                    "ActualValue must reflect the value set for the selected option (optionB).");
                Assert.That(row.ActualValue, Does.Not.Contain("10"),
                    "ActualValue must not show the value for optionA when optionB is selected.");
            });
        }

        [Test]
        public void VerifyIsOptionDependentReflectsParameter()
        {
            // Default parameter (IsOptionDependent = false)
            var rowNotOptionDependent = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.That(rowNotOptionDependent.IsOptionDependent, Is.False,
                "IsOptionDependent must be false when the parameter is not option-dependent.");

            // Mark the parameter as option-dependent
            this.parameter.IsOptionDependent = true;
            var rowOptionDependent = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain);

            Assert.That(rowOptionDependent.IsOptionDependent, Is.True,
                "IsOptionDependent must be true when the underlying Parameter.IsOptionDependent is true.");
        }

        [Test]
        public void VerifyStateDependentExposesStateValues()
        {
            // Arrange: make the parameter state-dependent.
            // ActualFiniteState.Name is derived — wire PossibleFiniteState entries through
            // a PossibleFiniteStateList so that GetDerivedName() can resolve them correctly.
            var possibleStateAlpha = new PossibleFiniteState { Iid = Guid.NewGuid(), Name = "Alpha" };
            var possibleStateBeta = new PossibleFiniteState { Iid = Guid.NewGuid(), Name = "Beta" };

            var possibleStateList = new PossibleFiniteStateList { Iid = Guid.NewGuid() };
            possibleStateList.PossibleState.Add(possibleStateAlpha);
            possibleStateList.PossibleState.Add(possibleStateBeta);

            var stateList = new ActualFiniteStateList { Iid = Guid.NewGuid() };
            stateList.PossibleFiniteStateList.Add(possibleStateList);

            var stateAlpha = new ActualFiniteState { Iid = Guid.NewGuid() };
            stateAlpha.PossibleState.Add(possibleStateAlpha);
            stateList.ActualState.Add(stateAlpha);

            var stateBeta = new ActualFiniteState { Iid = Guid.NewGuid() };
            stateBeta.PossibleState.Add(possibleStateBeta);
            stateList.ActualState.Add(stateBeta);

            this.parameter.StateDependence = stateList;

            // Clear the single value set added in SetUp and replace with two state-specific ones.
            this.parameter.ValueSet.Clear();

            this.parameter.ValueSet.Add(new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ActualState = stateAlpha,
                Manual = new ValueArray<string>(["1"]),
                Computed = new ValueArray<string>(["1"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"]),
                Published = new ValueArray<string>(["1"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            this.parameter.ValueSet.Add(new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ActualState = stateBeta,
                Manual = new ValueArray<string>(["2"]),
                Computed = new ValueArray<string>(["2"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"]),
                Published = new ValueArray<string>(["2"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            // Act
            var rowStateDependent = new ElementDefinitionDetailsRowViewModel(this.parameter, this.currentDomain, null, null);

            // Assert: IsStateDependent true, StateValues has one entry per state, ordered by name.
            Assert.Multiple(() =>
            {
                Assert.That(rowStateDependent.IsStateDependent, Is.True,
                    "IsStateDependent must be true when StateDependence is set and value sets carry ActualState.");
                Assert.That(rowStateDependent.StateValues.Count, Is.EqualTo(2),
                    "StateValues must contain one entry per ActualFiniteState.");
                Assert.That(rowStateDependent.StateValues[0].StateName, Is.EqualTo("Alpha"),
                    "StateValues must be ordered by StateName (Alpha < Beta).");
                Assert.That(rowStateDependent.StateValues[1].StateName, Is.EqualTo("Beta"));
            });

            // Non-state-dependent parameter → IsStateDependent false, StateValues empty.
            var plainParameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = this.foreignDomain,
                ParameterType = this.parameter.ParameterType
            };

            plainParameter.ValueSet.Add(new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["42"]),
                Computed = new ValueArray<string>(["42"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"]),
                Published = new ValueArray<string>(["42"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            this.containingDefinition.Parameter.Add(plainParameter);

            var rowPlain = new ElementDefinitionDetailsRowViewModel(plainParameter, this.currentDomain);

            Assert.Multiple(() =>
            {
                Assert.That(rowPlain.IsStateDependent, Is.False,
                    "IsStateDependent must be false for a parameter without StateDependence.");
                Assert.That(rowPlain.StateValues, Is.Empty,
                    "StateValues must be empty for a non-state-dependent parameter.");
            });
        }
    }
}
