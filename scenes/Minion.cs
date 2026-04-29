using Godot;

public partial class Minion : CharacterBody3D
{
    [Export] public int health { get; private set; }= 100;
    [Export] public NavigationAgent3D navigationAgent;
    [Export] public float speed = 5f;
    private bool _dead = false;

    public override void _PhysicsProcess(double delta)
    {
        if (_dead) return;

        if (navigationAgent.IsNavigationFinished())
        {
            Velocity = Vector3.Zero;
            MoveAndSlide();
            return;
        }
        //t
        Vector3 next = navigationAgent.GetNextPathPosition();

        Vector3 direction = next - GlobalPosition;
        direction.Y = 0;

        if (direction.Length() < 0.05f)
        {
            Velocity = Vector3.Zero;
        }
        else
        {
            Velocity = direction.Normalized() * speed;
        }

        MoveAndSlide();
    }

    public void Die()
    {
        _dead = true;
        if (IsInstanceValid(this)) QueueFree();
    }

    public void ModifyHealth(int amount)
    {
        if (_dead) return;
        health += amount;
        if (health <= 0) Die();
    }

    public void PathfindTo(Vector3 targetPosition)
    {
        if (_dead) return;
        navigationAgent.TargetPosition = targetPosition;
    }
}