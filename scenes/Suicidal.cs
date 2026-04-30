using Godot;
using System;

public partial class Suicidal : Node3D
{
    [Export] public float fadeDuration = 1f;
    [Export] public Label3D tempLabel;
    public override void _Ready()
    {
        FadeOut();
    }

    public async void FadeOut()
    {
        var tween = CreateTween();
        tween.TweenProperty(tempLabel, "modulate:a", 0f, fadeDuration).SetTrans(Tween.TransitionType.Linear);
        await ToSignal(tween, "finished");
        if (IsInstanceValid(this)) QueueFree();
    }
}
