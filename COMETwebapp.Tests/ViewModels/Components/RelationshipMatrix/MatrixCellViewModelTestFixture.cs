// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MatrixCellViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.RelationshipMatrix
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    using NUnit.Framework;

    [TestFixture]
    public class MatrixCellViewModelTestFixture
    {
        private ElementDefinition rowThing;
        private ElementDefinition columnThing;
        private BinaryRelationshipRule rule;

        [SetUp]
        public void SetUp()
        {
            this.rowThing = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "ROW", Name = "Row Element" };
            this.columnThing = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "COL", Name = "Column Element" };
            this.rule = new BinaryRelationshipRule { Iid = Guid.NewGuid(), ShortName = "rule1", Name = "rule one", ForwardRelationshipName = "relates to" };
        }

        private BinaryRelationship CreateRelationship(DefinedThing source, DefinedThing target)
        {
            return new BinaryRelationship { Iid = Guid.NewGuid(), Source = source, Target = target };
        }

        [Test]
        public void VerifyDirection()
        {
            var none = new MatrixCellViewModel(this.rowThing, this.columnThing, [], this.rule);
            Assert.That(none.Direction, Is.EqualTo(RelationshipDirectionKind.None));

            var rowToColumn = new MatrixCellViewModel(this.rowThing, this.columnThing, [this.CreateRelationship(this.rowThing, this.columnThing)], this.rule);
            Assert.That(rowToColumn.Direction, Is.EqualTo(RelationshipDirectionKind.RowToColumn));

            var columnToRow = new MatrixCellViewModel(this.rowThing, this.columnThing, [this.CreateRelationship(this.columnThing, this.rowThing)], this.rule);
            Assert.That(columnToRow.Direction, Is.EqualTo(RelationshipDirectionKind.ColumnToRow));

            var bidirectional = new MatrixCellViewModel(this.rowThing, this.columnThing,
                [this.CreateRelationship(this.rowThing, this.columnThing), this.CreateRelationship(this.columnThing, this.rowThing)], this.rule);

            Assert.That(bidirectional.Direction, Is.EqualTo(RelationshipDirectionKind.Bidirectional));
        }

        [Test]
        public void VerifyTooltip()
        {
            var cell = new MatrixCellViewModel(this.rowThing, this.columnThing, [this.CreateRelationship(this.rowThing, this.columnThing)], this.rule);

            Assert.Multiple(() =>
            {
                Assert.That(cell.Tooltip, Is.Not.Empty);
                Assert.That(cell.Tooltip, Does.Contain(this.rule.ForwardRelationshipName));
            });
        }

        [Test]
        public void VerifyDetails()
        {
            var cell = new MatrixCellViewModel(this.rowThing, this.columnThing, [this.CreateRelationship(this.rowThing, this.columnThing)], this.rule);

            Assert.Multiple(() =>
            {
                Assert.That(cell.RowDetails, Has.Some.Contains(this.rowThing.Name));
                Assert.That(cell.RowDetails, Has.Some.Contains(this.rowThing.ShortName));
                Assert.That(cell.ColumnDetails, Has.Some.Contains(this.columnThing.Name));
                Assert.That(cell.RelationDescriptions, Has.Count.EqualTo(1));
                Assert.That(cell.RelationDescriptions[0], Does.Contain(this.rule.ForwardRelationshipName));
            });
        }

        [Test]
        public void VerifyNoRelationDescriptions()
        {
            var cell = new MatrixCellViewModel(this.rowThing, this.columnThing, [], this.rule);

            Assert.That(cell.RelationDescriptions, Is.Empty);
        }
    }
}
