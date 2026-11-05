// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVViewModel.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using COMETwebapp.Utilities.VandV;

    using ReactiveUI;

    /// <summary>
    /// View model of the V&amp;V tab, see <see cref="IVandVViewModel" />.
    /// </summary>
    public class VandVViewModel : DisposableObject, IVandVViewModel
    {
        /// <summary>
        /// The <see cref="ILogger" /> used to record V&amp;V data that could not be read.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Backing field for <see cref="Model" />
        /// </summary>
        private VandVModel model = VandVModel.Build(null, null);

        /// <summary>
        /// Backing field for <see cref="LoadError" />
        /// </summary>
        private string loadError;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" />.</param>
        /// <param name="logger">The <see cref="ILogger" /> used to record a failed read or write.</param>
        public VandVViewModel(ISessionService sessionService, ILogger logger)
        {
            this.logger = logger;

            var setupViewModel = new VandVSetupViewModel(sessionService, logger);
            this.Disposables.Add(setupViewModel);
            this.SetupViewModel = setupViewModel;

            var registerViewModel = new VandVRegisterViewModel(logger);
            this.Disposables.Add(registerViewModel);
            this.RegisterViewModel = registerViewModel;
        }

        /// <summary>
        /// Gets the V&amp;V snapshot of the current iteration.
        /// </summary>
        public VandVModel Model
        {
            get => this.model;
            private set => this.RaiseAndSetIfChanged(ref this.model, value);
        }

        /// <summary>
        /// Gets the view model of the set-up dialog.
        /// </summary>
        public IVandVSetupViewModel SetupViewModel { get; }

        /// <summary>
        /// Gets the view model of the register.
        /// </summary>
        public IVandVRegisterViewModel RegisterViewModel { get; }

        /// <summary>
        /// Gets a value indicating whether the tab shows its empty state: the model is not set up for V&amp;V and holds no
        /// V&amp;V data yet, which is unknown when it could not be read.
        /// </summary>
        public bool ShowEmptyState => this.LoadError == null && this.SetupViewModel.NeedsSetup && this.Model.Items.Count == 0 && this.Model.Activities.Count == 0;

        /// <summary>
        /// Gets the message shown when the V&amp;V data of the iteration could not be read, or null.
        /// </summary>
        public string LoadError
        {
            get => this.loadError;
            private set => this.RaiseAndSetIfChanged(ref this.loadError, value);
        }

        /// <summary>
        /// Rebuilds the snapshot for the given iteration and passes it on. The stage gates come from the model library's
        /// chain, walked from the engineering model setup so a partially loaded iteration cannot throw. Any failure is kept in
        /// <see cref="LoadError" /> and never reaches the caller, so V&amp;V data that cannot be read leaves the other tabs working.
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" />, or null when none is open.</param>
        public void SetIteration(Iteration iteration)
        {
            var isOtherIteration = this.Model.Iteration?.Iid != iteration?.Iid;

            try
            {
                this.SetupViewModel.SetIteration(iteration);
                this.Model = VandVModel.Build(iteration, VandVSeeder.GetChain(this.SetupViewModel.TargetLibrary));
                this.RegisterViewModel.SetModel(this.Model);
                this.LoadError = null;
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "The V&V data of iteration {Iid} could not be read", iteration?.Iid);
                this.LoadError = "The V&V data of this model could not be read, so what is shown here may be out of date. See the log for details.";

                if (isOtherIteration)
                {
                    this.Model = VandVModel.Build(null, null);
                    this.RegisterViewModel.SetModel(this.Model);
                }
            }
        }
    }
}
