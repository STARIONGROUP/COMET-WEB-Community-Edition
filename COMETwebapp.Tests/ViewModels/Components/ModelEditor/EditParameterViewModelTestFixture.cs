// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EditParameterViewModelTestFixture.cs" company="Starion Group S.A.">
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

    using COMETwebapp.ViewModels.Components.ModelEditor.EditParameterViewModel;

    using DynamicData;

    using FluentResults;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EditParameterViewModelTestFixture
    {
        private EditParameterViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private CDPMessageBus messageBus;
        private Iteration iteration;
        private ElementDefinition elementDefinition;
        private Parameter parameter;
        private DomainOfExpertise currentDomain;
        private DomainOfExpertise foreignDomain;
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

            this.currentDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.foreignDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "PWR", Name = "Power" };

            var rdl = new SiteReferenceDataLibrary { ShortName = "siteRdl", Name = "Site RDL" };
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            rdl.ParameterType.Add(parameterType);

            var siteDirectory = new SiteDirectory { Domain = { this.currentDomain, this.foreignDomain }, SiteReferenceDataLibrary = { rdl } };
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            var modelSetup = new EngineeringModelSetup { Name = "ModelName", ShortName = "MDL", ActiveDomain = { this.currentDomain, this.foreignDomain } };

            this.elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), Name = "Box", ShortName = "BOX", Owner = this.currentDomain };
            this.elementDefinition.ParameterGroup.Add(new ParameterGroup { Iid = Guid.NewGuid(), Name = "Group" });

            this.parameter = new Parameter { Iid = Guid.NewGuid(), Owner = this.currentDomain, ParameterType = parameterType };

            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["1"]),
                Computed = new ValueArray<string>(["1"]),
                Reference = new ValueArray<string>(["-"]),
                Formula = new ValueArray<string>(["-"]),
                Published = new ValueArray<string>(["1"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.parameter.ValueSet.Add(parameterValueSet);

            var foreignSubscription = new ParameterSubscription { Iid = Guid.NewGuid(), Owner = this.foreignDomain };

            foreignSubscription.ValueSet.Add(new ParameterSubscriptionValueSet
            {
                Iid = Guid.NewGuid(),
                SubscribedValueSet = parameterValueSet,
                Manual = new ValueArray<string>(["2"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            this.parameter.ParameterSubscription.Add(foreignSubscription);
            this.elementDefinition.Parameter.Add(this.parameter);

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                Element = { this.elementDefinition },
                Container = new EngineeringModel { EngineeringModelSetup = modelSetup }
            };

            this.iteration.ActualFiniteStateList.Add(new ActualFiniteStateList { Iid = Guid.NewGuid() });

            this.sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());

            this.viewModel = new EditParameterViewModel(this.sessionService.Object, this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifySetParameterPopulatesTheForm()
        {
            this.viewModel.SetParameter(this.parameter, this.iteration, this.currentDomain);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Parameter, Is.Not.Null);
                Assert.That(this.viewModel.Parameter, Is.Not.SameAs(this.parameter), "The dialog must edit a clone, not the original.");
                Assert.That(this.viewModel.Parameter.Iid, Is.EqualTo(this.parameter.Iid));
                Assert.That(this.viewModel.IsParameter, Is.True);
                Assert.That(this.viewModel.ParameterAsParameter, Is.Not.Null);
                Assert.That(this.viewModel.ParameterTypeName, Is.EqualTo("Mass"));
                Assert.That(this.viewModel.ValueRows, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.PossibleFiniteStates, Is.Not.Empty);
                Assert.That(this.viewModel.ParameterGroups, Is.Not.Empty);
                Assert.That(this.viewModel.SubscriptionRows, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.SubscriptionRows[0].OwnerShortName, Is.EqualTo("PWR"));
            });
        }

        [Test]
        public void VerifySetParameterNullGuard()
        {
            Assert.That(() => this.viewModel.SetParameter(null, this.iteration, this.currentDomain), Throws.Nothing);
            Assert.That(this.viewModel.Parameter, Is.Null);
        }

        [Test]
        public async Task VerifySaveCommitsTheParameterClone()
        {
            var edited = false;
            this.viewModel.OnParameterEdited = new EventCallbackFactory().Create(this, () => edited = true);
            this.viewModel.SetParameter(this.parameter, this.iteration, this.currentDomain);

            await this.viewModel.SaveAsync();

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);
                Assert.That(this.capturedThings, Has.Some.InstanceOf<Parameter>());
                Assert.That(edited, Is.True);
            });
        }

        [Test]
        public async Task VerifySaveIncludesEditedValueSet()
        {
            this.viewModel.SetParameter(this.parameter, this.iteration, this.currentDomain);

            // Changing the switch stages the edit reactively (no per-row save button in the dialog).
            this.viewModel.ValueRows[0].ParameterSwitchKindSelectorViewModel.SwitchValue = ParameterSwitchKind.REFERENCE;

            await this.viewModel.SaveAsync();

            var editedValueSet = this.capturedThings.OfType<ParameterValueSetBase>().SingleOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(editedValueSet, Is.Not.Null, "A modified value set must be part of the batch.");
                Assert.That(editedValueSet.ValueSwitch, Is.EqualTo(ParameterSwitchKind.REFERENCE));
                Assert.That(this.parameter.ValueSet[0].ValueSwitch, Is.EqualTo(ParameterSwitchKind.MANUAL), "The original value set must be untouched.");
            });
        }

        [Test]
        public async Task VerifyDependenceChangeOmitsValueSets()
        {
            this.viewModel.SetParameter(this.parameter, this.iteration, this.currentDomain);

            this.viewModel.ValueRows[0].ParameterSwitchKindSelectorViewModel.SwitchValue = ParameterSwitchKind.REFERENCE;
            this.viewModel.ParameterAsParameter.IsOptionDependent = true;

            await this.viewModel.SaveAsync();

            Assert.Multiple(() =>
            {
                Assert.That(this.capturedThings, Has.Some.InstanceOf<Parameter>());
                Assert.That(this.capturedThings, Has.None.InstanceOf<ParameterValueSetBase>(),
                    "When the option/state dependence changes the server regenerates value sets, so none are sent.");
            });
        }

        [Test]
        public void VerifyCompoundParameterExpandsPerComponent()
        {
            var compoundParameterType = new CompoundParameterType { Iid = Guid.NewGuid(), Name = "coordinate", ShortName = "coord" };
            var xType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "x", ShortName = "x" };
            var yType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "y", ShortName = "y" };
            compoundParameterType.Component.Add(new ParameterTypeComponent { Iid = Guid.NewGuid(), ShortName = "cx", ParameterType = xType });
            compoundParameterType.Component.Add(new ParameterTypeComponent { Iid = Guid.NewGuid(), ShortName = "cy", ParameterType = yType });

            var compoundParameter = new Parameter { Iid = Guid.NewGuid(), Owner = this.currentDomain, ParameterType = compoundParameterType };

            compoundParameter.ValueSet.Add(new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["1", "2"]),
                Computed = new ValueArray<string>(["-", "-"]),
                Reference = new ValueArray<string>(["-", "-"]),
                Formula = new ValueArray<string>(["-", "-"]),
                Published = new ValueArray<string>(["1", "2"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            this.elementDefinition.Parameter.Add(compoundParameter);

            this.viewModel.SetParameter(compoundParameter, this.iteration, this.currentDomain);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ValueRows, Has.Count.EqualTo(2), "One row per compound component.");
                Assert.That(this.viewModel.ValueRows.Select(row => row.Name), Is.EquivalentTo(new[] { "cx", "cy" }));
                Assert.That(this.viewModel.ValueRows.Select(row => row.ParameterTypeName), Is.EquivalentTo(new[] { "x", "y" }));
                Assert.That(this.viewModel.ValueRows[0].ActualValue, Is.EqualTo("1"));
                Assert.That(this.viewModel.ValueRows[1].ActualValue, Is.EqualTo("2"));
            });
        }

        [Test]
        public void VerifyEditingMultipleCompoundComponentsAccumulates()
        {
            var compoundParameterType = new CompoundParameterType { Iid = Guid.NewGuid(), Name = "coordinate", ShortName = "coord" };
            var q = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "n", ShortName = "n" };
            compoundParameterType.Component.Add(new ParameterTypeComponent { Iid = Guid.NewGuid(), ShortName = "x", ParameterType = q });
            compoundParameterType.Component.Add(new ParameterTypeComponent { Iid = Guid.NewGuid(), ShortName = "y", ParameterType = q });
            compoundParameterType.Component.Add(new ParameterTypeComponent { Iid = Guid.NewGuid(), ShortName = "z", ParameterType = q });

            var valueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["1", "2", "3"]),
                Computed = new ValueArray<string>(["-", "-", "-"]),
                Reference = new ValueArray<string>(["-", "-", "-"]),
                Formula = new ValueArray<string>(["-", "-", "-"]),
                Published = new ValueArray<string>(["1", "2", "3"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var group = new EditParameterValueSetGroupViewModel(compoundParameterType, valueSet, this.messageBus);

            // Simulate the editors of two different components emitting edits (each emits the full array with only its
            // own index changed, exactly as ParameterTypeEditorBaseViewModel.UpdateValueSet does).
            group.ApplyManual(0, new ValueArray<string>(["5", "2", "3"]));
            group.ApplyManual(2, new ValueArray<string>(["1", "2", "9"]));

            var pending = group.GetPendingValueSet();

            Assert.Multiple(() =>
            {
                Assert.That(group.Rows, Has.Count.EqualTo(3));
                Assert.That(pending, Is.Not.Null);
                Assert.That(pending.Manual, Is.EqualTo(new[] { "5", "2", "9" }).AsCollection, "Both edited components must be kept, not just one.");
                Assert.That(valueSet.Manual, Is.EqualTo(new[] { "1", "2", "3" }).AsCollection, "The original must be untouched.");
            });

            group.Dispose();
        }

        [Test]
        public void VerifyNonCompoundMultiValueReplacesWholeArray()
        {
            // A SampledFunctionParameterType is not compound: it has a single editor (the table popup) that owns the
            // whole multi-element value array, so its emitted array must replace the pending one wholesale — merging
            // by index (as compound components do) would drop every element but the first.
            var sampledFunctionParameterType = new SampledFunctionParameterType { Iid = Guid.NewGuid(), Name = "sampled", ShortName = "sampled" };

            var valueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(["1", "2", "3"]),
                Computed = new ValueArray<string>(["-", "-", "-"]),
                Reference = new ValueArray<string>(["-", "-", "-"]),
                Formula = new ValueArray<string>(["-", "-", "-"]),
                Published = new ValueArray<string>(["1", "2", "3"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var group = new EditParameterValueSetGroupViewModel(sampledFunctionParameterType, valueSet, this.messageBus);

            group.ApplyManual(0, new ValueArray<string>(["7", "8", "9"]));

            var pending = group.GetPendingValueSet();

            Assert.Multiple(() =>
            {
                Assert.That(group.Rows, Has.Count.EqualTo(1));
                Assert.That(pending, Is.Not.Null);
                Assert.That(pending.Manual, Is.EqualTo(new[] { "7", "8", "9" }).AsCollection, "The whole emitted array must be staged, not just index 0.");
                Assert.That(valueSet.Manual, Is.EqualTo(new[] { "1", "2", "3" }).AsCollection, "The original must be untouched.");
            });

            group.Dispose();
        }

        [Test]
        public void VerifyChangingDependenceDisablesValueEditing()
        {
            this.viewModel.SetParameter(this.parameter, this.iteration, this.currentDomain);

            Assert.That(this.viewModel.ValuesEditable, Is.True);

            this.viewModel.IsOptionDependent = true;
            Assert.That(this.viewModel.ValuesEditable, Is.False, "Toggling option dependence blocks value editing until save.");
            Assert.That(this.viewModel.ValuesDisabledReason, Does.Contain("dependence"), "The note must explain the dependence change.");

            this.viewModel.IsOptionDependent = false;
            Assert.That(this.viewModel.ValuesEditable, Is.True, "Reverting the dependence re-enables value editing.");
            Assert.That(this.viewModel.ValuesDisabledReason, Is.Empty);
        }

        [Test]
        public void VerifyChangingOwnerDisablesValueEditing()
        {
            this.viewModel.SetParameter(this.parameter, this.iteration, this.currentDomain);

            Assert.That(this.viewModel.ValuesEditable, Is.True);

            this.viewModel.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise = this.foreignDomain;
            Assert.That(this.viewModel.ValuesEditable, Is.False, "Handing the parameter to another owner blocks value editing until save.");
            Assert.That(this.viewModel.ValuesDisabledReason, Does.Contain("owner"), "The note must explain the owner change (not the dependence).");

            this.viewModel.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise = this.currentDomain;
            Assert.That(this.viewModel.ValuesEditable, Is.True, "Reverting the owner re-enables value editing.");
        }

        [Test]
        public async Task VerifyCancelDoesNotWrite()
        {
            var edited = false;
            this.viewModel.OnParameterEdited = new EventCallbackFactory().Create(this, () => edited = true);
            this.viewModel.SetParameter(this.parameter, this.iteration, this.currentDomain);

            await this.viewModel.CancelAsync();

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Never);
                Assert.That(edited, Is.True);
            });
        }
    }
}
