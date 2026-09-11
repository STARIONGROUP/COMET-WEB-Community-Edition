// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PropertiesComponentTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Viewer.PropertiesPanel
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.Utilities;

    using COMETwebapp.Components.Viewer.PropertiesPanel;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer;
    using COMETwebapp.Model.Viewer.Primitives;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using FluentResults;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PropertiesComponentTestFixture
    {
        private BunitContext context;
        private PropertiesComponent properties;
        private IRenderedComponent<PropertiesComponent> renderedComponent;
        private PropertiesComponentViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            var babylonService = new Mock<IBabylonInterop>();
            this.context.Services.AddSingleton(babylonService);

            var selectionMediator = new Mock<ISelectionMediator>();
            this.context.Services.AddSingleton(selectionMediator);
            selectionMediator.Setup(x => x.SelectedSceneObjectClone).Returns(new SceneObject(It.IsAny<Primitive>()));

            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService);

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            var iterationService = new Mock<ISubscriptionService>();
            this.context.Services.AddSingleton(iterationService);
            this.messageBus = new CDPMessageBus();

            this.viewModel = new PropertiesComponentViewModel(babylonService.Object, this.sessionService.Object, selectionMediator.Object, this.messageBus)
            {
                IsVisible = true,
                ParameterValueSetRelations = []
            };

            this.renderedComponent = this.context.Render<PropertiesComponent>(parameters => { parameters.Add(p => p.ViewModel, this.viewModel); });

            this.properties = this.renderedComponent.Instance;
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.properties, Is.Not.Null);
                Assert.That(this.properties.ViewModel, Is.Not.Null);
            });
        }

        /// <summary>
        /// Verifies that disposing the view model unsubscribes from the <see cref="ISelectionMediator" /> events, so a
        /// long-lived (circuit-scoped) mediator no longer keeps the disposed view model alive nor fires its callbacks.
        /// </summary>
        [Test]
        public void VerifyDisposeUnsubscribesFromSelectionMediator()
        {
            var mediator = new Mock<ISelectionMediator>();
            var babylon = new Mock<IBabylonInterop>();
            var session = new Mock<ISessionService>();

            var vm = new PropertiesComponentViewModel(babylon.Object, session.Object, mediator.Object, this.messageBus)
            {
                IsVisible = false
            };

            vm.Dispose();
            mediator.Raise(x => x.OnModelSelectionChanged += null, new SceneObject(new Cube(1, 1, 1)));

            Assert.That(vm.IsVisible, Is.False);
        }

        [Test]
        public void VerifyElementValueChanges()
        {
            var compoundData = new OrderedItemList<ParameterTypeComponent>(null)
            {
                new()
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "firstValue",
                    Scale = new OrdinalScale
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                }
            };

            var parametertype = new CompoundParameterType
            {
                Iid = Guid.NewGuid()
            };

            parametertype.Component.AddRange(compoundData);

            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = parametertype };

            this.viewModel.SelectedParameter = parameter;

            var compoundValues = new List<string> { "1" };

            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(compoundValues),
                Container = new Iteration()
            };

            Assert.Multiple(() =>
            {
                Assert.That(() => this.viewModel.ParameterValueSetChanged((parameterValueSet, 0)), Throws.Nothing);
                Assert.That(() => this.viewModel.OnSubmit(), Throws.Nothing);
            });

            var compoundValues1 = new List<string> { "false" };

            var parameterValueSet1 = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(compoundValues1)
            };

            Assert.That(() => this.viewModel.ParameterValueSetChanged((parameterValueSet1, 0)), Throws.Nothing);
        }

        /// <summary>
        /// Editing a parameter value highlights its label as changed, the submit confirmation dialog
        /// exposes the old and new values, and submitting persists the change through the notification (toast) write and
        /// resets the changed-parameter tracking.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        [Test]
        public async Task VerifyChangedParameterFeedbackAndSubmit()
        {
            var scale = new RatioScale { Iid = Guid.NewGuid(), ShortName = "kg" };
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = parameterType, Scale = scale };
            var iteration = new Iteration { Iid = Guid.NewGuid() };

            var originalValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["1"]),
                Container = iteration
            };

            this.viewModel.ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet> { { parameter, originalValueSet } };
            this.viewModel.ParametersInUse = [parameter];
            this.viewModel.SelectedParameter = parameter;

            var editedValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["5"]),
                Container = iteration
            };

            await this.viewModel.ParameterValueSetChanged((editedValueSet, 0));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.HasChanges(parameter), Is.True);
                Assert.That(this.viewModel.ParameterHaveChanges, Is.True);
                Assert.That(this.viewModel.GetChangedParameters(), Has.Count.EqualTo(1));
                Assert.That(this.viewModel.GetParameterDisplayName(parameter), Is.EqualTo("Mass [kg]"));
                Assert.That(this.viewModel.GetOriginalValue(parameter), Is.EqualTo("1"));
                Assert.That(this.viewModel.ParameterValueSetRelations[parameter].ActualValue, Is.EqualTo(new ValueArray<string>(["5"])));
            }

            this.renderedComponent.Render();
            this.renderedComponent.WaitForAssertion(() =>
                Assert.That(this.renderedComponent.Find(".parameter-item").ClassList, Does.Contain("parameter-item-changed")));

            await this.viewModel.OnSubmit();

            using (Assert.EnterMultipleScope())
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(),
                    It.Is<NotificationDescription>(n => n.OnSuccess == "Parameter values updated successfully")), Times.Once);
                Assert.That(this.viewModel.HasChanges(parameter), Is.False);
                Assert.That(this.viewModel.GetChangedParameters(), Is.Empty);
            }
        }

        /// <summary>
        /// When the write fails, the pending changes stay tracked and the Submit button stays enabled
        /// (ParameterHaveChanges remains true) so the user can retry, instead of being silently stuck.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        [Test]
        public async Task VerifyOnSubmitFailureKeepsPendingChanges()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Fail("boom"));

            var scale = new RatioScale { Iid = Guid.NewGuid(), ShortName = "kg" };
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = parameterType, Scale = scale };
            var iteration = new Iteration { Iid = Guid.NewGuid() };

            var originalValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["1"]),
                Container = iteration
            };

            this.viewModel.ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet> { { parameter, originalValueSet } };
            this.viewModel.ParametersInUse = [parameter];
            this.viewModel.SelectedParameter = parameter;

            var editedValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["5"]),
                Container = iteration
            };

            await this.viewModel.ParameterValueSetChanged((editedValueSet, 0));
            await this.viewModel.OnSubmit();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.HasChanges(parameter), Is.True);
                Assert.That(this.viewModel.ParameterHaveChanges, Is.True);
                Assert.That(this.viewModel.GetChangedParameters(), Has.Count.EqualTo(1));
            }
        }

        /// <summary>
        /// Reverting an unsubmitted change discards it, restores the original value in the tracked
        /// relations and clears the changed state.
        /// </summary>
        [Test]
        public void VerifyRevertChangeRestoresOriginalValue()
        {
            var scale = new RatioScale { Iid = Guid.NewGuid(), ShortName = "kg" };
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = parameterType, Scale = scale };
            var iteration = new Iteration { Iid = Guid.NewGuid() };

            var originalValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["1"]),
                Container = iteration
            };

            this.viewModel.ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet> { { parameter, originalValueSet } };
            this.viewModel.ParametersInUse = [parameter];
            this.viewModel.SelectedParameter = parameter;

            var editedValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["5"]),
                Container = iteration
            };

            this.viewModel.ParameterValueSetChanged((editedValueSet, 0));
            Assert.That(this.viewModel.HasChanges(parameter), Is.True);

            this.viewModel.RevertChange(parameter);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.HasChanges(parameter), Is.False);
                Assert.That(this.viewModel.GetChangedParameters(), Is.Empty);
                Assert.That(this.viewModel.ParameterHaveChanges, Is.False);
                Assert.That(this.viewModel.ParameterValueSetRelations[parameter].ActualValue, Is.EqualTo(new ValueArray<string>(["1"])));
            }
        }

        /// <summary>
        /// The parameter editor is memoized per parameter so it is not rebuilt on every render (which
        /// would drop keyboard focus and deselect the value while the user is typing).
        /// </summary>
        [Test]
        public void VerifyDetailsEditorIsMemoizedPerParameter()
        {
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = parameterType };

            var valueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["1"])
            };

            this.viewModel.ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet> { { parameter, valueSet } };
            this.viewModel.ParametersInUse = [parameter];
            this.viewModel.SelectedParameter = parameter;

            var first = this.viewModel.CreateDetailsComponentViewModel();
            var second = this.viewModel.CreateDetailsComponentViewModel();

            Assert.That(first, Is.SameAs(second));
        }

        /// <summary>
        /// When the value is edited through the editor's own callback (as the real DevExpress editor does)
        /// and then reverted from the panel, the tracked value is restored - guarding against a parameter-identity
        /// mismatch between the editor callback and the revert affordance.
        /// </summary>
        [Test]
        public async Task VerifyRevertRestoresValueEditedThroughEditorCallback()
        {
            var scale = new RatioScale { Iid = Guid.NewGuid(), ShortName = "kg" };
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = parameterType, Scale = scale };
            var iteration = new Iteration { Iid = Guid.NewGuid() };

            var originalValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["1"]),
                Container = iteration
            };

            this.viewModel.ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet> { { parameter, originalValueSet } };
            this.viewModel.ParametersInUse = [parameter];
            this.viewModel.SelectedParameter = parameter;
            this.renderedComponent.Render();

            // Edit through the editor's own callback, exactly as the DevExpress editor fires it.
            var editor = this.renderedComponent.FindComponent<DetailsComponent>().Instance.ViewModel;

            var editedValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["5"]),
                Container = iteration
            };

            await this.renderedComponent.InvokeAsync(() => editor.ParameterEditorSelector.ParameterValueChanged.InvokeAsync((editedValueSet, 0)));
            this.renderedComponent.Render();

            Assert.That(this.viewModel.HasChanges(parameter), Is.True);

            this.renderedComponent.Find(".parameter-item-revert").Click();
            this.renderedComponent.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.HasChanges(parameter), Is.False);
                Assert.That(this.viewModel.ParameterValueSetRelations[parameter].ActualValue, Is.EqualTo(new ValueArray<string>(["1"])));
                Assert.That(this.renderedComponent.FindComponent<DetailsComponent>().Instance.ViewModel.ParameterEditorSelector.ValueSet.ActualValue, Is.EqualTo(new ValueArray<string>(["1"])));
            }
        }

        /// <summary>
        /// Verifies GH948 review: selecting a different scene object discards any unsubmitted change on the
        /// previously-selected one. Pending edits are applied directly against
        /// <see cref="ISelectionMediator.SelectedSceneObjectClone" />; keeping them staged past a selection change would
        /// let a later revert or dialog edit for that parameter mutate the newly-selected (wrong) object's clone instead.
        /// </summary>
        [Test]
        public void VerifySelectionChangeDiscardsPendingChange()
        {
            var mediator = new Mock<ISelectionMediator>();
            var firstObjectClone = new SceneObject(new Cube(1, 1, 1));
            mediator.SetupGet(x => x.SelectedSceneObjectClone).Returns(firstObjectClone);

            var vm = new PropertiesComponentViewModel(new Mock<IBabylonInterop>().Object, new Mock<ISessionService>().Object, mediator.Object, this.messageBus)
            {
                IsVisible = true
            };

            var scale = new RatioScale { Iid = Guid.NewGuid(), ShortName = "kg" };
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = parameterType, Scale = scale };
            var iteration = new Iteration { Iid = Guid.NewGuid() };

            var originalValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["1"]),
                Container = iteration
            };

            vm.ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet> { { parameter, originalValueSet } };
            vm.ParametersInUse = [parameter];
            vm.SelectedParameter = parameter;

            vm.ParameterValueSetChanged((new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["5"]),
                Container = iteration
            }, 0));

            Assert.That(vm.HasChanges(parameter), Is.True);

            // Selecting a different scene object clone must discard the pending change on the first one.
            var secondObjectClone = new SceneObject(new Cube(1, 1, 1));
            mediator.SetupGet(x => x.SelectedSceneObjectClone).Returns(secondObjectClone);
            mediator.Raise(x => x.OnModelSelectionChanged += null, secondObjectClone);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(vm.HasChanges(parameter), Is.False);
                Assert.That(vm.GetChangedParameters(), Is.Empty);
                Assert.That(vm.GetOriginalValue(parameter), Is.Empty);
                Assert.That(vm.ParameterHaveChanges, Is.False);
            }
        }

        /// <summary>
        /// Verifies GH948 review: reverting a ShapeKind change restores the original primitive type AND re-applies the
        /// other associated parameters (e.g. position) onto it, mirroring what the edit path already does - otherwise the
        /// reverted primitive is left with default (origin) geometry instead of the shape's actual position.
        /// </summary>
        [Test]
        public void VerifyRevertOfShapeKindResyncsAssociatedParameters()
        {
            var mediator = new Mock<ISelectionMediator>();
            var sceneObjectClone = new SceneObject(new Cube(1, 1, 1));
            mediator.SetupGet(x => x.SelectedSceneObjectClone).Returns(sceneObjectClone);

            var vm = new PropertiesComponentViewModel(new Mock<IBabylonInterop>().Object, new Mock<ISessionService>().Object, mediator.Object, this.messageBus)
            {
                IsVisible = true
            };

            var kindParameterType = new TextParameterType { Iid = Guid.NewGuid(), ShortName = SceneSettings.ShapeKindShortName };
            var kindParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = kindParameterType };

            var positionParameterType = new TextParameterType { Iid = Guid.NewGuid(), ShortName = ConstantValues.PositionShortName };
            var positionParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = positionParameterType };

            var iteration = new Iteration { Iid = Guid.NewGuid() };

            var kindValueSet = new ParameterValueSet { Iid = Guid.NewGuid(), ValueSwitch = ParameterSwitchKind.MANUAL, Manual = new ValueArray<string>(["box"]), Container = iteration };
            var positionValueSet = new ParameterValueSet { Iid = Guid.NewGuid(), ValueSwitch = ParameterSwitchKind.MANUAL, Manual = new ValueArray<string>(["5", "0", "0"]), Container = iteration };

            vm.ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet>
            {
                { kindParameter, kindValueSet },
                { positionParameter, positionValueSet }
            };

            vm.ParametersInUse = [kindParameter, positionParameter];
            vm.SelectedParameter = kindParameter;

            vm.ParameterValueSetChanged((new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["sphere"]),
                Container = iteration
            }, 0));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(sceneObjectClone.Primitive, Is.TypeOf<Sphere>());
                Assert.That(sceneObjectClone.Primitive.X, Is.EqualTo(5).Within(0.001), "the edit path should re-sync the position onto the new primitive");
            }

            vm.RevertChange(kindParameter);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(sceneObjectClone.Primitive, Is.TypeOf<Cube>(), "reverting the ShapeKind parameter should restore the original primitive type");
                Assert.That(sceneObjectClone.Primitive.X, Is.EqualTo(5).Within(0.001), "the revert path should re-sync the position onto the reverted primitive");
            }
        }

        /// <summary>
        /// Verifies GH948 review: while nothing is selected, the empty details editor is memoized instead of being
        /// rebuilt on every call - a new instance every render would force <c>DetailsComponent</c> (bound via
        /// <c>@key</c>) to fully remount on every render while the properties panel is idle.
        /// </summary>
        [Test]
        public void VerifyEmptyDetailsEditorIsMemoizedWhenNothingSelected()
        {
            this.viewModel.SelectedParameter = null;

            var first = this.viewModel.CreateDetailsComponentViewModel();
            var second = this.viewModel.CreateDetailsComponentViewModel();

            Assert.That(first, Is.SameAs(second));
        }

        /// <summary>
        /// Clicking the revert affordance rendered on a changed parameter label discards the change.
        /// </summary>
        [Test]
        public void VerifyRevertFromPanelList()
        {
            var scale = new RatioScale { Iid = Guid.NewGuid(), ShortName = "kg" };
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = parameterType, Scale = scale };
            var iteration = new Iteration { Iid = Guid.NewGuid() };

            var originalValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["1"]),
                Container = iteration
            };

            this.viewModel.ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet> { { parameter, originalValueSet } };
            this.viewModel.ParametersInUse = [parameter];
            this.viewModel.SelectedParameter = parameter;

            this.viewModel.ParameterValueSetChanged((new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["5"]),
                Container = iteration
            }, 0));

            this.renderedComponent.Render();
            this.renderedComponent.Find(".parameter-item-revert").Click();

            this.renderedComponent.WaitForAssertion(() => Assert.That(this.viewModel.HasChanges(parameter), Is.False));
        }

        /// <summary>
        /// After editing a value and clicking revert, the rendered editor is remounted (a new
        /// <see cref="DetailsComponent" /> view model) bound to the restored original value set, so the field shows the
        /// original value again.
        /// </summary>
        [Test]
        public void VerifyRevertRebuildsEditorWithOriginalValue()
        {
            var scale = new RatioScale { Iid = Guid.NewGuid(), ShortName = "kg" };
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = parameterType, Scale = scale };
            var iteration = new Iteration { Iid = Guid.NewGuid() };

            var originalValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["1"]),
                Container = iteration
            };

            this.viewModel.ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet> { { parameter, originalValueSet } };
            this.viewModel.ParametersInUse = [parameter];
            this.viewModel.SelectedParameter = parameter;
            this.renderedComponent.Render();

            var detailsBefore = this.renderedComponent.FindComponent<DetailsComponent>().Instance;

            this.viewModel.ParameterValueSetChanged((new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["5"]),
                Container = iteration
            }, 0));

            this.renderedComponent.Render();
            this.renderedComponent.Find(".parameter-item-revert").Click();
            this.renderedComponent.Render();

            var detailsComponent = this.renderedComponent.FindComponent<DetailsComponent>().Instance;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(detailsComponent, Is.Not.SameAs(detailsBefore), "the DetailsComponent should be remounted by @key after a revert");
                Assert.That(detailsComponent.ViewModel.ParameterEditorSelector.ValueSet.ActualValue, Is.EqualTo(new ValueArray<string>(["1"])));
            }
        }

        /// <summary>
        /// Clicking Submit opens the confirmation dialog, and clicking OK submits the change and closes it
        /// (covers OpenSubmitDialog, ConfirmSubmit success branch and OnSubmitDialogClosed).
        /// </summary>
        [Test]
        public void VerifyOpenAndConfirmSubmitDialog()
        {
            var parameter = this.StageSingleChange();

            this.renderedComponent.FindAll("button").First(button => button.TextContent.Contains("Submit")).Click();
            this.renderedComponent.WaitForAssertion(() => Assert.That(this.renderedComponent.FindAll(".submit-changes-table"), Is.Not.Empty));

            this.renderedComponent.FindAll("button").First(button => button.TextContent.Trim() == "OK").Click();

            using (Assert.EnterMultipleScope())
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);
                Assert.That(this.viewModel.HasChanges(parameter), Is.False);
            }
        }

        /// <summary>
        /// When the submit write fails, the confirmation dialog stays open and the change stays tracked
        /// so the user can retry (covers the ConfirmSubmit failure branch).
        /// </summary>
        [Test]
        public void VerifySubmitDialogStaysOpenOnFailure()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Fail("boom"));

            var parameter = this.StageSingleChange();

            this.renderedComponent.FindAll("button").First(button => button.TextContent.Contains("Submit")).Click();
            this.renderedComponent.WaitForAssertion(() => Assert.That(this.renderedComponent.FindAll(".submit-changes-table"), Is.Not.Empty));

            this.renderedComponent.FindAll("button").First(button => button.TextContent.Trim() == "OK").Click();

            this.renderedComponent.WaitForAssertion(() =>
            {
                Assert.That(this.viewModel.HasChanges(parameter), Is.True);
                Assert.That(this.renderedComponent.FindAll(".submit-changes-table"), Is.Not.Empty);
            });
        }

        /// <summary>
        /// Reverting the only change from inside the dialog discards it and closes the dialog (covers
        /// RevertChangeInDialog).
        /// </summary>
        [Test]
        public void VerifyRevertFromDialog()
        {
            var parameter = this.StageSingleChange();

            this.renderedComponent.FindAll("button").First(button => button.TextContent.Contains("Submit")).Click();
            this.renderedComponent.WaitForAssertion(() => Assert.That(this.renderedComponent.FindAll(".dialog-revert"), Is.Not.Empty));

            this.renderedComponent.Find(".dialog-revert").Click();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.HasChanges(parameter), Is.False);
                Assert.That(this.viewModel.GetChangedParameters(), Is.Empty);
            }
        }

        /// <summary>
        /// Stages a single pending change on a freshly selected parameter and renders the component, returning the
        /// changed <see cref="Parameter" />.
        /// </summary>
        /// <returns>The changed <see cref="Parameter" /></returns>
        private Parameter StageSingleChange()
        {
            var scale = new RatioScale { Iid = Guid.NewGuid(), ShortName = "kg" };
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };
            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = parameterType, Scale = scale };
            var iteration = new Iteration { Iid = Guid.NewGuid() };

            var originalValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["1"]),
                Container = iteration
            };

            this.viewModel.ParameterValueSetRelations = new Dictionary<ParameterBase, IValueSet> { { parameter, originalValueSet } };
            this.viewModel.ParametersInUse = [parameter];
            this.viewModel.SelectedParameter = parameter;

            this.viewModel.ParameterValueSetChanged((new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(["5"]),
                Container = iteration
            }, 0));

            this.renderedComponent.Render();
            return parameter;
        }

        [Test]
        public void VerifyThatComponentCanBeHidden()
        {
            this.properties.ViewModel.IsVisible = true;
            var component = this.renderedComponent.Find("#properties-header");
            Assert.That(component, Is.Not.Null);
            this.properties.ViewModel.IsVisible = false;
            Assert.Throws<ElementNotFoundException>(() => this.renderedComponent.Find("#properties-header"));
        }

        [Test]
        public void VerifyOnSelectionChanged()
        {
            var babylon = new Mock<IBabylonInterop>();
            var session = new Mock<ISessionService>();
            var mediator = new Mock<ISelectionMediator>();

            var viewModelUnderTest = new PropertiesComponentViewModel(babylon.Object, session.Object, mediator.Object, this.messageBus)
            {
                IsVisible = false
            };

            // SelectedSceneObjectClone is null — all state must be cleared
            mediator.Setup(x => x.SelectedSceneObjectClone).Returns((SceneObject)null);
            mediator.Raise(x => x.OnModelSelectionChanged += null, ((SceneObject)null)!);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(viewModelUnderTest.ParameterValueSetRelations, Is.Empty);
                Assert.That(viewModelUnderTest.ParametersInUse, Is.Empty);
                Assert.That(viewModelUnderTest.SelectedParameter, Is.Null);
            }

            // SceneObject with no parameters — ParametersAsociated is empty,
            // so ParametersInUse must be empty and SelectedParameter null.
            // This also covers the filter: parameters absent from ParameterValueSetRelations are excluded.
            var elementDef = new ElementDefinition()
            {
                Iid = Guid.NewGuid()
            };

            var elementUsage = new ElementUsage
            {
                Iid = Guid.NewGuid(),
                ElementDefinition = elementDef
            };

            elementDef.ContainedElement.Add(elementUsage);

            var sceneObject = SceneObject.Create(elementUsage, null, []);
            mediator.Setup(x => x.SelectedSceneObjectClone).Returns(sceneObject);
            mediator.Raise(x => x.OnModelSelectionChanged += null, sceneObject);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(viewModelUnderTest.ParametersInUse, Is.Empty);
                Assert.That(viewModelUnderTest.SelectedParameter, Is.Null);
            }
        }
    }
}
