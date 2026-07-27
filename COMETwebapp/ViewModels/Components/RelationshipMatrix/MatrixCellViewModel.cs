// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MatrixCellViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RelationshipMatrix
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Model.RelationshipMatrix;

    /// <summary>
    /// Represents a single cell of the Relationship Matrix, describing the <see cref="BinaryRelationship" />(s), if any,
    /// found between one row thing and one column thing. Rebuilt on every matrix pass, this is a plain (non-reactive)
    /// view model.
    /// </summary>
    public class MatrixCellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="MatrixCellViewModel" />
        /// </summary>
        /// <param name="sourceRow">The <see cref="DefinedThing" /> shown as the matrix row</param>
        /// <param name="sourceColumn">The <see cref="DefinedThing" /> shown as the matrix column</param>
        /// <param name="relationships">The <see cref="BinaryRelationship" />s found between <paramref name="sourceRow" /> and <paramref name="sourceColumn" />, in either direction</param>
        /// <param name="rule">The <see cref="BinaryRelationshipRule" /> that governs the matrix</param>
        public MatrixCellViewModel(DefinedThing sourceRow, DefinedThing sourceColumn, IReadOnlyList<BinaryRelationship> relationships, BinaryRelationshipRule rule)
        {
            this.SourceRow = sourceRow;
            this.SourceColumn = sourceColumn;
            this.Relationships = relationships;
            this.Rule = rule;
            this.Direction = ComputeDirection(sourceRow, relationships);
            this.Tooltip = ComputeTooltip(sourceRow, sourceColumn, rule, this.Direction);
        }

        /// <summary>
        /// Gets the <see cref="DefinedThing" /> shown as the matrix row.
        /// </summary>
        public DefinedThing SourceRow { get; }

        /// <summary>
        /// Gets the <see cref="DefinedThing" /> shown as the matrix column.
        /// </summary>
        public DefinedThing SourceColumn { get; }

        /// <summary>
        /// Gets the <see cref="BinaryRelationshipRule" /> that governs this cell.
        /// </summary>
        public BinaryRelationshipRule Rule { get; }

        /// <summary>
        /// Gets the <see cref="BinaryRelationship" />s found between <see cref="SourceRow" /> and <see cref="SourceColumn" />, in either direction.
        /// </summary>
        public IReadOnlyList<BinaryRelationship> Relationships { get; }

        /// <summary>
        /// Gets the <see cref="RelationshipDirectionKind" /> of this cell.
        /// </summary>
        public RelationshipDirectionKind Direction { get; }

        /// <summary>
        /// Gets the human-readable description of this cell's relationship, shown as a cell tooltip.
        /// </summary>
        public string Tooltip { get; }

        /// <summary>
        /// Gets the detail lines describing <see cref="SourceRow" />, shown in the item-details panel.
        /// </summary>
        public IReadOnlyList<string> RowDetails => DescribeThing(this.SourceRow);

        /// <summary>
        /// Gets the detail lines describing <see cref="SourceColumn" />, shown in the item-details panel.
        /// </summary>
        public IReadOnlyList<string> ColumnDetails => DescribeThing(this.SourceColumn);

        /// <summary>
        /// Gets one description per <see cref="BinaryRelationship" /> of this cell, in the form
        /// <c>source --(forward name)--&gt; target</c>.
        /// </summary>
        public IReadOnlyList<string> RelationDescriptions =>
            this.Relationships.Select(x => $"{x.Source.UserFriendlyName} --({this.Rule?.ForwardRelationshipName})--> {x.Target.UserFriendlyName}").ToList();

        /// <summary>
        /// Builds the detail lines (name, short name, owner, categories, definition) describing a <see cref="DefinedThing" />.
        /// </summary>
        /// <param name="thing">The <see cref="DefinedThing" /> to describe</param>
        /// <returns>The detail lines</returns>
        private static List<string> DescribeThing(DefinedThing thing)
        {
            var lines = new List<string>
            {
                $"Name: {thing.Name}",
                $"Short name: {thing.ShortName}"
            };

            if (thing is IOwnedThing { Owner: not null } ownedThing)
            {
                lines.Add($"Owner: {ownedThing.Owner.ShortName}");
            }

            if (thing is ICategorizableThing { Category.Count: > 0 } categorizableThing)
            {
                lines.Add($"Categories: {string.Join(", ", categorizableThing.Category.Select(x => x.Name))}");
            }

            var definition = thing.Definition.FirstOrDefault();

            if (definition is not null)
            {
                lines.Add($"Definition: {definition.Content}");
            }

            return lines;
        }

        /// <summary>
        /// Computes the <see cref="RelationshipDirectionKind" /> of a cell from its <see cref="BinaryRelationship" />s.
        /// </summary>
        /// <param name="sourceRow">The <see cref="DefinedThing" /> shown as the matrix row</param>
        /// <param name="relationships">The <see cref="BinaryRelationship" />s found between the row and the column</param>
        /// <returns>The computed <see cref="RelationshipDirectionKind" /></returns>
        private static RelationshipDirectionKind ComputeDirection(DefinedThing sourceRow, IReadOnlyList<BinaryRelationship> relationships)
        {
            if (relationships.Count == 0)
            {
                return RelationshipDirectionKind.None;
            }

            if (relationships.All(x => x.Source.Iid == sourceRow.Iid))
            {
                return RelationshipDirectionKind.RowToColumn;
            }

            if (relationships.All(x => x.Target.Iid == sourceRow.Iid))
            {
                return RelationshipDirectionKind.ColumnToRow;
            }

            return RelationshipDirectionKind.Bidirectional;
        }

        /// <summary>
        /// Computes the human-readable description of a cell's relationship.
        /// </summary>
        /// <param name="sourceRow">The <see cref="DefinedThing" /> shown as the matrix row</param>
        /// <param name="sourceColumn">The <see cref="DefinedThing" /> shown as the matrix column</param>
        /// <param name="rule">The <see cref="BinaryRelationshipRule" /> that governs the matrix</param>
        /// <param name="direction">The computed <see cref="RelationshipDirectionKind" /></param>
        /// <returns>The tooltip text, empty when <paramref name="rule" /> is <see langword="null" /> or <paramref name="direction" /> is <see cref="RelationshipDirectionKind.None" /></returns>
        private static string ComputeTooltip(DefinedThing sourceRow, DefinedThing sourceColumn, BinaryRelationshipRule rule, RelationshipDirectionKind direction)
        {
            if (rule is null)
            {
                return string.Empty;
            }

            return direction switch
            {
                RelationshipDirectionKind.RowToColumn => $"{sourceRow.Name} --({rule.ForwardRelationshipName})--> {sourceColumn.Name}",
                RelationshipDirectionKind.ColumnToRow => $"{sourceRow.Name} <--({rule.ForwardRelationshipName})-- {sourceColumn.Name}",
                RelationshipDirectionKind.Bidirectional => $"{sourceRow.Name} <--({rule.ForwardRelationshipName})--> {sourceColumn.Name}",
                _ => string.Empty
            };
        }
    }
}
