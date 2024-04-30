// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BatchParameterEditorViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Tests.ViewModels.Components.ParameterEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ParameterEditor.BatchParameterEditor;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class BatchParameterEditorViewModelTestFixture
    {
        private BatchParameterEditorViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<ILogger<BatchParameterEditorViewModel>> loggerMock;
        private Iteration iteration;
        private CDPMessageBus messageBus;
        private static readonly ValueArray<string> defaultValueArray = new(["-"]);

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.loggerMock = new Mock<ILogger<BatchParameterEditorViewModel>>();
            this.messageBus = new CDPMessageBus();
            this.viewModel = new BatchParameterEditorViewModel(this.sessionService.Object, this.messageBus, this.loggerMock.Object);

            var domain = new DomainOfExpertise();

            var parameterType = new SimpleQuantityKind
            {
                Iid = Guid.NewGuid(),
                Name = "mass",
                ShortName = "m"
            };

            var scale = new RatioScale
            {
                Name = "kilogram",
                ShortName = "kg"
            };

            var parameter1 = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = domain,
                ParameterType = parameterType,
                Scale = scale,
                ValueSet =
                {
                    new ParameterValueSet
                    {
                        Iid = Guid.NewGuid(),
                        Manual = defaultValueArray,
                        Published = defaultValueArray,
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualOption = new Option()
                    },
                    new ParameterValueSet
                    {
                        Iid = Guid.NewGuid(),
                        Manual = defaultValueArray,
                        Published = defaultValueArray,
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualState = new ActualFiniteState { Container = new ActualFiniteStateList() }
                    },
                    new ParameterValueSet
                    {
                        Iid = Guid.NewGuid(),
                        Manual = defaultValueArray,
                        Published = defaultValueArray,
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualState = new ActualFiniteState { Container = new ActualFiniteStateList() },
                        ActualOption = new Option()
                    },
                }
            };

            var elementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                Parameter = { parameter1 }
            };

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                Element = { elementDefinition },
                TopElement = elementDefinition,
                DefaultOption = new Option()
            };
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyCancelConfirmBehavior()
        {
            this.viewModel.CurrentIteration = this.iteration;
            Assert.That(async () => await this.viewModel.ConfirmCancelPopupViewModel.OnConfirm.InvokeAsync(), Throws.Exception);

            this.viewModel.ParameterTypeSelectorViewModel.SelectedParameterType = this.iteration.QueryParameterAndOverrideBases().First().ParameterType;
            this.viewModel.SelectedValueSetsRowsToUpdate = [this.viewModel.Rows.Items.First()];

            Assert.Multiple(() =>
            {
                Assert.That(async () => await this.viewModel.ConfirmCancelPopupViewModel.OnConfirm.InvokeAsync(), Throws.Nothing);
                this.sessionService.Verify(x => x.CreateOrUpdateThings(It.IsAny<Iteration>(), It.Is<List<Thing>>(c => c.Count > 0)), Times.Once);
            });

            this.viewModel.ConfirmCancelPopupViewModel.IsVisible = true;
            await this.viewModel.ConfirmCancelPopupViewModel.OnCancel.InvokeAsync();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ConfirmCancelPopupViewModel.IsVisible, Is.EqualTo(false));
                Assert.That(this.viewModel.IsLoading, Is.EqualTo(false));
                Assert.That(this.viewModel.IsVisible, Is.EqualTo(false));
                Assert.That(async () => await this.viewModel.ParameterTypeEditorSelectorViewModel.ParameterValueChanged.InvokeAsync((new ParameterValueSet(), 0)), Throws.Nothing);
                Assert.That(this.viewModel.ParameterTypeSelectorViewModel.SelectedParameterType, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyOpenAndFiltering()
        {
            this.viewModel.CurrentIteration = this.iteration;
            this.viewModel.OpenPopup();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ParameterTypeSelectorViewModel.SelectedParameterType, Is.Null);
                Assert.That(this.viewModel.OptionSelectorViewModel.SelectedOption, Is.Null);
                Assert.That(this.viewModel.FiniteStateSelectorViewModel.SelectedActualFiniteState, Is.Null);
                Assert.That(this.viewModel.SelectedValueSetsRowsToUpdate, Has.Count.EqualTo(0));
                Assert.That(this.viewModel.IsVisible, Is.EqualTo(true));
                Assert.That(this.viewModel.Rows.Items.Count(), Is.EqualTo(0));
            });

            var firstTopElementParameter = this.iteration.TopElement.Parameter[0];
            this.viewModel.ParameterTypeSelectorViewModel.SelectedParameterType = firstTopElementParameter.ParameterType;
            Assert.That(this.viewModel.Rows.Items.Count(), Is.EqualTo(3));

            this.viewModel.OptionSelectorViewModel.SelectedOption = firstTopElementParameter.ValueSet[0].ActualOption;
            Assert.That(this.viewModel.Rows.Items.Count(), Is.EqualTo(2));

            this.viewModel.FiniteStateSelectorViewModel.SelectedActualFiniteState = firstTopElementParameter.ValueSet[1].ActualState;
            Assert.That(this.viewModel.Rows.Items.Count(), Is.EqualTo(1));
        }

        [Test]
        public void VerifyViewModelSetIteration()
        {
            Assert.That(this.viewModel.Rows.Items.Count(), Is.EqualTo(0));
            this.viewModel.CurrentIteration = this.iteration;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentIteration, Is.EqualTo(this.iteration));
                Assert.That(this.viewModel.ParameterTypeSelectorViewModel.CurrentIteration, Is.EqualTo(this.iteration));
                Assert.That(this.viewModel.OptionSelectorViewModel.CurrentIteration, Is.EqualTo(this.iteration));
                Assert.That(this.viewModel.FiniteStateSelectorViewModel.CurrentIteration, Is.EqualTo(this.iteration));
            });
        }
    }
}
