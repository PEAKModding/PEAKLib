using UnityEngine;
using Zorro.UI;

namespace PEAKLib.ModConfig.Components;

internal class ModdedTABSButton : TAB_Button
{
    public string category = "";
    public GameObject? SelectedGraphic;

    private void Update()
    {
        text.color = Color.Lerp(
            text.color,
            Selected ? Color.black : Color.white,
            Time.unscaledDeltaTime * 7f
        );
        if (SelectedGraphic != null)
            SelectedGraphic.gameObject.SetActive(Selected);
    }
}
