using Godot;
using System;

public partial class GameRoom : Node3D
{
    [Export] Marker3D startPoint;
    [Export] string characterPath = "res://scenes/character.tscn";

    public override void _Ready()
    {
        var characterScene = GD.Load<PackedScene>(characterPath);
        var characterInstance = characterScene.Instantiate<Node3D>();
        AddChild(characterInstance);
        characterInstance.GlobalTransform = startPoint.GlobalTransform;
    }
}
