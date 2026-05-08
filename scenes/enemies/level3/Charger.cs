using Godot;
using System;
using System.Threading.Tasks;

public partial class Charger : Minion
{
    [Export] public PackedScene chargeHitbox;
    [Export] public float aggroRange = 10f;

    private bool _isAttacking = false;
    private bool _charging = false;
    private double _attackCD = 0f;

    public override void _Process(double delta)
    {
        base._Process(delta);
        _attackCD += delta;

        if (GameManager.Instance.player == null) return;
        var distanceToPlayer = GlobalPosition.DistanceTo(GameManager.Instance.player.GlobalPosition);
        if (distanceToPlayer <= aggroRange && !_charging)
        {
            _isAttacking = true;
        }
        else
        {
            _isAttacking = false;
        }

        if (distanceToPlayer <= aggroRange)
        {
            Rid map = GetWorld3D().NavigationMap;
            Vector3 target = NavigationServer3D.MapGetClosestPoint(map, GameManager.Instance.player.GlobalPosition);

            PathfindTo(target);
        }
        else
        {
            ResetPathfind();
        }

        if (_isAttacking && distanceToPlayer <= 8f && _canMove && _attackCD > 1)
        {
            _isAttacking = false;
            _ = ChargeAttack();
        }
    }


    public async Task ChargeAttack()
    {
        _charging = true;
        _canMove = false;
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
        
        var tween = CreateTween();
        tween.TweenProperty(this, "global_position", closestPoint, dashDistance / dashSpeed).SetTrans(Tween.TransitionType.Cubic);

        await ToSignal(tween, "finished");
        if (IsInstanceValid(hitboxInstance)) hitboxInstance.QueueFree();
        _charging = false;
        _canMove = true;
        _attackCD = 0;
    }
}
