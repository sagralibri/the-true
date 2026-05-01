using Godot;
using System;
using System.Collections.Generic;

public partial class SkillHitbox : Node3D
{
    [Export] public Area3D area;
    [Export] public int damage = 10;
    [Export] public float lifetime = 0.5f;
    public Minion origin;
    private bool _followingOrigin = false;

    private readonly HashSet<Node> _alreadyHit = new();

    public override void _Ready()
    {
        area.BodyEntered += OnBodyEntered;
        
        GetTree().CreateTimer(lifetime).Timeout += () =>
        {
            if (IsInstanceValid(this)) QueueFree();
        };
    }

    public override void _Process(double delta)
    {
        if (_followingOrigin && IsInstanceValid(origin))
        {
            GlobalPosition = origin.GlobalPosition;
        }
    }

    public void FollowOriginPos()
    {
        _followingOrigin = true;
    }

    private void OnBodyEntered(Node body)
    {
        if (_alreadyHit.Contains(body)) return;

        if (body is Minion minion)
        {
            if (!minion.IsAbleToHit(origin.side)) return;
            minion.ModifyHealth(-damage);
            _alreadyHit.Add(body);
        }
    }

}
