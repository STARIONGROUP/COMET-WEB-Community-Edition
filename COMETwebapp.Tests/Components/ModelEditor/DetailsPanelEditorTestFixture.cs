// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DetailsPanelEditorTestFixture.cs" company="Starion Group S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DetailsPanelEditorTestFixture
    {
        /// <summary>
        /// The bunit <see cref="BunitContext" /> used to render the component under test.
        /// </summary>
        private BunitContext context;

        /// <summary>
        /// The mocked <see cref="IElementDefinitionDetailsViewModel" /> bound to the component.
        /// </summary>
        private Mock<IElementDefinitionDetailsViewModel> viewModel;

        /// <summary>
        /// Initializes the bunit context and a mock view model with empty <see cref="IElementDefinitionDetailsViewModel.Rows" />.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.viewModel = new Mock<IElementDefinitionDetailsViewModel>();
            this.viewModel.Setup(x => x.Rows).Returns(new List<ElementDefinitionDetailsRowViewModel>());
        }

        /// <summary>
        /// Disposes of the bunit context.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        /// <summary>
        /// Builds a fully-populated <see cref="ElementDefinition" /> with an owner, two categories and one definition.
        /// </summary>
        /// <returns>The built <see cref="ElementDefinition" />.</returns>
        private static ElementDefinition BuildElementDefinitionWithEverything()
        {
            var owner = new DomainOfExpertise { ShortName = "SYS", Name = "System" };

            var firstCategory = new Category { ShortName = "edCat", Name = "EdCategory", PermissibleClass = { ClassKind.ElementDefinition } };
            var secondCategory = new Category { ShortName = "edCat2", Name = "AnotherEdCategory", PermissibleClass = { ClassKind.ElementDefinition } };

            var definition = new Definition { Content = "An element definition used for testing.", LanguageCode = "en" };

            var element = new ElementDefinition
            {
                Name = "Battery",
                ShortName = "BAT",
                Owner = owner
            };

            element.Category.Add(firstCategory);
            element.Category.Add(secondCategory);
            element.Definition.Add(definition);

            return element;
        }

        [Test]
        public void VerifySummaryCardRendersWhenElementSelected()
        {
            var element = BuildElementDefinitionWithEverything();
            this.viewModel.Setup(x => x.SelectedSystemNode).Returns(element);

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));
            var markup = rendered.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Contain("cardview-detailspanel-summary"), "Summary card container must render.");
                Assert.That(markup, Does.Contain("Battery"), "Element Name must appear in the summary card.");
                Assert.That(markup, Does.Contain("BAT"), "Element ShortName must appear in the summary card.");
                Assert.That(markup, Does.Contain("SYS"), "Owner ShortName must appear in the summary card.");
                Assert.That(markup, Does.Contain("EdCategory"), "First category name must appear.");
                Assert.That(markup, Does.Contain("AnotherEdCategory"), "Second category name must appear.");
                Assert.That(markup, Does.Contain("An element definition used for testing."), "Definition content must appear.");
            });
        }

        [Test]
        public void VerifyNothingRendersWhenSelectionIsNull()
        {
            this.viewModel.Setup(x => x.SelectedSystemNode).Returns((ElementBase)null);

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));
            var markup = rendered.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Not.Contain("cardview-detailspanel-summary"), "Summary card must not render when no element is selected.");
                Assert.That(markup, Does.Not.Contain("cardview-detailspanel-scrollarea"), "Parameter CardView must not render when no element is selected.");
            });
        }

        [Test]
        public void VerifyCategoryRowHiddenWhenNoCategories()
        {
            var owner = new DomainOfExpertise { ShortName = "SYS", Name = "System" };
            var element = new ElementDefinition { Name = "Naked", ShortName = "NAK", Owner = owner };
            this.viewModel.Setup(x => x.SelectedSystemNode).Returns(element);

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));
            var markup = rendered.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Contain("cardview-detailspanel-summary"), "Summary card must still render.");
                Assert.That(markup, Does.Contain("Naked"), "Element Name must still appear.");
                Assert.That(markup, Does.Not.Contain("Category:"), "No category pill should be emitted when the element has no categories.");
            });
        }

        [Test]
        public void VerifyDefinitionRowHiddenWhenEmpty()
        {
            var owner = new DomainOfExpertise { ShortName = "SYS", Name = "System" };
            var element = new ElementDefinition { Name = "Plain", ShortName = "PLN", Owner = owner };
            this.viewModel.Setup(x => x.SelectedSystemNode).Returns(element);

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));
            var markup = rendered.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Contain("cardview-detailspanel-summary"), "Summary card must still render.");
                Assert.That(markup, Does.Not.Contain("border-top:1px dotted darkgray"), "Definition row marker must not appear when Definition is empty.");
            });
        }

        [Test]
        public void VerifyEditButtonRendersWhenCallbackBound()
        {
            var element = BuildElementDefinitionWithEverything();
            this.viewModel.Setup(x => x.SelectedSystemNode).Returns(element);

            var editInvoked = false;

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.OnEditElement, EventCallback.Factory.Create(this, () => editInvoked = true)));

            Assert.That(rendered.Markup, Does.Contain("oi-pencil"), "Edit button must render when OnEditElement is bound.");

            rendered.Find("#editElement").Click();
            Assert.That(editInvoked, Is.True, "Clicking Edit must invoke the OnEditElement callback.");
        }

        [Test]
        public void VerifyDeleteButtonRendersWhenCallbackBound()
        {
            var element = BuildElementDefinitionWithEverything();
            this.viewModel.Setup(x => x.SelectedSystemNode).Returns(element);

            var deleteInvoked = false;

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.OnDeleteElement, EventCallback.Factory.Create(this, () => deleteInvoked = true)));

            Assert.That(rendered.Markup, Does.Contain("oi-trash"), "Delete button must render when OnDeleteElement is bound.");

            rendered.Find("#deleteElement").Click();
            Assert.That(deleteInvoked, Is.True, "Clicking Delete must invoke the OnDeleteElement callback.");
        }

        [Test]
        public void VerifyDeleteButtonDisabledWhenDisableDeleteTrue()
        {
            var element = BuildElementDefinitionWithEverything();
            this.viewModel.Setup(x => x.SelectedSystemNode).Returns(element);

            var deleteInvoked = false;

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.OnDeleteElement, EventCallback.Factory.Create(this, () => deleteInvoked = true))
                .Add(p => p.DisableDelete, true));

            var deleteButton = rendered.Find("#deleteElement");

            Assert.Multiple(() =>
            {
                Assert.That(deleteButton.GetAttribute("disabled"), Is.Not.Null,
                    "Delete button must render disabled when DisableDelete is true (e.g. selection is the top element).");
                Assert.That(deleteButton.GetAttribute("title"), Does.Contain("top element"),
                    "Tooltip must mention the top-element rule when delete is disabled.");
            });

            Assert.That(deleteInvoked, Is.False, "Disabled Delete button must not invoke the callback even if rendered.");
        }

        [Test]
        public void VerifyEditAndDeleteButtonsHiddenWhenCallbacksUnbound()
        {
            var element = BuildElementDefinitionWithEverything();
            this.viewModel.Setup(x => x.SelectedSystemNode).Returns(element);

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));

            Assert.Multiple(() =>
            {
                Assert.That(rendered.Markup, Does.Not.Contain("oi-pencil"), "Edit button must not render when OnEditElement is unbound.");
                Assert.That(rendered.Markup, Does.Not.Contain("id=\"deleteElement\""), "Delete button must not render when OnDeleteElement is unbound.");
            });
        }

        [Test]
        public void VerifySubscribeButtonRendersOnlyWhenCanSubscribeAndCallbackBound()
        {
            var element = BuildElementDefinitionWithEverything();
            this.viewModel.Setup(x => x.SelectedSystemNode).Returns(element);

            var currentDomain = BuildDomain("SYS");
            var foreignDomain = BuildDomain("PWR");

            var subscribableParameter = BuildParameterOwnedBy(foreignDomain);
            var ownedParameter = BuildParameterOwnedBy(currentDomain);

            var rowAllowingSubscribe = new ElementDefinitionDetailsRowViewModel(subscribableParameter, currentDomain);
            var rowOwnedByCurrentDomain = new ElementDefinitionDetailsRowViewModel(ownedParameter, currentDomain);

            this.viewModel.Setup(x => x.Rows).Returns(new List<ElementDefinitionDetailsRowViewModel> { rowAllowingSubscribe, rowOwnedByCurrentDomain });

            Parameter capturedParameter = null;

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.OnCreateSubscription, EventCallback.Factory.Create<Parameter>(this, p => capturedParameter = p)));

            var subscribeButtons = rendered.FindAll(".oi-bell");

            Assert.Multiple(() =>
            {
                Assert.That(rowAllowingSubscribe.CanSubscribe, Is.True);
                Assert.That(rowOwnedByCurrentDomain.CanSubscribe, Is.False);
                Assert.That(subscribeButtons.Count, Is.EqualTo(1),
                    "Exactly one subscribe button must render — only on the row whose current domain can subscribe.");
            });

            subscribeButtons[0].Click();
            Assert.That(capturedParameter, Is.SameAs(subscribableParameter));
        }

        [Test]
        public void VerifyUnsubscribeButtonRendersOnlyWhenSubscribedAndCallbackBound()
        {
            var element = BuildElementDefinitionWithEverything();
            this.viewModel.Setup(x => x.SelectedSystemNode).Returns(element);

            var currentDomain = BuildDomain("SYS");
            var foreignDomain = BuildDomain("PWR");

            var subscribedParameter = BuildParameterOwnedBy(foreignDomain);

            var subscription = new ParameterSubscription { Iid = Guid.NewGuid(), Owner = currentDomain };
            subscribedParameter.ParameterSubscription.Add(subscription);

            var rowSubscribed = new ElementDefinitionDetailsRowViewModel(subscribedParameter, currentDomain);
            var rowSubscribable = new ElementDefinitionDetailsRowViewModel(BuildParameterOwnedBy(foreignDomain), currentDomain);

            this.viewModel.Setup(x => x.Rows).Returns(new List<ElementDefinitionDetailsRowViewModel> { rowSubscribed, rowSubscribable });

            ParameterSubscription capturedSubscription = null;

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.OnDeleteSubscription, EventCallback.Factory.Create<ParameterSubscription>(this, s => capturedSubscription = s)));

            var bellButtons = rendered.FindAll(".oi-bell");

            Assert.Multiple(() =>
            {
                Assert.That(rowSubscribed.HasCurrentDomainSubscription, Is.True);
                Assert.That(rowSubscribable.HasCurrentDomainSubscription, Is.False);
                Assert.That(bellButtons.Count, Is.EqualTo(1),
                    "Only the subscribed row must render an unsubscribe (bell) button when only OnDeleteSubscription is bound.");
            });

            bellButtons[0].Click();
            Assert.That(capturedSubscription, Is.SameAs(subscription));
        }

        [Test]
        public void VerifySubscribeAndUnsubscribeAreMutuallyExclusivePerCard()
        {
            var element = BuildElementDefinitionWithEverything();
            this.viewModel.Setup(x => x.SelectedSystemNode).Returns(element);

            var currentDomain = BuildDomain("SYS");
            var foreignDomain = BuildDomain("PWR");

            var subscribedParameter = BuildParameterOwnedBy(foreignDomain);
            subscribedParameter.ParameterSubscription.Add(new ParameterSubscription { Iid = Guid.NewGuid(), Owner = currentDomain });

            var rowSubscribed = new ElementDefinitionDetailsRowViewModel(subscribedParameter, currentDomain);

            this.viewModel.Setup(x => x.Rows).Returns(new List<ElementDefinitionDetailsRowViewModel> { rowSubscribed });

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.OnCreateSubscription, EventCallback.Factory.Create<Parameter>(this, _ => { }))
                .Add(p => p.OnDeleteSubscription, EventCallback.Factory.Create<ParameterSubscription>(this, _ => { })));

            var bellButtons = rendered.FindAll(".oi-bell");

            Assert.That(bellButtons.Count, Is.EqualTo(1),
                "Subscribe and unsubscribe affordances must never both render on the same card — the if/else if branch enforces this.");
        }

        /// <summary>
        /// Builds a <see cref="DomainOfExpertise" /> with the supplied short name.
        /// </summary>
        private static DomainOfExpertise BuildDomain(string shortName)
        {
            return new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = shortName, Name = shortName };
        }

        /// <summary>
        /// Builds a <see cref="Parameter" /> owned by the supplied <see cref="DomainOfExpertise" />. The
        /// parameter is placed inside a fresh <see cref="ElementDefinition" /> container because
        /// <see cref="Parameter.ModelCode" /> requires a container to be set.
        /// </summary>
        private static Parameter BuildParameterOwnedBy(DomainOfExpertise owner)
        {
            var parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), Name = "Mass", ShortName = "m" };

            var parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = owner,
                ParameterType = parameterType
            };

            parameter.ValueSet.Add(new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Manual = new CDP4Common.Types.ValueArray<string>(["1"]),
                Computed = new CDP4Common.Types.ValueArray<string>(["1"]),
                Reference = new CDP4Common.Types.ValueArray<string>(["-"]),
                Formula = new CDP4Common.Types.ValueArray<string>(["-"]),
                Published = new CDP4Common.Types.ValueArray<string>(["1"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            var containingDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "Container",
                ShortName = "CONT",
                Owner = owner
            };

            containingDefinition.Parameter.Add(parameter);

            return parameter;
        }

        [Test]
        public void VerifyElementUsageInheritsCategoriesFromDefinition()
        {
            var owner = new DomainOfExpertise { ShortName = "SYS", Name = "System" };

            var inheritedCategory = new Category { ShortName = "edCat", Name = "InheritedCategory", PermissibleClass = { ClassKind.ElementDefinition } };

            var elementDefinition = new ElementDefinition { Name = "BatteryDef", ShortName = "BATDEF", Owner = owner };
            elementDefinition.Category.Add(inheritedCategory);

            var elementUsage = new ElementUsage
            {
                Name = "BatteryUsage",
                ShortName = "BATU",
                Owner = owner,
                ElementDefinition = elementDefinition
            };

            this.viewModel.Setup(x => x.SelectedSystemNode).Returns(elementUsage);

            var rendered = this.context.Render<DetailsPanelEditor>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));
            var markup = rendered.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Contain("BatteryUsage"), "ElementUsage Name must appear.");
                Assert.That(markup, Does.Contain("BATU"), "ElementUsage ShortName must appear.");
                Assert.That(markup, Does.Contain("InheritedCategory"),
                    "ElementUsage must surface the referenced ElementDefinition's categories via GetAllCategories().");
            });
        }
    }
}
