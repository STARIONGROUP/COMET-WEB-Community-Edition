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
    using CDP4Common.Types;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.Utilities;

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
        /// any subscription or override awareness. Used by callers that do not need to expose
        /// subscribe/unsubscribe or override affordances on the rendered card.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> to project as a row.</param>
        public ElementDefinitionDetailsRowViewModel(Parameter parameter) : this(parameter, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionDetailsRowViewModel" /> class for the
        /// supplied <see cref="Parameter" />, computing subscription-related flags relative to
        /// <paramref name="currentDomain" />. The card has no override awareness.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> to project as a row.</param>
        /// <param name="currentDomain">
        /// The currently logged-in <see cref="DomainOfExpertise" />, or <c>null</c> when subscription
        /// awareness is not required by the caller.
        /// </param>
        public ElementDefinitionDetailsRowViewModel(Parameter parameter, DomainOfExpertise currentDomain) : this(parameter, currentDomain, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionDetailsRowViewModel" /> class for the
        /// supplied <see cref="Parameter" />, computing both subscription-related flags relative to
        /// <paramref name="currentDomain" /> and override-related flags relative to
        /// <paramref name="hostElementUsage" />. Option filtering is not applied (falls back to the first
        /// value set), making this equivalent to passing <c>selectedOption: null</c>.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> to project as a row.</param>
        /// <param name="currentDomain">
        /// The currently logged-in <see cref="DomainOfExpertise" />, or <c>null</c> when subscription
        /// awareness is not required by the caller.
        /// </param>
        /// <param name="hostElementUsage">
        /// The <see cref="ElementUsage" /> currently selected in the Model Editor tree on which a
        /// <see cref="ParameterOverride" /> may exist or be created, or <c>null</c> when the selected node is
        /// the <see cref="ElementDefinition" /> itself (in which case override affordances are suppressed).
        /// </param>
        public ElementDefinitionDetailsRowViewModel(Parameter parameter, DomainOfExpertise currentDomain, ElementUsage hostElementUsage)
            : this(parameter, currentDomain, hostElementUsage, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionDetailsRowViewModel" /> class for the
        /// supplied <see cref="Parameter" />, computing subscription-related flags relative to
        /// <paramref name="currentDomain" />, override-related flags relative to <paramref name="hostElementUsage" />,
        /// and option-dependent value selection relative to <paramref name="selectedOption" />.
        /// State-dependent value sets are exposed via <see cref="StateValues" /> when the parameter's
        /// <see cref="Parameter.StateDependence" /> is set.
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /> to project as a row.</param>
        /// <param name="currentDomain">
        /// The currently logged-in <see cref="DomainOfExpertise" />, or <c>null</c> when subscription
        /// awareness is not required by the caller.
        /// </param>
        /// <param name="hostElementUsage">
        /// The <see cref="ElementUsage" /> currently selected in the Model Editor tree on which a
        /// <see cref="ParameterOverride" /> may exist or be created, or <c>null</c> when the selected node is
        /// the <see cref="ElementDefinition" /> itself (in which case override affordances are suppressed).
        /// </param>
        /// <param name="selectedOption">
        /// The <see cref="Option" /> currently selected in the product tree, or <c>null</c> when option-filtering
        /// is not required (the first available value set is used in that case).
        /// </param>
        public ElementDefinitionDetailsRowViewModel(Parameter parameter, DomainOfExpertise currentDomain, ElementUsage hostElementUsage, Option selectedOption)
        {
            this.ParameterTypeName = parameter.ParameterType.Name;
            this.Group = parameter.Group;
            this.GroupName = parameter.Group?.Name ?? string.Empty;
            this.ShortName = parameter.ParameterType.ShortName;

            this.IsOwnedByCurrentDomain = currentDomain != null && parameter.Owner != null && parameter.Owner.Iid == currentDomain.Iid;
            this.CurrentDomainSubscription = currentDomain == null
                ? null
                : parameter.ParameterSubscription.FirstOrDefault(ps => ps.Owner != null && ps.Owner.Iid == currentDomain.Iid);
            this.HasCurrentDomainSubscription = this.CurrentDomainSubscription != null;
            this.CanSubscribe = currentDomain != null && !this.IsOwnedByCurrentDomain && !this.HasCurrentDomainSubscription;
            this.HasOtherDomainSubscriptions = parameter.ParameterSubscription.Any(ps => ps.Owner != null && (currentDomain == null || ps.Owner.Iid != currentDomain.Iid));
            this.OtherDomainSubscriptionOwners = string.Join(", ", parameter.ParameterSubscription
                .Where(ps => ps.Owner != null && (currentDomain == null || ps.Owner.Iid != currentDomain.Iid))
                .Select(ps => ps.Owner.ShortName)
                .Distinct());

            this.HostElementUsage = hostElementUsage;
            this.CurrentOverride = hostElementUsage?.ParameterOverride.FirstOrDefault(po => po.Parameter != null && po.Parameter.Iid == parameter.Iid);
            this.HasOverride = this.CurrentOverride != null;
            this.CanCreateOverride = hostElementUsage != null
                                     && currentDomain != null
                                     && !this.HasOverride
                                     && (this.IsOwnedByCurrentDomain || parameter.AllowDifferentOwnerOfOverride);

            // Filter value sets by the selected option when the parameter is option-dependent.
            var optionValueSets = parameter.ValueSet
                .Where(vs => !parameter.IsOptionDependent
                             || selectedOption == null
                             || (vs.ActualOption != null && vs.ActualOption.Iid == selectedOption.Iid))
                .ToList();

            var sourceValueSet = optionValueSets.FirstOrDefault();
            
            IValueSet EffectiveValueSet(ParameterValueSet valueSet)
            {
                if (this.HasOverride)
                {
                    var overrideValueSet = this.CurrentOverride.ValueSet.FirstOrDefault(ovs => ovs.ParameterValueSet != null && ovs.ParameterValueSet.Iid == valueSet.Iid)
                                           ?? this.CurrentOverride.ValueSet.FirstOrDefault();

                    if (overrideValueSet != null)
                    {
                        return overrideValueSet;
                    }
                }

                if (this.HasCurrentDomainSubscription)
                {
                    var subscriptionValueSet = this.CurrentDomainSubscription.ValueSet.FirstOrDefault(svs => svs.SubscribedValueSet != null && svs.SubscribedValueSet.Iid == valueSet.Iid)
                                               ?? this.CurrentDomainSubscription.ValueSet.FirstOrDefault();

                    if (subscriptionValueSet != null)
                    {
                        return subscriptionValueSet;
                    }
                }

                return valueSet;
            }

            var effectiveSource = sourceValueSet != null ? EffectiveValueSet(sourceValueSet) : null;
            this.ActualValue = effectiveSource != null ? ParameterValueFormatter.Format(effectiveSource.ActualValue, parameter.ParameterType, parameter.Scale) : string.Empty;
            this.SwitchValue = effectiveSource?.ValueSwitch.ToString() ?? string.Empty;

            this.PublishedValue = sourceValueSet != null ? ParameterValueFormatter.Format(sourceValueSet.Published, parameter.ParameterType, parameter.Scale) : string.Empty;

            this.Owner = this.HasOverride && this.CurrentOverride.Owner != null
                ? this.CurrentOverride.Owner.ShortName
                : parameter.Owner.ShortName;

            // State-dependent value breakdown: one row per ActualState. The Actual / Switch reflect the current
            // domain's effective value set (override or subscription); Published stays the owner's published value.
            this.IsStateDependent = parameter.StateDependence != null && optionValueSets.Any(vs => vs.ActualState != null);

            this.StateValues = this.IsStateDependent
                ? optionValueSets
                    .Where(vs => vs.ActualState != null)
                    .OrderBy(vs => vs.ActualState.Name)
                    .Select(vs =>
                    {
                        var effective = EffectiveValueSet(vs);
                        return new ParameterStateValueRowViewModel(
                            vs.ActualState.Name,
                            ParameterValueFormatter.Format(effective.ActualValue, parameter.ParameterType, parameter.Scale),
                            ParameterValueFormatter.Format(vs.Published, parameter.ParameterType, parameter.Scale),
                            effective.ValueSwitch.ToString());
                    })
                    .ToList()
                : [];

            this.IsOptionDependent = parameter.IsOptionDependent;
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

        /// <summary>
        /// Gets a value indicating whether the underlying <see cref="Parameter" /> has at least one
        /// <see cref="ParameterSubscription" /> owned by a domain other than the currently logged-in one.
        /// Useful to inform the owner of a parameter that other teams are tracking its value.
        /// </summary>
        public bool HasOtherDomainSubscriptions { get; }

        /// <summary>
        /// Gets a comma-separated string of the distinct short names of all
        /// <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise" />s that have a
        /// <see cref="ParameterSubscription" /> on the underlying <see cref="Parameter" />, excluding the
        /// currently logged-in domain. Empty when no such subscriptions exist.
        /// </summary>
        public string OtherDomainSubscriptionOwners { get; }

        /// <summary>
        /// Gets the <see cref="ElementUsage" /> currently selected in the Model Editor tree on which the
        /// override affordances apply, or <c>null</c> when the selected node is the
        /// <see cref="ElementDefinition" /> itself (in which case override affordances are suppressed).
        /// </summary>
        public ElementUsage HostElementUsage { get; }

        /// <summary>
        /// Gets the <see cref="ParameterOverride" /> on <see cref="HostElementUsage" /> whose
        /// <see cref="ParameterOverride.Parameter" /> matches the row's underlying <see cref="Parameter" />,
        /// or <c>null</c> when no such override exists. Surfaced so the rendering component can hand it to a
        /// delete-override callback.
        /// </summary>
        public ParameterOverride CurrentOverride { get; }

        /// <summary>
        /// Gets a value indicating whether a <see cref="ParameterOverride" /> for the underlying
        /// <see cref="Parameter" /> already exists on the <see cref="HostElementUsage" />.
        /// </summary>
        public bool HasOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the currently logged-in <see cref="DomainOfExpertise" /> can
        /// create a <see cref="ParameterOverride" /> on <see cref="HostElementUsage" /> for the underlying
        /// <see cref="Parameter" /> — i.e. a host usage and current domain are known, no override yet exists,
        /// and either the current domain owns the parameter or
        /// <see cref="ParameterBase.AllowDifferentOwnerOfOverride" /> is <c>true</c>.
        /// </summary>
        public bool CanCreateOverride { get; }

        /// <summary>
        /// Gets the <see cref="ParameterGroup" /> the underlying <see cref="Parameter" /> belongs to, or
        /// <c>null</c> when the parameter is not assigned to any group.
        /// </summary>
        public ParameterGroup Group { get; }

        /// <summary>
        /// Gets the name of the <see cref="ParameterGroup" /> the underlying <see cref="Parameter" /> belongs
        /// to, or an empty string when the parameter is not assigned to any group.
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Gets a value indicating whether the underlying <see cref="Parameter" /> is option-dependent —
        /// i.e. different options may carry different values. When <c>true</c>, the card may display a gear
        /// icon to signal the dependency.
        /// </summary>
        public bool IsOptionDependent { get; }

        /// <summary>
        /// Gets a value indicating whether the underlying <see cref="Parameter" /> is state-dependent — i.e.
        /// its <see cref="Parameter.StateDependence" /> is set and at least one of the (option-filtered) value
        /// sets carries an <see cref="CDP4Common.EngineeringModelData.ActualFiniteState" />.
        /// When <c>true</c>, the card should render the per-state breakdown from <see cref="StateValues" />
        /// instead of the single Actual / Published rows.
        /// </summary>
        public bool IsStateDependent { get; }

        /// <summary>
        /// Gets the per-state value rows for a state-dependent parameter, ordered by
        /// <see cref="ParameterStateValueRowViewModel.StateName" />. Empty when <see cref="IsStateDependent" />
        /// is <c>false</c>.
        /// </summary>
        public IReadOnlyList<ParameterStateValueRowViewModel> StateValues { get; }
    }
}
