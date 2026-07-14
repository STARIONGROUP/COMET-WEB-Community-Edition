// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DragEventArgsExtensionsTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Utilities
{
    using CDP4DalCommon.Protocol.Operations;

    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components.Web;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for the <see cref="DragEventArgsExtensions" />
    /// </summary>
    [TestFixture]
    public class DragEventArgsExtensionsTestFixture
    {
        [Test]
        public void VerifyGetCopyOperationKind()
        {
            var ctrlOnly = new DragEventArgs { CtrlKey = true, ShiftKey = false };
            var shiftOnly = new DragEventArgs { CtrlKey = false, ShiftKey = true };
            var ctrlAndShift = new DragEventArgs { CtrlKey = true, ShiftKey = true };
            var noModifier = new DragEventArgs { CtrlKey = false, ShiftKey = false };

            Assert.Multiple(() =>
            {
                Assert.That(ctrlOnly.GetCopyOperationKind(), Is.EqualTo(OperationKind.CopyKeepValuesChangeOwner));
                Assert.That(shiftOnly.GetCopyOperationKind(), Is.EqualTo(OperationKind.Copy));
                Assert.That(ctrlAndShift.GetCopyOperationKind(), Is.EqualTo(OperationKind.CopyKeepValues));
                Assert.That(noModifier.GetCopyOperationKind(), Is.Null);
            });
        }
    }
}
