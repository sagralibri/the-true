using Godot;
using System;
using System.Threading.Tasks;

public partial class Boss2 : Minion
{
    [Export] public PackedScene chargeHitbox;
    [Export] public PackedScene wallHitbox;
    [Export] public PackedScene strikeHitbox;
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
        BossEvents.attackEnd -= OnAttackEnd;
    }

    public async void Wall()
    {
        var hitboxInstance = wallHitbox.Instantiate() as DelayedHitbox;
        AddChild(hitboxInstance);
        hitboxInstance.FollowPlayer();
        hitboxInstance.origin = this;
    }



    public async Task ChargeAttack(bool callAttackEnd = true)
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "global_position", GameManager.Instance.player.GlobalPosition + new Vector3(5, 0, 0), 0.4f);

        await ToSignal(tween, "finished");

        float angleDeg = (float)GD.RandRange(180f, 520f);
        float angleRad = Mathf.DegToRad(angleDeg);

        Vector3 playerPos = GameManager.Instance.player.GlobalPosition;
        Vector3 offset = GlobalPosition - playerPos;
        float radius = offset.Length();

        float duration = _desperation ? 0.3f : 1f;

        tween = CreateTween();
        tween.TweenMethod(
            Callable.From<float>(t =>
            {
                float angle = t;
                Vector3 rotated = offset.Rotated(Vector3.Up, angle);
                GlobalPosition = playerPos + rotated;
                LookAt(playerPos);
            }),
            0f,
            angleRad,
            duration
        );

        await ToSignal(tween, "finished");

        var hitboxInstance = chargeHitbox.Instantiate() as DelayedHitbox;
        hitboxInstance.delay = 0.4f;
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

        Vector3 vectorToPlayer = (cacheDirection - GlobalPosition).Normalized();

        Rid map = GetWorld3D().NavigationMap;
        Vector3 closestPoint = NavigationServer3D.MapGetClosestPoint(map, GlobalPosition + vectorToPlayer * 10);
        
        tween = CreateTween();
        tween.TweenProperty(this, "global_position", closestPoint, dashDistance / dashSpeed).SetTrans(Tween.TransitionType.Cubic);

        await ToSignal(tween, "finished");
        if (callAttackEnd) OnAttackEnd();
        if (IsInstanceValid(hitboxInstance)) hitboxInstance.QueueFree();
    }

    public async Task SkyStrike()
    {
        int strikeCount = _desperation ? 20 : 8;
        for (int i = 0; i < strikeCount; i++)
        {
            var hitboxInstance = strikeHitbox.Instantiate() as DelayedHitbox;
            hitboxInstance.delay -= 0.3f - i * 0.02f;
            AddChild(hitboxInstance);

            hitboxInstance.origin = this;
            
            
            Vector3 offset = new Vector3(
                (float)GD.RandRange(-5, 5f),
                0,
                (float)GD.RandRange(-5, 5f)
            );

            Rid map = GetWorld3D().NavigationMap;
            Vector3 closestPoint = NavigationServer3D.MapGetClosestPoint(map, GameManager.Instance.player.GlobalPosition + offset);

            hitboxInstance.GlobalPosition = closestPoint;
        }

        await ToSignal(GetTree().CreateTimer(2f), "timeout");
        OnAttackEnd();
    }



    public async void OnAttackEnd()
    {
        _desperation = health < 100;
        GD.Print("Selecting attack");
        int selection = GD.RandRange(0, 2);
        GD.Print($"Index of {selection}");
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        GD.Print("Executing");
        switch (selection)
        {
            case 0:
                _ = SkyStrike();
                break;
            case 1:
                _ = ChargeAttack();
                break;
            case 2:
                if (!_desperation)
                {
                    Wall();
                    break;
                }
                _ = SkyStrike();
                break;
        }
    }

    public override void Die()
    {
        BossEvents.bossDead.Invoke();
        base.Die();
    }
}
