using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public Minion player;
    public override void _Ready()
    {
        Instance = this;
    }
}