// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ArchiveLoginViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components
{
    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Services.SessionManagement;

    using FluentResults;

    using Microsoft.AspNetCore.Components.Forms;

    using ReactiveUI;

    /// <summary>
    /// View Model that enables the user to open a read-only session against an uploaded ECSS-E-TM-10-25 Annex C3 archive
    /// </summary>
    public class ArchiveLoginViewModel : ReactiveObject, IArchiveLoginViewModel
    {
        /// <summary>
        /// The <see cref="IAuthenticationService" />
        /// </summary>
        private readonly IAuthenticationService authenticationService;

        /// <summary>
        /// The <see cref="IArchiveFileService" />
        /// </summary>
        private readonly IArchiveFileService archiveFileService;

        /// <summary>
        /// Backing field for <see cref="AuthenticationDto" />
        /// </summary>
        private AuthenticationDto authenticationDto;

        /// <summary>
        /// Backing field for <see cref="SelectedFile" />
        /// </summary>
        private IBrowserFile selectedFile;

        /// <summary>
        /// Backing field for <see cref="AuthenticationResult" />
        /// </summary>
        private Result authenticationResult = new();

        /// <summary>
        /// Backing field for <see cref="IsLoading" />
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveLoginViewModel" /> class.
        /// </summary>
        /// <param name="authenticationService">The <see cref="IAuthenticationService" /></param>
        /// <param name="archiveFileService">The <see cref="IArchiveFileService" /></param>
        public ArchiveLoginViewModel(IAuthenticationService authenticationService, IArchiveFileService archiveFileService)
        {
            this.authenticationService = authenticationService;
            this.archiveFileService = archiveFileService;
            this.ResetAuthenticationDto();
        }

        /// <summary>
        /// The <see cref="AuthenticationDto" /> used for perfoming a login
        /// </summary>
        public AuthenticationDto AuthenticationDto
        {
            get => this.authenticationDto;
            private set => this.RaiseAndSetIfChanged(ref this.authenticationDto, value);
        }

        /// <summary>
        /// Gets or sets the archive that has been selected by the user
        /// </summary>
        public IBrowserFile SelectedFile
        {
            get => this.selectedFile;
            set => this.RaiseAndSetIfChanged(ref this.selectedFile, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Result" /> of an authentication
        /// </summary>
        public Result AuthenticationResult
        {
            get => this.authenticationResult;
            set => this.RaiseAndSetIfChanged(ref this.authenticationResult, value);
        }

        /// <summary>
        /// Gets or sets the loading state
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.RaiseAndSetIfChanged(ref this.isLoading, value);
        }

        /// <summary>
        /// Attempt to open a read-only session against the selected archive
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task ExecuteLogin()
        {
            this.IsLoading = true;

            try
            {
                if (this.SelectedFile == null)
                {
                    this.AuthenticationResult = Result.Fail("Select an Annex C3 archive to open");
                    return;
                }

                var persistResult = await this.archiveFileService.PersistAsync(this.SelectedFile);

                if (persistResult.IsFailed)
                {
                    this.AuthenticationResult = persistResult.ToResult();
                    return;
                }

                this.AuthenticationResult = await this.authenticationService.LoginFromArchive(persistResult.Value, this.AuthenticationDto.UserName, this.AuthenticationDto.Password);

                if (this.AuthenticationResult.IsSuccess)
                {
                    this.ResetAuthenticationDto();
                    this.SelectedFile = null;
                }
                else
                {
                    this.archiveFileService.Remove();
                }
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Resets the <see cref="AuthenticationDto" /> property to a new object with the default parameters
        /// </summary>
        private void ResetAuthenticationDto()
        {
            this.AuthenticationDto = new AuthenticationDto
            {
                ShouldValidateCredentials = true
            };
        }
    }
}
