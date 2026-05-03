using System.Threading.Tasks;
using Godot;

public partial class DelayedHitbox : SkillHitbox
{
    [Export] public float delay = 1f;
    [Export] public Node3D warningNode;
    [Export] public Node3D hitNode;
    [Export] public bool callAttackEnd;

    public override void _Ready()
    {
        area.BodyEntered += OnBodyEntered;

        area.Monitoring = false;
        hitNode.Visible = false;
        warningNode.Visible = true;

        _ = StartDelay();
    }

    private async Task StartDelay()
    {
        await ToSignal(GetTree().CreateTimer(delay), "timeout");
        warningNode.Visible = false;
        hitNode.Visible = true;
        area.Monitoring = true;

        GetTree().CreateTimer(lifetime).Timeout += () =>
        {
            if (IsInstanceValid(this)) QueueFree();
            if (!callAttackEnd) return;
            BossEvents.attackEnd.Invoke();
        };
    }
}