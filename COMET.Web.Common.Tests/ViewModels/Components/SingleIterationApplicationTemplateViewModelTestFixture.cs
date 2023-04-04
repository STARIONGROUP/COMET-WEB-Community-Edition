// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationTemplateViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Tests.ViewModels.Components
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;
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
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SessionService, Is.EqualTo(this.sessionService.Object));
                Assert.That(this.viewModel.IterationSelectorViewModel, Is.EqualTo(this.iterationSelectorViewModel.Object));
                Assert.That(this.viewModel.SelectedIteration, Is.Null);
                Assert.That(this.viewModel.IsOnIterationSelectionMode, Is.False);
            });
        }

        [Test]
        public void VerifyOnOpenIterationCountChanged()
        {
            this.openIterations.Add(new Iteration());
            Assert.That(this.viewModel.SelectedIteration, Is.Not.Null);
            this.viewModel.SelectedIteration = null;
            this.openIterations.Add(new Iteration());
            Assert.That(this.viewModel.SelectedIteration, Is.Null);
            this.openIterations.RemoveAt(1);
            Assert.That(this.viewModel.SelectedIteration, Is.Not.Null);
            this.openIterations.Clear();
            Assert.That(this.viewModel.SelectedIteration, Is.Null);
        }

        [Test]
        public void VerifySelectIteration()
        {
            var iterations = new List<Iteration>
            {
                new ()
                {
                    Iid = Guid.NewGuid()
                },
                new ()
                {
                    Iid = Guid.NewGuid()
                }
            };

            this.openIterations.AddRange(iterations);
            this.viewModel.AskToSelectIteration();
            
            Assert.Multiple(() =>
            {
                this.iterationSelectorViewModel.Verify(x => x.UpdateProperties(this.openIterations.Items), Times.Once);
                Assert.That(this.viewModel.IsOnIterationSelectionMode, Is.True);
            });

            this.viewModel.SelectIteration(iterations.First());
            
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnIterationSelectionMode, Is.False);
                Assert.That(this.viewModel.SelectedIteration, Is.EqualTo(iterations.First()));
            });
        }
    }
}
