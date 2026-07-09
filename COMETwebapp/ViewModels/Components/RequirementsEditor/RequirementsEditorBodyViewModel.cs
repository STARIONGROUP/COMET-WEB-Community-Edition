// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsEditorBodyViewModel.cs" company="Starion Group S.A.">
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

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// View model that handles the logic for the Requirements Editor application: it exposes the
    /// <see cref="RequirementsSpecification" />s of the iteration as documents, and the search/filter state used to
    /// render them.
    /// </summary>
    public class RequirementsEditorBodyViewModel : SingleIterationApplicationBaseViewModel, IRequirementsEditorBodyViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedSpecification" />
        /// </summary>
        private RequirementsSpecification selectedSpecification;

        /// <summary>
        /// Backing field for <see cref="SearchText" />
        /// </summary>
        private string searchText;

        /// <summary>
        /// Backing field for <see cref="IsTocCollapsed" />
        /// </summary>
        private bool isTocCollapsed;

        /// <summary>
        /// Backing field for <see cref="ShowOwner" />
        /// </summary>
        private bool showOwner = true;

        /// <summary>
        /// Backing field for <see cref="ShowCategory" />
        /// </summary>
        private bool showCategory = true;

        /// <summary>
        /// Backing field for <see cref="IndentGroups" />
        /// </summary>
        private bool indentGroups = true;

        /// <summary>
        /// The <see cref="Guid" />s of the table-of-contents tree nodes that are currently collapsed.
        /// </summary>
        private readonly HashSet<Guid> collapsedTreeNodes = [];

        /// <summary>
        /// The <see cref="Guid" />s of the document groups that are currently collapsed.
        /// </summary>
        private readonly HashSet<Guid> collapsedDocumentGroups = [];

        /// <summary>
        /// Backing field for <see cref="DisplayMode" />
        /// </summary>
        private RequirementRowDisplayMode displayMode = RequirementRowDisplayMode.ShortNameNameAndDefinition;

        /// <summary>
        /// Backing field for <see cref="SelectedOwners" />
        /// </summary>
        private IEnumerable<DomainOfExpertise> selectedOwners = [];

        /// <summary>
        /// Backing field for <see cref="SelectedCategories" />
        /// </summary>
        private IEnumerable<Category> selectedCategories = [];

        /// <summary>
        /// Backing field for <see cref="IsOnEditMode" />
        /// </summary>
        private bool isOnEditMode;

        /// <summary>
        /// The <see cref="ILogger{TCategoryName}" /> used to log CRUD failures.
        /// </summary>
        private readonly ILogger<RequirementsEditorBodyViewModel> logger;

        /// <summary>
        /// True when the create/edit form holds a fresh instance to add, false when it holds a clone to update.
        /// </summary>
        private bool isCreating;

        /// <summary>
        /// The <see cref="RequirementsContainer" /> a newly-created group or requirement is placed under.
        /// </summary>
        private RequirementsContainer creationParent;

        /// <summary>
        /// Creates a new instance of <see cref="RequirementsEditorBodyViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public RequirementsEditorBodyViewModel(ISessionService sessionService, ICDPMessageBus messageBus, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ILogger<RequirementsEditorBodyViewModel> logger) : base(sessionService, messageBus)
        {
            this.ShowHideDeprecatedThingsService = showHideDeprecatedThingsService;
            this.logger = logger;

            this.ConfirmCancelPopupViewModel = new ConfirmCancelPopupViewModel
            {
                OnCancel = new EventCallbackFactory().Create(this, () => this.ConfirmCancelPopupViewModel.IsVisible = false)
            };
        }

        /// <summary>
        /// Gets the view model driving the confirm dialog used for deprecate, restore and delete actions.
        /// </summary>
        public IConfirmCancelPopupViewModel ConfirmCancelPopupViewModel { get; }

        /// <summary>
        /// Gets the view model driving the create/edit form, built lazily the first time a dialog is opened.
        /// </summary>
        public IEditRequirementThingViewModel EditViewModel { get; private set; }

        /// <summary>
        /// Gets the header text shown on the create/edit popup.
        /// </summary>
        public string EditPopupHeader { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the create/edit popup is open.
        /// </summary>
        public bool IsOnEditMode
        {
            get => this.isOnEditMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnEditMode, value);
        }

        /// <summary>
        /// Gets the <see cref="IShowHideDeprecatedThingsService" /> that drives whether deprecated things are shown.
        /// </summary>
        public IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// Gets the <see cref="RequirementsSpecification" />s of the current iteration; deprecated ones are included
        /// only when the global "Show Deprecated Things" toggle is on.
        /// </summary>
        public IEnumerable<RequirementsSpecification> AvailableSpecifications =>
            this.CurrentThing == null
                ? []
                : this.CurrentThing.RequirementsSpecification
                    .Where(x => this.ShowHideDeprecatedThingsService.ShowDeprecatedThings || !x.IsDeprecated)
                    .OrderBy(x => x.ShortName);

        /// <summary>
        /// Gets or sets the <see cref="RequirementsSpecification" /> currently shown as a document.
        /// </summary>
        public RequirementsSpecification SelectedSpecification
        {
            get => this.selectedSpecification;
            set => this.RaiseAndSetIfChanged(ref this.selectedSpecification, value);
        }

        /// <summary>
        /// Gets or sets the keyword used to filter requirements on their definition text.
        /// </summary>
        public string SearchText
        {
            get => this.searchText;
            set => this.RaiseAndSetIfChanged(ref this.searchText, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the left table-of-contents tree is collapsed.
        /// </summary>
        public bool IsTocCollapsed
        {
            get => this.isTocCollapsed;
            set => this.RaiseAndSetIfChanged(ref this.isTocCollapsed, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the owner pill is shown on specifications, groups and requirements.
        /// </summary>
        public bool ShowOwner
        {
            get => this.showOwner;
            set => this.RaiseAndSetIfChanged(ref this.showOwner, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the category pills are shown on specifications, groups and requirements.
        /// </summary>
        public bool ShowCategory
        {
            get => this.showCategory;
            set => this.RaiseAndSetIfChanged(ref this.showCategory, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether groups are indented in the document. When off, groups and
        /// requirements are rendered flush-left regardless of nesting depth.
        /// </summary>
        public bool IndentGroups
        {
            get => this.indentGroups;
            set => this.RaiseAndSetIfChanged(ref this.indentGroups, value);
        }

        /// <summary>
        /// Gets or sets the way each requirement is rendered as a row.
        /// </summary>
        public RequirementRowDisplayMode DisplayMode
        {
            get => this.displayMode;
            set => this.RaiseAndSetIfChanged(ref this.displayMode, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="DomainOfExpertise" /> owners; an empty selection means no owner filtering.
        /// </summary>
        public IEnumerable<DomainOfExpertise> SelectedOwners
        {
            get => this.selectedOwners;
            set => this.RaiseAndSetIfChanged(ref this.selectedOwners, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Category" /> values; an empty selection means no category filtering.
        /// </summary>
        public IEnumerable<Category> SelectedCategories
        {
            get => this.selectedCategories;
            set => this.RaiseAndSetIfChanged(ref this.selectedCategories, value);
        }

        /// <summary>
        /// Gets the distinct <see cref="DomainOfExpertise" /> owners available to filter on.
        /// </summary>
        public IReadOnlyList<DomainOfExpertise> AvailableOwners { get; private set; } = [];

        /// <summary>
        /// Gets the distinct <see cref="Category" /> values available to filter on.
        /// </summary>
        public IReadOnlyList<Category> AvailableCategories { get; private set; } = [];

        /// <summary>
        /// Gets a value indicating whether any search or filter is currently narrowing the document.
        /// </summary>
        private bool IsFilterActive => !string.IsNullOrWhiteSpace(this.SearchText) || this.SelectedOwners.Any() || this.SelectedCategories.Any();

        /// <summary>
        /// Gets the visible, non-deprecated requirements located directly under the given <paramref name="container" />.
        /// </summary>
        /// <param name="container">The <see cref="RequirementsContainer" /> (specification or group)</param>
        /// <returns>The requirements to render under that container</returns>
        public IEnumerable<Requirement> GetRequirements(RequirementsContainer container)
        {
            if (this.SelectedSpecification == null)
            {
                return [];
            }

            var group = container as RequirementsGroup;

            return this.SelectedSpecification.Requirement
                .Where(x => (this.ShowHideDeprecatedThingsService.ShowDeprecatedThings || !x.IsDeprecated) && ReferenceEquals(x.Group, group) && this.PassesFilter(x))
                .OrderBy(x => x.ShortName);
        }

        /// <summary>
        /// Gets the visible child <see cref="RequirementsGroup" />s of the given <paramref name="container" />.
        /// </summary>
        /// <param name="container">The <see cref="RequirementsContainer" /> (specification or group)</param>
        /// <returns>The child groups to render</returns>
        public IEnumerable<RequirementsGroup> GetGroups(RequirementsContainer container)
        {
            return container.Group
                .Where(this.ShouldDisplayGroup)
                .OrderBy(x => x.ShortName);
        }

        /// <summary>
        /// Determines whether the given <paramref name="group" /> should be displayed given the active search and filters.
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup" /></param>
        /// <returns>true if no filter is active, or the group (or a descendant) has visible requirements</returns>
        public bool ShouldDisplayGroup(RequirementsGroup group)
        {
            if (!this.IsFilterActive)
            {
                return true;
            }

            return this.GetRequirements(group).Any() || group.Group.Any(this.ShouldDisplayGroup);
        }

        /// <summary>
        /// Gets whether the tree node with the given <paramref name="iid" /> is collapsed in the table-of-contents tree.
        /// </summary>
        /// <param name="iid">The identifier of the specification or group node</param>
        /// <returns>true if collapsed</returns>
        public bool IsTreeNodeCollapsed(Guid iid)
        {
            return this.collapsedTreeNodes.Contains(iid);
        }

        /// <summary>
        /// Toggles the collapsed state of the table-of-contents tree node with the given <paramref name="iid" />.
        /// </summary>
        /// <param name="iid">The identifier of the specification or group node</param>
        public void ToggleTreeNode(Guid iid)
        {
            if (!this.collapsedTreeNodes.Add(iid))
            {
                this.collapsedTreeNodes.Remove(iid);
            }
        }

        /// <summary>
        /// Gets whether the group with the given <paramref name="iid" /> is collapsed in the document panel.
        /// </summary>
        /// <param name="iid">The identifier of the group</param>
        /// <returns>true if collapsed</returns>
        public bool IsDocumentGroupCollapsed(Guid iid)
        {
            return this.collapsedDocumentGroups.Contains(iid);
        }

        /// <summary>
        /// Toggles the collapsed state of the document group with the given <paramref name="iid" />.
        /// </summary>
        /// <param name="iid">The identifier of the group</param>
        public void ToggleDocumentGroup(Guid iid)
        {
            if (!this.collapsedDocumentGroups.Add(iid))
            {
                this.collapsedDocumentGroups.Remove(iid);
            }
        }

        /// <summary>
        /// Opens the create form for a new <see cref="RequirementsSpecification" /> directly under the iteration.
        /// </summary>
        public void OpenCreateSpecification()
        {
            this.OpenForCreate(new RequirementsSpecification { Iid = Guid.NewGuid() }, null, "Create Specification");
        }

        /// <summary>
        /// Opens the create form for a new <see cref="RequirementsGroup" /> under the given <paramref name="parent" />.
        /// </summary>
        /// <param name="parent">The specification or group the new group is placed under.</param>
        public void OpenCreateGroup(RequirementsContainer parent)
        {
            this.OpenForCreate(new RequirementsGroup { Iid = Guid.NewGuid() }, parent, "Create Requirement Group");
        }

        /// <summary>
        /// Opens the create form for a new <see cref="Requirement" /> under the given <paramref name="parent" />.
        /// </summary>
        /// <param name="parent">The specification or group the new requirement is filed under.</param>
        public void OpenCreateRequirement(RequirementsContainer parent)
        {
            var requirement = new Requirement { Iid = Guid.NewGuid(), Group = parent as RequirementsGroup };
            this.OpenForCreate(requirement, parent, "Create Requirement");
        }

        /// <summary>
        /// Opens the edit form for the given <paramref name="thing" /> (a specification, group or requirement).
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> to edit.</param>
        public void OpenEdit(Thing thing)
        {
            this.EnsureEditViewModel();
            this.isCreating = false;
            this.creationParent = null;

            this.EditPopupHeader = thing switch
            {
                RequirementsSpecification => "Edit Specification",
                RequirementsGroup => "Edit Requirement Group",
                _ => "Edit Requirement"
            };

            this.EditViewModel.InitializeViewModel(thing.Clone(true), this.CurrentThing, this.GetSelectedSpecificationGroups());
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Opens the confirm dialog to toggle the deprecation of the given deprecatable <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The <see cref="Requirement" /> or <see cref="RequirementsSpecification" /> to deprecate or restore.</param>
        public void ConfirmDeprecation(Thing thing)
        {
            if (thing is not IDeprecatableThing deprecatable)
            {
                return;
            }

            var willDeprecate = !deprecatable.IsDeprecated;
            var label = GetLabel(thing);

            this.ConfirmCancelPopupViewModel.HeaderText = willDeprecate ? "Confirm deprecation" : "Confirm restore";
            this.ConfirmCancelPopupViewModel.ContentText = $"Are you sure you want to {(willDeprecate ? "deprecate" : "restore")} the {GetKindLabel(thing)} '{label}'?";
            this.ConfirmCancelPopupViewModel.OnConfirm = new EventCallbackFactory().Create(this, () => this.SetDeprecationAsync(thing, willDeprecate));
            this.ConfirmCancelPopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Opens the confirm dialog to permanently delete the given <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> to delete.</param>
        public void ConfirmDeletion(Thing thing)
        {
            this.ConfirmCancelPopupViewModel.HeaderText = "Confirm deletion";
            this.ConfirmCancelPopupViewModel.ContentText = $"Are you sure you want to permanently delete the {GetKindLabel(thing)} '{GetLabel(thing)}'?";
            this.ConfirmCancelPopupViewModel.OnConfirm = new EventCallbackFactory().Create(this, () => this.DeleteAsync(thing));
            this.ConfirmCancelPopupViewModel.IsVisible = true;
        }

        /// <summary>
        /// Persists an inline edit of the first <see cref="Definition" /> of the given <paramref name="requirement" />.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /> whose definition changed.</param>
        /// <param name="content">The new definition content.</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task SaveInlineDefinitionAsync(Requirement requirement, string content)
        {
            if (requirement == null || string.Equals(requirement.Definition.FirstOrDefault()?.Content ?? string.Empty, content ?? string.Empty, StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                this.IsLoading = true;

                var clone = (Requirement)requirement.Clone(true);
                var definition = clone.Definition.FirstOrDefault();

                if (definition == null)
                {
                    definition = new Definition { Iid = Guid.NewGuid(), LanguageCode = "en-GB" };
                    clone.Definition.Add(definition);
                }

                definition.Content = content;

                var specificationClone = requirement.GetContainerOfType<RequirementsSpecification>().Clone(false);
                var thingsToWrite = new List<Thing> { specificationClone, clone };
                thingsToWrite.AddRange(clone.Definition);

                var result = await this.SessionService.CreateOrUpdateThingsWithNotification(specificationClone, thingsToWrite, BuildNotification(requirement, "updated", "update"));

                if (result.IsSuccess)
                {
                    await this.ReloadPreservingSelection();
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while editing the definition of the Requirement with iid {Iid}", requirement.Iid);
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Handles the refresh of the current session by reloading the specifications while preserving the selection.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            return this.ReloadPreservingSelection();
        }

        /// <summary>
        /// Builds the flattened list of every <see cref="RequirementsGroup" /> of the selected specification, used to file a requirement.
        /// </summary>
        /// <returns>All groups of the selected specification, or an empty list when none is selected.</returns>
        private List<RequirementsGroup> GetSelectedSpecificationGroups()
        {
            var result = new List<RequirementsGroup>();

            if (this.SelectedSpecification == null)
            {
                return result;
            }

            void Collect(RequirementsContainer container)
            {
                foreach (var group in container.Group.OrderBy(x => x.ShortName))
                {
                    result.Add(group);
                    Collect(group);
                }
            }

            Collect(this.SelectedSpecification);
            return result;
        }

        /// <summary>
        /// Builds the <see cref="EditViewModel" /> on first use so the existing session mocks are not touched until a dialog opens.
        /// </summary>
        private void EnsureEditViewModel()
        {
            this.EditViewModel ??= new EditRequirementThingViewModel(this.SessionService, this.MessageBus)
            {
                OnValidSubmit = new EventCallbackFactory().Create(this, this.OnEditValidSubmitAsync)
            };
        }

        /// <summary>
        /// Configures the create form with a fresh <paramref name="thing" /> and opens the popup.
        /// </summary>
        /// <param name="thing">The fresh instance to create.</param>
        /// <param name="parent">The container the group or requirement is placed under; null for a specification.</param>
        /// <param name="header">The popup header.</param>
        private void OpenForCreate(Thing thing, RequirementsContainer parent, string header)
        {
            this.EnsureEditViewModel();
            this.isCreating = true;
            this.creationParent = parent;
            this.EditPopupHeader = header;
            this.EditViewModel.InitializeViewModel(thing, this.CurrentThing, this.GetSelectedSpecificationGroups());
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Commits the create or edit held by <see cref="EditViewModel" /> to the session.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnEditValidSubmitAsync()
        {
            var thing = this.EditViewModel.Thing;

            try
            {
                this.IsLoading = true;

                Thing topContainer;
                var thingsToWrite = new List<Thing> { thing };

                switch (thing)
                {
                    case RequirementsSpecification specification:
                    {
                        var iterationClone = this.CurrentThing.Clone(false);

                        if (this.isCreating)
                        {
                            specification.Container = this.CurrentThing;
                            iterationClone.RequirementsSpecification.Add(specification);
                        }

                        topContainer = iterationClone;
                        thingsToWrite.Add(iterationClone);
                        break;
                    }

                    case RequirementsGroup group:
                    {
                        if (this.isCreating)
                        {
                            group.Container = this.creationParent;
                            var parentClone = this.creationParent.Clone(false);
                            ((RequirementsContainer)parentClone).Group.Add(group);
                            topContainer = parentClone;
                            thingsToWrite.Add(parentClone);
                        }
                        else
                        {
                            var containerClone = group.Container.Clone(false);
                            topContainer = containerClone;
                            thingsToWrite.Add(containerClone);
                        }

                        break;
                    }

                    case Requirement requirement:
                    {
                        var requirementSpecification = this.isCreating
                            ? this.creationParent as RequirementsSpecification ?? this.creationParent.GetContainerOfType<RequirementsSpecification>()
                            : requirement.GetContainerOfType<RequirementsSpecification>();

                        var specificationClone = (RequirementsSpecification)requirementSpecification.Clone(false);

                        if (this.isCreating)
                        {
                            requirement.Container = requirementSpecification;
                            specificationClone.Requirement.Add(requirement);
                        }

                        topContainer = specificationClone;
                        thingsToWrite.Add(specificationClone);
                        break;
                    }

                    default:
                        return;
                }

                thingsToWrite.AddRange(((DefinedThing)thing).Definition);

                var result = await this.SessionService.CreateOrUpdateThingsWithNotification(topContainer, thingsToWrite, BuildNotification(thing, this.isCreating ? "created" : "updated", this.isCreating ? "create" : "update"));

                if (result.IsSuccess)
                {
                    this.IsOnEditMode = false;
                    await this.ReloadPreservingSelection();
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while saving the {ClassKind} with iid {Iid}", thing.ClassKind, thing.Iid);
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Persists a deprecation toggle for the given <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The deprecatable <see cref="Thing" />.</param>
        /// <param name="deprecate">True to deprecate, false to restore.</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task SetDeprecationAsync(Thing thing, bool deprecate)
        {
            this.ConfirmCancelPopupViewModel.IsVisible = false;

            try
            {
                this.IsLoading = true;

                var clone = thing.Clone(false);
                ((IDeprecatableThing)clone).IsDeprecated = deprecate;
                var containerClone = thing.Container.Clone(false);

                var result = await this.SessionService.CreateOrUpdateThingsWithNotification(containerClone, [containerClone, clone], BuildNotification(thing, deprecate ? "deprecated" : "restored", deprecate ? "deprecate" : "restore"));

                if (result.IsSuccess)
                {
                    await this.ReloadPreservingSelection();
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while changing the deprecation of the {ClassKind} with iid {Iid}", thing.ClassKind, thing.Iid);
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Permanently deletes the given <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> to delete.</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task DeleteAsync(Thing thing)
        {
            this.ConfirmCancelPopupViewModel.IsVisible = false;

            try
            {
                this.IsLoading = true;

                var containerClone = thing.Container.Clone(false);
                var clone = thing.Clone(false);

                var result = await this.SessionService.DeleteThingsWithNotification(containerClone, [clone], BuildNotification(thing, "deleted", "delete"));

                if (result.IsSuccess)
                {
                    if (ReferenceEquals(this.SelectedSpecification, thing))
                    {
                        this.SelectedSpecification = null;
                    }

                    await this.ReloadPreservingSelection();
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while deleting the {ClassKind} with iid {Iid}", thing.ClassKind, thing.Iid);
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Reloads the specifications while preserving the current selection when still present.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ReloadPreservingSelection()
        {
            var previouslySelected = this.SelectedSpecification;
            await this.OnThingChanged();

            if (previouslySelected != null && this.AvailableSpecifications.Contains(previouslySelected))
            {
                this.SelectedSpecification = previouslySelected;
            }
        }

        /// <summary>
        /// Builds the notification shown after a CRUD operation.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> operated on.</param>
        /// <param name="pastVerb">The past-tense verb for the success message (e.g. "created").</param>
        /// <param name="actionVerb">The infinitive verb for the failure message (e.g. "create").</param>
        /// <returns>The <see cref="NotificationDescription" />.</returns>
        private static NotificationDescription BuildNotification(Thing thing, string pastVerb, string actionVerb)
        {
            var kind = GetKindLabel(thing);
            var label = GetLabel(thing);

            return new NotificationDescription
            {
                OnSuccess = $"{kind} '{label}' {pastVerb}",
                OnError = $"Could not {actionVerb} the {kind} '{label}'"
            };
        }

        /// <summary>
        /// Gets the human-readable label of the given <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" />.</param>
        /// <returns>The name, or the short name when the name is empty.</returns>
        private static string GetLabel(Thing thing)
        {
            if (thing is not DefinedThing defined)
            {
                return thing.ClassKind.ToString();
            }

            return string.IsNullOrWhiteSpace(defined.Name) ? defined.ShortName : defined.Name;
        }

        /// <summary>
        /// Gets the human-readable kind label of the given <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" />.</param>
        /// <returns>The kind label.</returns>
        private static string GetKindLabel(Thing thing)
        {
            return thing switch
            {
                RequirementsSpecification => "Specification",
                RequirementsGroup => "Requirement group",
                Requirement => "Requirement",
                _ => "item"
            };
        }

        /// <summary>
        /// Update this view model properties when the <see cref="Iteration" /> has changed.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();

            this.IsLoading = true;

            var allRequirements = this.CurrentThing.RequirementsSpecification
                .Where(x => !x.IsDeprecated)
                .SelectMany(x => x.Requirement)
                .Where(x => !x.IsDeprecated)
                .ToList();

            this.AvailableOwners = allRequirements
                .Select(x => x.Owner)
                .Where(x => x != null)
                .DistinctBy(x => x.Iid)
                .OrderBy(x => x.ShortName)
                .ToList();

            this.AvailableCategories = allRequirements
                .SelectMany(x => x.Category)
                .DistinctBy(x => x.Iid)
                .OrderBy(x => x.ShortName)
                .ToList();

            this.SelectedSpecification = this.AvailableSpecifications.FirstOrDefault();

            this.IsLoading = false;
        }

        /// <summary>
        /// Determines whether the given <paramref name="requirement" /> passes the active search and filters.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>true if it should be shown</returns>
        private bool PassesFilter(Requirement requirement)
        {
            if (!string.IsNullOrWhiteSpace(this.SearchText)
                && !requirement.Definition.Any(x => x.Content != null && x.Content.Contains(this.SearchText, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (this.SelectedOwners.Any() && !this.SelectedOwners.Contains(requirement.Owner))
            {
                return false;
            }

            return !this.SelectedCategories.Any() || requirement.Category.Intersect(this.SelectedCategories).Any();
        }
    }
}
