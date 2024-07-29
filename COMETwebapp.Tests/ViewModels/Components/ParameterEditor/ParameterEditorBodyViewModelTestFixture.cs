// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorBodyViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.ParameterEditor
{
    using System.Collections.Concurrent;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.ViewModels.Components.ParameterEditor;
    using COMETwebapp.ViewModels.Components.ParameterEditor.BatchParameterEditor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterEditorBodyViewModelTestFixture
    {
        private ParameterEditorBodyViewModel viewModel;
        private Mock<IParameterTableViewModel> tableViewModel;
        private Mock<IBatchParameterEditorViewModel> batchParameterEditorViewModel;
        private Iteration iteration;
        private Mock<ISession> session;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            var cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var uri = new Uri("http://localhost");
            var domain = new DomainOfExpertise { Iid = Guid.NewGuid() };
            var elementUsage1 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element1" };
            var elementUsage2 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element2" };
            var elementUsage3 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element3" };
            var elementUsage4 = new ElementUsage { Iid = Guid.NewGuid(), Name = "element4" };

            var elementDefinition1 = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "element1",
                Parameter =
                {
                    new Parameter
                    {
                        Iid = Guid.NewGuid(),
                        Owner = domain,
                        ParameterType = new TextParameterType
                        {
                            Name = "textParamType"
                        },
                        ValueSet =
                        {
                            new ParameterValueSet
                            {
                                Iid = Guid.NewGuid(),
                                Computed = new ValueArray<string>(new[] { "-" }),
                                Formula = new ValueArray<string>(new[] { "-" })
                            }
                        }
                    }
                }
            };

            var elementDefinition2 = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "element2",
                Parameter =
                {
                    new Parameter
                    {
                        Iid = Guid.NewGuid(),
                        Owner = domain,
                        ParameterType = new EnumerationParameterType { Name = "enumParamType" },
                        ValueSet =
                        {
                            new ParameterValueSet
                            {
                                Iid = Guid.NewGuid(),
                                Computed = new ValueArray<string>(new[] { "-" }),
                                Formula = new ValueArray<string>(new[] { "-" })
                            }
                        }
                    }
                }
            };

            var elementDefinition3 = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "element3",
                Parameter =
                {
                    new Parameter
                    {
                        Iid = Guid.NewGuid(),
                        Owner = domain,
                        ParameterType = new CompoundParameterType { Name = "compoundParamType" },
                        ValueSet =
                        {
                            new ParameterValueSet
                            {
                                Iid = Guid.NewGuid(),
                                Computed = new ValueArray<string>(new[] { "-" }),
                                Formula = new ValueArray<string>(new[] { "-" })
                            }
                        }
                    }
                }
            };

            var elementDefinition4 = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "element4",
                Parameter =
                {
                    new Parameter
                    {
                        Iid = Guid.NewGuid(),
                        Owner = domain,
                        ParameterType = new BooleanParameterType { Name = "booleanParamType" },
                        ValueSet =
                        {
                            new ParameterValueSet
                            {
                                Iid = Guid.NewGuid(),
                                Computed = new ValueArray<string>(new[] { "-" }),
                                Formula = new ValueArray<string>(new[] { "-" })
                            }
                        }
                    }
                }
            };

            elementUsage1.ElementDefinition = elementDefinition1;
            elementUsage2.ElementDefinition = elementDefinition2;
            elementUsage3.ElementDefinition = elementDefinition3;
            elementUsage4.ElementDefinition = elementDefinition4;

            var topElement = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "topElement",
                Parameter =
                {
                    new Parameter
                    {
                        Iid = Guid.NewGuid(),
                        Owner = domain,
                        ParameterType = new TextParameterType { Name = "textParamType" },
                        ValueSet =
                        {
                            new ParameterValueSet
                            {
                                Iid = Guid.NewGuid(),
                                Computed = new ValueArray<string>(new[] { "-" }),
                                Formula = new ValueArray<string>(new[] { "-" })
                            }
                        }
                    }
                },
                ContainedElement =
                {
                    elementUsage1, elementUsage2, elementUsage3, elementUsage4
                }
            };

            this.iteration = new Iteration();
            this.iteration.Iid = Guid.NewGuid();
            this.iteration.TopElement = topElement;
            this.iteration.Element.AddRange(new List<ElementDefinition> { topElement, elementDefinition1, elementDefinition2, elementDefinition3, elementDefinition4 });
            this.iteration.Option.Add(new Option(Guid.NewGuid(), cache, uri));
            this.iteration.Option.Add(new Option(Guid.NewGuid(), cache, uri));
            this.iteration.DefaultOption = this.iteration.Option.First();

            var sessionService = new Mock<ISessionService>();
            this.session = new Mock<ISession>();
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            sessionService.Setup(x => x.Session).Returns(this.session.Object);
            sessionService.Setup(x => x.GetDomainOfExpertise(this.iteration)).Returns(domain);

            var subscriptionService = new Mock<ISubscriptionService>();
            this.tableViewModel = new Mock<IParameterTableViewModel>();
            this.batchParameterEditorViewModel = new Mock<IBatchParameterEditorViewModel>();
            this.messageBus = new CDPMessageBus();

            this.viewModel = new ParameterEditorBodyViewModel(sessionService.Object, subscriptionService.Object, this.tableViewModel.Object, this.messageBus, this.batchParameterEditorViewModel.Object)
            {
                CurrentThing = this.iteration
            };
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
        }

        [Test]
        public async Task VerifyApplyFilters()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.ElementSelector.SelectedElementBase = this.viewModel.ElementSelector.AvailableElements.First();
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.tableViewModel.Verify(x => x.ApplyFilters(this.iteration.DefaultOption, this.viewModel.ElementSelector.SelectedElementBase, null, true), Times.AtLeastOnce);

            this.viewModel.ElementSelector.SelectedElementBase = null;
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.ParameterTypeSelector.SelectedParameterType = this.viewModel.ParameterTypeSelector.AvailableParameterTypes.First();
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.tableViewModel.Verify(x => x.ApplyFilters(this.iteration.DefaultOption,
                null, this.viewModel.ParameterTypeSelector.SelectedParameterType, true), Times.Once);

            this.viewModel.ParameterTypeSelector.SelectedParameterType = null;
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.OptionSelector.SelectedOption = this.viewModel.CurrentThing.Option.Last();
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.tableViewModel.Verify(x => x.ApplyFilters(this.iteration.Option.Last(),
                null, null, true), Times.Once);

            this.viewModel.IsOwnedParameters = false;
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.tableViewModel.Verify(x => x.ApplyFilters(this.iteration.Option.Last(),
                null, null, false), Times.Once);
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SubscriptionService, Is.Not.Null);
                Assert.That(this.viewModel.ElementSelector, Is.Not.Null);
                Assert.That(this.viewModel.ParameterTypeSelector, Is.Not.Null);
                Assert.That(this.viewModel.OptionSelector, Is.Not.Null);
                Assert.That(this.viewModel.OptionSelector.SelectedOption, Is.Not.Null);
                Assert.That(this.viewModel.IsOwnedParameters, Is.True);
            });
        }

        [Test]
        public async Task VerifyRefresh()
        {
            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.EndUpdate));

            Assert.Multiple(() =>
            {
                this.tableViewModel.Verify(x => x.AddRows(It.IsAny<IEnumerable<Thing>>()), Times.Never);
                this.tableViewModel.Verify(x => x.RemoveRows(It.IsAny<IEnumerable<Thing>>()), Times.Never);
                this.tableViewModel.Verify(x => x.UpdateRows(It.IsAny<IEnumerable<Thing>>()), Times.Never);
            });

            var elementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid()
            };

            this.iteration.Element.Add(elementDefinition);
            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Added);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.session.Object);

            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);
            this.tableViewModel.Verify(x => x.AddRows(It.Is<IEnumerable<Thing>>(c => c.Any())), Times.Once);

            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Updated);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.session.Object);

            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);
            this.tableViewModel.Verify(x => x.UpdateRows(It.Is<IEnumerable<Thing>>(c => c.Any())), Times.Once);

            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Removed);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.session.Object);

            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);
            this.tableViewModel.Verify(x => x.RemoveRows(It.Is<IEnumerable<Thing>>(c => c.Any())), Times.Once);

            this.messageBus.SendObjectChangeEvent(new ElementDefinition
            {
                Container = new Iteration()
            }, EventKind.Removed);

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.session.Object);

            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);
            this.tableViewModel.Verify(x => x.RemoveRows(It.Is<IEnumerable<Thing>>(c => c.Any())), Times.Once);
        }
    }
}
