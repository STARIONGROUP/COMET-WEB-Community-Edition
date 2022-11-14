﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesBase.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.PropertiesPanel
{
    using COMETwebapp.Primitives;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// The properties component used for displaying data about the selected primitive
    /// </summary>
    public class PropertiesBase : ComponentBase
    {
        /// <summary>
        /// The <see cref="COMETwebapp.Primitives.Primitive"/> to fill the panel with
        /// </summary>
        [Parameter]
        public Primitive Primitive { get; set; }       
    }
}