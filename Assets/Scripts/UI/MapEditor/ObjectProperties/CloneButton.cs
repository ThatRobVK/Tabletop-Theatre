/*
 * Tabletop Theatre
 * Copyright (C) 2020-2022 Robert van Kooten
 * Original source code: https://github.com/ThatRobVK/Tabletop-Theatre
 * License: https://github.com/ThatRobVK/Tabletop-Theatre/blob/main/LICENSE
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#pragma warning disable IDE0083 // Use pattern matching - this isn't supported by the version of C# used by Unity

using TT.State;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.ObjectProperties
{
    [RequireComponent(typeof(ToggledButton))]
    public class CloneButton : MonoBehaviour
    {

        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            GetComponent<ToggledButton>().OnClick += HandleButtonClicked;
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the clone button is clicked. Create a copy of the selected object.
        /// </summary>
        private async void HandleButtonClicked()
        {
            var fromObject = WorldObjectBase.Current;

            if (fromObject && StateController.CurrentStateType != StateType.ItemPlacement)
            {
                var newObject = await WorldObjectFactory.Clone(fromObject);
                fromObject.SwitchSelection(newObject);

                StateController.CurrentState.ToPlacement();
                newObject.PickUp();

                // Prevent clones from being generated each time we place
                Helpers.Settings.editorSettings.continuousPlacement = false;
            }
        }

        #endregion
    }
}


