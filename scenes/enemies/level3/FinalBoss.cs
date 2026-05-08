using Godot;
using System;
using System.Threading.Tasks;

public partial class FinalBoss : Minion
{
    [Export] public PackedScene spinHitbox;
    [Export] public PackedScene shockwaveHitbox;
    [Export] public PackedScene fissureHitbox;

    private bool _desperation = false;
    public override void _Ready()
    {
        base._Ready();

        BossEvents.attackEnd += OnAttackEnd;

        OnAttackEnd();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        BossEvents.attackEnd += OnAttackEnd;
    }

    public async Task SuperFissure()
    {
        for (int i = 0; i < 4; i++)
        {
            var hitboxScene = fissureHitbox.Instantiate() as DelayedHitbox;
            hitboxScene.origin = this;
            hitboxScene.callAttackEnd = false;

            AddChild(hitboxScene);
            hitboxScene.GlobalPosition = GameManager.Instance.player.GlobalPosition + new Vector3(
                (float)GD.RandRange(-10f, 10f),
                0,
                (float)GD.RandRange(-10f, 10f)
            );
        }

        await ToSignal(GetTree().CreateTimer(2f), "timeout");
        OnAttackEnd();
    }


    public async Task LightsOut()
    {
        BossEvents.turnLightsOff.Invoke();
        var hitboxScene = shockwaveHitbox.Instantiate() as DelayedHitbox;
        hitboxScene.delay = 2;
        hitboxScene.origin = this;
        hitboxScene.Scale = hitboxScene.Scale * 0.75f;
        AddChild(hitboxScene);
        hitboxScene.GlobalPosition = GameManager.Instance.player.GlobalPosition;

        await ToSignal(GetTree().CreateTimer(hitboxScene.delay / (_desperation ? 1.75 : 2)), "timeout");

        BossEvents.turnLightsOn.Invoke();
    }

    public async Task Spinner()
    {
        GD.Print("SPAWNING SPINNER");
        var hitboxScene = spinHitbox.Instantiate() as DelayedHitbox;
        GetViewport().AddChild(hitboxScene);
        hitboxScene.GlobalPosition = new Vector3(280.1f, 0, 4.871f);
        hitboxScene.origin = this;

        await ToSignal(GetTree().CreateTimer(hitboxScene.delay), "timeout");

        var tween = CreateTween();
        tween.TweenProperty(hitboxScene, "rotation", new Vector3(0, Mathf.DegToRad(360 * (_desperation ? -1 : 1)), 0), _desperation ? 4.5f : 5f)
        .SetEase(Tween.EaseType.In)
        .SetTrans(Tween.TransitionType.Sine);

        await ToSignal(tween, "finished");

        if (IsInstanceValid(hitboxScene)) hitboxScene.QueueFree();
        OnAttackEnd();
    }

    public async void OnAttackEnd()
    {
        _desperation = health < 100;
        GD.Print("Selecting attack");
        int selection = GD.RandRange(0, 2);
        GD.Print("Executing");
        switch (selection)
        {
            case 0:
                _ = Spinner();
                break;
            case 1:
                _ = LightsOut();
                break;
            case 2:
                _ = SuperFissure();
                break;
        }
    }

    public override void Die()
    {
        BossEvents.bossDead?.Invoke();
        base.Die();
    }

}
