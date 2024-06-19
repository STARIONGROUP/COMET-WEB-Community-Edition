﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeEditorSelectorViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.ViewModels.Components.ParameterEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterTypeEditorSelectorViewModelTestFixture
    {
        private ParameterTypeEditorSelectorViewModel viewModel;
        private CDPMessageBus messageBus;

        private IParameterEditorBaseViewModel<T> CreateParameterEditorViewModel<T>() where T : ParameterType, new()
        {
            var valueSet = new ParameterValueSet
            {
                Manual = new ValueArray<string>(new[] { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var parameter = new T();
            this.viewModel = new ParameterTypeEditorSelectorViewModel(parameter, valueSet, false, this.messageBus);
            return this.viewModel.CreateParameterEditorViewModel<T>();
        }

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyCreateBooleanParameterTypeEditor()
        {
            var booleanParameterEditor = this.CreateParameterEditorViewModel<BooleanParameterType>();

            Assert.Multiple(() =>
            {
                Assert.That(booleanParameterEditor, Is.TypeOf<BooleanParameterTypeEditorViewModel>());
                Assert.That(booleanParameterEditor.IsReadOnly, Is.False);
                Assert.That(() => booleanParameterEditor.OnParameterValueChanged("false"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);

            booleanParameterEditor.ValueArray = new ValueArray<string>(new[] { "" });

            Assert.Multiple(() =>
            {
                Assert.That(booleanParameterEditor.IsReadOnly, Is.True);
                Assert.That(() => booleanParameterEditor.OnParameterValueChanged("false"), Throws.InvalidOperationException);
                Assert.That(() => booleanParameterEditor.OnParameterValueChanged("test"), Throws.Nothing);
            });
        }

        [Test]
        public void VerifyCreateCompoundParameterTypeEditor()
        {
            var compoundParameterEditor = this.CreateParameterEditorViewModel<CompoundParameterType>();

            Assert.Multiple(() =>
            {
                Assert.That(compoundParameterEditor, Is.TypeOf<CompoundParameterTypeEditorViewModel>());
                Assert.That(compoundParameterEditor.IsReadOnly, Is.False);
                Assert.That(() => compoundParameterEditor.OnParameterValueChanged(new CompoundParameterTypeValueChangedEventArgs(0, "-")), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);

            compoundParameterEditor.ValueArray = new ValueArray<string>(new[] { "" });

            Assert.Multiple(() =>
            {
                Assert.That(compoundParameterEditor.IsReadOnly, Is.True);
                Assert.That(() => compoundParameterEditor.OnParameterValueChanged(new CompoundParameterTypeValueChangedEventArgs(0, "-")), Throws.InvalidOperationException);
            });
        }

        [Test]
        public void VerifyCreateDateParameterTypeEditor()
        {
            var dateParameterEditor = this.CreateParameterEditorViewModel<DateParameterType>();

            Assert.Multiple(() =>
            {
                Assert.That(dateParameterEditor, Is.TypeOf<DateParameterTypeEditorViewModel>());
                Assert.That(dateParameterEditor.IsReadOnly, Is.False);
                Assert.That(() => dateParameterEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);

            dateParameterEditor.ValueArray = new ValueArray<string>(new[] { "" });

            Assert.Multiple(() =>
            {
                Assert.That(dateParameterEditor.IsReadOnly, Is.True);
                Assert.That(() => dateParameterEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
                Assert.That(() => dateParameterEditor.OnParameterValueChanged("test"), Throws.Nothing);
            });
        }

        [Test]
        public void VerifyCreateDateTimeParameterTypeEditor()
        {
            var dateTimeParameterEditor = this.CreateParameterEditorViewModel<DateTimeParameterType>();

            Assert.Multiple(() =>
            {
                Assert.That(dateTimeParameterEditor, Is.TypeOf<DateTimeParameterTypeEditorViewModel>());
                Assert.That(dateTimeParameterEditor.IsReadOnly, Is.False);
                Assert.That(() => dateTimeParameterEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);

            dateTimeParameterEditor.ValueArray = new ValueArray<string>(new[] { "" });

            Assert.Multiple(() =>
            {
                Assert.That(dateTimeParameterEditor.IsReadOnly, Is.True);
                Assert.That(() => dateTimeParameterEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
                Assert.That(() => dateTimeParameterEditor.OnParameterValueChanged("test"), Throws.Nothing);
            });
        }

        [Test]
        public void VerifyCreateEnumerationParameterTypeEditor()
        {
            var enumerationParameterEditor = this.CreateParameterEditorViewModel<EnumerationParameterType>();

            Assert.Multiple(() =>
            {
                Assert.That(enumerationParameterEditor, Is.TypeOf<EnumerationParameterTypeEditorViewModel>());
                Assert.That(enumerationParameterEditor.IsReadOnly, Is.False);
                Assert.That(() => enumerationParameterEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);

            enumerationParameterEditor.ValueArray = new ValueArray<string>(new[] { "" });

            Assert.Multiple(() =>
            {
                Assert.That(enumerationParameterEditor.IsReadOnly, Is.True);
                Assert.That(() => enumerationParameterEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
                Assert.That(() => enumerationParameterEditor.OnParameterValueChanged("test"), Throws.Nothing);
            });
        }

        [Test]
        public void VerifyCreateQuantityKindEditor()
        {
            var valueSet = new ParameterValueSet
            {
                Reference = new ValueArray<string>(new[] { "-" }),
                ValueSwitch = ParameterSwitchKind.REFERENCE
            };

            var parameter = new SimpleQuantityKind();
            this.viewModel = new ParameterTypeEditorSelectorViewModel(parameter, valueSet, false, this.messageBus);
            var quantityKindEditor = this.viewModel.CreateParameterEditorViewModel<QuantityKind>();

            Assert.Multiple(() =>
            {
                Assert.That(quantityKindEditor, Is.TypeOf<QuantityKindParameterTypeEditorViewModel>());
                Assert.That(quantityKindEditor.IsReadOnly, Is.False);
                Assert.That(() => quantityKindEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);

            quantityKindEditor.ValueArray = new ValueArray<string>(new[] { "" });

            Assert.Multiple(() =>
            {
                Assert.That(quantityKindEditor.IsReadOnly, Is.True);
                Assert.That(() => quantityKindEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
                Assert.That(() => quantityKindEditor.OnParameterValueChanged("test"), Throws.Nothing);
            });
        }

        [Test]
        public void VerifyCreateTextParameterTypeEditor()
        {
            var textParameterEditor = this.CreateParameterEditorViewModel<TextParameterType>();

            Assert.Multiple(() =>
            {
                Assert.That(textParameterEditor, Is.TypeOf<TextParameterTypeEditorViewModel>());
                Assert.That(textParameterEditor.IsReadOnly, Is.False);
                Assert.That(() => textParameterEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);

            textParameterEditor.ValueArray = new ValueArray<string>(new[] { "" });

            Assert.Multiple(() =>
            {
                Assert.That(textParameterEditor.IsReadOnly, Is.True);
                Assert.That(() => textParameterEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
                Assert.That(() => textParameterEditor.OnParameterValueChanged(5), Throws.Nothing);
            });
        }

        [Test]
        public void VerifyCreateTimeOfDayParameterTypeEditor()
        {
            var timeOfDayParameterEditor = this.CreateParameterEditorViewModel<TimeOfDayParameterType>();

            Assert.Multiple(() =>
            {
                Assert.That(timeOfDayParameterEditor, Is.TypeOf<TimeOfDayParameterTypeEditorViewModel>());
                Assert.That(timeOfDayParameterEditor.IsReadOnly, Is.False);
                Assert.That(() => timeOfDayParameterEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);

            timeOfDayParameterEditor.ValueArray = new ValueArray<string>(new[] { "" });

            Assert.Multiple(() =>
            {
                Assert.That(timeOfDayParameterEditor.IsReadOnly, Is.True);
                Assert.That(() => timeOfDayParameterEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
                Assert.That(() => timeOfDayParameterEditor.OnParameterValueChanged("test"), Throws.Nothing);
            });
        }
    }
}
