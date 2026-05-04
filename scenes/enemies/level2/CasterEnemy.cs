using Godot;
using System;
using System.Threading.Tasks;

public partial class CasterEnemy : Minion
{
    [Export] public float aggroRange = 15f;
    [Export] public PackedScene attackHitbox;
    private bool _isAttacking = false;
    
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (GameManager.Instance.player == null) return;
        var distanceToPlayer = GlobalPosition.DistanceTo(GameManager.Instance.player.GlobalPosition);
        if (distanceToPlayer <= aggroRange && _canMove)
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

        if (_isAttacking && distanceToPlayer <= 8f && _canMove)
        {
            _isAttacking = false;
            _ = Attack();
        }
    }

    public async Task Attack()
    {
        if (GameManager.Instance.player == null) return;
        _canMove = false;
        var attack = attackHitbox.Instantiate<DelayedHitbox>();
        AddChild(attack);
        attack.GlobalPosition = GameManager.Instance.player.GlobalPosition;
        attack.origin = this;
        await ToSignal(GetTree().CreateTimer(1.2f), "timeout");
        attack.origin = this;
        _canMove = true;

    }
}
