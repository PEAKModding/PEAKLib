using UnityEngine;

[DisallowMultipleComponent]
internal class LuggageMarker : MonoBehaviour
{
    [Tooltip("name used by loader to determine luggage type or spawnable key (e.g. 'luggage_explorer_1')")]
    public string LuggageKey = "";

    [Tooltip("If true this is a prefab (will be instantiated); if false loader will attempt to convert the GameObject in-place.")]
    public bool IsPrefabModel = false;
}
