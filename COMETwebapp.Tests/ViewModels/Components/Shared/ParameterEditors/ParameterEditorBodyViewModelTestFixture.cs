// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorBodyViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.Shared.ParameterEditors
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.ViewModels.Components.ParameterEditor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterEditorBodyViewModelTestFixture
    {
        private IParameterEditorBodyViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            var cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var uri = new Uri("http://localhost");

            var elementUsage1 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element1" };
            var elementUsage2 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element2" };
            var elementUsage3 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element3" };
            var elementUsage4 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element4" };

            var elementDefinition1 = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "element1",
                ContainedElement = { elementUsage1 },
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
                ContainedElement = { elementUsage2 },
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
                ContainedElement = { elementUsage3 },
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
                ContainedElement = { elementUsage4 },
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
                }
            };

            var iteration = new Iteration();
            iteration.TopElement = topElement;
            iteration.Element.AddRange(new List<ElementDefinition>() { elementDefinition1, elementDefinition2, elementDefinition3, elementDefinition4 });
            iteration.Option.Add(new Option(Guid.NewGuid(), cache, uri));
            iteration.Option.Add(new Option(Guid.NewGuid(), cache, uri));
            iteration.DefaultOption = iteration.Option.First();

            var sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            sessionService.Setup(x => x.Session).Returns(session.Object);

            var subscriptionService = new Mock<ISubscriptionService>();

            var paramerTypesList = new List<ParameterType>()
            {
                new TextParameterType() {Name = "textParamType"},
                new BooleanParameterType(){ Name = "booleanParamType" },
                new CompoundParameterType(){ Name = "compoundParamType" },
                new EnumerationParameterType(){ Name = "enumParamType" },
                new TextParameterType(){ Name = "textParamType" }
            };

            this.viewModel = new ParameterEditorBodyViewModel(sessionService.Object, subscriptionService.Object);
            this.viewModel.CurrentIteration = iteration;
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SubscriptionService, Is.Not.Null);
                Assert.That(this.viewModel.Elements, Is.Not.Null);
                Assert.That(this.viewModel.FilteredElements, Is.Not.Null);
                Assert.That(this.viewModel.ElementSelector, Is.Not.Null);
                Assert.That(this.viewModel.ParameterTypeSelector, Is.Not.Null);
                Assert.That(this.viewModel.OptionSelector, Is.Not.Null);
                Assert.That(this.viewModel.FiniteStateSelector, Is.Not.Null);
                Assert.That(this.viewModel.IsOwnedParameters, Is.False);
            });
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Elements, Is.Not.Null);
                Assert.That(this.viewModel.Elements.ToList(), Has.Count.EqualTo(5));
            });
        }

        [Test]
        public void VerifyApplyFilters()
        {
            this.viewModel.InitializeViewModel();
            var firstElement = this.viewModel.Elements.First();

            this.viewModel.OptionSelector.SelectedOption = null;
            this.viewModel.ParameterTypeSelector.SelectedParameterType = null;
            this.viewModel.FiniteStateSelector.SelectedActualFiniteState = null;

            this.viewModel.ElementSelector.SelectedElementBase = firstElement;
            this.viewModel.ApplyFilters(this.viewModel.Elements);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.FilteredElements, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.FilteredElements.Items.Contains(firstElement), Is.True);
            });

            this.viewModel.ElementSelector.SelectedElementBase = null;
            this.viewModel.ParameterTypeSelector.SelectedParameterType = new TextParameterType() { Name = "textParamType" };
            this.viewModel.ApplyFilters(this.viewModel.Elements);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.FilteredElements, Has.Count.EqualTo(2));
            });

            this.viewModel.ParameterTypeSelector.SelectedParameterType = null;
            this.viewModel.OptionSelector.SelectedOption = this.viewModel.CurrentIteration.DefaultOption;
            this.viewModel.ApplyFilters(this.viewModel.Elements);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.FilteredElements, Has.Count.EqualTo(1));
            });
        }
    }
}
