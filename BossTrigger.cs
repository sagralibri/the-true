using Godot;
using System;
using System.Threading.Tasks;

public partial class BossTrigger : Area3D
{
    [Export] public Camera3D bossCamera;
    [Export] public NavigationRegion3D disableRegion;
    [Export] public PackedScene boss;
    [Export] public Marker3D arenaCenter;

    public override async void _Ready()
    {
        base._Ready();
        bossCamera.Current = false;
        this.BodyEntered += OnBodyEntered;

        // debug
        // wait for one frame pass
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GameManager.Instance.player.GlobalPosition = arenaCenter.GlobalPosition;
    }

    private void OnBodyEntered(Node3D body)
    {
        GD.Print("Body entered");
        if (body is Movable player)
        {
            bossCamera.Current = true;
            disableRegion.Enabled = false;
            var bossInstance = boss.Instantiate() as Node3D;
            GetParent().AddChild(bossInstance);
            bossInstance.GlobalPosition = arenaCenter.GlobalPosition;
            QueueFree();
        }
    }
}
