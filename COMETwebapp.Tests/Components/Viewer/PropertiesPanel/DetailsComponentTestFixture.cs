// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DetailsComponentTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.Viewer.PropertiesPanel
{
    using System.Collections.Generic;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Components.Viewer.PropertiesPanel;
    using COMETwebapp.Enumerations;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using Orientation = COMETwebapp.Model.Orientation;
    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class DetailsComponentTestFixture
    {
        private TestContext context;
        private DetailsComponent details;
        private IRenderedComponent<DetailsComponent> renderedComponent;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.Services.AddDevExpressBlazor();
            this.context.JSInterop.SetupVoid("DxBlazor.AdaptiveDropDown.init");


            var viewModel = new Mock<IDetailsComponentViewModel>();

            this.renderedComponent = this.context.RenderComponent<DetailsComponent>(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });
            
            this.details = this.renderedComponent.Instance;
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.details, Is.Not.Null);
                Assert.That(this.details.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyThatOrientationViewModelCanBeCreated()
        {
            var viewModel = new Mock<IDetailsComponentViewModel>();

            var parameter = new Parameter()
            {
                ParameterType = new CompoundParameterType()
                {
                    Component =
                    {
                        new ParameterTypeComponent(){ShortName = "one",Scale = new OrdinalScale(){ShortName = "mm"}},
                        new ParameterTypeComponent(){ShortName = "two",Scale = new OrdinalScale(){ShortName = "mm"}},
                        new ParameterTypeComponent(){ShortName = "three",Scale = new OrdinalScale(){ShortName = "mm"}},
                    }
                }
            };

            viewModel.Setup(x => x.CurrentValueSet.ActualValue).Returns(new ValueArray<string>(new List<string>() { "0", "0", "0", }));
            viewModel.Setup(x => x.IsVisible).Returns(true);
            viewModel.Setup(x => x.SelectedParameter).Returns(parameter);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            this.details = this.renderedComponent.Instance;

            var orientationVM = this.details.CreateOrientationViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(viewModel, Is.Not.Null);
                Assert.That(this.details, Is.Not.Null);
                Assert.That(orientationVM, Is.Not.Null);
                Assert.That(orientationVM.Orientation, Is.Not.Null);
                Assert.That(orientationVM.Orientation.X, Is.EqualTo(0));
                Assert.That(orientationVM.Orientation.Y, Is.EqualTo(0));
                Assert.That(orientationVM.Orientation.Z, Is.EqualTo(0));
                Assert.That(orientationVM.Orientation.AngleFormat, Is.EqualTo(AngleFormat.Degrees));
            });
        }

        [Test]
        public void VerifyThatEnumerationParameterExist()
        {
            var viewModel = new Mock<IDetailsComponentViewModel>();

            var parameter = new Parameter()
            {
                ParameterType = new EnumerationParameterType()
                {
                    ShortName = "EnumerationType",
                    ValueDefinition =
                    {
                        new EnumerationValueDefinition(){Name = "True"},
                        new EnumerationValueDefinition(){Name = "False"},
                    }
                }
            };

            viewModel.Setup(x=> x.OnParameterValueChange(It.IsAny<int>(),It.IsAny<string>()));
            viewModel.Setup(x => x.IsVisible).Returns(true);
            viewModel.Setup(x => x.SelectedParameter).Returns(parameter);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            var result = this.renderedComponent.Find(".enumeration-parameter-type");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void VerifyThatBooleanParmeterExist()
        {
            var viewModel = new Mock<IDetailsComponentViewModel>();

            var parameter = new Parameter()
            {
                ParameterType = new BooleanParameterType()
            };

            viewModel.Setup(x => x.OnParameterValueChange(It.IsAny<int>(), It.IsAny<string>()));
            viewModel.Setup(x => x.IsVisible).Returns(true);
            viewModel.Setup(x => x.SelectedParameter).Returns(parameter);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            var result = this.renderedComponent.Find(".boolean-parameter-type");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void VerifyThatDateParameterExist()
        {
            var viewModel = new Mock<IDetailsComponentViewModel>();

            var parameter = new Parameter()
            {
                ParameterType = new DateParameterType()
            };

            viewModel.Setup(x => x.OnParameterValueChange(It.IsAny<int>(), It.IsAny<string>()));
            viewModel.Setup(x => x.IsVisible).Returns(true);
            viewModel.Setup(x => x.SelectedParameter).Returns(parameter);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            var result = this.renderedComponent.Find(".date-parameter-type");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void VerifyThatTextParameterExist()
        {
            var viewModel = new Mock<IDetailsComponentViewModel>();

            var parameter = new Parameter()
            {
                ParameterType = new TextParameterType()
            };

            viewModel.Setup(x => x.OnParameterValueChange(It.IsAny<int>(), It.IsAny<string>()));
            viewModel.Setup(x => x.IsVisible).Returns(true);
            viewModel.Setup(x => x.SelectedParameter).Returns(parameter);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            var result = this.renderedComponent.Find(".text-parameter-type");

            Assert.That(result, Is.Not.Null);

            var colorParameter = new Parameter()
            {
                ParameterType = new TextParameterType()
                {
                    ShortName = SceneSettings.ColorShortName
                }
            };

            viewModel.Setup(x => x.SelectedParameter).Returns(colorParameter);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            result = this.renderedComponent.Find("#color");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void VerifyThatDateTimeParameterExist()
        {
            var viewModel = new Mock<IDetailsComponentViewModel>();

            var parameter = new Parameter()
            {
                ParameterType = new DateTimeParameterType()
            };
            viewModel.Setup(x => x.CurrentValueSet.ActualValue).Returns(new ValueArray<string>(new List<string>() { "2022-11-10T21:00:00" }));
            viewModel.Setup(x => x.OnParameterValueChange(It.IsAny<int>(), It.IsAny<string>()));
            viewModel.Setup(x => x.IsVisible).Returns(true);
            viewModel.Setup(x => x.SelectedParameter).Returns(parameter);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            var result = this.renderedComponent.FindAll(".date-time-parameter-type");
            
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Has.Count.EqualTo(4));
            });
        }

        [Test]
        public void VerifyThatTimeOfDayParameterExist()
        {
            var viewModel = new Mock<IDetailsComponentViewModel>();

            var parameter = new Parameter()
            {
                ParameterType = new TimeOfDayParameterType()
            };

            viewModel.Setup(x => x.CurrentValueSet.ActualValue).Returns(new ValueArray<string>(new List<string>(){"00:00:00"}));
            viewModel.Setup(x => x.OnParameterValueChange(It.IsAny<int>(), It.IsAny<string>()));
            viewModel.Setup(x => x.IsVisible).Returns(true);
            viewModel.Setup(x => x.SelectedParameter).Returns(parameter);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            var result = this.renderedComponent.FindAll(".time-of-day-parameter");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Has.Count.EqualTo(3));
            });
        }

        [Test]
        public void VerifyThatQuantityKindExist()
        {
            var viewModel = new Mock<IDetailsComponentViewModel>();

            var parameter = new Parameter()
            {
                ParameterType = new SimpleQuantityKind()
                {
                    Name = "Quantity Kind",
                    DefaultScale = new OrdinalScale()
                    {
                        ShortName = "mm"
                    }
                }
            };

            viewModel.Setup(x => x.OnParameterValueChange(It.IsAny<int>(), It.IsAny<string>()));
            viewModel.Setup(x => x.IsVisible).Returns(true);
            viewModel.Setup(x => x.SelectedParameter).Returns(parameter);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            var result = this.renderedComponent.Find(".quantity-kind-parameter");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void VerifyThatCompoundParameterType()
        {
            var viewModel = new Mock<IDetailsComponentViewModel>();

            var parameter = new Parameter()
            {
                ParameterType = new CompoundParameterType()
                {
                    Component =
                    {
                        new ParameterTypeComponent(){ShortName = "one",Scale = new OrdinalScale(){ShortName = "mm"}},
                        new ParameterTypeComponent(){ShortName = "two",Scale = new OrdinalScale(){ShortName = "mm"}},
                        new ParameterTypeComponent(){ShortName = "three",Scale = new OrdinalScale(){ShortName = "mm"}},
                        new ParameterTypeComponent(){ShortName = "four",Scale = new OrdinalScale(){ShortName = "mm"}}
                    }
                }
            };

            viewModel.Setup(x => x.CurrentValueSet.ActualValue).Returns(new ValueArray<string>(new List<string>() { "1","1", "1","1" }));
            viewModel.Setup(x => x.OnParameterValueChange(It.IsAny<int>(), It.IsAny<string>()));
            viewModel.Setup(x => x.IsVisible).Returns(true);
            viewModel.Setup(x => x.SelectedParameter).Returns(parameter);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            var result = this.renderedComponent.FindAll(".compound-parameter-type");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Has.Count.EqualTo(4));
            });
        }
    }
}
