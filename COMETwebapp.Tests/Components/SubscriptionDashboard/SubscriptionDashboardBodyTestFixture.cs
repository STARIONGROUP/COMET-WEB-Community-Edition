// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscriptionDashboardBodyTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Components.SubscriptionDashboard
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using DynamicData;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.Utilities;

    using COMETwebapp.Components.SubscriptionDashboard;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionDashboardBodyTestFixture
    {
        private BunitContext context;
        private ISubscriptionDashboardBodyViewModel viewModel;
        private Mock<ISubscriptionService> subscriptionService;
        private Mock<ISessionService> sessionService;
        private ISubscribedTableViewModel subscribedTableViewModel;
        private CDPMessageBus messageBus;
        private SourceList<Iteration> openIterations;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.sessionService = new Mock<ISessionService>();
            this.openIterations = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.openIterations);
            this.subscriptionService = new Mock<ISubscriptionService>();
            this.subscriptionService.Setup(x => x.SubscriptionsWithUpdate).Returns(new Dictionary<Guid, List<Guid>>());
            this.subscribedTableViewModel = new SubscribedTableViewModel(this.subscriptionService.Object);
            this.messageBus = new CDPMessageBus();
            this.viewModel = new SubscriptionDashboardBodyViewModel(this.sessionService.Object, this.subscribedTableViewModel, this.messageBus);
            
            this.context.ConfigureDevExpressBlazor();
            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());
            this.context.Services.AddSingleton(configuration.Object);
            this.context.Services.AddSingleton(this.viewModel);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyWithoutInitializationValueComponent()
        {
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(new DomainOfExpertise() { Name = "Thermal" });
            
            var iteration = new Iteration();
            this.openIterations.Add(iteration);

            _ = this.context.Render<SubscriptionDashboardBody>(parameters =>
            {
                parameters.Add(p => p.CurrentThing, iteration);
            });

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.OptionSelector.SelectedOptions, Is.Empty);
                Assert.That(this.viewModel.ParameterTypeSelector.SelectedParameterTypes, Is.Empty);
            });
        }

        /// <summary>
        /// Verifies that the multi-value "parameters" URL key is applied on initialization, that changing
        /// a selector afterward round-trips the selection through <c>UpdateUrl</c>, and that clicking a
        /// missing value in the domain-of-expertise table redirects to the Parameter Editor.
        /// </summary>
        [Test]
        public void VerifyMultiValueInitializationAndUpdateUrlRoundTrip()
        {
            var thermal = new DomainOfExpertise { Iid = Guid.NewGuid(), Name = "Thermal" };
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(thermal);

            var navigation = this.context.Services.GetService<NavigationManager>();

            var parameterId = Guid.NewGuid();
            var optionId = Guid.NewGuid();

            var queryParameters = new Dictionary<string, string>
            {
                [QueryKeys.OptionsKey] = optionId.ToShortGuid(),
                [QueryKeys.ParametersKey] = parameterId.ToShortGuid()
            };

            navigation.NavigateTo(QueryHelpers.AddQueryString("http://localhost", queryParameters));

            var parameterType = new TextParameterType { Iid = parameterId };

            var parameter = new Parameter
            {
                ParameterType = parameterType,
                Owner = thermal,
                ParameterSubscription = { new ParameterSubscription { Iid = Guid.NewGuid(), Owner = new DomainOfExpertise() } }
            };

            var iteration = new Iteration { Element = { new ElementDefinition { Parameter = { parameter } } } };
            iteration.TopElement = iteration.Element[0];
            iteration.Option.Add(new Option { Iid = optionId });
            this.openIterations.Add(iteration);

            var rendered = this.context.Render<SubscriptionDashboardBody>(parameters => parameters.Add(p => p.CurrentThing, iteration));

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.OptionSelector.SelectedOptions, Is.Not.Empty, "The multi-value 'parameters' URL key must apply the option filter.");
                Assert.That(this.viewModel.ParameterTypeSelector.SelectedParameterTypes, Is.Not.Empty, "The multi-value 'parameters' URL key must apply the parameter-type filter.");
            });

            Assert.That(() => this.viewModel.OptionSelector.SelectedOptions = [], Throws.Nothing,
                "Clearing the selection must exercise UpdateUrl without throwing.");

            var domainTable = rendered.FindComponent<DomainOfExpertiseSubscriptionTable>();

            Assert.That(async () => await rendered.InvokeAsync(() => domainTable.Instance.OnMissingValueClick.InvokeAsync(parameter)), Throws.Nothing,
                "Clicking a missing value must redirect to the Parameter Editor without throwing.");
        }

        [Test]
        public void VerifyWithValidInitialValuesComponent()
        {
            var thermal = new DomainOfExpertise()
            {
                Iid = Guid.NewGuid(),
                Name = "Thermal"
            };

            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(thermal);

            var navigation = this.context.Services.GetService<NavigationManager>();

            var parameterId = Guid.NewGuid();
            var optionId = Guid.NewGuid();

            var queryParameters = new Dictionary<string, string>
            {
                [QueryKeys.OptionsKey] = optionId.ToShortGuid(),
                [QueryKeys.ParameterKey] = parameterId.ToShortGuid()
            };

            var url = QueryHelpers.AddQueryString("http://localhost", queryParameters);

            navigation.NavigateTo(url);

            var iteration = new Iteration();
            iteration.Option.Add(new Option(){Iid = optionId });

            var parameterType = new TextParameterType()
            {
                Iid = parameterId
            };

            iteration.Element.Add(new ElementDefinition()
            {
                Parameter =
                {
                    new Parameter()
                    {
                        ParameterType = parameterType , 
                        Owner = thermal,
                        ParameterSubscription =
                        {
                            new ParameterSubscription()
                            {
                                Iid = Guid.NewGuid(),
                                Owner = new DomainOfExpertise()
                            }
                        }
                    }
                }
            });

            iteration.TopElement = iteration.Element[0];
            this.openIterations.Add(iteration);

            _ = this.context.Render<SubscriptionDashboardBody>(parameters =>
            {
                parameters.Add(p => p.CurrentThing, iteration);
            });

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.OptionSelector.SelectedOptions, Is.Not.Empty);
                Assert.That(this.viewModel.ParameterTypeSelector.SelectedParameterTypes, Is.Not.Empty);
            });

            var mockedViewModel = new Mock<ISubscriptionDashboardBodyViewModel>();
            mockedViewModel.Setup(x => x.CurrentDomain).Returns(this.viewModel.CurrentDomain);
            mockedViewModel.Setup(x => x.CurrentThing).Returns(this.viewModel.CurrentThing);
            mockedViewModel.Setup(x => x.DomainOfExpertiseSubscriptionTable).Returns(this.viewModel.DomainOfExpertiseSubscriptionTable);
            mockedViewModel.Setup(x => x.SubscribedTable).Returns(this.viewModel.SubscribedTable);
            mockedViewModel.Setup(x => x.OptionSelector).Returns(this.viewModel.OptionSelector);
            mockedViewModel.Setup(x => x.ParameterTypeSelector).Returns(this.viewModel.ParameterTypeSelector);

            var rendered = this.context.Render<SubscriptionDashboardBody>(parameters =>
            {
                parameters.Add(p => p.CurrentThing, iteration);
                parameters.Add(p => p.ParameterizedViewModel, mockedViewModel.Object);
            });

            Assert.Multiple(() =>
            {
                Assert.That(rendered.Instance.ViewModel, Is.EqualTo(mockedViewModel.Object));
                Assert.That(rendered.Instance.InjectedViewModel, Is.EqualTo(null));
            });
        }
    }
}
