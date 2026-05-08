using Godot;
using System;
using System.Threading.Tasks;

public partial class BossTrigger : Area3D
{
    [Export] public NavigationRegion3D disableRegion;
    [Export] public PackedScene boss;
    [Export] public Marker3D arenaCenter;
    private bool _debug = false;

    public override async void _Ready()
    {
        base._Ready();
        this.BodyEntered += OnBodyEntered;

        // debug
        // wait for one frame pass
        if (!_debug) return;
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GameManager.Instance.player.GlobalPosition = arenaCenter.GlobalPosition;
    }

    private void OnBodyEntered(Node3D body)
    {
        GD.Print("Body entered");
        if (body is Movable player)
        {
            disableRegion.QueueFree();
            var bossInstance = boss.Instantiate() as Node3D;
            GetParent().AddChild(bossInstance);
            bossInstance.GlobalPosition = arenaCenter.GlobalPosition;
            QueueFree();
        }
    }
}
