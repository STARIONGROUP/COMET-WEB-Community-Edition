// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PublicationsViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.ViewModels.Components.Publications
{
    using CDP4Dal;

    using CDP4Common.SiteDirectoryData;
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.Publications.Rows;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the Publications component. Manages the creation of new publications. 
    /// </summary>
    public class PublicationsViewModel : DisposableObject, IPublicationsViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="Iteration"/> that's being used
        /// </summary>
        public Iteration CurrentIteration { get; private set; }

        /// <summary>
        /// Gets or set the list of <see cref="ParameterOrOverrideBase"/> that can be published
        /// </summary>
        public List<ParameterOrOverrideBase> PublishableParameters { get; set; } = new();

        /// <summary>
        /// Gets or sets the rows used in the Publications component
        /// </summary>
        public SourceList<PublicationRowViewModel> Rows { get; set; } = new();

        /// <summary>
        /// Gets the <see cref="ISessionService"/>
        /// </summary>
        private ISessionService SessionService { get; }

        /// <summary>
        /// Backing field for the <see cref="CanPublish"/> property
        /// </summary>
        private bool canPublish;

        /// <summary>
        /// Gets or sets if the publication is possible
        /// </summary>
        public bool CanPublish
        {
            get => this.canPublish;
            set => this.RaiseAndSetIfChanged(ref this.canPublish, value);
        }

        /// <summary>
        /// Gets or sets the DataSourceUri
        /// </summary>
        public string DataSource { get; private set; }

        /// <summary>
        /// Gets or sets the name of the current <see cref="Person"/> in the <see cref="ISession"/>
        /// </summary>
        public string PersonName { get; private set; }

        /// <summary>
        /// Gets or sets the name of the current <see cref="EngineeringModel"/>
        /// </summary>
        public string ModelName { get; private set; }

        /// <summary>
        /// Gets or sets the name of the current <see cref="Iteration"/>
        /// </summary>
        public string IterationName { get; set; }

        /// <summary>
        /// Gets or sets the name of the current <see cref="DomainOfExpertise"/>
        /// </summary>
        public string DomainName { get; private set; }

        /// <summary>
        /// Creates a new instance of type <see cref="PublicationsViewModel"/>
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService"/></param>
        public PublicationsViewModel(ISessionService sessionService)
        {
            this.SessionService = sessionService;
        }

        /// <summary>
        /// Updates the properties of this ViewModel
        /// </summary>
        /// <param name="iteration">the iteration to use</param>
        public void UpdateProperties(Iteration iteration)
        {
            this.CurrentIteration = iteration;
            this.PublishableParameters = iteration.QueryParameterAndOverrideBases().Where(x => x.CanBePublished).ToList();

            this.DataSource = this.SessionService.Session.DataSourceUri;
            this.PersonName = this.SessionService.Session.ActivePerson.Name;
            this.ModelName = iteration.QueryModelName();
            this.IterationName = iteration.IterationSetup.IterationNumber.ToString();
            this.DomainName = this.SessionService.GetDomainOfExpertise(iteration)?.Name;

            this.Rows.Clear();
            this.Rows.AddRange(CreateRows(this.PublishableParameters));
        }

        /// <summary>
        /// Creates the rows used for the Publications component
        /// </summary>
        /// <param name="parameters">the parameters used to create the rows</param>
        /// <returns>A collection of <see cref="PublicationRowViewModel"/></returns>
        private static IEnumerable<PublicationRowViewModel> CreateRows(IEnumerable<ParameterOrOverrideBase> parameters)
        {
            return parameters.SelectMany(param =>
                param.ValueSets.Select(valueSet => new PublicationRowViewModel(param, valueSet))).ToList();
        }

        /// <summary>
        /// Execute the publication.
        /// </summary>
        /// <returns>An asynchronous operation</returns>
        public async Task ExecutePublish()
        {
            if (!this.CanPublish)
            {
                return;
            }

            var rowsToPublish = this.Rows.Items.Where(x => x.IsSelected).ToList();
            var parametersToPublish = rowsToPublish.Select(x => x.ParameterOrOverride).ToList();
            
            var publication = new Publication(Guid.NewGuid(), null, null);
            var iteration = this.CurrentIteration.Clone(false);

            iteration.Publication.Add(publication);

            publication.Container = iteration;

            publication.PublishedParameter = parametersToPublish;

            await this.SessionService.CreateThing(iteration, publication);

            this.RemovePublishedData(rowsToPublish, parametersToPublish);
        }

        /// <summary>
        /// Removes the published data from the collections
        /// </summary>
        /// <param name="publishedRows">the rows to delete</param>
        /// <param name="publishedParameters">the parameters to delete</param>
        private void RemovePublishedData(IEnumerable<PublicationRowViewModel> publishedRows, IEnumerable<ParameterOrOverrideBase> publishedParameters)
        {
            this.PublishableParameters.RemoveMany(publishedParameters);
            this.Rows.RemoveMany(publishedRows);
        }
    }
}
