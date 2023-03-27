// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTableViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.Tests.ViewModels.Components.ParameterEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.ViewModels.Components.ParameterEditor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterTableViewModelTestFixture
    {
        private ParameterTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private DomainOfExpertise domain;
        private Iteration iteration;
        private Option option;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.viewModel = new ParameterTableViewModel(this.sessionService.Object);

            this.option = new Option() { Iid = Guid.NewGuid() };
            var option2 = new Option() { Iid = Guid.NewGuid() };

            this.domain = new DomainOfExpertise()
            {
                Iid = Guid.NewGuid(),
                ShortName = "SYS"
            };

            var parameterType = new SimpleQuantityKind()
            {
                Iid = Guid.NewGuid(),
                Name = "mass",
                ShortName = "m",
            };

            var scale = new RatioScale()
            {
                Name = "kilogram",
                ShortName = "kg"
            };

            var parameter1 = new Parameter()
            {
                Iid = Guid.NewGuid(),
                Owner = this.domain,
                ParameterType = parameterType,
                Scale = scale,
                ValueSet = 
                { 
                    new ParameterValueSet()
                    {
                        Iid = Guid.NewGuid(),
                        Manual = new ValueArray<string>(new []{"-"}),
                        Published = new ValueArray<string>(new []{"-"}),
                        Formula = new ValueArray<string>(new []{"-"}),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var parameter2 = new Parameter()
            {
                Iid = Guid.NewGuid(),
                Owner = this.domain,
                ParameterType = parameterType,
                Scale = scale,
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Iid = Guid.NewGuid(),
                        Published = new ValueArray<string>(new []{"-"}),
                        Manual = new ValueArray<string>(new []{"-"}),
                        Formula = new ValueArray<string>(new []{"-"}),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var parameter3 = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterType,
                Scale = scale,
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Iid = Guid.NewGuid(),
                        Published = new ValueArray<string>(new []{"-"}),
                        Manual = new ValueArray<string>(new []{"-"}),
                        Formula = new ValueArray<string>(new []{"-"}),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var parameterOverride = new ParameterOverride
            {
                Iid = Guid.NewGuid(),
                Owner = this.domain,
                Parameter = parameter1,
                ValueSet =
                {
                    new ParameterOverrideValueSet
                    {
                        Iid = Guid.NewGuid(),
                        Published = new ValueArray<string>(new []{"-"}),
                        ParameterValueSet = parameter1.ValueSet[0],
                        Manual =  new ValueArray<string>(new []{"-"}),
                        Formula = new ValueArray<string>(new []{"-"}),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var topElement = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "Container",
                Parameter = { parameter3 }
            };

            var elementDefinition = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
                Parameter = { parameter1, parameter2 }
            };

            var usage1 = new ElementUsage()
            {
                Iid = Guid.NewGuid(),
                ElementDefinition = elementDefinition,
                ParameterOverride = { parameterOverride },
                ExcludeOption = new List<Option>{option2}
            };

            topElement.ContainedElement.Add(usage1);

            this.iteration = new Iteration()
            {
                Iid = Guid.NewGuid(),
                Element = { topElement, elementDefinition },
                TopElement = topElement,
                DefaultOption = this.option
            };

            this.iteration.Option.Add(this.option);
            this.iteration.Option.Add(option2);
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel(this.iteration, this.domain, this.option);
            Assert.That(this.viewModel.Rows.Count, Is.EqualTo(4));
        }

        [Test]
        public void VerifyParameterRowProperties()
        {
            this.viewModel.InitializeViewModel(this.iteration, this.domain, this.option);
            var parameterRow = this.viewModel.Rows.Items.First();
            
            Assert.Multiple(() =>
            {
                Assert.That(parameterRow.OwnerName, Is.EqualTo(this.domain.ShortName));
                Assert.That(parameterRow.ElementBaseName, Is.EqualTo(this.iteration.TopElement.Name));
                Assert.That(parameterRow.ParameterName, Is.EqualTo("mass"));
                Assert.That(parameterRow.Option, Is.Empty);
                Assert.That(parameterRow.State, Is.Empty);
            });
        }

        [Test]
        public async Task VerifyParameterRowBehavior()
        {
            this.viewModel.InitializeViewModel(this.iteration, this.domain, this.option);
            var parameterRow = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(() => parameterRow.ParameterSwitchKindSelectorViewModel.SwitchValue = ParameterSwitchKind.MANUAL, Throws.Nothing);
                Assert.That(parameterRow.ParameterSwitchKindSelectorViewModel.SwitchValue == ParameterSwitchKind.MANUAL);
            });

            await parameterRow.ParameterSwitchKindSelectorViewModel.OnUpdate.InvokeAsync();
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Never);

            await parameterRow.ParameterTypeEditorSelectorViewModel.ParameterValueChanged.InvokeAsync(parameterRow.Parameter.ValueSets.First());
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Never);

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.viewModel.InitializeViewModel(this.iteration, this.domain, this.option);
            parameterRow = this.viewModel.Rows.Items.First();

            await parameterRow.ParameterSwitchKindSelectorViewModel.OnUpdate.InvokeAsync();
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Never);

            Assert.Multiple(() =>
            {
                Assert.That(() => parameterRow.ParameterSwitchKindSelectorViewModel.SwitchValue = ParameterSwitchKind.REFERENCE, Throws.Nothing);
                Assert.That(parameterRow.ParameterSwitchKindSelectorViewModel.SwitchValue == ParameterSwitchKind.REFERENCE);
            });

            await parameterRow.ParameterSwitchKindSelectorViewModel.OnUpdate.InvokeAsync();
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Once);

            await parameterRow.ParameterTypeEditorSelectorViewModel.ParameterValueChanged.InvokeAsync();
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Once);

            await parameterRow.ParameterTypeEditorSelectorViewModel.ParameterValueChanged.InvokeAsync(parameterRow.Parameter.ValueSets.First());
            this.sessionService.Verify(x => x.UpdateThings(this.iteration, It.IsAny<IEnumerable<Thing>>()), Times.Exactly(2));
        }

        [Test]
        public void VerifyFiltering()
        {
            this.viewModel.InitializeViewModel(this.iteration, this.domain, this.option);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(4));

            this.viewModel.ApplyFilters(this.iteration.Option.Last(), null, null, true);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            this.viewModel.ApplyFilters(this.iteration.DefaultOption, this.iteration.Element.Last(), null, true);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(3));

            this.viewModel.ApplyFilters(this.iteration.DefaultOption,null, new ArrayParameterType(){Iid = Guid.NewGuid()}, true);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(0));

            this.viewModel.ApplyFilters(this.iteration.DefaultOption, null, this.iteration.TopElement.Parameter.First().ParameterType, true);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(4));

            this.viewModel.ApplyFilters(this.iteration.DefaultOption, null, null, false);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(4));
        }
    }
}
