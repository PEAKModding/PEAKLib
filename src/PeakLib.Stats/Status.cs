using System.Collections.Generic;
using System.Data;
using PEAKLib.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PEAKLib.Items;

/// <summary>
/// A PEAKLib status effect (the type that decreases your stamina).
/// For non-stamina effects on the player, see Affliction.
/// </summary>
public class Status
{
    /// <summary>
    /// Name of the status effect
    /// </summary>
    public string Name = "";

    /// <summary>
    /// Color of the status effect on the stamina bar
    /// </summary>
    public Color Color = Color.white;

    /// <summary>
    /// Max amount (as a fraction of the stamina bar) the status can go up to
    /// </summary>
    public float MaxAmount = 2f;

    /// <summary>
    /// Delay after applying before status starts being reduced.
    /// </summary>
    public float ReductionCooldown;

    /// <summary>
    /// Amount to reduce status per second.
    /// </summary>
    public float ReductionPerSecond;

    /// <summary>
    /// Allow clearing the status effect.
    /// For example, Napberry and Pandora's Lunchbox can do this
    /// </summary>
    public bool AllowClear = true;

    /// <summary>
    /// Type for handling status update calls.
    /// </summary>
    /// <param name="chaf"></param>
    /// <param name="status"></param>
    public delegate void UpdateStatusHandler(CharacterAfflictions chaf, Status status);
    /// <summary>
    /// Callback to update status, for custom behavior.
    /// Overrides default behavior for ReductionCooldown and ReductionPerSecond.
    /// </summary>
    public UpdateStatusHandler? Update;

    /// <summary>
    /// Icon used in the Stamina Bar UI
    /// </summary>
    public Sprite? Icon;

    /// <summary>
    /// SFX scriptable object used internally
    /// </summary>
    public SFX_Instance? SFX;

    internal int Index;
    /// <summary>
    /// This leads to invalid enums, but that's the point.
    /// </summary>
    public CharacterAfflictions.STATUSTYPE Type => (CharacterAfflictions.STATUSTYPE)Index;
}
