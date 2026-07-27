// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RelationshipMatrixExporterTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Services.RelationshipMatrix
{
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using ClosedXML.Excel;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.Services.RelationshipMatrix;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    using NUnit.Framework;

    [TestFixture]
    public class RelationshipMatrixExporterTestFixture
    {
        [Test]
        public void VerifyExportProducesTheThreeImeWorksheets()
        {
            using var rowConfiguration = new SourceConfigurationViewModel(() => { }) { SelectedClassKind = ClassKind.ElementDefinition };
            using var columnConfiguration = new SourceConfigurationViewModel(() => { }) { SelectedClassKind = ClassKind.ElementDefinition };

            var rowElement = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "ROW", Name = "Row element" };
            var columnElement = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "COL", Name = "Column element" };
            var rule = new BinaryRelationshipRule { Iid = Guid.NewGuid(), ShortName = "rule1", Name = "rule one", ForwardRelationshipName = "relates to" };
            var engineeringModel = new EngineeringModel { EngineeringModelSetup = new EngineeringModelSetup { Name = "Model" } };

            var payload = new RelationshipMatrixExportPayload(
                [rowElement], [columnElement],
                (_, _) => RelationshipDirectionKind.None,
                rowConfiguration, columnConfiguration,
                rule, false, [],
                engineeringModel, 1, new DateTime(2026, 1, 1));

            var exporter = new RelationshipMatrixExporter(payload);

            using var stream = exporter.Export();
            using var workbook = new XLWorkbook(stream);

            Assert.Multiple(() =>
            {
                Assert.That(exporter.FileName, Is.EqualTo("RelationshipMatrix.xlsx"));
                Assert.That(workbook.Worksheets.Select(x => x.Name), Is.EqualTo(new[] { "Matrix", "Relationships", "Configuration" }));
            });
        }
    }
}
