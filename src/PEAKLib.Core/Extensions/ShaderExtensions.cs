using System;
using System.Collections.Generic;
using UnityEngine;

namespace PEAKLib.Core.Extensions;

/// <summary>
/// Shader extensions for component-oriented operations within PEAKLib.Core/dependencies.
/// </summary>
public static class ShaderExtensions
{
    private static Dictionary<string, Shader> _peakShaders = null!;
    internal static Dictionary<string, Shader> PeakShaders
    {
        get
        {
            if (_peakShaders == null)
            {
                var shaders = new List<string>
                {
                    "W/Peak_Standard",
                    "W/Character",
                    "W/Peak_Transparent",
                    "W/Peak_Glass",
                    "W/Peak_Clip",
                    "W/Peak_glass_liquid",
                    "W/Peak_GroundTransition",
                    "W/Peak_Guidebook",
                    "W/Peak_Honey",
                    "W/Peak_Ice",
                    "W/Peak_Rock",
                    "W/Peak_Rope",
                    "W/Peak_Splash",
                    "W/Peak_Waterfall",
                    "W/Vine",
                };
                _peakShaders = new();
                foreach (var sh in shaders)
                {
                    if (!TryFindShader(sh, out Shader shader))
                    {
                        continue;
                    }
                    _peakShaders[sh] = shader;
                }
            }
            return _peakShaders;
        }
    }

    internal static bool TryFindShader(string name, out Shader shader)
    {
        shader = Shader.Find(name);
        if (shader == null)
        {
            CorePlugin.Log.LogWarning($"{nameof(PeakShaders)}: Shader {name} was not found.");
            return false;
        }
        _peakShaders[name] = shader;
        return true;
    }

    /// <summary>
    /// Replaces shaders within the provided gameObject(s) with the real ones from the game at runtime.
    /// </summary>
    /// <typeparam name="T"><see cref="IGameObjectContent"/>-implementing generic type,
    /// without which no component access is available.</typeparam>
    /// <param name="content">RegisteredContent instance to replace its GameObjects' shaders.</param>
    public static void ReplaceShaders<T>(this RegisteredContent<T> content)
        where T : IContent<T>, IGameObjectContent
    {
        ThrowHelper.ThrowIfArgumentNull(content);

        foreach (GameObject gameObject in content.Content.EnumerateGameObjects())
        {
            if (!gameObject)
            {
                continue;
            }

            ReplaceShaders(gameObject);
        }
    }

    /// <summary>
    /// Replaces shaders within the provided GameObject with the real ones from the game at runtime.
    /// This isn't actually an extension method since it's not likely this will be need to be used often,
    /// but it exists for if you aren't dealing with PEAKLib content and need to replace shaders.
    /// </summary>
    /// <param name="gameObject">The GameObject whose shaders to replace.</param>
    public static void ReplaceShaders(GameObject gameObject)
    {
        if (gameObject == null)
        {
            throw new ArgumentNullException(
                nameof(gameObject),
                "Can't replace shaders on a null GameObject."
            );
        }

        foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
        {
            if (!renderer)
            {
                continue;
            }

            foreach (Material mat in renderer.materials)
            {
                if (!mat)
                {
                    continue;
                }

                if (PeakShaders.TryGetValue(mat.shader.name, out Shader shader))
                {
                    mat.shader = shader;
                    continue;
                }

                if (!TryFindShader(mat.shader.name, out shader))
                {
                    continue;
                }

                // Ensure shader actually gets applied even if an error occurs with adding to cache dict
                mat.shader = shader;

                if (!PeakShaders.TryAdd(mat.shader.name, shader))
                {
                    CorePlugin.Log.LogWarning(
                        $"{nameof(PeakShaders)}: Could not add new shader {mat.shader.name} to dictionary."
                    );
                }
            }
        }
    }
}
