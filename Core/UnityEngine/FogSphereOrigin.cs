using UnityEngine;

[DisallowMultipleComponent]
internal class FogSphereOrigin : MonoBehaviour
{
    [Tooltip("Initial fog radius / size (matches game FogSphereOrigin.size)")]
    public float Size = 800f;

    [Tooltip("When players reach this height the fog moves.")]
    public float MoveOnHeight = 0f;

    [Tooltip("When players reach this forward threshold the fog moves.")]
    public float MoveOnForward = 0f;

    [Tooltip("If true, this origin disables fog for this segment.")]
    public bool DisableFog = false;
}
