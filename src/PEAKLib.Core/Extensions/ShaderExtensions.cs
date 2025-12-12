using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PEAKLib.Core.Extensions;

/// <summary>
/// Shader extensions for component-oriented operations within corelib/dependencies.
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
                    var shader = Shader.Find(sh);
                    if (shader == null)
                    {
                        CorePlugin.Log.LogWarning(
                            $"{nameof(PeakShaders)}: Shader {sh} was not found."
                        );
                        continue;
                    }
                    _peakShaders[sh] = shader;
                }
            }
            return _peakShaders;
        }
    }

    /// <summary>
    /// Corelib-level shader replacement operation. WIP.
    /// </summary>
    /// <typeparam name="T">IComponentContent-implementing generic type, without which no component access is available.</typeparam>
    /// <param name="content">RegisteredContent instance using the extension.</param>
    public static void ReplaceShaders<T>(this RegisteredContent<T> content) where T : IContent<T>, IComponentContent
    {
        foreach (Renderer renderer in content.Content.Component.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in renderer.materials)
            {
                if (!PeakShaders.TryGetValue(mat.shader.name, out var peakShader))
                {
                    continue;
                }

                // Replace dummy shader
                mat.shader = peakShader;
            }
        }
    }
}

