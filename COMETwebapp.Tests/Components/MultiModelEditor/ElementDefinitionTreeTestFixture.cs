// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTreeTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.MultiModelEditor
{
    using Bunit;
    using Bunit.Rendering;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.MultiModelEditor;
    using COMETwebapp.ViewModels.Components.MultiModelEditor;
    using COMETwebapp.ViewModels.Components.MultiModelEditor.Rows;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ElementDefinitionTreeTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ElementDefinitionTree> renderedComponent;
        private ElementDefinitionTree tree;
        private Mock<IElementDefinitionTreeViewModel> elementDefinitionTreeViewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());
            var elementDefinition = new ElementDefinition();

            var row = new ElementDefinitionTreeRowViewModel
            {
                ElementName = "Test1",
                ElementBase = elementDefinition,
                OwnerShortName = "Owner",
                IsTopElement = true
            };

            this.elementDefinitionTreeViewModel = new Mock<IElementDefinitionTreeViewModel>();
            this.elementDefinitionTreeViewModel.Setup(x => x.Rows).Returns([row]);

            this.context.Services.AddSingleton(configuration.Object);
            this.context.Services.AddSingleton(this.elementDefinitionTreeViewModel.Object);
            this.context.Services.AddSingleton<ISessionService, SessionService>();

            this.renderedComponent = this.context.RenderComponent<ElementDefinitionTree>();
            this.tree = this.renderedComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderedComponent, Is.Not.Null);
                Assert.That(this.tree, Is.Not.Null);
                Assert.That(this.tree.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyElementSelection()
        {
            ElementBaseTreeRowViewModel selectedModel = null;

            this.renderedComponent = this.context.RenderComponent<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.SelectionChanged, model => selectedModel = model);
            });

            var treeView = this.renderedComponent.FindComponent<DxTreeView>();

            Assert.Multiple(() =>
            {
                Assert.That(treeView.Instance.GetSelectedNodeInfo(), Is.Null);
                Assert.That(selectedModel, Is.Null);
            });

            var firstSourceRow = treeView.Find(".dxbl-treeview-item-container");
            firstSourceRow.Click();

            Assert.Multiple(() =>
            {
                Assert.That(treeView.Instance.GetSelectedNodeInfo(), Is.Not.Null);
                Assert.That(selectedModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyIsModelSelectionEnabled()
        {
            Assert.That(() => this.renderedComponent.FindComponent<DxComboBox<IterationData, IterationData>>(), Throws.TypeOf<ComponentNotFoundException>());

            this.renderedComponent = this.context.RenderComponent<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.IsModelSelectionEnabled, true);
            });

            Assert.That(this.renderedComponent.FindComponent<DxComboBox<IterationData, IterationData>>(), Is.Not.Null);
        }

        [Test]
        public void VerifyInitialIteration()
        {
            var iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup
                {
                    IterationNumber = 1,
                    Container = new EngineeringModelSetup
                    {
                        Iid = Guid.NewGuid(),
                        Name = "ModelName",
                        ShortName = "ModelShortName"
                    }
                }
            };

            this.elementDefinitionTreeViewModel.VerifySet(x => x.Iteration = iteration, Times.Never);

            this.renderedComponent = this.context.RenderComponent<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.InitialIteration, iteration);
            });

            this.elementDefinitionTreeViewModel.VerifySet(x => x.Iteration = iteration, Times.Once);
        }

        [Test]
        public void VerifyInitialIterationChangeFromViewModel()
        {
            var iteration1 = new Iteration
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup
                {
                    IterationNumber = 1,
                    Container = new EngineeringModelSetup
                    {
                        Iid = Guid.NewGuid(),
                        Name = "ModelName",
                        ShortName = "ModelShortName"
                    }
                }
            };

            var iteration2 = new Iteration
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup
                {
                    IterationNumber = 2,
                    Container = iteration1.IterationSetup.Container
                }
            };

            this.elementDefinitionTreeViewModel.VerifySet(x => x.Iteration = iteration1, Times.Never);

            this.renderedComponent = this.context.RenderComponent<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.InitialIteration, iteration1);
            });

            this.elementDefinitionTreeViewModel.VerifySet(x => x.Iteration = iteration1, Times.Once);
            this.elementDefinitionTreeViewModel.Setup(x => x.Iteration).Returns(iteration2);

            this.renderedComponent.Render();

            Assert.That(this.renderedComponent.Instance.InitialIteration, Is.EqualTo(iteration2));
        }

        [Test]
        public void VerifyDragIsNotAllowed()
        {
            var firstItem = this.renderedComponent.Find(".dxbl-text").FirstElementChild;

            Assert.Multiple(() =>
            {
                Assert.That(firstItem, Is.Not.Null);
                Assert.That(firstItem.InnerHtml, Contains.Substring("Test1"));
                Assert.That(firstItem.Attributes.Length, Is.EqualTo(2));
            });
        }

        [Test]
        public void VerifyDragIsAllowed()
        {
            this.renderedComponent = this.context.RenderComponent<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.AllowDrag, true);
            });

            var firstItem = this.renderedComponent.Find(".dxbl-text").FirstElementChild;

            Assert.Multiple(() =>
            {
                Assert.That(firstItem, Is.Not.Null);
                Assert.That(firstItem.InnerHtml, Contains.Substring("Test1"));
                Assert.That(firstItem.Attributes.Length, Is.EqualTo(10));
            });
        }
    }
}
