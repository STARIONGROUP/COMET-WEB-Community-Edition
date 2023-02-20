// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterSubscriptionRowViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.SubscriptionDashboard.Rows
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Extensions;

    /// <summary>
    /// Row View Model that is used to display information related to <see cref="ParameterSubscription" />
    /// </summary>
    public class ParameterSubscriptionRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionRowViewModel" /> class.
        /// </summary>
        /// <param name="subscription">The represented <see cref="subscription" /></param>
        /// <param name="option">The selected <see cref="Option" /></param>
        /// <param name="actualFiniteState">The <see cref="ActualFiniteState" /></param>
        public ParameterSubscriptionRowViewModel(ParameterSubscription subscription, Option option, ActualFiniteState actualFiniteState)
        {
            this.Parameter = (ParameterOrOverrideBase)subscription.Container;
            this.Element = (ElementBase)this.Parameter.Container;
            this.Option = option;
            this.State = actualFiniteState;
            this.ValueSet = this.Parameter.QueryParameterBaseValueSet(option, actualFiniteState);
            this.SubscriptionValueSet = (ParameterSubscriptionValueSet)subscription.QueryParameterBaseValueSet(option, actualFiniteState);
            this.Changes = this.SubscriptionValueSet.QueryParameterSubscriptionValueSetEvolution();
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> that reflect changes of the referenced <see cref="ParameterValueSet"/>
        /// </summary>
        public Dictionary<int, ValueArray<string>> Changes { get; set; }

        /// <summary>
        /// Gets the <see cref="ActualFiniteState" />
        /// </summary>
        public ActualFiniteState State { get; }

        /// <summary>
        /// Gets the display name of the current <see cref="ActualFiniteState" />
        /// </summary>
        public string StateName => this.State == null ? "-" : this.State.Name;

        /// <summary>
        /// Gets the <see cref="Option" />
        /// </summary>
        public Option Option { get; }

        /// <summary>
        /// Gets the display name of the current <see cref="Option" />
        /// </summary>
        public string OptionName => this.Option == null ? "-" : this.Option.Name;

        /// <summary>
        /// The <see cref="IValueSet" /> of the associated <see cref="ParameterSubscription" />
        /// </summary>
        public ParameterSubscriptionValueSet SubscriptionValueSet { get; }

        /// <summary>
        /// The <see cref="IValueSet" /> of the associated <see cref="Parameter" />
        /// </summary>
        public IValueSet ValueSet { get; }

        /// <summary>
        /// The associated <see cref="ElementBase" />
        /// </summary>
        public ElementBase Element { get; }

        /// <summary>
        /// The name of the <see cref="ElementBase" />
        /// </summary>
        public string ElementName => this.Element.Name;

        /// <summary>
        /// The associated <see cref="ParameterOrOverrideBase" />
        /// </summary>
        public ParameterOrOverrideBase Parameter { get; }

        /// <summary>
        /// The name of the <see cref="ParameterType" />
        /// </summary>
        public string ParameterName => this.Parameter.ParameterType.Name;
    }
}
