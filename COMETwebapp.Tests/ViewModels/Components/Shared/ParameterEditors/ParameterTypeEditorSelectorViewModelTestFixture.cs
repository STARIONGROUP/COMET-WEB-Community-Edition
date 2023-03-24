// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeEditorSelectorViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.ViewModels.Components.Shared.ParameterEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterTypeEditorSelectorViewModelTestFixture
    {
        private IParameterTypeEditorSelectorViewModel viewModel;

        [Test]
        public void VerifyCreateBooleanParameterTypeEditor()
        {
            var booleanParameterEditor = this.CreateParameterEditorViewModel<BooleanParameterType>();

            Assert.Multiple(() =>
            {
                Assert.That(booleanParameterEditor, Is.TypeOf<BooleanParameterTypeEditorViewModel>());
                Assert.That(booleanParameterEditor.IsReadOnly, Is.False);
                Assert.That(async () => await booleanParameterEditor.OnParameterValueChanged("false"), Throws.Nothing);
            });

            booleanParameterEditor.CompoundIndex = 0;

            Assert.Multiple(() =>
            {
                Assert.That(booleanParameterEditor.IsReadOnly, Is.False);
                Assert.That(async () => await booleanParameterEditor.OnParameterValueChanged("false"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);
           
            Assert.Multiple(() =>
            {
                Assert.That(booleanParameterEditor.IsReadOnly, Is.True);
                Assert.That(async () => await booleanParameterEditor.OnParameterValueChanged("false"), Throws.InvalidOperationException);
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
                Assert.That(async () => await compoundParameterEditor.OnParameterValueChanged(new CompoundParameterTypeValueChangedEventArgs(0,"-")), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);
            
            Assert.Multiple(() =>
            {
                Assert.That(compoundParameterEditor.IsReadOnly, Is.True);
                Assert.That(async () => await compoundParameterEditor.OnParameterValueChanged(new CompoundParameterTypeValueChangedEventArgs(0, "-")), Throws.InvalidOperationException);
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
                Assert.That(async () => await dateParameterEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            dateParameterEditor.CompoundIndex = 0;

            Assert.Multiple(() =>
            {
                Assert.That(dateParameterEditor.IsReadOnly, Is.False);
                Assert.That(async () => await dateParameterEditor.OnParameterValueChanged("false"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);
            
            Assert.Multiple(() =>
            {
                Assert.That(dateParameterEditor.IsReadOnly, Is.True);
                Assert.That(async () => await dateParameterEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
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
                Assert.That(async () => await dateTimeParameterEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            dateTimeParameterEditor.CompoundIndex = 0;

            Assert.Multiple(() =>
            {
                Assert.That(dateTimeParameterEditor.IsReadOnly, Is.False);
                Assert.That(async () => await dateTimeParameterEditor.OnParameterValueChanged("false"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);
            
            Assert.Multiple(() =>
            {
                Assert.That(dateTimeParameterEditor.IsReadOnly, Is.True);
                Assert.That(async () => await dateTimeParameterEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
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
                Assert.That(async () => await enumerationParameterEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            enumerationParameterEditor.CompoundIndex = 0;

            Assert.Multiple(() =>
            {
                Assert.That(enumerationParameterEditor.IsReadOnly, Is.False);
                Assert.That(async () => await enumerationParameterEditor.OnParameterValueChanged("false"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);

            Assert.Multiple(() =>
            { 
                Assert.That(enumerationParameterEditor.IsReadOnly, Is.True); 
                Assert.That(async () => await enumerationParameterEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
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
                Assert.That(async () => await textParameterEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            textParameterEditor.CompoundIndex = 0;

            Assert.Multiple(() =>
            {
                Assert.That(textParameterEditor.IsReadOnly, Is.False);
                Assert.That(async () => await textParameterEditor.OnParameterValueChanged("false"), Throws.Nothing);
            });
            
            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);
            
            Assert.Multiple(() =>
            {
                Assert.That(textParameterEditor.IsReadOnly, Is.True);
                Assert.That(async () => await textParameterEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
            });
        }

        [Test]
        public void VerifyCreateQuantityKindEditor()
        {
            var valueSet = new ParameterValueSet()
            {
                Reference = new ValueArray<string>(new[] { "-" }),
                ValueSwitch = ParameterSwitchKind.REFERENCE
            };

            var parameter = new SimpleQuantityKind();
            this.viewModel = new ParameterTypeEditorSelectorViewModel(parameter, valueSet, false);
            var quantityKindEditor = this.viewModel.CreateParameterEditorViewModel<QuantityKind>();

            quantityKindEditor.CompoundIndex = 0;

            Assert.Multiple(() =>
            {
                Assert.That(quantityKindEditor.IsReadOnly, Is.False);
                Assert.That(async () => await quantityKindEditor.OnParameterValueChanged("false"), Throws.Nothing);
            });

            Assert.Multiple(() =>
            {
                Assert.That(quantityKindEditor, Is.TypeOf<QuantityKindParameterTypeEditorViewModel>());
                Assert.That(quantityKindEditor.IsReadOnly, Is.False);
                Assert.That(async () => await quantityKindEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);
            
            Assert.Multiple(() =>
            {
                Assert.That(quantityKindEditor.IsReadOnly, Is.True); 
                Assert.That(async () => await quantityKindEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
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
                Assert.That(async () => await timeOfDayParameterEditor.OnParameterValueChanged("-"), Throws.Nothing);
            });

            timeOfDayParameterEditor.CompoundIndex = 0;

            Assert.Multiple(() =>
            {
                Assert.That(timeOfDayParameterEditor.IsReadOnly, Is.False);
                Assert.That(async () => await timeOfDayParameterEditor.OnParameterValueChanged("false"), Throws.Nothing);
            });

            this.viewModel.UpdateSwitchKind(ParameterSwitchKind.COMPUTED);            

            Assert.Multiple(() =>
            {
                Assert.That(timeOfDayParameterEditor.IsReadOnly, Is.True);
                Assert.That(async () => await timeOfDayParameterEditor.OnParameterValueChanged("-"), Throws.InvalidOperationException);
            });
        }

        private IParameterEditorBaseViewModel<T> CreateParameterEditorViewModel<T>() where T: ParameterType, new()
        {
            var valueSet = new ParameterValueSet()
            {
                Manual = new ValueArray<string>(new[]{"-"} ),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var parameter = new T();
            this.viewModel = new ParameterTypeEditorSelectorViewModel(parameter, valueSet, false);
            return this.viewModel.CreateParameterEditorViewModel<T>();
        }
    }
}
