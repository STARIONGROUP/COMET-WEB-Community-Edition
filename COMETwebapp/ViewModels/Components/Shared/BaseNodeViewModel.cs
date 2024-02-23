// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BaseNodeViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.Shared
{
    using COMET.Web.Common.Utilities.DisposableObject;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// ViewModel that handle information related to <see cref="BaseNodeViewModel{T}" /> inside a tree
    /// </summary>
    public abstract class BaseNodeViewModel<T> : DisposableObject, IBaseNodeViewModel where T : BaseNodeViewModel<T>
    {
        /// <summary>
        /// Backing field for the <see cref="IsDrawn" />
        /// </summary>
        private bool isDrawn = true;

        /// <summary>
        /// Backing field for the <see cref="IsExpanded" />
        /// </summary>
        private bool isExpanded;

        /// <summary>
        /// Backing field for the <see cref="IsSelected" />
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Creates a new instance of type <see cref="BaseNodeViewModel{T}" />
        /// </summary>
        /// <param name="title">the title of the node</param>
        protected BaseNodeViewModel(string title)
        {
            this.Title = title;
        }

        /// <summary>
        /// Gets or sets the parent of this <see cref="BaseNodeViewModel{T}" />
        /// </summary>
        public T Parent { get; set; }

        /// <summary>
        /// Field for containing the children of this <see cref="BaseNodeViewModel{T}" />
        /// </summary>
        protected List<T> Children { get; set; } = [];

        /// <summary>
        /// Level of the tree. Increases by one for each nested element
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the title of this <see cref="BaseNodeViewModel{T}" />
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="BaseNodeViewModel{T}" /> is expanded
        /// </summary>
        public bool IsExpanded
        {
            get => this.isExpanded;
            set => this.RaiseAndSetIfChanged(ref this.isExpanded, value);
        }

        /// <summary>
        /// Gets or sets if the <see cref="BaseNodeViewModel{T}" /> is drawn
        /// </summary>
        public bool IsDrawn
        {
            get => this.isDrawn;
            set => this.RaiseAndSetIfChanged(ref this.isDrawn, value);
        }

        /// <summary>
        /// Gets or sets if the <see cref="BaseNodeViewModel{T}" /> is selected
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.RaiseAndSetIfChanged(ref this.isSelected, value);
        }

        /// <summary>
        /// Adds a child to this <see cref="BaseNodeViewModel{T}" />
        /// </summary>
        /// <param name="baseNodeViewModel">the child to add</param>
        /// <returns>this <see cref="BaseNodeViewModel{T}" /></returns>
        public T AddChild(T baseNodeViewModel)
        {
            ArgumentNullException.ThrowIfNull(baseNodeViewModel);

            baseNodeViewModel.Parent = (T)this;
            this.Children.Add(baseNodeViewModel);

            return (T)this;
        }

        /// <summary>
        /// Removes a child from this <see cref="BaseNodeViewModel{T}" />
        /// </summary>
        /// <param name="baseNodeViewModel">the child to remove</param>
        /// <returns>this <see cref="BaseNodeViewModel{T}" /></returns>
        public T RemoveChild(T baseNodeViewModel)
        {
            ArgumentNullException.ThrowIfNull(baseNodeViewModel);

            baseNodeViewModel.Parent = null;
            this.Children.Remove(baseNodeViewModel);

            return (T)this;
        }

        /// <summary>
        /// Gets the <see cref="BaseNodeViewModel{T}" /> that is on top of the hierarchy
        /// </summary>
        /// <returns>the <see cref="BaseNodeViewModel{T}" /> or this baseNode if the RootViewModel can't be computed</returns>
        public T GetRootNode()
        {
            var currentParent = this.Parent;

            while (currentParent != null)
            {
                if (currentParent.Parent is null)
                {
                    return currentParent;
                }

                currentParent = currentParent.Parent;
            }

            return (T)this;
        }

        /// <summary>
        /// Gets a flat list of the descendants of this baseNode
        /// </summary>
        /// <returns>the flat list</returns>
        public List<T> GetFlatListOfDescendants(bool includeSelf = false)
        {
            var descendants = new List<T>();
            GetListOfDescendantsRecursively((T)this, ref descendants);

            if (includeSelf && !descendants.Contains(this))
            {
                descendants.Add((T)this);
            }

            return descendants;
        }

        /// <summary>
        /// Sort all descendants of this node by the title
        /// </summary>
        public void OrderAllDescendantsByShortName()
        {
            this.OrderChildrenByShortNameHelper((T)this);
        }

        /// <summary>
        /// Gets the parent baseNode of this <see cref="BaseNodeViewModel{T}" />
        /// </summary>
        /// <returns>the parent baseNode</returns>
        public T GetParentNode()
        {
            return this.Parent;
        }

        /// <summary>
        /// Gets the children of this <see cref="BaseNodeViewModel{T}" />
        /// </summary>
        /// <returns>the children of the baseNode</returns>
        public ICollection<T> GetChildren()
        {
            return this.Children.AsReadOnly();
        }

        /// <summary>
        /// Gets if this method is the first child.
        /// </summary>
        /// <returns>true if it's the first child, or last and the parent only contains this baseNode, false otherwise.</returns>
        public bool IsFirstChild()
        {
            if (this.Parent is null)
            {
                return true;
            }

            return this.Parent.GetChildren().IndexOf(this) == 0;
        }

        /// <summary>
        /// Gets if this method is the last child.
        /// </summary>
        /// <returns>true if it's the last child, or first and the parent only contains this baseNode, false otherwise.</returns>
        public bool IsLastChild()
        {
            if (this.Parent is null)
            {
                return true;
            }

            return this.Parent.GetChildren().Last() == this;
        }

        /// <summary>
        /// Gets the <see cref="BaseNodeViewModel{T}" /> that is on top of the hierarchy by the <paramref name="numberOfLevels" />
        /// specified
        /// </summary>
        /// <returns>the <see cref="BaseNodeViewModel{T}" /> or null if the ascendant can't be computed</returns>
        public T GetAscendant(int numberOfLevels)
        {
            if (numberOfLevels == 0)
            {
                return null;
            }

            var levelAscended = 1;
            var currentParent = this.Parent;

            while (currentParent is not null && levelAscended < numberOfLevels)
            {
                if (currentParent.Parent is null)
                {
                    return currentParent;
                }

                levelAscended++;
                currentParent = currentParent.Parent;
            }

            return currentParent;
        }

        /// <summary>
        /// Gets if the <paramref name="baseNodeViewModel" /> is direct child of this one
        /// </summary>
        /// <param name="baseNodeViewModel">the node to check</param>
        /// <returns>true if is a direct child, false otherwise</returns>
        public bool IsDirectChild(T baseNodeViewModel)
        {
            if (this == baseNodeViewModel)
            {
                throw new NotSupportedException("The baseNode can't be child of itself.");
            }

            return this.GetChildren().Contains(baseNodeViewModel);
        }

        /// <summary>
        /// Gets if <paramref name="baseNodeViewModel" /> is descendant of this one
        /// </summary>
        /// <param name="baseNodeViewModel">the node to check</param>
        /// <returns>true if is a descendant, false otherwise</returns>
        public bool IsDescendant(T baseNodeViewModel)
        {
            if (this == baseNodeViewModel)
            {
                throw new NotSupportedException("The baseNode can't be descendant of itself.");
            }

            return this.GetFlatListOfDescendants().Contains(baseNodeViewModel);
        }

        /// <summary>
        /// Callback method for when a node is selected
        /// </summary>
        public abstract void RaiseTreeSelectionChanged();

        /// <summary>
        /// Helper method for <see cref="GetFlatListOfDescendants" />
        /// </summary>
        /// <param name="current">the current evaluated <see cref="BaseNodeViewModel{T}" /></param>
        /// <param name="descendants">the list of descendants till this moment</param>
        public static void GetListOfDescendantsRecursively(T current, ref List<T> descendants)
        {
            foreach (var child in current.GetChildren())
            {
                if (!descendants.Contains(child))
                {
                    descendants.Add(child);
                }

                GetListOfDescendantsRecursively(child, ref descendants);
            }
        }

        /// <summary>
        /// Helper method for <see cref="OrderAllDescendantsByShortName" />
        /// </summary>
        /// <param name="current">the current evaluated <see cref="BaseNodeViewModel{T}" /></param>
        private void OrderChildrenByShortNameHelper(T current)
        {
            current.Children = current.GetChildren().OrderBy(x => x.Title).ToList();

            foreach (var child in current.GetChildren())
            {
                this.OrderChildrenByShortNameHelper(child);
            }
        }
    }
}
