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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TT.Data
{
    [Serializable]
    public class MapObjectBase
    {
        public string objectId;
        [FormerlySerializedAs("Name")] public string name;
        [FormerlySerializedAs("Position")] public Vector3 position;
        [FormerlySerializedAs("Rotation")] public Vector3 rotation;
        [FormerlySerializedAs("Scale")] public Vector3 scale;
        [FormerlySerializedAs("PrefabAddress")] public string prefabAddress;
        [FormerlySerializedAs("GameLayer")] public string gameLayer;
        [FormerlySerializedAs("Options")] public List<MapObjectOption> options;
        [FormerlySerializedAs("Starred")] public bool starred;
        [FormerlySerializedAs("ScaleMultiplier")] public float scaleMultiplier;
        [FormerlySerializedAs("Type")] public WorldObjectType type;

        public Guid ObjectId
        {
            get => Guid.Parse(objectId);
            set => objectId = value.ToString();
        }
    }
}