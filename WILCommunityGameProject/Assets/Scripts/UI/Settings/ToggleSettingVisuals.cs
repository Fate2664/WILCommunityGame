using System;
using DG.Tweening;
using Nova;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using ColorUtility = UnityEngine.ColorUtility;

[Serializable]
public class ToggleSettingVisuals : ItemVisuals
{
    public UIBlock2D Background = null;
    public TextBlock SettingLabel = null;
    public UIBlock2D CheckBox = null;
    public UIBlock2D CheckMark = null;
    public Texture2D WhiteCheckMark = null;
    public Texture2D BlackCheckMark = null;
    public static float HoverScale = 1.05f;

    public bool isSelected
    {
        get => Background.BodyEnabled;
        set
        {
            Background.BodyEnabled = value;
            Background.Shadow.Enabled = value;
            SettingLabel.Color = value ? Color.black : Color.white;
            CheckBox.Color = value ? HexToColor("AB7A55") : HexToColor("765033");
            CheckBox.Border.Color = value ? HexToColor("26190F") : Color.white;
            CheckBox.Border.Width = value ? 6f : 4f;
            CheckMark.SetImage(value ? BlackCheckMark : WhiteCheckMark);
        }
    }

    private static Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }

    public bool isCheckedVisual
    {
        get => CheckMark.BodyEnabled;
        set => CheckMark.BodyEnabled = value;
    }

    public static void HandleHover(Gesture.OnHover evt, ToggleSettingVisuals target)
    {
        if (SettingsMenu.Instance.popup.IsOpen) return;

        target.Background.DOKill();
        target.Background.transform.DOScale(target.SettingLabel.transform.localScale * HoverScale, 0.15f).SetEase(Ease.OutBack);
        target.isSelected = true;
    }

    public static void HandleUnHover(Gesture.OnUnhover evt, ToggleSettingVisuals target)
    {
        if (SettingsMenu.Instance.popup.IsOpen)  return;
        
        target.Background.DOKill();
        target.Background.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutQuad);
        target.isSelected = false;
    }

    public static void HandlePress(Gesture.OnPress evt, ToggleSettingVisuals target)
    {
        if  (SettingsMenu.Instance.popup.IsOpen) return;
        
        //Play SFX
    }

}
