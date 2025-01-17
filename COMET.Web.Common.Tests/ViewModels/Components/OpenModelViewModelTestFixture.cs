﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenModelViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.ViewModels.Components
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class OpenModelViewModelTestFixture
    {
        private OpenModelViewModel viewModel;
        private Mock<IConfigurationService> configurationService;
        private Mock<ISessionService> sessionService;
        private Mock<ICacheService> cacheService;
        private const string RdlShortName = "filterRdl";
        private List<EngineeringModelSetup> models;

        [SetUp]
        public void Setup()
        {
            this.configurationService = new Mock<IConfigurationService>();
            this.sessionService = new Mock<ISessionService>();
            this.cacheService = new Mock<ICacheService>();
            var iterations = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(iterations);

            this.viewModel = new OpenModelViewModel(this.sessionService.Object, this.configurationService.Object, this.cacheService.Object);
            this.models = CreateData().ToList();
            this.sessionService.Setup(x => x.GetParticipantModels()).Returns(this.models);

            object result;
            this.cacheService.Setup(x => x.TryGetBrowserSessionSetting(BrowserSessionSettingKey.LastUsedDomainOfExpertise, out result)).Returns(false);

            this.cacheService.Setup(x => x.TryGetBrowserSessionSetting(BrowserSessionSettingKey.LastUsedEngineeringModel, out result)).Returns(false);

            this.cacheService.Setup(x => x.TryGetBrowserSessionSetting(BrowserSessionSettingKey.LastUsedIterationData, out result)).Returns(false);
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            //Initialize without server configuration
            this.viewModel.InitializesProperties();
            Assert.That(this.viewModel.AvailableEngineeringModelSetups.Count(), Is.EqualTo(5));
            
            //Initialize with server configuration without rdl filter
            var serverConfiguration = new ServerConfiguration();
            this.configurationService.Setup(x => x.ServerConfiguration).Returns(serverConfiguration);
            this.viewModel.InitializesProperties();
            Assert.That(this.viewModel.AvailableEngineeringModelSetups.Count(), Is.EqualTo(5));

            //Initialize with server configuration with rdl filter
            serverConfiguration.RdlFilter = new EngineeringModelRdlFilter
            {
                Kinds = new[] { EngineeringModelKind.STUDY_MODEL },
                RdlShortNames = new[] { RdlShortName }
            };

            this.viewModel.InitializesProperties();
            Assert.That(this.viewModel.AvailableEngineeringModelSetups.Count(), Is.EqualTo(2));

            //Assert that a template model won't appear in the dialog
            this.models[0].Kind = EngineeringModelKind.TEMPLATE_MODEL;
            this.viewModel.InitializesProperties();
            Assert.That(this.viewModel.AvailableEngineeringModelSetups.Count(), Is.EqualTo(1));

            //Filter just by name
            serverConfiguration.RdlFilter.Kinds = Enumerable.Empty<EngineeringModelKind>();
            this.viewModel.InitializesProperties();
            Assert.That(this.viewModel.AvailableEngineeringModelSetups.Count(), Is.EqualTo(2));

            //Filter just by kind
            serverConfiguration.RdlFilter.Kinds = new[] { EngineeringModelKind.STUDY_MODEL };
            serverConfiguration.RdlFilter.RdlShortNames = Enumerable.Empty<string>();
            this.viewModel.InitializesProperties();
            Assert.That(this.viewModel.AvailableEngineeringModelSetups.Count(), Is.EqualTo(4));
        }

        [Test]
        public void VerifyInitializeViewModelWithBrowserSessionSettings()
        {
            //Initialize without server configuration
            this.viewModel.InitializesProperties();
            Assert.That(this.viewModel.AvailableEngineeringModelSetups.Count(), Is.EqualTo(5));

            var preselectedEngineeringModel = this.viewModel.AvailableEngineeringModelSetups.First();

            object domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);
            object engineeringModelSetup = preselectedEngineeringModel;
            object iterationData = new IterationData(preselectedEngineeringModel.IterationSetup.First());

            this.cacheService.Setup(x => x.TryGetBrowserSessionSetting(BrowserSessionSettingKey.LastUsedDomainOfExpertise, out domainOfExpertise)).Returns(true);

            this.cacheService.Setup(x => x.TryGetBrowserSessionSetting(BrowserSessionSettingKey.LastUsedEngineeringModel, out engineeringModelSetup)).Returns(true);

            this.cacheService.Setup(x => x.TryGetBrowserSessionSetting(BrowserSessionSettingKey.LastUsedIterationData, out iterationData)).Returns(true);

            this.viewModel.InitializesProperties();
            Assert.That(this.viewModel.SelectedEngineeringModel, Is.EqualTo(engineeringModelSetup));
            Assert.That(this.viewModel.SelectedIterationSetup, Is.EqualTo(iterationData));
            Assert.That(this.viewModel.SelectedDomainOfExpertise, Is.EqualTo(domainOfExpertise));
        }

        private static List<EngineeringModelSetup> CreateData()
        {
            var rdl1 = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null)
            {
                ShortName = "rdl1",
                RequiredRdl = new SiteReferenceDataLibrary()
                {
                    ShortName = RdlShortName
                }
            };

            var rdl2 = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null)
            {
                ShortName = "rdl2",
                RequiredRdl = new SiteReferenceDataLibrary()
                {
                    ShortName = "another"
                }
            };

            return
            [
                new EngineeringModelSetup(Guid.NewGuid(), null, null)
                {
                    Name = "Model1",
                    RequiredRdl = { rdl1 },
                    IterationSetup =
                    {
                        new IterationSetup(Guid.NewGuid(), null, null)
                    },
                    Kind = EngineeringModelKind.STUDY_MODEL
                },

                new EngineeringModelSetup(Guid.NewGuid(), null, null)
                {
                    Name = "Model2",
                    RequiredRdl = { rdl1 },
                    IterationSetup =
                    {
                        new IterationSetup(Guid.NewGuid(), null, null)
                    },
                    Kind = EngineeringModelKind.STUDY_MODEL
                },

                new EngineeringModelSetup(Guid.NewGuid(), null, null)
                {
                    Name = "Model3",
                    RequiredRdl = { rdl2 },
                    IterationSetup =
                    {
                        new IterationSetup(Guid.NewGuid(), null, null)
                    },
                    Kind = EngineeringModelKind.STUDY_MODEL
                },

                new EngineeringModelSetup(Guid.NewGuid(), null, null)
                {
                    Name = "Model4",
                    RequiredRdl = { rdl2 },
                    IterationSetup =
                    {
                        new IterationSetup(Guid.NewGuid(), null, null)
                    },
                    Kind = EngineeringModelKind.STUDY_MODEL
                },

                new EngineeringModelSetup(Guid.NewGuid(), null, null)
                {
                    Name = "Model5",
                    RequiredRdl = { rdl2 },
                    IterationSetup =
                    {
                        new IterationSetup(Guid.NewGuid(), null, null)
                    },
                    Kind = EngineeringModelKind.STUDY_MODEL
                }
            ];
        }
    }
}
