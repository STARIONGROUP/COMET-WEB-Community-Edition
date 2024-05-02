// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationTemplateViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.ViewModels.Components.Applications
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SingleIterationApplicationTemplateViewModelTestFixture
    {
        private SingleIterationApplicationTemplateViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IIterationSelectorViewModel> iterationSelectorViewModel;
        private SourceList<Iteration> openIterations;

        [SetUp]
        public void Setup()
        {
            this.openIterations = new SourceList<Iteration>();
            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.openIterations);
            this.iterationSelectorViewModel = new Mock<IIterationSelectorViewModel>();
            this.viewModel = new SingleIterationApplicationTemplateViewModel(this.sessionService.Object, this.iterationSelectorViewModel.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyOnOpenIterationCountChanged()
        {
            var iterationToAdd = new Iteration()
            {
                IterationSetup = new IterationSetup(Guid.NewGuid(), null, null)
                {
                    Container = new EngineeringModelSetup(Guid.NewGuid(), null, null)
                }
            };

            this.openIterations.Add(iterationToAdd);
            Assert.That(this.viewModel.SelectedThing, Is.Not.Null);
            this.viewModel.SelectedThing = null;
            this.openIterations.Add(new Iteration());
            Assert.That(this.viewModel.SelectedThing, Is.Null);
            this.openIterations.RemoveAt(1);
            Assert.That(this.viewModel.SelectedThing, Is.Not.Null);
            this.openIterations.Clear();
            Assert.That(this.viewModel.SelectedThing, Is.Null);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SessionService, Is.EqualTo(this.sessionService.Object));
                Assert.That(this.viewModel.IterationSelectorViewModel, Is.EqualTo(this.iterationSelectorViewModel.Object));
                Assert.That(this.viewModel.SelectedThing, Is.Null);
                Assert.That(this.viewModel.IsOnSelectionMode, Is.False);
            });
        }

        [Test]
        public void VerifySelectIteration()
        {
            var iterations = new List<Iteration>
            {
                new()
                {
                    Iid = Guid.NewGuid(),
                    IterationSetup = new IterationSetup(Guid.NewGuid(), null, null)
                    {
                        IterationNumber = 1,
                        Container = new EngineeringModelSetup(Guid.NewGuid(), null, null)
                    }
                },
                new()
                {
                    Iid = Guid.NewGuid()
                }
            };

            this.openIterations.AddRange(iterations);
            this.viewModel.AskToSelectThing();

            Assert.Multiple(() =>
            {
                this.iterationSelectorViewModel.Verify(x => x.UpdateProperties(this.openIterations.Items), Times.Once);
                Assert.That(this.viewModel.IsOnSelectionMode, Is.True);
            });

            this.viewModel.OnThingSelect(iterations[0]);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnSelectionMode, Is.False);
                Assert.That(this.viewModel.SelectedThing, Is.EqualTo(iterations[0]));
            });
        }
    }
}
