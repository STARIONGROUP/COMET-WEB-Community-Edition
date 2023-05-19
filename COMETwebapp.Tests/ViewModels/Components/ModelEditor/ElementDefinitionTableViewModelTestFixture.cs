// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTableViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.Tests.ViewModels.Components.ModelEdior
{
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ModelEditor;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ElementDefinitionTableViewModelTestFixture
    {
        private ElementDefinitionTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

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
            this.viewModel = new ElementDefinitionTableViewModel(this.sessionService.Object);
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
    }
}
