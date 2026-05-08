using Godot;
using System;
using System.Threading.Tasks;

public partial class TestEnemy : Minion
{
    [Export] public float aggroRange = 15f;
    [Export] public PackedScene attackHitbox;
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
            target.Y = 0;

            PathfindTo(target);
        }
        else
        {
            ResetPathfind();
        }

        if (_isAttacking && distanceToPlayer <= 2f && _canMove)
        {
            _isAttacking = false;
            _ = Attack();
        }
    }

    public async Task Attack()
    {
        if (GameManager.Instance.player == null) return;
        _canMove = false;
        Vector3 cachePlayerPos = GameManager.Instance.player.GlobalPosition;
        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        _canMove = true;
        var attack = attackHitbox.Instantiate<SkillHitbox>();
        AddChild(attack);
        attack.LookAt(cachePlayerPos);
        attack.GlobalPosition = GlobalPosition;
        attack.origin = this;

    }
}
