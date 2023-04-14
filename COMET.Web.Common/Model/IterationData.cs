// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationData.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Model
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Represents data relative to an <see cref="Iteration" />
    /// </summary>
    public class IterationData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IterationData" /> class.
        /// </summary>
        /// <param name="iterationSetup">The <see cref="IterationSetup" /></param>
        /// <param name="displayModelName">
        /// Value asserting that the <see cref="IterationName" /> should contains the name of the
        /// <see cref="EngineeringModelSetup" />
        /// </param>
        public IterationData(IterationSetup iterationSetup, bool displayModelName = false)
        {
            this.IterationSetupId = iterationSetup.Iid;
            this.IterationName = $"Iteration {iterationSetup.IterationNumber} - ";
            this.IterationName += iterationSetup.FrozenOn == null ? "Active" : iterationSetup.FrozenOn;

            if (displayModelName)
            {
                var model = (EngineeringModelSetup)iterationSetup.Container;
                this.IterationName = $"{model.Name} - {this.IterationName}";
            }
        }

        /// <summary>
        /// The <see cref="Guid" /> of the <see cref="IterationSetup" />
        /// </summary>
        public Guid IterationSetupId { get; private set; }

        /// <summary>
        /// The Iteration name
        /// </summary>
        public string IterationName { get; private set; }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise,
        /// <see langword="false" />.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is IterationData otherData)
            {
                return this.IterationSetupId == otherData.IterationSetupId && this.IterationName == otherData.IterationName;
            }

            return false;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return this.IterationSetupId.GetHashCode() + this.IterationName.GetHashCode();
        }
    }
}
