// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionDetailsRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.SystemRepresentation.Rows
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for  <see cref="ElementDefinition" />
    /// </summary>
    public class ElementDefinitionDetailsRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="ParameterTypeName" />
        /// </summary>
        private string parameterTypeName;

        /// <summary>
        /// Backing field for <see cref="ShortName" />
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="ActualValue" />
        /// </summary>
        private string actualValue;

        /// <summary>
        /// Backing field for <see cref="PublishedValue" />
        /// </summary>
        private string publishedValue;

        /// <summary>
        /// Backing field for <see cref="Owner" />
        /// </summary>
        private string owner;

        /// <summary>
        /// Backing field for <see cref="SwitchValue" />
        /// </summary>
        private string switchValue;

        /// <summary>
        /// Backing field for <see cref="modelCode" />
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Backing field for <see cref="Parameter" />
        /// </summary>
        private Parameter parameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionDetailsRowViewModel" /> class without
        /// any subscription awareness. Used by callers that do not need to expose subscribe/unsubscribe
        /// affordances on the rendered card.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> to project as a row.</param>
        public ElementDefinitionDetailsRowViewModel(Parameter parameter) : this(parameter, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionDetailsRowViewModel" /> class for the
        /// supplied <see cref="Parameter" />, computing subscription-related flags relative to
        /// <paramref name="currentDomain" />.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> to project as a row.</param>
        /// <param name="currentDomain">
        /// The currently logged-in <see cref="DomainOfExpertise" />, or <c>null</c> when subscription
        /// awareness is not required by the caller.
        /// </param>
        public ElementDefinitionDetailsRowViewModel(Parameter parameter, DomainOfExpertise currentDomain)
        {
            this.ParameterTypeName = parameter.ParameterType.Name;
            this.ShortName = parameter.ParameterType.ShortName;

            this.IsOwnedByCurrentDomain = currentDomain != null && parameter.Owner != null && parameter.Owner.Iid == currentDomain.Iid;
            this.CurrentDomainSubscription = currentDomain == null
                ? null
                : parameter.ParameterSubscription.FirstOrDefault(ps => ps.Owner != null && ps.Owner.Iid == currentDomain.Iid);
            this.HasCurrentDomainSubscription = this.CurrentDomainSubscription != null;
            this.CanSubscribe = currentDomain != null && !this.IsOwnedByCurrentDomain && !this.HasCurrentDomainSubscription;

            var sourceValueSet = parameter.ValueSet.FirstOrDefault();
            var subscriptionValueSet = this.HasCurrentDomainSubscription
                ? this.CurrentDomainSubscription.ValueSet.FirstOrDefault()
                : null;

            if (subscriptionValueSet != null)
            {
                this.ActualValue = subscriptionValueSet.ActualValue.AsCommaSeparated();
                this.SwitchValue = subscriptionValueSet.ValueSwitch.ToString();

                if (subscriptionValueSet.ActualValue.Count > 1)
                {
                    this.ActualValue = "{" + this.ActualValue + "}";
                }
            }
            else
            {
                this.ActualValue = sourceValueSet?.ActualValue.AsCommaSeparated() ?? string.Empty;
                this.SwitchValue = sourceValueSet?.ValueSwitch.ToString() ?? string.Empty;

                if (sourceValueSet?.ActualValue.Count > 1)
                {
                    this.ActualValue = "{" + this.ActualValue + "}";
                }
            }

            this.PublishedValue = sourceValueSet?.Published.AsCommaSeparated() ?? string.Empty;

            if (sourceValueSet?.Published.Count > 1)
            {
                this.PublishedValue = "{" + this.PublishedValue + "}";
            }

            if (parameter.Scale != null)
            {
                this.ActualValue += " [" + parameter.Scale.ShortName + "]";
                this.PublishedValue += " [" + parameter.Scale.ShortName + "]";
            }

            this.Owner = parameter.Owner.ShortName;

            this.ModelCode = parameter.ModelCode();
            this.Parameter = parameter;
        }

        /// <summary>
        /// The Name of the <see cref="ParameterType" />
        /// </summary>
        public string ParameterTypeName
        {
            get => this.parameterTypeName;
            set => this.RaiseAndSetIfChanged(ref this.parameterTypeName, value);
        }

        /// <summary>
        /// The short name of the <see cref="ParameterType" />
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// The actual value of the <see cref="Parameter" />
        /// </summary>
        public string ActualValue
        {
            get => this.actualValue;
            set => this.RaiseAndSetIfChanged(ref this.actualValue, value);
        }

        /// <summary>
        /// The published value of the <see cref="Parameter" />
        /// </summary>
        public string PublishedValue
        {
            get => this.publishedValue;
            set => this.RaiseAndSetIfChanged(ref this.publishedValue, value);
        }

        /// <summary>
        /// The short name of the Owner of the <see cref="Parameter" />
        /// </summary>
        public string Owner
        {
            get => this.owner;
            set => this.RaiseAndSetIfChanged(ref this.owner, value);
        }

        /// <summary>
        /// The Switch Value of the <see cref="Parameter" />
        /// </summary>
        public string SwitchValue
        {
            get => this.switchValue;
            set => this.RaiseAndSetIfChanged(ref this.switchValue, value);
        }

        /// <summary>
        /// The model code of the <see cref="Parameter" />
        /// </summary>
        public string ModelCode
        {
            get => this.modelCode;
            set => this.RaiseAndSetIfChanged(ref this.modelCode, value);
        }

        /// <summary>
        /// The <see cref="Parameter" /> of selected <see cref="ElementDefinition" />
        /// </summary>
        public Parameter Parameter
        {
            get => this.parameter;
            set => this.RaiseAndSetIfChanged(ref this.parameter, value);
        }

        /// <summary>
        /// Gets a value indicating whether the underlying <see cref="Parameter" /> is owned by the currently
        /// logged-in <see cref="DomainOfExpertise" />.
        /// </summary>
        public bool IsOwnedByCurrentDomain { get; }

        /// <summary>
        /// Gets the <see cref="ParameterSubscription" /> on the underlying <see cref="Parameter" /> whose
        /// owner is the currently logged-in <see cref="DomainOfExpertise" />, or <c>null</c> when no such
        /// subscription exists. Surfaced so the rendering component can hand it to a delete-subscription
        /// callback.
        /// </summary>
        public ParameterSubscription CurrentDomainSubscription { get; }

        /// <summary>
        /// Gets a value indicating whether the currently logged-in <see cref="DomainOfExpertise" /> already
        /// has a <see cref="ParameterSubscription" /> on the underlying <see cref="Parameter" />.
        /// </summary>
        public bool HasCurrentDomainSubscription { get; }

        /// <summary>
        /// Gets a value indicating whether the currently logged-in <see cref="DomainOfExpertise" /> can
        /// subscribe to the underlying <see cref="Parameter" /> — i.e. a current domain is known, the
        /// parameter is owned by another domain, and no subscription already exists for the current domain.
        /// </summary>
        public bool CanSubscribe { get; }
    }
}
