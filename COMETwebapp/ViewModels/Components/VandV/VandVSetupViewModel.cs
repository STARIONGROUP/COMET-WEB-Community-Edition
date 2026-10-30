// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVSetupViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.VandV
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using COMETwebapp.Utilities.VandV;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// View model of the V&amp;V set-up dialog, see <see cref="IVandVSetupViewModel" />.
    /// </summary>
    public class VandVSetupViewModel : DisposableObject, IVandVSetupViewModel
    {
        /// <summary>
        /// The <see cref="ISessionService" /> used to check permissions and write.
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The <see cref="ILogger" /> used to record a failed write.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The stage gates being defined.
        /// </summary>
        private readonly List<string> stageGates = [..VandVConstants.DefaultStageGates];

        /// <summary>
        /// Backing field for <see cref="IsVisible" />
        /// </summary>
        private bool isVisible;

        /// <summary>
        /// Backing field for <see cref="IsBusy" />
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Backing field for <see cref="Check" />
        /// </summary>
        private VandVReferenceDataCheck check = VandVManifest.Check(null);

        /// <summary>
        /// Backing field for <see cref="CanSeed" />
        /// </summary>
        private bool canSeed;

        /// <summary>
        /// Backing field for <see cref="LastError" />
        /// </summary>
        private string lastError;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVSetupViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" />.</param>
        /// <param name="logger">The <see cref="ILogger" /> used to record a failed write.</param>
        public VandVSetupViewModel(ISessionService sessionService, ILogger logger)
        {
            this.sessionService = sessionService;
            this.logger = logger;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the set-up dialog is open.
        /// </summary>
        public bool IsVisible
        {
            get => this.isVisible;
            set => this.RaiseAndSetIfChanged(ref this.isVisible, value);
        }

        /// <summary>
        /// Gets a value indicating whether a write is in progress.
        /// </summary>
        public bool IsBusy
        {
            get => this.isBusy;
            private set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
        }

        /// <summary>
        /// Gets a value indicating whether the model has a library to seed into and something in it is missing.
        /// </summary>
        public bool NeedsSetup => this.TargetLibrary != null && !this.Check.IsComplete;

        /// <summary>
        /// Gets the model's reference data library the missing things are seeded into, or null when the model has none.
        /// </summary>
        public ModelReferenceDataLibrary TargetLibrary { get; private set; }

        /// <summary>
        /// Gets the result of the last check of the model's chain of libraries.
        /// </summary>
        public VandVReferenceDataCheck Check
        {
            get => this.check;
            private set => this.RaiseAndSetIfChanged(ref this.check, value);
        }

        /// <summary>
        /// Gets one line per missing thing, for the dialog.
        /// </summary>
        public IReadOnlyList<string> MissingItems { get; private set; } = [];

        /// <summary>
        /// Gets a value indicating whether the current user may create everything that is missing in <see cref="TargetLibrary" />.
        /// </summary>
        public bool CanSeed
        {
            get => this.canSeed;
            private set => this.RaiseAndSetIfChanged(ref this.canSeed, value);
        }

        /// <summary>
        /// Gets the reason the last set-up attempt failed, or null; cleared when the dialog is opened again.
        /// </summary>
        public string LastError
        {
            get => this.lastError;
            private set => this.RaiseAndSetIfChanged(ref this.lastError, value);
        }

        /// <summary>
        /// Gets a value indicating whether the stage gate type is missing, so the user defines the stage gates before seeding.
        /// </summary>
        public bool NeedsStageGates => this.Check.MissingParameterTypes.Any(x => x.ShortName == VandVConstants.Stage);

        /// <summary>
        /// Gets the stage gates being defined, in project order.
        /// </summary>
        public IReadOnlyList<string> StageGates => this.stageGates;

        /// <summary>
        /// Gets the error in <see cref="StageGates" />, or null when they can be seeded.
        /// </summary>
        public string StageGateError => VandVSeeder.GetStageGateError(this.stageGates);

        /// <summary>
        /// Points the view model at the given iteration and re-checks its reference data.
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" />, or null when none is open.</param>
        public void SetIteration(Iteration iteration)
        {
            this.TargetLibrary = (iteration?.Container as EngineeringModel)?.EngineeringModelSetup?.RequiredRdl.FirstOrDefault();
            this.Refresh();
        }

        /// <summary>
        /// Re-checks the reference data, resets the stage gates to the defaults and opens the dialog.
        /// </summary>
        public void Open()
        {
            this.Refresh();
            this.stageGates.Clear();
            this.stageGates.AddRange(VandVConstants.DefaultStageGates);
            this.LastError = null;
            this.IsVisible = true;
        }

        /// <summary>
        /// Appends an empty stage gate.
        /// </summary>
        public void AddStageGate()
        {
            this.stageGates.Add(string.Empty);
        }

        /// <summary>
        /// Removes the stage gate at the given position.
        /// </summary>
        /// <param name="index">The position.</param>
        public void RemoveStageGate(int index)
        {
            if (index >= 0 && index < this.stageGates.Count)
            {
                this.stageGates.RemoveAt(index);
            }
        }

        /// <summary>
        /// Moves the stage gate at the given position by the given offset, when the target position exists.
        /// </summary>
        /// <param name="index">The position.</param>
        /// <param name="offset">-1 to move it up, 1 to move it down.</param>
        public void MoveStageGate(int index, int offset)
        {
            var target = index + offset;

            if (index < 0 || index >= this.stageGates.Count || target < 0 || target >= this.stageGates.Count)
            {
                return;
            }

            (this.stageGates[index], this.stageGates[target]) = (this.stageGates[target], this.stageGates[index]);
        }

        /// <summary>
        /// Renames the stage gate at the given position.
        /// </summary>
        /// <param name="index">The position.</param>
        /// <param name="name">The new name.</param>
        public void SetStageGate(int index, string name)
        {
            if (index >= 0 && index < this.stageGates.Count)
            {
                this.stageGates[index] = name;
            }
        }

        /// <summary>
        /// Refreshes the session, re-checks the reference data and seeds whatever is missing in one transaction; closes the
        /// dialog on success and records <see cref="LastError" /> on failure. The refresh picks up a set-up another user or an
        /// administrator did meanwhile, and a second call while one is running is refused, so neither creates duplicates.
        /// </summary>
        /// <returns>The <see cref="Result" /> of the write.</returns>
        public async Task<Result> SeedAsync()
        {
            if (this.IsBusy)
            {
                return Result.Fail("The V&V set-up is already in progress.");
            }

            this.IsBusy = true;

            try
            {
                var result = await this.SeedMissingAsync();
                this.LastError = result.IsFailed ? string.Join(" ", result.Errors.Select(x => x.Message)) : null;
                return result;
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Refreshes the session, re-checks the reference data and writes whatever is missing.
        /// </summary>
        /// <returns>The <see cref="Result" /> of the write.</returns>
        private async Task<Result> SeedMissingAsync()
        {
            try
            {
                await this.sessionService.Session.Refresh();
                this.Refresh();

                if (this.Check.IsComplete || this.TargetLibrary == null)
                {
                    this.IsVisible = false;
                    return Result.Ok();
                }

                if (!this.CanSeed)
                {
                    return Result.Fail("You are not allowed to create the V&V reference data in this model's library.");
                }

                if (this.NeedsStageGates && this.StageGateError != null)
                {
                    return Result.Fail(this.StageGateError);
                }

                var libraryClone = this.TargetLibrary.Clone(false);
                var created = VandVSeeder.BuildThingsToCreate(libraryClone, this.Check, VandVSeeder.ParseStageGates(this.stageGates), VandVSeeder.GetChain(this.TargetLibrary));

                var result = await this.sessionService.CreateOrUpdateThingsWithNotification(libraryClone, [libraryClone, ..created], new NotificationDescription
                {
                    OnSuccess = $"V&V reference data set up in '{this.TargetLibrary.Name}'",
                    OnError = $"Could not set up the V&V reference data in '{this.TargetLibrary.Name}'"
                });

                if (result.IsSuccess)
                {
                    this.IsVisible = false;
                }

                return result;
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while setting up the V&V reference data");
                return Result.Fail(exception.Message);
            }
        }

        /// <summary>
        /// Re-checks the target library's chain against the manifest and the current user's create permissions.
        /// </summary>
        private void Refresh()
        {
            var newCheck = VandVManifest.Check(VandVSeeder.GetChain(this.TargetLibrary));
            this.MissingItems = VandVSeeder.DescribeMissing(newCheck);
            this.Check = newCheck;

            this.CanSeed = this.TargetLibrary != null
                           && VandVSeeder.GetRequiredClassKinds(this.Check).All(x => this.sessionService.Session.PermissionService.CanWrite(x, this.TargetLibrary));
        }
    }
}
