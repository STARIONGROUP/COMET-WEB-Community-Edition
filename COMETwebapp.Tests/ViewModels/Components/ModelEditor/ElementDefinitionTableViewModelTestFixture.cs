// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTableViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ModelEditor;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ElementDefinitionTableViewModelTestFixture
    {
        private ElementDefinitionTableViewModel viewModel;
		private DomainOfExpertise domain;
		private Mock<ISessionService> sessionService;
        private Iteration iteration;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

			this.domain = new DomainOfExpertise()
			{
				Iid = Guid.NewGuid(),
				ShortName = "SYS"
			};

			session.Setup(x => x.RetrieveSiteDirectory()).Returns(new SiteDirectory() { Domain = { this.domain } });

			var topElement = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "Container",
            };

            var elementDefinition = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "Box",
            };

            var usage1 = new ElementUsage()
            {
                Name = "Box1",
                Iid = Guid.NewGuid(),
                ElementDefinition = elementDefinition
            };

            topElement.ContainedElement.Add(usage1);

            this.iteration = new Iteration()
            {
                Iid = Guid.NewGuid(),
                Element = { topElement, elementDefinition }
            };

            var iterations = new SourceList<Iteration>();
            iterations.Add(this.iteration);

            this.sessionService.Setup(x => x.OpenIterations).Returns(iterations);
         
            this.viewModel = new ElementDefinitionTableViewModel(this.sessionService.Object, this.messageBus)
            {
                CurrentThing = this.iteration
            };
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Elements.Count, Is.EqualTo(3));
                Assert.That(this.viewModel.RowsSource, Has.Count.EqualTo(3));
                Assert.That(this.viewModel.RowsTarget, Has.Count.EqualTo(3));
                Assert.That(this.viewModel.RowsSource[0].ElementDefinitionName, Is.EqualTo("Container"));
                Assert.That(this.viewModel.RowsSource[0].ElementUsageName, Is.Null);
                Assert.That(this.viewModel.RowsSource[1].ElementDefinitionName, Is.EqualTo("Container"));
                Assert.That(this.viewModel.RowsSource[1].ElementUsageName, Is.EqualTo("Box1"));
                Assert.That(this.viewModel.RowsSource[0].ElementBase, Is.EqualTo(this.iteration.Element.FirstOrDefault()));
            });
        }

        [Test]
        public void VerifyRecordChange()
        {
            var elementDefinition = new ElementDefinition()
            {
                Iid = Guid.NewGuid()
            };

            this.iteration.Element.Add(elementDefinition);
            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Added);
            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate));

            this.messageBus.SendObjectChangeEvent(this.viewModel.RowsSource[0].ElementBase, EventKind.Removed);
            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate));

            this.messageBus.SendObjectChangeEvent(this.viewModel.RowsSource[0].ElementBase, EventKind.Updated);
            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate));

            Assert.That(this.viewModel.RowsSource, Has.Count.EqualTo(3));
        }

		[Test]
		public void VerifyElementCreationPopup()
		{
			Assert.That(this.viewModel.IsOnCreationMode, Is.False);

            this.viewModel.OpenCreateElementDefinitionCreationPopup();

            Assert.That(this.viewModel.IsOnCreationMode, Is.True);
		}

		[Test]
		public async Task VerifyAddingElementDefinition()
		{
			this.viewModel.ElementDefinitionCreationViewModel.ElementDefinition = new ElementDefinition
			{
				ShortName = "A",
				Name = "B",
				Owner = this.domain
			};

			this.viewModel.ElementDefinitionCreationViewModel.SelectedCategories = new List<Category> { new() { Name = "C" } };
			this.viewModel.ElementDefinitionCreationViewModel.IsTopElement = true;

			this.viewModel.ElementDefinitionCreationViewModel.ElementDefinition.Category = this.viewModel.ElementDefinitionCreationViewModel.SelectedCategories.ToList();

			await this.viewModel.ElementDefinitionCreationViewModel.OnValidSubmit.InvokeAsync();

			Assert.That(this.viewModel.IsOnCreationMode, Is.False);
		}
	}
}
