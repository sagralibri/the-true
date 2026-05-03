using Godot;

public partial class XpDropper : Node
{
    [Export] public PackedScene xpOrbScene;
    [Export] public int amount = 5;

    public void DropXp(Vector3 position)
    {
        if (xpOrbScene == null) return;

        for (int i = 0; i < amount; i++)
        {
            var orb = xpOrbScene.Instantiate<Node3D>();
            GetTree().CurrentScene.AddChild(orb);
            orb.GlobalPosition = position;
            orb.GlobalPosition += new Vector3((float)GD.RandRange(-1.0, 1), 0, (float)GD.RandRange(-1.0, 1));
        }
    }
}