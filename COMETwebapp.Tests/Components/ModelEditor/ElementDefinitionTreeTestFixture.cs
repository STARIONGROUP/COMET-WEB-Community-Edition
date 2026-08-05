// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTreeTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Tests.Components.ModelEditor
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

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.Rows;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;


    [TestFixture]
    public class ElementDefinitionTreeTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<ElementDefinitionTree> renderedComponent;
        private ElementDefinitionTree tree;
        private Mock<IElementDefinitionTreeViewModel> elementDefinitionTreeViewModel;

        [SetUp]
        public void SetUp()
        {
            context = new BunitContext();
            context.ConfigureDevExpressBlazor();

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

            elementDefinitionTreeViewModel = new Mock<IElementDefinitionTreeViewModel>();
            elementDefinitionTreeViewModel.Setup(x => x.Rows).Returns([row]);

            context.Services.AddSingleton(configuration.Object);
            context.Services.AddSingleton(elementDefinitionTreeViewModel.Object);
            context.Services.AddSingleton<ISessionService, SessionService>();

            renderedComponent = context.Render<ElementDefinitionTree>();
            tree = renderedComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(renderedComponent, Is.Not.Null);
                Assert.That(tree, Is.Not.Null);
                Assert.That(tree.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyElementSelection()
        {
            ElementBaseTreeRowViewModel selectedModel = null;

            renderedComponent = context.Render<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.SelectionChanged, model => selectedModel = model);
            });

            var treeView = renderedComponent.FindComponent<DxTreeView>();

            Assert.Multiple(() =>
            {
                Assert.That(treeView.Instance.GetSelectedNodeInfo(), Is.Null);
                Assert.That(selectedModel, Is.Null);
            });

            var firstSourceRow = treeView.Find("[data-testid=element-node]");
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
            Assert.That(() => renderedComponent.FindComponent<DxComboBox<IterationData, IterationData>>(), Throws.TypeOf<ComponentNotFoundException>());

            renderedComponent = context.Render<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.IsModelSelectionEnabled, true);
            });

            Assert.That(renderedComponent.FindComponent<DxComboBox<IterationData, IterationData>>(), Is.Not.Null);
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

            elementDefinitionTreeViewModel.VerifySet(x => x.Iteration = iteration, Times.Never);

            renderedComponent = context.Render<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.InitialIteration, iteration);
            });

            elementDefinitionTreeViewModel.VerifySet(x => x.Iteration = iteration, Times.Once);
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

            elementDefinitionTreeViewModel.VerifySet(x => x.Iteration = iteration1, Times.Never);

            renderedComponent = context.Render<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.InitialIteration, iteration1);
            });

            elementDefinitionTreeViewModel.VerifySet(x => x.Iteration = iteration1, Times.Once);
            elementDefinitionTreeViewModel.Setup(x => x.Iteration).Returns(iteration2);

            renderedComponent.Render();

            Assert.That(renderedComponent.Instance.InitialIteration, Is.EqualTo(iteration2));
        }

        /// <summary>
        /// Verifies that the owner and the category pills can each be toggled off on the tree nodes, the way the product tree
        /// of the System Representation allows it
        /// </summary>
        [Test]
        public void VerifyOwnerAndCategoryPillsCanBeToggled()
        {
            // The pills are shown by default.
            Assert.That(renderedComponent.FindAll(".starion-pill"), Is.Not.Empty);

            renderedComponent = context.Render<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.ShowOwner, false)
                    .Add(p => p.ShowCategories, false);
            });

            Assert.That(renderedComponent.FindAll(".starion-pill"), Is.Empty, "Unchecking both options must hide every pill.");
        }

        /// <summary>
        /// Verifies that unchecking <see cref="ElementDefinitionTree.ShowName" /> renders the node's short
        /// name instead of the <see cref="CardField" />-driven full name.
        /// </summary>
        [Test]
        public void VerifyShowNameFalseRendersShortName()
        {
            renderedComponent = context.Render<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.ShowName, false);
            });

            var firstItem = renderedComponent.Find("[data-testid=element-node]");

            Assert.That(firstItem.TextContent, Does.Not.Contain("Test1"),
                "With ShowName off, the node must fall back to the ElementBase ShortName instead of the row's full name.");
        }

        [Test]
        public void VerifyDragIsNotAllowed()
        {
            var firstItem = renderedComponent.Find("[data-testid=element-node]");

            Assert.Multiple(() =>
            {
                Assert.That(firstItem, Is.Not.Null);
                Assert.That(firstItem.InnerHtml, Contains.Substring("Test1"));
                Assert.That(firstItem.Attributes.Length, Is.EqualTo(3));
            });
        }

        [Test]
        public void VerifyDragIsAllowed()
        {
            renderedComponent = context.Render<ElementDefinitionTree>(parameters =>
            {
                parameters
                    .Add(p => p.AllowDrag, true);
            });

            var firstItem = renderedComponent.Find("[data-testid=element-node]");

            Assert.Multiple(() =>
            {
                Assert.That(firstItem, Is.Not.Null);
                Assert.That(firstItem.InnerHtml, Contains.Substring("Test1"));
                Assert.That(firstItem.Attributes.Length, Is.EqualTo(11));
            });
        }
    }
}
