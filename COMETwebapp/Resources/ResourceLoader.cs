// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ResourceLoader.cs" company="Starion Group S.A.">
//     Copyright (c) 2025 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Resources;

    /// <summary>
    /// Class responsible for loading embedded resources.
    /// </summary>
    public class ResourceLoader : IResourceLoader
    {
        /// <summary>
        /// Load an embedded resource
        /// </summary>
        /// <param name="path">
        /// The path of the embedded resource
        /// </param>
        /// <returns>
        /// a string containing the contents of the embedded resource
        /// </returns>
        public string LoadEmbeddedResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(path);

            using var reader = new StreamReader(stream ?? throw new MissingManifestResourceException());

            return reader.ReadToEnd();
        }

        /// <summary>
        /// queries the version number from the executing assembly
        /// </summary>
        /// <returns>
        /// a string representation of the version of the application
        /// </returns>
        public string QueryVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();

            return this.GetAssemblyVersion(assembly);
        }

        /// <summary>
        /// Gets the version number of an <see cref="Assembly"/>
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/></param>
        /// <returns>The version number of the <see cref="Assembly"/></returns>
        private string GetAssemblyVersion(Assembly assembly)
        {
            var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (infoVersion != null)
            {
                var plusIndex = infoVersion.InformationalVersion.IndexOf('+');

                if (plusIndex != -1)
                {
                    return infoVersion.InformationalVersion.Substring(0, plusIndex);
                }
            }

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            var productVersion = fileVersionInfo.ProductVersion;

            if (productVersion != null)
            {
                var plusIndex = productVersion.IndexOf('+');

                if (plusIndex != -1)
                {
                    return productVersion.Substring(0, plusIndex);
                }
            }

            return productVersion ?? "unknown";
        }

        /// <summary>
        /// Queries the logo with version info from the embedded resources
        /// </summary>
        /// <returns>
        /// the logo
        /// </returns>
        public string QueryLogo()
        {
            var version = this.QueryVersion();

            var logo = this.LoadEmbeddedResource("COMETwebapp.Resources.ascii-art.txt")
                .Replace("COMETWebVersion", version);

            return logo;
        }
    }
}
