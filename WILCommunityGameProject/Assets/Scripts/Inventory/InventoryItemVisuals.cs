using Nova;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WILCommunityGame;

[System.Serializable]
public class InventoryItemVisuals : ItemVisuals
{
    public UIBlock2D ItemRoot;
    public UIBlock ContentRoot;
    public UIBlock2D Image;
    public TextBlock Count;
    public UIBlock ToolTipRoot;
    public TextBlock ToolTipText;

    [Header("Animations")] 
    public float ToolTipDelay = 0.5f;
    public Color DefaultColor;
    public Color HoverColor;
    public Color PressedColor;

    [NonSerialized] private UIManager _uiManager;
    [NonSerialized] private InventoryItem boundItem;
    private Coroutine toolTipCoroutine;
    private bool isHovered = false;

    public void Bind(InventoryItem data)
    {
        Bind(data, null);
    }

    public void Bind(InventoryItem data, UIManager panel)
    {
        _uiManager = panel;
        boundItem = data;

        if (ItemRoot != null)
        {
            ItemRoot.Color = DefaultColor;
        }

        if (data.isEmpty)
        {
            if (ContentRoot != null) ContentRoot.gameObject.SetActive(false);
            if (ToolTipRoot != null) ToolTipRoot.gameObject.SetActive(false);
        }
        else
        {
            if (ContentRoot != null) ContentRoot.gameObject.SetActive(true);
            if (ToolTipRoot != null) ToolTipRoot.gameObject.SetActive(false);
            Image.SetImage(data.item.itemDesc.Icon);
            if (Count != null) Count.Text = data.count.ToString();
            if (ToolTipText != null) ToolTipText.Text = data.item.itemDesc.ToolTip;
        }
    }

    #region ToolTips

    private void StartToolTipDelay()
    {
        if (ToolTipRoot == null) return;
        CancelToolTip();
        isHovered = true;
        toolTipCoroutine = View.StartCoroutine(ShowToolTipAfterDelay());
    }

    private IEnumerator ShowToolTipAfterDelay()
    {
        yield return new WaitForSeconds(ToolTipDelay);

        if (isHovered)
        {
            ToolTipRoot.gameObject.SetActive(true);
        }
        toolTipCoroutine = null;
    }

    private void CancelToolTip()
    {
        if (ToolTipRoot == null) return;
        isHovered = false;

        if (toolTipCoroutine != null)
        {
            View.StopCoroutine(toolTipCoroutine);
            toolTipCoroutine = null;
        }
        
        ToolTipRoot.gameObject.SetActive(false);
    }
    #endregion

    public void EquipBoundItem()
    {
        if (_uiManager == null || boundItem == null || boundItem.isEmpty) return;
        
        _uiManager.EquipItem(boundItem);
    }

    #region Gesture Methods

    internal void OnHover()
    {
        ItemRoot.Color = HoverColor;
        StartToolTipDelay();
    }

    internal void OnPress()
    {
        ItemRoot.Color = PressedColor;
        EquipBoundItem();
    }

    internal void OnUnhover()
    {
        ItemRoot.Color = DefaultColor;
        CancelToolTip();
    }

    internal void OnRelease()
    {
        ItemRoot.Color = HoverColor;
    }

    

    internal static void HandleHover(Gesture.OnHover evt, InventoryItemVisuals target, int index)
    {
        target.OnHover();
    }

    internal static void HandleHover(Gesture.OnHover evt, InventoryItemVisuals target)
    {
        target.OnHover();
    }

    internal static void HandlePress(Gesture.OnPress evt, InventoryItemVisuals target, int index)
    {
        target.OnPress();
    }

    internal static void HandlePress(Gesture.OnPress evt, InventoryItemVisuals target)
    {
        target.OnPress();
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, InventoryItemVisuals target, int index)
    {
        target.OnUnhover();
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, InventoryItemVisuals target)
    {
        target.OnUnhover();
    }

    internal static void HandleRelease(Gesture.OnRelease evt, InventoryItemVisuals target, int index)
    {
        target.OnRelease();
    }

    internal static void HandleRelease(Gesture.OnRelease evt, InventoryItemVisuals target)
    {
        target.OnRelease();
    }

    #endregion
}
