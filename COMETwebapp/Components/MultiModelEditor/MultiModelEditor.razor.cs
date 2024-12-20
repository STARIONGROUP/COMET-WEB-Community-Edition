// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiModelEditor.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Components.MultiModelEditor
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Extensions;

    using COMETwebapp.ViewModels.Components.ModelEditor.Rows;
    using COMETwebapp.ViewModels.Components.MultiModelEditor.Rows;

    using DevExpress.Blazor;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="MultiModelEditor" /> component
    /// </summary>
    public partial class MultiModelEditor
    {
        /// <summary>
        /// Holds a reference to the data of the node where another node is dragged over
        /// </summary>
        private (ElementDefinitionTree, object) DragOverObject;

        /// <summary>
        /// Holds a reference to the data of the node that is currently dragged
        /// </summary>
        private (ElementDefinitionTree, ElementBaseTreeRowViewModel) DragObject;

        /// <summary>
        /// The validation messages to display
        /// </summary>
        private string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the source tree component
        /// </summary>
        public ElementDefinitionTree SourceTree { get; set; }

        /// <summary>
        /// Gets or sets the target tree component
        /// </summary>
        public ElementDefinitionTree TargetTree { get; set; }

        /// <summary>
        /// Handles the post-assignement flow of the <see cref="ApplicationBase{TViewModel}.ViewModel" /> property
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            base.OnViewModelAssigned();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnCreationMode).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnAddingParameterMode).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
        }

        /// <summary>
        /// Method executed when a <see cref="ElementDefinitionRowViewModel" /> is selected
        /// </summary>
        /// <param name="elementRowViewModel">The <see cref="ElementBaseTreeRowViewModel" /></param>
        private void OnElementSelected(ElementBaseTreeRowViewModel elementRowViewModel)
        {
            this.ViewModel.SelectElement(elementRowViewModel?.ElementBase);
        }

        /// <summary>
        /// Is executed when dragging has been started for a specific node (<see cref="ElementBaseTreeRowViewModel"/>) in a specific <see cref="ElementDefinitionTree"/>
        /// </summary>
        /// <param name="nodeData">A <see cref="Tuple"/> that contains the specific <see cref="ElementDefinitionTree"/> and the specific node (<see cref="ElementBaseTreeRowViewModel"/>)</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private Task OnDragStart((ElementDefinitionTree, ElementBaseTreeRowViewModel) nodeData)
        {
            this.DragObject = nodeData;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is executed when dragging has been ended for a specific node (<see cref="ElementBaseTreeRowViewModel"/>) in a specific <see cref="ElementDefinitionTree"/>
        /// </summary>
        /// <param name="nodeData">A <see cref="Tuple"/> that contains the specific <see cref="ElementDefinitionTree"/> and the specific node (<see cref="ElementBaseTreeRowViewModel"/>)</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private Task OnDragEnd((ElementDefinitionTree, ElementBaseTreeRowViewModel) nodeData)
        {
            this.DragObject = (null, null);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is executed when a dragged node (<see cref="ElementBaseTreeRowViewModel"/>) has been dropped onto another element in a specific <see cref="ElementDefinitionTree"/>
        /// </summary>
        /// <param name="nodeData">A <see cref="Tuple"/> that contains the specific <see cref="ElementDefinitionTree"/> and the specific node (<see cref="ElementBaseTreeRowViewModel"/>)</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private async Task OnDrop((ElementDefinitionTree, ElementBaseTreeRowViewModel) nodeData)
        {
            this.ErrorMessage = string.Empty;

            if (this.DragObject.Item2 is not ElementDefinitionTreeTreeRowViewModel elementDefinitionTreeRowViewModel)
            {
                return;
            }

            try
            {
                if (nodeData.Item2 == null)
                {
                    // Drop in the same model
                    await this.ViewModel.CopyAndAddNewElement(elementDefinitionTreeRowViewModel.ElementBase);
                }
                else
                {
                    await this.ViewModel.AddNewElementUsage(elementDefinitionTreeRowViewModel.ElementBase, nodeData.Item2.ElementBase);
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            finally
            {
                this.DragOverObject = (null, null);
                this.DragObject = (null, null);

                this.StateHasChanged();
            }
        }

        /// <summary>
        /// Is executed when a dragged node (<see cref="ElementBaseTreeRowViewModel"/>) hovers over a specific element (<see cref="object"/>) in a specific <see cref="ElementDefinitionTree"/>
        /// </summary>
        /// <param name="elementData">A <see cref="Tuple"/> that contains the specific <see cref="ElementDefinitionTree"/> and the specific element (<see cref="object"/>)</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private Task OnDragEnter((ElementDefinitionTree, object) elementData)
        {
            this.DragOverObject = elementData;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is executed when a dragged node (<see cref="ElementBaseTreeRowViewModel"/>) leaves a previously hovered over specific element (<see cref="object"/>) in a specific <see cref="ElementDefinitionTree"/>
        /// </summary>
        /// <param name="elementData">A <see cref="Tuple"/> that contains the specific <see cref="ElementDefinitionTree"/> and the specific element (<see cref="object"/>)</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private Task OnDragLeave((ElementDefinitionTree, object) elementData)
        {
            this.DragOverObject = (null, null);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the AllowNodeDrop property for a specific node in a <see cref="ElementDefinitionTree"/>, based on the calculated <see cref="ElementBase"/> data for <see cref="DragOverObject"/> and <see cref="DragObject"/>
        /// </summary>
        /// <param name="elementDefinitionTree">The <see cref="ElementDefinitionTree"/> to calculate this for</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private Task SetDropIsAllowed(ElementDefinitionTree elementDefinitionTree)
        {
            elementDefinitionTree.AllowNodeDrop = this.CalculateDropIsAllowed(elementDefinitionTree);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Calculates is dropping of a dragged node (<see cref="DragObject"/>) is allowed onto the node where it hovers over (<see cref="DragOverObject"/>)
        /// </summary>
        /// <param name="elementDefinitionTree">The <see cref="ElementDefinitionTree"/> where to calculate for</param>
        /// <returns>A value indicating is dropping is actually allowed</returns>
        private bool CalculateDropIsAllowed(ElementDefinitionTree elementDefinitionTree)
        {
            var dragOverObject = this.DragOverObject;
            var dragObject = this.DragObject;

            if (elementDefinitionTree.AllowDrop)
            {
                if (dragObject != (null, null))
                {
                    if (dragOverObject == dragObject)
                    {
                        return false;
                    }

                    if (dragOverObject.Item2 is ElementDefinitionTreeTreeRowViewModel dragOverVm)
                    {
                        if (dragObject.Item2 is ElementDefinitionTreeTreeRowViewModel dragVm)
                        {
                            if (dragOverVm.ElementBase == dragVm.ElementBase)
                            {
                                return false;
                            }

                            if (dragOverVm.ElementBase.GetContainerOfType<Iteration>() == dragVm.ElementBase.GetContainerOfType<Iteration>())
                            {
                                return true;
                            }

                            return false;
                        }

                        return false;
                    }

                    if (dragOverObject.Item1 == elementDefinitionTree)
                    {
                        return true;
                    }

                    return false;
                }

                return false;
            }

            return false;
        }
    }
}
