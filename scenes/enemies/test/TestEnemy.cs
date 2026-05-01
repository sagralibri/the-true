using Godot;
using System;

public partial class TestEnemy : Minion
{
    [Export] public float aggroRange = 15f;
    private bool _isAttacking = false;
    
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (GameManager.Instance.player == null) return;
        var distanceToPlayer = GlobalPosition.DistanceTo(GameManager.Instance.player.GlobalPosition);
        if (distanceToPlayer <= aggroRange)
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
    }
}
