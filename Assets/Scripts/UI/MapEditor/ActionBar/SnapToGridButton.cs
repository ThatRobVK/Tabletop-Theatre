﻿/*
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

using TT.Data;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.ActionBar
{
    [RequireComponent(typeof(Toggle))]
    public class SnapToGridButton : MonoBehaviour
    {
        private SettingsObject _settings;

        void OnEnable()
        {
            _settings = FindObjectOfType<SettingsObject>();
            GetComponent<Toggle>().isOn = _settings.editorSettings.snapToGrid;
            GetComponent<Toggle>().onValueChanged.AddListener(HandleValueChanged);
        }

        private void HandleValueChanged(bool value)
        {
            _settings.editorSettings.snapToGrid = value;
        }
    }
}