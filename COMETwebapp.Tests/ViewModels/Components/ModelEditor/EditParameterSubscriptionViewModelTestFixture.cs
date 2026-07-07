// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParameterSubscriptionViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.ViewModels.Components.ModelEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ModelEditor.EditParameterSubscriptionViewModel;

    using FluentResults;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EditParameterSubscriptionViewModelTestFixture
    {
        private EditParameterSubscriptionViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private CDPMessageBus messageBus;
        private Iteration iteration;
        private ParameterSubscription subscription;
        private DomainOfExpertise currentDomain;
        private List<Thing> capturedThings;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            this.capturedThings = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, _) => this.capturedThings = things.ToList())
                .ReturnsAsync(Result.Ok());

            this.currentDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "PWR", Name = "Power" };
            var ownerDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };

            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            var modelSetup = new EngineeringModelSetup { Name = "M", ShortName = "M", ActiveDomain = { this.currentDomain, ownerDomain } };
            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Box", ShortName = "BOX", Owner = ownerDomain };
            var parameter = new Parameter { Iid = Guid.NewGuid(), Owner = ownerDomain, ParameterType = parameterType };

            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["3"]),
                Computed = new ValueArray<string>(["3"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"]),
                Published = new ValueArray<string>(["3"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            parameter.ValueSet.Add(parameterValueSet);

            this.subscription = new ParameterSubscription { Iid = Guid.NewGuid(), Owner = this.currentDomain };

            this.subscription.ValueSet.Add(new ParameterSubscriptionValueSet
            {
                Iid = Guid.NewGuid(),
                SubscribedValueSet = parameterValueSet,
                Manual = new ValueArray<string>(["1"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            parameter.ParameterSubscription.Add(this.subscription);
            elementDefinition.Parameter.Add(parameter);

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                Element = { elementDefinition },
                Container = new EngineeringModel { EngineeringModelSetup = modelSetup }
            };

            this.viewModel = new EditParameterSubscriptionViewModel(this.sessionService.Object, this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifySetSubscriptionPopulatesTheForm()
        {
            this.viewModel.SetSubscription(this.subscription, this.iteration);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Subscription, Is.Not.Null);
                Assert.That(this.viewModel.Subscription, Is.SameAs(this.subscription), "Subscription is only a populated-guard; the save clones the value sets, so no defensive clone is made.");
                Assert.That(this.viewModel.SubscribedParameterName, Is.EqualTo("Mass (BOX)"));
                Assert.That(this.viewModel.OwnerShortName, Is.EqualTo("PWR"));
                Assert.That(this.viewModel.ValueRows, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void VerifyCompoundSubscriptionExpandsPerComponent()
        {
            var compoundParameterType = new CompoundParameterType { Iid = Guid.NewGuid(), Name = "coordinate", ShortName = "coord" };
            var q = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "n", ShortName = "n" };
            compoundParameterType.Component.Add(new ParameterTypeComponent { Iid = Guid.NewGuid(), ShortName = "x", ParameterType = q });
            compoundParameterType.Component.Add(new ParameterTypeComponent { Iid = Guid.NewGuid(), ShortName = "y", ParameterType = q });
            compoundParameterType.Component.Add(new ParameterTypeComponent { Iid = Guid.NewGuid(), ShortName = "z", ParameterType = q });

            var ownerDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Box", ShortName = "BOX", Owner = ownerDomain };
            var parameter = new Parameter { Iid = Guid.NewGuid(), Owner = ownerDomain, ParameterType = compoundParameterType };

            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["1", "2", "3"]),
                Computed = new ValueArray<string>(["1", "2", "3"]),
                Reference = new ValueArray<string>(["-", "-", "-"]),
                Formula = new ValueArray<string>(["-", "-", "-"]),
                Published = new ValueArray<string>(["1", "2", "3"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            parameter.ValueSet.Add(parameterValueSet);

            var compoundSubscription = new ParameterSubscription { Iid = Guid.NewGuid(), Owner = this.currentDomain };

            compoundSubscription.ValueSet.Add(new ParameterSubscriptionValueSet
            {
                Iid = Guid.NewGuid(),
                SubscribedValueSet = parameterValueSet,
                Manual = new ValueArray<string>(["4", "5", "6"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            parameter.ParameterSubscription.Add(compoundSubscription);
            elementDefinition.Parameter.Add(parameter);

            this.viewModel.SetSubscription(compoundSubscription, this.iteration);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ValueRows, Has.Count.EqualTo(3), "One subscription row per compound component.");
                Assert.That(this.viewModel.ValueRows.Select(row => row.Name), Is.EquivalentTo(new[] { "x", "y", "z" }));
                Assert.That(this.viewModel.ValueRows.All(row => row.OwnerShortName == "PWR"));
            });
        }

        [Test]
        public async Task VerifySaveCommitsEditedSubscriptionValueSet()
        {
            this.viewModel.SetSubscription(this.subscription, this.iteration);

            this.viewModel.ValueRows[0].ComponentRow.ParameterSwitchKindSelectorViewModel.SwitchValue = ParameterSwitchKind.COMPUTED;

            await this.viewModel.SaveAsync();

            var edited = this.capturedThings?.OfType<ParameterSubscriptionValueSet>().SingleOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(edited, Is.Not.Null, "The edited subscription value set must be committed.");
                Assert.That(edited.ValueSwitch, Is.EqualTo(ParameterSwitchKind.COMPUTED));
                Assert.That(this.subscription.ValueSet[0].ValueSwitch, Is.EqualTo(ParameterSwitchKind.MANUAL), "The original must be untouched.");
            });
        }

        [Test]
        public async Task VerifyCancelDoesNotWrite()
        {
            var edited = false;
            this.viewModel.OnSubscriptionEdited = new EventCallbackFactory().Create(this, () => edited = true);
            this.viewModel.SetSubscription(this.subscription, this.iteration);

            await this.viewModel.CancelAsync();

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Never);
                Assert.That(edited, Is.True);
            });
        }
    }
}
