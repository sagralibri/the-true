using Godot;
using System;

public partial class SkillIndicator : Control
{
    private Movable.AbilityInfo _abilityInfo;
    [Export] public RichTextLabel timeLabel;
    [Export] public Sprite2D icon;
    [Export] public ProgressBar cooldownBar;

    public void Initialize(Movable.AbilityInfo abilityInfo)
    {
        _abilityInfo = abilityInfo;
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
