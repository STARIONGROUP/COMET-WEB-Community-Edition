// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewerBodyViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.Tests.ViewModels.Pages.Viewer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using Moq;
    
    using NUnit.Framework;
    
    [TestFixture]
    public class ViewerBodyViewModelTestFixture
    {
        private IViewerBodyViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            var sessionServiceMock = new Mock<ISessionService>();

            var elementUsage1 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element1" };
            var elementUsage2 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element2" };
            var elementUsage3 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element3" };
            var elementUsage4 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element4" };
            
            var elementDefinition1 = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "element1",
                Parameter =
                {
                    new Parameter
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = new TextParameterType { Name = "textParamType" }
                    }
                }
            };
            
            var elementDefinition2 = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "element2",
                Parameter =
                {
                    new Parameter
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = new EnumerationParameterType { Name = "enumParamType" }
                    }
                }
            };
            
            var elementDefinition3 = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "element3",
                Parameter =
                {
                    new Parameter
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = new CompoundParameterType { Name = "compoundParamType" }
                    }
                }
            };
            
            var elementDefinition4 = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "element4",
                Parameter =
                {
                    new Parameter
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = new BooleanParameterType { Name = "booleanParamType" }
                    }
                }
            };

            elementUsage1.ElementDefinition = elementDefinition1;
            elementUsage2.ElementDefinition = elementDefinition2;
            elementUsage3.ElementDefinition = elementDefinition3;
            elementUsage4.ElementDefinition = elementDefinition4;

            elementDefinition1.ContainedElement.Add(elementUsage1);
            elementDefinition2.ContainedElement.Add(elementUsage2);
            elementDefinition3.ContainedElement.Add(elementUsage3);
            elementDefinition4.ContainedElement.Add(elementUsage4);

            var topElement = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "topElement",
                Parameter =
                {
                    new Parameter
                {
                        Iid = Guid.NewGuid(),
                        ParameterType = new TextParameterType {Name = "textParamType"}
                }
                },
            };

            var iteration = new Iteration();
            iteration.TopElement = topElement;
            iteration.Element.AddRange(new List<ElementDefinition>(){elementDefinition1,elementDefinition2,elementDefinition3,elementDefinition4});
            iteration.Option.Add(new Option());
            iteration.Option.Add(new Option());
            iteration.DefaultOption = iteration.Option.First();

            var possibleState1 = new PossibleFiniteState();
            var possibleState2 = new PossibleFiniteState();

            var actualState1 = new ActualFiniteState()
            {
                PossibleState = { possibleState1 }
            };

            var actualState2 = new ActualFiniteState()
            {
                PossibleState = { possibleState2 }
            };

            var possibleFiniteStateList = new PossibleFiniteStateList();
            possibleFiniteStateList.DefaultState = possibleState1;
            possibleFiniteStateList.PossibleState.Add(possibleState1);
            possibleFiniteStateList.PossibleState.Add(possibleState2);

            var actualFiniteStateList = new ActualFiniteStateList();
            actualFiniteStateList.ActualState.Add(actualState1);
            actualFiniteStateList.ActualState.Add(actualState2);

            iteration.ActualFiniteStateList.Add(actualFiniteStateList);
            iteration.ActualFiniteStateList.Add(new ActualFiniteStateList());

            var selectionMediatorMock = new Mock<ISelectionMediator>();

            var babylonInterop = new Mock<IBabylonInterop>();

            this.viewModel = new ViewerBodyViewModel(sessionServiceMock.Object, selectionMediatorMock.Object, babylonInterop.Object);
            this.viewModel.CurrentIteration = iteration;
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public async Task VerifyViewModel()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectionMediator, Is.Not.Null);
                Assert.That(this.viewModel.Elements, Is.Not.Null);
                Assert.That(this.viewModel.Elements, Has.Count.EqualTo(5));
                Assert.That(this.viewModel.OptionSelector, Is.Not.Null);
                Assert.That(this.viewModel.OptionSelector.AvailableOptions.ToList(), Has.Count.EqualTo(2));
                Assert.That(this.viewModel.MultipleFiniteStateSelector, Is.Not.Null);
                Assert.That(this.viewModel.MultipleFiniteStateSelector.ActualFiniteStateListsCollection, Has.Count.EqualTo(2));
                Assert.That(this.viewModel.MultipleFiniteStateSelector.SelectedFiniteStates, Is.Not.Null);
                Assert.That(this.viewModel.MultipleFiniteStateSelector.SelectedFiniteStates.ToList(), Has.Count.EqualTo(1));
            });
        }

        [Test]
        public async Task VerifyInitializeElements()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);
            var elements = this.viewModel.InitializeElements().ToList();

            Assert.Multiple(() =>
            {
                Assert.That(elements, Is.Not.Null);
                Assert.That(elements, Has.Count.EqualTo(5));
                Assert.That(elements.Any(x => x.Name == "topElement"), Is.True);
                Assert.That(elements.Any(x => x.Name == "element1"), Is.True);
                Assert.That(elements.Any(x => x.Name == "element2"), Is.True);
                Assert.That(elements.Any(x => x.Name == "element3"), Is.True);
                Assert.That(elements.Any(x => x.Name == "element4"), Is.True);
            });
        }

        [Test]
        public async Task VerifyOnOptionChange()
        {
            await this.viewModel.InitializeViewModel();
            var previousOption = this.viewModel.OptionSelector.SelectedOption;
            this.viewModel.OptionSelector.SelectedOption = this.viewModel.OptionSelector.AvailableOptions.Last();
            Assert.That(previousOption, Is.Not.EqualTo(this.viewModel.OptionSelector.SelectedOption));
        }
    }
}
