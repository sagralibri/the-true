using Godot;
using System;

public partial class StartMenu : Control
{
    [Export] public Button startButton;

    public override void _Ready()
    {
        startButton.Pressed += OnButtonPressed;
    }


    public void OnButtonPressed()
    {
        GameManager.Instance.ChangeScene();
        QueueFree();
    }
}
