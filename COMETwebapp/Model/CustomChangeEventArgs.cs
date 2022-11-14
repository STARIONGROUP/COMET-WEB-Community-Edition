// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomChangeEventArgs.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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

namespace COMETwebapp.Model
{
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Supplies information about an change event that is being raised.
    /// </summary>
    public class CustomChangeEventArgs : ChangeEventArgs
    {
        /// <summary>
        /// Gets the object that send the change
        /// </summary>
        public object Sender { get; }

        /// <summary>
        /// Creates a new instance of type <see cref="CustomChangeEventArgs"/>
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="value">the value that is being raised</param>
        public CustomChangeEventArgs(object sender, object? value)
        {
            this.Sender = sender;
            this.Value = value;
        }
    }
}
