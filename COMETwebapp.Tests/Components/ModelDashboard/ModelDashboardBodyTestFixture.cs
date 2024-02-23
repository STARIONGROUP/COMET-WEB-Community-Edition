// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelDashboardBodyTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.ModelDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.Utilities;

    using COMETwebapp.Components.ModelDashboard;
    using COMETwebapp.Components.ModelDashboard.Elements;
    using COMETwebapp.Components.ModelDashboard.ParameterValues;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.ModelDashboard;
    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    public class ModelDashboardBodyTestFixture
    {
        private TestContext context;
        private ModelDashboardBodyViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private ICDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.sessionService = new Mock<ISessionService>();
            this.messageBus = new CDPMessageBus();

            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddAntDesign();
            this.context.Services.AddSingleton<IModelDashboardBodyViewModel, ModelDashboardBodyViewModel>();
            this.context.Services.AddSingleton<IParameterDashboardViewModel, ParameterDashboardViewModel>();
            this.context.Services.AddSingleton(this.sessionService.Object);
            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());
            this.context.Services.AddSingleton(configuration.Object);
            this.context.Services.AddSingleton(this.messageBus);
            this.viewModel = this.context.Services.GetService<IModelDashboardBodyViewModel>() as ModelDashboardBodyViewModel;
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyModelDashboardComponent()
        {
            var renderer = this.context.RenderComponent<ModelDashboardBody>();
            Assert.That(this.viewModel.CurrentThing, Is.Null);
            this.messageBus.SendMessage(new DomainChangedEvent(null, null));
            Assert.That(this.viewModel.CurrentDomain, Is.Null);

            var iteration = new Iteration()
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup()
                {
                    Iid = Guid.NewGuid(),
                    Container = new EngineeringModelSetup()
                    {
                        Iid = Guid.NewGuid()
                    }
                }
            };

            var systemDomain = new DomainOfExpertise()
            {
                Iid = Guid.NewGuid(),
                ShortName = "SYS"
            };

            var themalDomain = new DomainOfExpertise()
            {
                Iid = Guid.NewGuid(),
                ShortName = "THE"
            };

            var domains = new List<DomainOfExpertise>{ systemDomain, themalDomain };
            this.sessionService.Setup(x => x.GetModelDomains(It.IsAny<EngineeringModelSetup>())).Returns(domains);
            this.sessionService.Setup(x => x.GetDomainOfExpertise(iteration)).Returns(systemDomain);

            var option = new Option()
            {
                Iid = Guid.NewGuid(),
                Name = "Option 1"
            };

            iteration.Option.Add(option);

            var actualFiniteStateList = new ActualFiniteStateList()
            {
                Iid = Guid.NewGuid()
            };

            var possibleFiniteStateList = new List<PossibleFiniteState>{ new () {Iid = Guid.NewGuid(), Name = "State1"}, new () { Iid = Guid.NewGuid(), Name = "State2" }};
            
            actualFiniteStateList.PossibleFiniteStateList.Add(new PossibleFiniteStateList()
            {
                PossibleState = { possibleFiniteStateList[0], possibleFiniteStateList[1] }
            });

            actualFiniteStateList.ActualState.Add(new ActualFiniteState(){Iid = Guid.NewGuid(), PossibleState = { possibleFiniteStateList[0] } });
            actualFiniteStateList.ActualState.Add(new ActualFiniteState(){Iid = Guid.NewGuid(), PossibleState = { possibleFiniteStateList[1] } });
            iteration.ActualFiniteStateList.Add(actualFiniteStateList);

            var scale = new RatioScale()
            {
                Name = "percent",
                ShortName = "%"
            };

            var massParameterType = new SimpleQuantityKind()
            {
                Iid = Guid.NewGuid(),
                ShortName = "m",
                Name = "mass"
            };

            var textParameter = new TextParameterType()
            {
                Iid = Guid.NewGuid(),
                ShortName = "entry",
                Name = "Entry"
            };

            var compoundType = new CompoundParameterType()
            {
                Iid = Guid.NewGuid(),
                Component = 
                { 
                    new ParameterTypeComponent()
                    {
                        ParameterType = massParameterType
                    },
                    new ParameterTypeComponent()
                    {
                        ParameterType = textParameter
                    }
                }
            };

            var element1 = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "Accelorometer box",
                ShortName = "acc_box",
                Owner = systemDomain
            };

            var parameter1 = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = massParameterType,
                Scale = scale,
                Owner = themalDomain
            };

            var currentValues = new List<string> { "2" };
            var publishedValues = new List<string> { "123" };
            var noValues = new List<string> { "-" };

            parameter1.ValueSet.Add(new ParameterValueSet()
            {
                Iid = Guid.NewGuid(),
                Published = new ValueArray<string>(publishedValues),
                Manual = new ValueArray<string>(currentValues),
                Formula = new ValueArray<string>(noValues),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            });

            var parameter2 = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = textParameter,
                IsOptionDependent = true,
                Owner = themalDomain
            };

            parameter2.ValueSet.Add(new ParameterValueSet()
            {
                Iid = Guid.NewGuid(),
                Manual = new ValueArray<string>(noValues),
                Published = new ValueArray<string>(noValues),
                Formula = new ValueArray<string>(noValues),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ActualOption = option
            });

            element1.Parameter.Add(parameter1);
            element1.Parameter.Add(parameter2);

            var element2 = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "Power Engine",
                ShortName = "pwr_engine",
                Owner = systemDomain,
                ContainedElement = { new ElementUsage(){ Iid = Guid.NewGuid(), ElementDefinition = element1} }
            };

            var parameter3 = new Parameter()
            {
                Iid = Guid.NewGuid(),
                ParameterType = compoundType,
                StateDependence = actualFiniteStateList,
                Owner = systemDomain
            };

            var compoundValues = new List<string> { "2" , "a"};

            parameter3.ValueSet.Add(new ParameterValueSet()
            {
                ActualState = actualFiniteStateList.ActualState[0],
                Published = new ValueArray<string>(compoundValues),
                Formula = new ValueArray<string>(noValues),
                Manual = new ValueArray<string>(compoundValues),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            parameter3.ValueSet.Add(new ParameterValueSet()
            {
                ActualState = actualFiniteStateList.ActualState[^1],
                Published = new ValueArray<string>(compoundValues),
                Formula = new ValueArray<string>(noValues),
                Manual = new ValueArray<string>(compoundValues),
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            element2.Parameter.Add(parameter3);

            var element3 = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "something",
                ShortName = "smthg",
                Owner = themalDomain
            };

            iteration.Element.AddRange(new List<ElementDefinition>{element1, element2, element3});
            iteration.TopElement = element1;
            this.viewModel.CurrentThing = iteration;
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.OptionSelector.AvailableOptions.ToList(), Has.Count.EqualTo(1));
                Assert.That(this.viewModel.FiniteStateSelector.AvailableFiniteStates.ToList(), Has.Count.EqualTo(2));
                Assert.That(this.viewModel.ParameterTypeSelector.AvailableParameterTypes.ToList(), Has.Count.EqualTo(3));
                Assert.That(this.viewModel.ElementDashboard.UnreferencedElements, Is.Not.Empty);
                Assert.That(this.viewModel.ElementDashboard.UnusedElements, Is.Not.Empty);
            });

            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(themalDomain);
            this.messageBus.SendMessage(new DomainChangedEvent(iteration, themalDomain));
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);
            await Task.Delay(1000);

            var buttonToDo = renderer.Find(".todo-btn");
            await renderer.InvokeAsync(() => buttonToDo.ClickAsync(new MouseEventArgs()));
            var navigation = this.context.Services.GetService<NavigationManager>();
            Assert.That(navigation.Uri, Does.Not.Contain("ParameterEditor"));

            var options = new Dictionary<string, string>
            {
                [QueryKeys.IterationKey] = Guid.NewGuid().ToShortGuid(),
                [QueryKeys.ModelKey] = Guid.NewGuid().ToShortGuid(),
                [QueryKeys.ServerKey] = "http://localhost",
                [QueryKeys.DomainKey] = Guid.NewGuid().ToShortGuid()
            };

            navigation.NavigateTo(QueryHelpers.AddQueryString("http://localhost/", options));
            await renderer.InvokeAsync(() => buttonToDo.ClickAsync(new MouseEventArgs()));
            Assert.That(navigation.Uri, Does.Contain("ParameterEditor"));

            var elementDashboard = renderer.FindComponent<ElementDashboard>();
            var parameterDashboard = renderer.FindComponent<ParameterDashboard>();

            Assert.Multiple(() =>
            {
                Assert.That(() => elementDashboard.Instance.OnAccessData((WebAppConstantValues.UnusedElements, "SYS")), Throws.Nothing);
                Assert.That(() => elementDashboard.Instance.OnAccessData((WebAppConstantValues.UsedElements, "SYS")), Throws.Nothing);
                Assert.That(() => elementDashboard.Instance.OnAccessData((WebAppConstantValues.UnreferencedElements, "SYS")), Throws.Nothing);
                Assert.That(() => elementDashboard.Instance.OnAccessData((WebAppConstantValues.ReferencedElements, "SYS")), Throws.Nothing);
                Assert.That(() => elementDashboard.Instance.OnAccessData(("Referenced", "SYS")), Throws.Nothing);
                Assert.That(() => parameterDashboard.Instance.OnAccessData((WebAppConstantValues.PublishedParameters, "THE")), Throws.Nothing);
                Assert.That(() => parameterDashboard.Instance.OnAccessData((WebAppConstantValues.PublishableParameters, "THE")), Throws.Nothing);    
                Assert.That(() => parameterDashboard.Instance.OnAccessData((WebAppConstantValues.ParametersWithMissingValues, "THE")), Throws.Nothing);    
                Assert.That(() => parameterDashboard.Instance.OnAccessData((WebAppConstantValues.ParametersWithValues, "THE")), Throws.Nothing);    
                Assert.That(() => parameterDashboard.Instance.OnAccessData(("Referenced", "THE")), Throws.Nothing);  
            });

            this.viewModel.OptionSelector.SelectedOption = this.viewModel.OptionSelector.AvailableOptions.First();
            Assert.That(navigation.Uri, Does.Contain("option="));

            this.viewModel.FiniteStateSelector.SelectedActualFiniteState = this.viewModel.FiniteStateSelector.AvailableFiniteStates.First();
            Assert.That(navigation.Uri, Does.Contain("state="));

            this.viewModel.ParameterTypeSelector.SelectedParameterType = this.viewModel.ParameterTypeSelector.AvailableParameterTypes.First();
            Assert.That(navigation.Uri, Does.Contain("parameter="));

            Assert.That(() => this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate)), Throws.Nothing);
        }
    }
}
