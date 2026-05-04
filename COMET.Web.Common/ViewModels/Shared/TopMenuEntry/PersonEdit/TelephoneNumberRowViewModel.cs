// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TelephoneNumberRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMET.Web.Common.ViewModels.Shared.TopMenuEntry.PersonEdit
{
    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// Editable row representing a single <see cref="TelephoneNumber" /> belonging to the active
    /// <see cref="Person" />. Mirrors <see cref="EmailAddressRowViewModel" /> for telephone entries.
    /// </summary>
    public class TelephoneNumberRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="VcardType" />.
        /// </summary>
        private IEnumerable<VcardTelephoneNumberKind> vcardType;

        /// <summary>
        /// Backing field for <see cref="Value" />.
        /// </summary>
        private string value;

        /// <summary>
        /// Backing field for <see cref="IsDefault" />.
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Initializes a new <see cref="TelephoneNumberRowViewModel" /> wrapping an existing SDK
        /// <see cref="TelephoneNumber" />.
        /// </summary>
        /// <param name="original">The original SDK <see cref="TelephoneNumber" /> being wrapped.</param>
        /// <param name="isDefault">Whether this number is the active person's <see cref="Person.DefaultTelephoneNumber" />.</param>
        public TelephoneNumberRowViewModel(TelephoneNumber original, bool isDefault)
        {
            this.Original = original;
            this.vcardType = new List<VcardTelephoneNumberKind>(original.VcardType);
            this.value = original.Value;
            this.isDefault = isDefault;
        }

        /// <summary>
        /// Initializes a new <see cref="TelephoneNumberRowViewModel" /> representing an empty entry the
        /// user is about to fill in. <see cref="Original" /> stays <c>null</c> so the save pipeline knows
        /// to create a brand-new <see cref="TelephoneNumber" /> on persist.
        /// </summary>
        public TelephoneNumberRowViewModel()
        {
            this.vcardType = new List<VcardTelephoneNumberKind> { VcardTelephoneNumberKind.WORK };
            this.value = string.Empty;
            this.isDefault = false;
        }

        /// <summary>
        /// Gets the original SDK <see cref="TelephoneNumber" /> wrapped by this row, or <c>null</c> when
        /// the row was added in the form and has not yet been persisted.
        /// </summary>
        public TelephoneNumber Original { get; }

        /// <summary>
        /// Gets or sets the editable list of <see cref="VcardTelephoneNumberKind" /> displayed by the
        /// form. The vCard standard allows several kinds per number (e.g. WORK + VOICE) which is why
        /// this is a collection rather than a single value. Exposed as <see cref="IEnumerable{T}" /> so
        /// it can be bound to <c>DxTagBox.Values</c>, which expects that signature.
        /// </summary>
        public IEnumerable<VcardTelephoneNumberKind> VcardType
        {
            get => this.vcardType;
            set => this.RaiseAndSetIfChanged(ref this.vcardType, value);
        }

        /// <summary>
        /// Gets or sets the editable telephone value displayed by the form.
        /// </summary>
        public string Value
        {
            get => this.value;
            set => this.RaiseAndSetIfChanged(ref this.value, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this row is the default telephone number of the active
        /// person. Setting this to <c>true</c> in the form does not automatically clear the flag on the
        /// other rows — <see cref="PersonEditViewModel" /> takes responsibility for that during persist.
        /// </summary>
        public bool IsDefault
        {
            get => this.isDefault;
            set => this.RaiseAndSetIfChanged(ref this.isDefault, value);
        }
    }
}
