using System.Security.Cryptography.X509Certificates;
using Godot;

public partial class Minion : CharacterBody3D
{
    [Export] public string name;
    [Export] public int health { get; private set; }= 100;
    public int maxHealth = 0;
    [Export] public NavigationAgent3D navigationAgent;
    [Export] public float speed = 5f;
    [Export] public Side side = Side.Neutral;
    [Export] public Marker3D healthBarMarker;
    [Export] string healthBarPath = "res://scenes/healthDisplay.tscn";
    private bool _dead = false;
    private Vector3 _finalTarget;
    private bool _hasMoveTarget = false;
    private HealthDisplay _healthDisplay;

    public override void _Ready()
    {
        maxHealth = health;
        var healthBarScene = GD.Load<PackedScene>(healthBarPath);
        var healthBarInstance = healthBarScene.Instantiate<HealthDisplay>();
        healthBarMarker.AddChild(healthBarInstance);
        healthBarInstance.Initialize(this);
        _healthDisplay = healthBarInstance;
        _healthDisplay.MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    public override void _Process(double delta)
    {
        _healthDisplay.Position = GetViewport().GetCamera3D().UnprojectPosition(healthBarMarker.GlobalPosition);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_dead || !_hasMoveTarget) return;

        Vector3 destination = navigationAgent.IsNavigationFinished()
            ? _finalTarget
            : navigationAgent.GetNextPathPosition();

        Vector3 direction = destination - GlobalPosition;
        direction.Y = 0;

        float distance = direction.Length();
        float moveStep = speed * (float)delta;
        float stopDistance = 0.08f;

        if (GlobalPosition.DistanceTo(_finalTarget) <= stopDistance || distance <= moveStep)
        {
            Velocity = Vector3.Zero;
            _hasMoveTarget = false;
            MoveAndSlide();
            return;
        }

        Velocity = direction.Normalized() * speed;
        MoveAndSlide();
    }

    public void ResetPathfind()
    {
        _finalTarget = GlobalPosition;
        _hasMoveTarget = false;
        navigationAgent.TargetPosition = GlobalPosition;
    }

    public void Die()
    {
        _dead = true;
        if (IsInstanceValid(this)) QueueFree();
    }

    public void ModifyHealth(int amount)
    {
        GD.Print("Damage taken: " + amount);
        if (_dead) return;
        health += amount;
        _healthDisplay.UpdateHealthDisplay();
        if (health <= 0) Die();
    }

    public void PathfindTo(Vector3 targetPosition)
    {
        if (_dead) return;

        _finalTarget = targetPosition;
        _hasMoveTarget = true;
        
        navigationAgent.TargetPosition = targetPosition;
    }

    public bool IsAbleToHit(Side side)
    {
        if (this.side == Side.Neutral || side == Side.Neutral) return true;
        return this.side != side;
    }
}

public enum Side
{
    Player,
    Enemy,
    Neutral
}