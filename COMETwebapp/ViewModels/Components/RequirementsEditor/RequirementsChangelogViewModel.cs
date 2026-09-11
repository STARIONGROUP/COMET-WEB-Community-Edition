// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsChangelogViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using COMETwebapp.Services.Export;
    using COMETwebapp.Services.RequirementsEditor;

    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// View model that loads a baseline (frozen) <see cref="IterationSetup" /> and computes the requirements changelog
    /// between it and the current <see cref="Iteration" /> using the <see cref="RequirementsChangelogCalculator" />.
    /// </summary>
    public class RequirementsChangelogViewModel : DisposableObject, IRequirementsChangelogViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" /> used to load the baseline iteration.
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The <see cref="IExportService" /> used to export the changelog to a downloadable file.
        /// </summary>
        private readonly IExportService exportService;

        /// <summary>
        /// The <see cref="ILogger" /> used to record an export failure.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Backing field for <see cref="SelectedBaseline" />
        /// </summary>
        private IterationSetup selectedBaseline;

        /// <summary>
        /// Backing field for <see cref="Changes" />
        /// </summary>
        private IReadOnlyList<RequirementChange> changes = [];

        /// <summary>
        /// Backing field for <see cref="IsLoading" />
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Backing field for <see cref="HasCompared" />
        /// </summary>
        private bool hasCompared;

        /// <summary>
        /// Backing field for <see cref="Message" />
        /// </summary>
        private string message = string.Empty;

        /// <summary>
        /// Backing field for <see cref="ScrollToElementId" />
        /// </summary>
        private Guid? scrollToElementId;

        /// <summary>
        /// Creates a new instance of <see cref="RequirementsChangelogViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="exportService">The <see cref="IExportService" /></param>
        /// <param name="logger">The <see cref="ILogger" /> used to record an export failure</param>
        public RequirementsChangelogViewModel(ISessionService sessionService, IExportService exportService, ILogger logger)
        {
            this.sessionService = sessionService;
            this.exportService = exportService;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the current <see cref="Iteration" /> the changelog compares its baseline against.
        /// </summary>
        public Iteration CurrentIteration { get; private set; }

        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise" /> used to open the baseline iteration.
        /// </summary>
        public DomainOfExpertise CurrentDomain { get; private set; }

        /// <summary>
        /// Gets the frozen <see cref="IterationSetup" />s of the current model that are older than the current iteration
        /// and can therefore be selected as a baseline.
        /// </summary>
        public IReadOnlyList<IterationSetup> AvailableBaselines { get; private set; } = [];

        /// <summary>
        /// Gets or sets the <see cref="IterationSetup" /> the current iteration is compared against.
        /// </summary>
        public IterationSetup SelectedBaseline
        {
            get => this.selectedBaseline;
            set => this.RaiseAndSetIfChanged(ref this.selectedBaseline, value);
        }

        /// <summary>
        /// Gets the <see cref="RequirementChange" />s computed by the last <see cref="CompareAsync" />.
        /// </summary>
        public IReadOnlyList<RequirementChange> Changes
        {
            get => this.changes;
            private set => this.RaiseAndSetIfChanged(ref this.changes, value);
        }

        /// <summary>
        /// Gets a value indicating whether the baseline iteration is being loaded and compared.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.RaiseAndSetIfChanged(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="CompareAsync" /> has completed at least once.
        /// </summary>
        public bool HasCompared
        {
            get => this.hasCompared;
            private set => this.RaiseAndSetIfChanged(ref this.hasCompared, value);
        }

        /// <summary>
        /// Gets the message to show the user when the comparison could not be performed.
        /// </summary>
        public string Message
        {
            get => this.message;
            private set => this.RaiseAndSetIfChanged(ref this.message, value);
        }

        /// <summary>
        /// Gets the <see cref="CDP4Common.CommonData.Thing.Iid" /> of the changelog row to scroll into view, set by
        /// <see cref="RequestScrollTo" /> and cleared once the scroll has been performed, or null when no scroll is
        /// pending.
        /// </summary>
        public Guid? ScrollToElementId
        {
            get => this.scrollToElementId;
            private set => this.RaiseAndSetIfChanged(ref this.scrollToElementId, value);
        }

        /// <summary>
        /// Sets the current <see cref="Iteration" /> and <see cref="DomainOfExpertise" />, recomputing the available
        /// baselines and resetting any previous comparison.
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" />, or null when none is open</param>
        /// <param name="domain">The current <see cref="DomainOfExpertise" /></param>
        public void SetIteration(Iteration iteration, DomainOfExpertise domain)
        {
            this.CurrentDomain = domain;

            // Only reset when the open iteration actually changes. The body reloads (and re-calls this) on every
            // session refresh - including the one our own CompareAsync triggers when it closes the transiently-opened
            // baseline - and resetting there would wipe the changelog we just computed.
            if (this.CurrentIteration != null && iteration?.Iid == this.CurrentIteration.Iid)
            {
                return;
            }

            this.CurrentIteration = iteration;
            this.Changes = [];
            this.HasCompared = false;

            var modelSetup = iteration?.IterationSetup?.GetContainerOfType<EngineeringModelSetup>();

            this.AvailableBaselines = modelSetup == null
                ? []
                : modelSetup.IterationSetup
                    .Where(x => x.FrozenOn != null && x.IterationNumber < iteration.IterationSetup.IterationNumber)
                    .OrderByDescending(x => x.IterationNumber)
                    .ToList();

            this.SelectedBaseline = this.AvailableBaselines.FirstOrDefault();
        }

        /// <summary>
        /// Loads the <see cref="SelectedBaseline" /> iteration and computes the <see cref="Changes" /> against the
        /// <see cref="CurrentIteration" />.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CompareAsync()
        {
            if (this.SelectedBaseline == null || this.CurrentIteration == null)
            {
                return;
            }

            this.IsLoading = true;

            try
            {
                var openedHere = false;

                var baseIteration = this.sessionService.OpenIterations.Items
                    .FirstOrDefault(x => x.IterationSetup?.Iid == this.SelectedBaseline.Iid);

                if (baseIteration == null)
                {
                    var result = await this.sessionService.ReadIteration(this.SelectedBaseline, this.CurrentDomain);

                    if (result.IsFailed)
                    {
                        this.Message = "The baseline iteration could not be loaded.";
                        this.Changes = [];
                        this.HasCompared = true;
                        return;
                    }

                    baseIteration = result.Value;
                    openedHere = true;
                }

                this.Changes = RequirementsChangelogCalculator.Compare(baseIteration, this.CurrentIteration);
                this.Message = string.Empty;
                this.HasCompared = true;

                if (openedHere)
                {
                    await this.sessionService.CloseIteration(baseIteration);
                }
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Exports the given <paramref name="changes" /> to an Excel workbook and offers it for download. Does nothing
        /// when there are no changes to export.
        /// </summary>
        /// <param name="changes">The <see cref="RequirementChange" />s to export</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ExportAsync(IReadOnlyList<RequirementChange> changes)
        {
            if (changes == null || changes.Count == 0)
            {
                return;
            }

            try
            {
                var exporter = new RequirementsChangelogExcelExporter(changes, this.CurrentIteration?.IterationSetup?.IterationNumber, this.SelectedBaseline?.IterationNumber);
                await this.exportService.ExportAndDownloadAsync(exporter);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while exporting the requirements changelog");
                this.Message = "The changelog could not be exported.";
            }
        }

        /// <summary>
        /// Requests that the changelog row of the given <paramref name="elementId" /> is scrolled into view, setting
        /// <see cref="ScrollToElementId" />.
        /// </summary>
        /// <param name="elementId">The <see cref="CDP4Common.CommonData.Thing.Iid" /> of the changelog row to scroll to</param>
        public void RequestScrollTo(Guid elementId)
        {
            this.ScrollToElementId = elementId;
        }

        /// <summary>
        /// Clears <see cref="ScrollToElementId" /> once the pending scroll has been performed.
        /// </summary>
        public void ClearScrollTarget()
        {
            this.ScrollToElementId = null;
        }
    }
}
