using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public static class Hitstop
{
    static bool isInHitstop = false;

    public static async Task Do(Node context, float duration = 0.1f)
    {
        if (isInHitstop) return;

        isInHitstop = true;
        try
        {
            Engine.TimeScale = 0.05f;

            await context.ToSignal(context.GetTree().CreateTimer(duration, true, true, true), "timeout");
        }
        finally
        {
            Engine.TimeScale = 1f;
            isInHitstop = false;
        }
    }

    public static void ForceUndoHitstop()
    {
        Engine.TimeScale = 1f;
        isInHitstop = false;
    }
}

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public Minion player;
    public int experiencePoints = 0;
    public int level = 1;
    public int xpToNext = 100;
    private Node currentScene;
    private int sceneIndex;

    private Dictionary<int, PackedScene> scenes = new Dictionary<int, PackedScene>
    {
        {1, GD.Load<PackedScene>("res://test_area.tscn")},
        {2, GD.Load<PackedScene>("res://level2.tscn")},
        {3, GD.Load<PackedScene>("res://level3.tscn")},
        {4, GD.Load<PackedScene>("res://scenes/ui/end.tscn")}
    
    };

    public override void _Ready()
    {
        Instance = this;

        BossEvents.bossDead += ChangeScene;
    }

    private double _hitstopTime;

    public override void _Process(double delta)
    {
        if (Engine.TimeScale != 1)
        {
            _hitstopTime += delta;
        }
        else
        {
            _hitstopTime = 0;
        }

        if (_hitstopTime > 1)
        {
            Hitstop.ForceUndoHitstop();
        }
    }

    public override void _ExitTree()
    {
        BossEvents.bossDead -= ChangeScene;
    }

    public void AddExperience(int amount)
    {
        InfoEvents.OnExperienceGain?.Invoke();
        experiencePoints += amount;
        if (experiencePoints >= xpToNext)
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        level++;
        experiencePoints -= xpToNext;
        while (experiencePoints >= xpToNext)
        {
            experiencePoints -= xpToNext;
            level++;
        }
    }

    public void ChangeScene()
    {
        if (currentScene != null)
        {
            currentScene.QueueFree();
        }
        sceneIndex += 1;

        currentScene = scenes[sceneIndex].Instantiate();
        GetViewport().AddChild(currentScene);
    }

    public async void ResetScene()
    {
        currentScene.QueueFree();
        currentScene = scenes[sceneIndex].Instantiate();
        GetViewport().AddChild(currentScene);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Hitstop.ForceUndoHitstop();
    }
}