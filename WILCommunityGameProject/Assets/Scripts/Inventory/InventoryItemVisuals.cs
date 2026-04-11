using Nova;
using System;
using UnityEngine;
using UnityEngine.UI;
using WILCommunityGame;

[System.Serializable]
public class InventoryItemVisuals : ItemVisuals
{
    public UIBlock2D itemRoot;
    public UIBlock contentRoot;
    public UIBlock2D Image;
    public TextBlock Count;

    [Header("Animations")]
    public Color DefaultColor;
    public Color HoverColor;
    public Color PressedColor;

    public void Bind(InventoryItem data)
    {
        if (data.isEmpty)
        {
            contentRoot.gameObject.SetActive(false);
        }
        else
        {
            contentRoot.gameObject.SetActive(true);
            Image.SetImage(data.item.itemDesc.Icon);
            Count.Text = data.count.ToString();
        }
    }

    internal static void HandleHover(Gesture.OnHover evt, InventoryItemVisuals target, int index)
    {
        target.itemRoot.Color = target.HoverColor;
    }

    internal static void HandlePress(Gesture.OnPress evt, InventoryItemVisuals target, int index)
    {
        target.itemRoot.Color = target.PressedColor;
        Debug.Log("Pressed Item");
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, InventoryItemVisuals target, int index)
    {
        target.itemRoot.Color = target.DefaultColor;
    }

    internal static void HandleRelease(Gesture.OnRelease evt, InventoryItemVisuals target, int index)
    {
        target.itemRoot.Color = target.HoverColor;
    }
}
