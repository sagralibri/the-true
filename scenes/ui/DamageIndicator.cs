using Godot;
using System;

public partial class DamageIndicator : Control
{
    [Export] public RichTextLabel damageLabel;
    [Export] public float fadeDuration = 1f;

    public void Initialize(int damage)
    {
        damageLabel.Text = $"{damage}";
        FadeOut();
    }

    public async void FadeOut()
    {
        var tween = CreateTween();
        tween.TweenProperty(damageLabel, "modulate:a", 0f, fadeDuration).SetTrans(Tween.TransitionType.Linear);
        await ToSignal(tween, "finished");
        if (IsInstanceValid(this)) QueueFree();
    }
}
