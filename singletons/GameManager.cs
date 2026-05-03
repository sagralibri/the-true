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
}

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public Minion player;
    public int experiencePoints = 0;
    public int level = 1;
    public int xpToNext = 100;
    public override void _Ready()
    {
        Instance = this;
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
}