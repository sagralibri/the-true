using Godot;
using System;
using System.Threading.Tasks;

public static class BossEvents
{
    public static Action attackEnd;
    public static Action turnLightsOn;
    public static Action turnLightsOff;
    public static Action bossDead;
}


public partial class TestBoss : Minion
{
    public Vector3 arenaCenter;
    [Export] public PackedScene shockwaveHitbox;
    [Export] public PackedScene chargeHitbox;
    [Export] public PackedScene fissureHitbox;

    public override void _Ready()
    {
        base._Ready();
        BossEvents.attackEnd += OnAttackEnd;

        FissureAttack();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }


    public async Task ShockwaveAttack()
    {
        var hitboxInstance = shockwaveHitbox.Instantiate() as DelayedHitbox;
        GetParent().AddChild(hitboxInstance);
        hitboxInstance.GlobalPosition = GlobalPosition;
        hitboxInstance.origin = this;

        var tween = CreateTween();
        tween.TweenProperty(this, "global_position", this.GlobalPosition + new Vector3(0,2,0), hitboxInstance.delay/2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

        await ToSignal(tween, "finished");

        tween = CreateTween();
        tween.TweenProperty(this, "global_position", this.GlobalPosition - new Vector3(0,2,0), hitboxInstance.delay/2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

        await ToSignal(tween, "finished");


    }

    public async Task ChargeAttack()
    {
        var hitboxInstance = chargeHitbox.Instantiate() as DelayedHitbox;
        AddChild(hitboxInstance);
        hitboxInstance.GlobalPosition = GlobalPosition;
        Vector3 cacheDirection = GameManager.Instance.player.GlobalPosition;
        hitboxInstance.LookAt(cacheDirection);
        hitboxInstance.Rotation = new Vector3(0, hitboxInstance.Rotation.Y, 0);
        hitboxInstance.origin = this;
        hitboxInstance.FollowOriginPos();

        await ToSignal(GetTree().CreateTimer(hitboxInstance.delay), "timeout");


        ResetPathfind();
        float dashDistance = 10f;
        float dashSpeed = 20f;

        Rid map = GetWorld3D().NavigationMap;
        Vector3 closestPoint = NavigationServer3D.MapGetClosestPoint(map, cacheDirection);
        closestPoint.Y = GlobalPosition.Y;
        
        var tween = CreateTween();
        tween.TweenProperty(this, "global_position", closestPoint, dashDistance / dashSpeed).SetTrans(Tween.TransitionType.Cubic);

        await ToSignal(tween, "finished");
        OnAttackEnd();
        if (IsInstanceValid(hitboxInstance)) hitboxInstance.QueueFree();

    }

    public void FissureAttack()
    {
        var hitboxInstance = fissureHitbox.Instantiate() as SkillHitbox;
        AddChild(hitboxInstance);
        hitboxInstance.GlobalPosition = GlobalPosition;
        hitboxInstance.LookAt(GameManager.Instance.player.GlobalPosition);
        hitboxInstance.Rotation = new Vector3(0, hitboxInstance.Rotation.Y, 0);
        hitboxInstance.origin = this;
    }

    private async void OnAttackEnd()
    {
        _ = SelectAttack();
    }

    public async Task SelectAttack()
    {
        GD.Print("Selecting attack");
        int selection = GD.RandRange(0, 2);
        GD.Print($"Index of {selection}");
        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        GD.Print("Executing");
        switch (selection)
        {
            case 0:
                FissureAttack();
                break;
            case 1:
                _ = ChargeAttack();
                break;
            case 2:
                _ = ShockwaveAttack();
                break;
        }
    }

    public override void Die()
    {
        BossEvents.bossDead.Invoke();
        base.Die();
    }
}
