using System.Threading.Tasks;
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
    [Export] public bool showHealthBar = true;
    private bool _dead = false;
    protected Vector3 _finalTarget;
    protected bool _hasMoveTarget = false;
    protected bool _canMove = true;
    private HealthDisplay _healthDisplay;

    public override async void _Ready()
    {
        maxHealth = health;
        var healthBarScene = GD.Load<PackedScene>(healthBarPath);
        var healthBarInstance = healthBarScene.Instantiate<HealthDisplay>();
        healthBarMarker.AddChild(healthBarInstance);
        healthBarInstance.Initialize(this);
        _healthDisplay = healthBarInstance;
        _healthDisplay.MouseFilter = Control.MouseFilterEnum.Ignore;
        _healthDisplay.Visible = showHealthBar;
        PathfindTo(this.GlobalPosition);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        ResetPathfind();
    }

    // temporary

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.F)
        {
            PathfindTo(GameManager.Instance.player.GlobalPosition);
            GD.Print("Pathfinding to player at position: " + GameManager.Instance.player.GlobalPosition);
        }
    }

    public override void _Process(double delta)
    {
        _healthDisplay.Position = GetViewport().GetCamera3D().UnprojectPosition(healthBarMarker.GlobalPosition);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_dead || !_hasMoveTarget || !_canMove)
            return;

        Vector3 destination = navigationAgent.IsNavigationFinished()
            ? _finalTarget
            : navigationAgent.GetNextPathPosition();

        Vector3 flatDestination = new Vector3(destination.X, GlobalPosition.Y, destination.Z);
        Vector3 direction = flatDestination - GlobalPosition;

        float moveStep = speed * (float)delta;
        float stopDistance = 0.08f;

        Vector3 flatFinalTarget = new Vector3(_finalTarget.X, GlobalPosition.Y, _finalTarget.Z);

        if (GlobalPosition.DistanceTo(flatFinalTarget) <= stopDistance || direction.Length() <= moveStep)
        {
            Velocity = Vector3.Zero;
            _hasMoveTarget = false;
            MoveAndSlide();
            return;
        }

        Velocity = direction.Normalized() * speed;
        Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);

        MoveAndSlide();
    }

    public void ResetPathfind()
    {
        _finalTarget = GlobalPosition;
        _hasMoveTarget = false;
        navigationAgent.TargetPosition = GlobalPosition;
    }

    public virtual void Die()
    {
        _dead = true;

        if (!IsInstanceValid(this)) return;

        var xpDropper = GetNodeOrNull<XpDropper>("XpDropper");
        xpDropper?.DropXp(GlobalPosition);

        QueueFree();
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
        if (!_canMove) return;

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