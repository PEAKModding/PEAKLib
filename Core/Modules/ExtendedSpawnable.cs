using System;
using UnityEngine;

namespace PEAKLib.Levels.Core
{
    [CreateAssetMenu(fileName = "ExtendedSpawnable", menuName = "PEAK/ExtendedContent/ExtendedSpawnable", order = 11)]
    internal class ExtendedSpawnable : ExtendedContent<GameObject>
    {
        [Tooltip("Registry name other bundles will use to reference this spawnable")]
        public string SpawnableName = string.Empty;
        [Tooltip("Prefab name inside the same bundle (asset name)")]
        public string PrefabName = string.Empty;
        [Tooltip("Optional bundle group name to prefer resolving from")]
        public string BundlePath = string.Empty;

        internal override void Register(ExtendedMod mod)
        {
            mod.RegisterExtendedContentInternal(this);
        }

        internal override void Initialize()
        {
        }
    }
}
