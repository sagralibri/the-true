using Godot;
using System;
using System.Threading.Tasks;

public static class InfoEvents
{
    public static Action OnExperienceGain;
}

public partial class PlayerInfo : Control
{
    [Export] public Control xp;
    [Export] public RichTextLabel levelLabel;
    [Export] public ProgressBar xpBar;
    private Tween _xpTween;

    public override void _Ready()
    {
        UpdateInfo();
        InfoEvents.OnExperienceGain += UpdateInfo;
        xp.Visible = false;
    }

    private void UpdateInfo()
    {
        GD.Print("Updating player info display");
        var gm = GameManager.Instance;
        levelLabel.Text = $"Level {gm.level}";
        xpBar.Value = gm.experiencePoints;
        xpBar.MaxValue = gm.xpToNext;
        _ = ShowXp();
    }

    public async Task ShowXp()
    {
        _xpTween?.Kill();
        xp.Visible = true;
        _xpTween = CreateTween();
        xp.Modulate = new Color(1, 1, 1, 1);
        _xpTween.TweenProperty(xp, "modulate:a", 0f, 3f).SetTrans(Tween.TransitionType.Linear);

        await ToSignal(_xpTween, "finished");
        xp.Visible = false;
    }

    public override void _ExitTree()
    {
        InfoEvents.OnExperienceGain -= UpdateInfo;
    }
}
