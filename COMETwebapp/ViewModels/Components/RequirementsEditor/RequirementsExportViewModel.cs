// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsExportViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using COMETwebapp.Model.RequirementsEditor.Export;
    using COMETwebapp.Services.Export;
    using COMETwebapp.Services.RequirementsEditor;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.Utilities;

    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// View model that drives the export of the requirements of an <see cref="Iteration" /> to a downloadable Excel
    /// workbook, decoupled from the document rendering owned by the <see cref="RequirementsEditorBodyViewModel" />.
    /// </summary>
    public class RequirementsExportViewModel : DisposableObject, IRequirementsExportViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" /> used to resolve the open reference data libraries' rules.
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The <see cref="IExportService" /> used to run the exporter and offer its output for download.
        /// </summary>
        private readonly IExportService exportService;

        /// <summary>
        /// The <see cref="IShowHideDeprecatedThingsService" /> that drives whether deprecated specifications are
        /// offered as a default export scope.
        /// </summary>
        private readonly IShowHideDeprecatedThingsService showHideDeprecatedThingsService;

        /// <summary>
        /// The <see cref="ILogger" /> used to record an export failure.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Backing field for <see cref="IsVisible" />
        /// </summary>
        private bool isVisible;

        /// <summary>
        /// The <see cref="Iteration" /> the export is performed against, set by <see cref="SetIteration" />.
        /// </summary>
        private Iteration currentIteration;

        /// <summary>
        /// Creates a new instance of <see cref="RequirementsExportViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="exportService">The <see cref="IExportService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="logger">The <see cref="ILogger" /> used to record an export failure</param>
        public RequirementsExportViewModel(ISessionService sessionService, IExportService exportService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ILogger logger)
        {
            this.sessionService = sessionService;
            this.exportService = exportService;
            this.showHideDeprecatedThingsService = showHideDeprecatedThingsService;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the mutable configuration bound to the export dialog and read by <see cref="ExportAsync" />.
        /// </summary>
        public RequirementsExportConfiguration ExportConfiguration { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the export configuration dialog is open.
        /// </summary>
        public bool IsVisible
        {
            get => this.isVisible;
            set => this.RaiseAndSetIfChanged(ref this.isVisible, value);
        }

        /// <summary>
        /// Gets the non-deprecated <see cref="RequirementsSpecification" />s of the current iteration, offered as the
        /// default export scope and the specification picker choices.
        /// </summary>
        public IEnumerable<RequirementsSpecification> AvailableSpecifications =>
            this.currentIteration == null
                ? []
                : this.currentIteration.RequirementsSpecification
                    .Where(x => this.showHideDeprecatedThingsService.ShowDeprecatedThings || !x.IsDeprecated)
                    .OrderBy(x => x.ShortName);

        /// <summary>
        /// Sets the <see cref="Iteration" /> the export is performed against.
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" />, or null when none is open</param>
        public void SetIteration(Iteration iteration)
        {
            this.currentIteration = iteration;
        }

        /// <summary>
        /// Exports the requirements of the iteration to an Excel workbook, driven by <see cref="ExportConfiguration" />,
        /// and offers it for download. Closes the export dialog on success.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ExportAsync()
        {
            if (this.currentIteration == null)
            {
                return;
            }

            try
            {
                var configuration = this.ExportConfiguration;

                var specifications = configuration.SelectedSpecifications.Count == 0
                    ? this.AvailableSpecifications.ToList()
                    : configuration.SelectedSpecifications.ToList();

                var payload = new RequirementsExportPayload(
                    specifications,
                    configuration,
                    this.GetRelationshipDetails,
                    constraint => this.GetConstraintExportText(constraint, configuration.IncludeConstraintLinkedElementAndValue));

                await this.exportService.ExportAndDownloadAsync(new RequirementsExcelExporter(payload));
                this.IsVisible = false;
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while exporting the requirements");
            }
        }

        /// <summary>
        /// Gets the distinct <see cref="ParameterType" />s used by the simple parameter values of every specification's
        /// requirements, offered as export column choices; ordered by short name.
        /// </summary>
        /// <returns>The exportable parameter types</returns>
        public IReadOnlyList<ParameterType> GetExportableParameterTypes()
        {
            return this.AvailableSpecifications
                .SelectMany(specification => specification.Requirement)
                .SelectMany(requirement => requirement.ParameterValue)
                .Select(value => value.ParameterType)
                .DistinctBy(parameterType => parameterType.Iid)
                .OrderBy(parameterType => parameterType.ShortName)
                .ToList();
        }

        /// <summary>
        /// Gets the distinct definition language codes used across every specification's requirements, offered as export
        /// language choices; ordered alphabetically.
        /// </summary>
        /// <returns>The exportable definition language codes</returns>
        public IReadOnlyList<string> GetExportableDefinitionLanguages()
        {
            return this.AvailableSpecifications
                .SelectMany(specification => specification.Requirement)
                .SelectMany(requirement => requirement.Definition)
                .Select(definition => definition.LanguageCode)
                .Where(languageCode => !string.IsNullOrWhiteSpace(languageCode))
                .Distinct()
                .OrderBy(languageCode => languageCode)
                .ToList();
        }

        /// <summary>
        /// Gets the distinct <see cref="Category" />s carried by the iteration's relationships, offered as export
        /// relationship-filter choices; ordered by name.
        /// </summary>
        /// <returns>The exportable relationship categories</returns>
        public IReadOnlyList<Category> GetExportableRelationshipCategories()
        {
            if (this.currentIteration == null)
            {
                return [];
            }

            return this.currentIteration.Relationship
                .SelectMany(relationship => relationship.Category)
                .DistinctBy(category => category.Iid)
                .OrderBy(category => category.Name)
                .ToList();
        }

        /// <summary>
        /// Gets a relationship detail for every <see cref="CDP4Common.EngineeringModelData.BinaryRelationship" /> and
        /// <see cref="CDP4Common.EngineeringModelData.MultiRelationship" /> of the iteration the given
        /// <paramref name="requirement" /> participates in, resolved to its matched rules so the export can lay out a
        /// column per rule and direction. A relationship that matches several rules yields one detail per matched rule;
        /// one that matches no rule yields a single detail with a null rule.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The relationship details</returns>
        public IReadOnlyList<RequirementRelationshipDetail> GetRelationshipDetails(Requirement requirement)
        {
            return this.currentIteration == null ? [] : RequirementsTraceabilityHelper.GetRelationshipDetails(requirement, this.currentIteration, this.GetOpenReferenceDataLibraryRules());
        }

        /// <summary>
        /// Gets the <see cref="Rule" />s declared by the open reference data libraries, used to resolve the rules a
        /// relationship matches.
        /// </summary>
        /// <returns>The open reference data libraries' rules</returns>
        private IReadOnlyCollection<Rule> GetOpenReferenceDataLibraryRules()
        {
            return this.sessionService.Session.OpenReferenceDataLibraries.SelectMany(x => x.Rule).ToList();
        }

        /// <summary>
        /// Renders the given <paramref name="constraint" /> as a one-line export string: its top-level expressions,
        /// optionally followed by the model code and published value of every parameter linked to its relational
        /// expressions.
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint" /></param>
        /// <param name="includeLinkedElementAndValue">Whether to append the linked element and value</param>
        /// <returns>The export string</returns>
        private string GetConstraintExportText(ParametricConstraint constraint, bool includeLinkedElementAndValue)
        {
            var text = string.Join("; ", BooleanExpressionHelper.GetTopExpressions(constraint).Select(BooleanExpressionHelper.GetExpressionSummary));

            if (!includeLinkedElementAndValue)
            {
                return text;
            }

            var links = constraint.Expression
                .OfType<RelationalExpression>()
                .Select(expression => (Code: this.GetBoundParameterModelCode(expression), Value: this.GetBoundParameterPublishedValue(expression)))
                .Where(link => link.Code != null)
                .Select(link => $"{link.Code} = {link.Value}")
                .ToList();

            return links.Count == 0 ? text : $"{text} (linked: {string.Join(", ", links)})";
        }

        /// <summary>
        /// Gets the <see cref="ParameterOrOverrideBase" /> bound to the given <paramref name="expression" /> through a
        /// <see cref="CDP4Common.EngineeringModelData.BinaryRelationship" />, the way requirement verification links a
        /// parameter to a relational expression.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <returns>The bound parameter, or null when none is bound</returns>
        private ParameterOrOverrideBase GetBoundParameter(RelationalExpression expression)
        {
            return this.currentIteration?.Relationship.OfType<BinaryRelationship>()
                .Select(x => (x.Source == expression ? x.Target : x.Target == expression ? x.Source : null) as ParameterOrOverrideBase)
                .FirstOrDefault(x => x != null);
        }

        /// <summary>
        /// Gets the model code of the <see cref="ParameterOrOverrideBase" /> bound to the given
        /// <paramref name="expression" />.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <returns>The model code, or null when no parameter is bound</returns>
        private string GetBoundParameterModelCode(RelationalExpression expression)
        {
            return this.GetBoundParameter(expression)?.ModelCode();
        }

        /// <summary>
        /// Gets the published value of the <see cref="ParameterOrOverrideBase" /> bound to the given
        /// <paramref name="expression" />: the value the bound element last published, to compare against the
        /// constraint's threshold.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <returns>The formatted published value, or null when no parameter is bound or its published value is ambiguous</returns>
        private string GetBoundParameterPublishedValue(RelationalExpression expression)
        {
            var parameter = this.GetBoundParameter(expression);
            var valueSets = parameter?.ValueSets.OfType<ParameterValueSetBase>().ToList() ?? [];

            return valueSets.Count == 1 ? ParameterValueFormatter.Format(valueSets[0].Published, parameter.Scale) : null;
        }
    }
}
