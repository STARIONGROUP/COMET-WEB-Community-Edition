// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscriptionDashboardBodyTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.Utilities;

    using COMETwebapp.Components.SubscriptionDashboard;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class SubscriptionDashboardBodyTestFixture
    {
        private TestContext context;
        private ISubscriptionDashboardBodyViewModel viewModel;
        private Mock<ISubscriptionService> subscriptionService;
        private Mock<ISessionService> sessionService;
        private ISubscribedTableViewModel subscribedTableViewModel;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.sessionService = new Mock<ISessionService>();
            this.subscriptionService = new Mock<ISubscriptionService>();
            this.subscriptionService.Setup(x => x.SubscriptionsWithUpdate).Returns(new Dictionary<Guid, List<Guid>>());
            this.subscribedTableViewModel = new SubscribedTableViewModel(this.subscriptionService.Object);
            this.messageBus = new CDPMessageBus();
            this.viewModel = new SubscriptionDashboardBodyViewModel(this.sessionService.Object, this.subscribedTableViewModel, this.messageBus);
            
            this.context.ConfigureDevExpressBlazor();
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x.GetSection(ConfigurationKeys.ServerConfigurationKey).Get<ServerConfiguration>()).Returns(new ServerConfiguration());
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
            
            _ = this.context.RenderComponent<SubscriptionDashboardBody>(parameters =>
            {
                parameters.Add(p => p.CurrentThing, new Iteration());
            });

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.OptionSelector.SelectedOption, Is.Null);
                Assert.That(this.viewModel.ParameterTypeSelector.SelectedParameterType, Is.Null);
            });
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
                [QueryKeys.OptionKey] = optionId.ToShortGuid(),
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

            _ = this.context.RenderComponent<SubscriptionDashboardBody>(parameters =>
            {
                parameters.Add(p => p.CurrentThing, iteration);
            });

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.OptionSelector.SelectedOption, Is.Not.Null);
                Assert.That(this.viewModel.ParameterTypeSelector.SelectedParameterType, Is.Not.Null);
            });
        }
    }
}
