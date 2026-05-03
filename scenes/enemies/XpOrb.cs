using Godot;
using System;

public partial class XpOrb : Node3D
{
    [Export] public Area3D pickupArea;

    public override void _Ready()
    {
        pickupArea.BodyEntered += OnPickupAreaBodyEntered;
    }

    public override void _ExitTree()
    {
        pickupArea.BodyEntered -= OnPickupAreaBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        RotateY((float)delta);
        var distanceToPlayer = GlobalPosition.DistanceTo(GameManager.Instance.player.GlobalPosition);
        if (distanceToPlayer <= 5f)
        {
            Vector3 directionToPlayer = (GameManager.Instance.player.GlobalPosition - GlobalPosition).Normalized();
            float moveSpeed = 5f;
            GlobalPosition += directionToPlayer * moveSpeed * (float)delta;
        }
    }


    public void OnPickupAreaBodyEntered(Node body)
    {
        if (body is Minion minion && minion == GameManager.Instance.player)
        {
            GameManager.Instance.AddExperience(10);
            QueueFree();
        }
    }
}
