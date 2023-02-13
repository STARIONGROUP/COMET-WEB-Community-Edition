// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Pages.ParameterEditor
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.IterationServices;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.ViewModels.Pages.ParameterEditor;

    using Moq;

    using NUnit.Framework;

    using ElementDefinition = CDP4Common.EngineeringModelData.ElementDefinition;
    using ElementUsage = CDP4Common.EngineeringModelData.ElementUsage;
    using Iteration = CDP4Common.EngineeringModelData.Iteration;
    using Option = CDP4Common.EngineeringModelData.Option;
    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterEditorViewModelTestFixture
    {
        private TestContext context;
        private IParameterEditorViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            var cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var uri = new Uri("http://localhost");

            var elementUsage1 = new ElementUsage() { Iid = Guid.NewGuid(), Name = "element1" };
            var elementUsage2 = new ElementUsage() { Iid = Guid.NewGuid(), Name = "element2" };
            var elementUsage3 = new ElementUsage() { Iid = Guid.NewGuid(), Name = "element3" };
            var elementUsage4 = new ElementUsage() { Iid = Guid.NewGuid(), Name = "element4" };

            var elementDefinition1 = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "element1",
                ContainedElement = { elementUsage1 },
                Parameter =
                {
                    new Parameter()
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = new TextParameterType(){ Name = "textParamType" }
                    }
                }
            };

            var elementDefinition2 = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "element2",
                ContainedElement = { elementUsage2 },
                Parameter =
                {
                    new Parameter()
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = new EnumerationParameterType(){ Name = "enumParamType" }
                    }
                }
            };

            var elementDefinition3 = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "element3",
                ContainedElement = { elementUsage3 },
                Parameter =
                {
                    new Parameter()
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = new CompoundParameterType(){ Name = "compoundParamType" }
                    }
                }
            };

            var elementDefinition4 = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "element4",
                ContainedElement = { elementUsage4 },
                Parameter =
                {
                    new Parameter()
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = new BooleanParameterType(){ Name = "booleanParamType" }
                    }
                }
            };

            elementUsage1.ElementDefinition = elementDefinition1;
            elementUsage2.ElementDefinition = elementDefinition2;
            elementUsage3.ElementDefinition = elementDefinition3;
            elementUsage4.ElementDefinition = elementDefinition4;

            var topElement = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "topElement",
                ContainedElement =
                {
                    elementDefinition1.ContainedElement[0],
                    elementDefinition2.ContainedElement[0],
                    elementDefinition3.ContainedElement[0],
                    elementDefinition4.ContainedElement[0],
                },
                Parameter =
                {
                    new Parameter()
                    {
                        Iid = Guid.NewGuid(),
                        ParameterType = new TextParameterType() {Name = "textParamType"}
                    }
                }
            };

            var iteration = new Iteration();
            iteration.TopElement = topElement;
            iteration.Element.AddRange(new List<ElementDefinition>() { elementDefinition1, elementDefinition2, elementDefinition3, elementDefinition4 });
            iteration.Option.Add(new Option(Guid.NewGuid(), cache, uri));
            iteration.Option.Add(new Option(Guid.NewGuid(), cache, uri));
            iteration.DefaultOption = iteration.Option.First();

            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.DefaultIteration).Returns(iteration);

            var iterationService = new Mock<IIterationService>();

            var paramerTypesList = new List<ParameterType>()
            {
                new TextParameterType() {Name = "textParamType"},
                new BooleanParameterType(){ Name = "booleanParamType" },
                new CompoundParameterType(){ Name = "compoundParamType" },
                new EnumerationParameterType(){ Name = "enumParamType" },
                new TextParameterType(){ Name = "textParamType" }
            };

            iterationService.Setup(x => x.GetParameterTypes(It.IsAny<Iteration>())).Returns(paramerTypesList);
            this.viewModel = new ParameterEditorViewModel(sessionService.Object, iterationService.Object);
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IterationService, Is.Not.Null);
                Assert.That(this.viewModel.SessionService, Is.Not.Null);
                Assert.That(this.viewModel.Elements, Is.Not.Null);
                Assert.That(this.viewModel.FilteredElements, Is.Not.Null);
                Assert.That(this.viewModel.IsOwnedParameters, Is.False);
                Assert.That(this.viewModel.SelectedElementFilter, Is.Null);
                Assert.That(this.viewModel.SelectedParameterTypeFilter, Is.Null);
                Assert.That(this.viewModel.SelectedOptionFilter, Is.Null);
                Assert.That(this.viewModel.SelectedStateFilter, Is.Null);
                Assert.That(this.viewModel.ParameterTypes, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Elements, Is.Not.Null);
                Assert.That(this.viewModel.Elements, Has.Count.EqualTo(0));
            });

            this.viewModel.InitializeViewModel();
        }
    }
}
