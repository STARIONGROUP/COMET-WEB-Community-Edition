// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RelationshipMatrixExportPayload.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Services.RelationshipMatrix
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    /// <summary>
    /// Immutable snapshot of everything the <see cref="RelationshipMatrixExporter" /> needs to build its workbook,
    /// decoupling the exporter from the view model.
    /// </summary>
    /// <param name="Rows">The <see cref="DefinedThing" />s shown as matrix rows</param>
    /// <param name="Columns">The <see cref="DefinedThing" />s shown as matrix columns</param>
    /// <param name="DirectionOf">Resolves the <see cref="RelationshipDirectionKind" /> for a (row, column) pair</param>
    /// <param name="RowConfiguration">The <see cref="ISourceConfigurationViewModel" /> that configures the rows</param>
    /// <param name="ColumnConfiguration">The <see cref="ISourceConfigurationViewModel" /> that configures the columns</param>
    /// <param name="Rule">The <see cref="BinaryRelationshipRule" /> governing the matrix</param>
    /// <param name="ShowNonRelatedBackgroundColor">Whether cells without a relationship are highlighted</param>
    /// <param name="Relationships">The current <see cref="BinaryRelationship" />s shown in the matrix</param>
    /// <param name="EngineeringModel">The <see cref="EngineeringModel" /> the matrix belongs to</param>
    /// <param name="IterationNumber">The iteration number</param>
    /// <param name="GeneratedOn">The export timestamp</param>
    public record RelationshipMatrixExportPayload(
        IReadOnlyList<DefinedThing> Rows,
        IReadOnlyList<DefinedThing> Columns,
        Func<DefinedThing, DefinedThing, RelationshipDirectionKind> DirectionOf,
        ISourceConfigurationViewModel RowConfiguration,
        ISourceConfigurationViewModel ColumnConfiguration,
        BinaryRelationshipRule Rule,
        bool ShowNonRelatedBackgroundColor,
        IReadOnlyList<BinaryRelationship> Relationships,
        EngineeringModel EngineeringModel,
        int IterationNumber,
        DateTime GeneratedOn);
}
