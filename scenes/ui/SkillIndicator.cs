using Godot;
using System;

public partial class SkillIndicator : Control
{
    private Movable.AbilityInfo _abilityInfo;
    [Export] public RichTextLabel timeLabel;
    [Export] public Sprite2D icon;
    [Export] public ProgressBar cooldownBar;
    [Export] public Texture2D slashIcon;
    [Export] public Texture2D dashIcon;
    [Export] public RichTextLabel inputLabel;

    public void Initialize(Movable.AbilityInfo abilityInfo)
    {
        _abilityInfo = abilityInfo;
        if (_abilityInfo.abilityType == Movable.AbilityType.Slash)
        {
            // scale to back to old size
            Vector2 oldSize = icon.Texture.GetSize();
            Vector2 newSize = slashIcon.GetSize();
            Vector2 increaseFactor = oldSize / newSize;
            icon.Scale *= increaseFactor;
            icon.Texture = slashIcon;
        }
        else if (_abilityInfo.abilityType == Movable.AbilityType.Dash)
        {
            // scale to back to old size
            Vector2 oldSize = icon.Texture.GetSize();
            Vector2 newSize = dashIcon.GetSize();
            Vector2 increaseFactor = oldSize / newSize;
            icon.Scale *= increaseFactor;
            icon.Texture = dashIcon;
        }

        var events = InputMap.ActionGetEvents(_abilityInfo.actionName);
        foreach (var ev in events)
        {
            if (ev is InputEventKey keyEvent)
            {
                inputLabel.Text = keyEvent.AsText();
                foreach (char c in inputLabel.Text)
                {
                    if (char.IsLetterOrDigit(c))
                    {
                        inputLabel.Text = c.ToString();
                        break;
                    }
                }
                break;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_abilityInfo == null) return;

        if (_abilityInfo.onCooldown)
        {
            cooldownBar.Value = (float)(1 - (_abilityInfo.sinceLastUse / _abilityInfo.cooldown)) * 100;
            timeLabel.Text = $"{Math.Ceiling(_abilityInfo.cooldown - _abilityInfo.sinceLastUse)}s";
        }
        else
        {
            cooldownBar.Value = 0;
            timeLabel.Text = "";
        }
    }
}
