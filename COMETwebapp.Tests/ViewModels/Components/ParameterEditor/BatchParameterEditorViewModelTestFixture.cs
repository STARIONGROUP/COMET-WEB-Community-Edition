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
                        Manual = new ValueArray<string>(new[] { "-" }),
                        Published = new ValueArray<string>(new[] { "-" }),
                        Formula = new ValueArray<string>(new[] { "-" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
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
        public void VerifyViewModelSetIteration()
        {
            this.viewModel.CurrentIteration = this.iteration;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentIteration, Is.EqualTo(this.iteration));
                Assert.That(this.viewModel.ParameterTypeSelectorViewModel.CurrentIteration, Is.EqualTo(this.iteration));
            });
        }

        [Test]
        public async Task VerifyCancelConfirmBehavior()
        {
            this.viewModel.CurrentIteration = this.iteration;
            Assert.That(async () => await this.viewModel.ConfirmCancelPopupViewModel.OnConfirm.InvokeAsync(), Throws.Exception);

            this.viewModel.ParameterTypeSelectorViewModel.SelectedParameterType = this.iteration.QueryParameterAndOverrideBases().First().ParameterType;

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
    }
}
