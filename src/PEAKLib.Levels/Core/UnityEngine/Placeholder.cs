using UnityEngine;

[DisallowMultipleComponent]
internal class Placeholder : MonoBehaviour
{
    [Tooltip("Key name used by SpawnableRegistry or the prefab name to look for in bundles.")]
    public string SpawnableName = "";

    [Tooltip("Optional role (e.g. 'campfire', 'spawner', 'fogOrigin', 'luggage', etc.).")]
    public string Role = "";

    [Tooltip("If true, the placeholder will replace the game provided object at load time. " +
             "If false the loader will try to map it to an in-game object and attach gameplay components.")]
    public bool Replace = false;

    [Tooltip("Optional JSON/options string used by loader (e.g. for fog size).")]
    [TextArea(2, 6)]
    public string Options = "";

    [Tooltip("Optional author notes.")]
    [TextArea(1, 4)]
    public string Notes = "";
}
