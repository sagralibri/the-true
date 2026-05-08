using System.Threading.Tasks;
using Godot;

public partial class DelayedHitbox : SkillHitbox
{
    [Export] public float delay = 1f;
    [Export] public Node3D warningNode;
    [Export] public Node3D hitNode;
    [Export] public bool callAttackEnd;
    private bool _followingPlayer = false;
    private bool _hitboxOut = false;

    public override void _Ready()
    {
        area.BodyEntered += OnBodyEntered;

        area.Monitoring = false;
        hitNode.Visible = false;
        warningNode.Visible = true;

        _ = StartDelay();
    }

    public void FollowPlayer()
    {
        _followingPlayer = true;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_followingPlayer && !_hitboxOut)
        {
            GlobalPosition = GameManager.Instance.player.GlobalPosition;
        }
    }

    private async Task StartDelay()
    {
        await ToSignal(GetTree().CreateTimer(delay), "timeout");
        _hitboxOut = true;
        warningNode.Visible = false;
        hitNode.Visible = true;
        area.Monitoring = true;

        GetTree().CreateTimer(lifetime).Timeout += () =>
        {
            if (IsInstanceValid(this)) QueueFree();
            if (!callAttackEnd) return;
            BossEvents.attackEnd?.Invoke();
        };
    }
}