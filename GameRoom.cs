using Godot;
using System;




public partial class GameRoom : Node3D
{
    [Export] Marker3D startPoint;
    [Export] string characterPath = "res://scenes/character.tscn";
    private string uiPath = "res://scenes/ui/playerInfo.tscn"; // temp
    private string abilityPath = "res://scenes/ui/SkillIndicator.tscn"; // temp

    public override void _Ready()
    {
        var characterScene = GD.Load<PackedScene>(characterPath);
        var characterInstance = characterScene.Instantiate<Movable>();
        AddChild(characterInstance);
        characterInstance.GlobalTransform = startPoint.GlobalTransform;

        GD.Print("Loading UI");


        var uiScene = GD.Load<PackedScene>(uiPath);
        var uiInstance = uiScene.Instantiate<Control>();
        AddChild(uiInstance);
        foreach (var ability in characterInstance.abilities)
        {
            var container = uiInstance.GetNode<HBoxContainer>("HBoxContainer");
            var abilityScene = GD.Load<PackedScene>(abilityPath);
            var abilityInstance = abilityScene.Instantiate<SkillIndicator>();
            container.AddChild(abilityInstance);
            abilityInstance.Initialize(ability);
        }
    }
}
