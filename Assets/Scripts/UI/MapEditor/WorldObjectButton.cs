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

using System;
using System.Globalization;
using System.Linq;
using TMPro;
using TT.CameraControllers;
using TT.World;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor
{
    public class WorldObjectButton : MonoBehaviour
    {
        [SerializeField][Tooltip("The Text object where the object name is displayed.")] private TMP_Text objectNameLabel;
        [SerializeField][Tooltip("The button the user presses to go to an object.")] private Button button;
        [SerializeField][Tooltip("The star image to show if the item is starred.")] private Image star;

        private Guid _objectId;


        void Start()
        {
            button.onClick.AddListener(HandleButtonClicked);
        }


        private void HandleButtonClicked()
        {
            var worldObject = WorldObjectBase.All.Where(x => x.ObjectId == _objectId).FirstOrDefault();
            if (worldObject != null)
            {
                worldObject.SwitchSelection();
                CameraController.Current.MoveTo(worldObject.Position);
            }
        }


        /// <summary>
        /// Displays the object name.
        /// </summary>
        /// <param name="worldObject">The world object to display.</param>
        public void Initialise(WorldObjectBase worldObject)
        {
            objectNameLabel.text = $"{worldObject.name} ({worldObject.Position.x.ToString(CultureInfo.CurrentCulture)}, {worldObject.Position.z.ToString(CultureInfo.CurrentCulture)})";
            star.gameObject.SetActive(worldObject.Starred);
            _objectId = worldObject.ObjectId;
        }
    }
}