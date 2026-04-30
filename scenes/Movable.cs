using Godot;
using System;
using System.Collections.Generic;

public partial class Movable : Minion
{
	[Export] PackedScene clickMarker;
	[Export] PackedScene abilityOne;
	private float _rayLength = 1000f;
	public List<AbilityInfo> abilities = new List<AbilityInfo>();

	public override void _Ready()
	{
		base._Ready();
		abilities.Add(new AbilityInfo
		{
			name = "Fireball",
			actionName = "ability_1",
			cooldown = 2.0,
			hitboxScene = abilityOne
		});
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		foreach (var ability in abilities)
		{
			ability.Update(delta);
		}
	}

	public class AbilityInfo
    {
        public string name;
		public string actionName;
        public double cooldown;
        public double sinceLastUse;
        public PackedScene hitboxScene;
        public bool onCooldown;
		public bool canBuffer = true;
		public bool buffered;

        public void Update(double delta)
        {
            if (onCooldown)
            {
                sinceLastUse += delta;
                if (sinceLastUse >= cooldown)
                {
                    onCooldown = false;
                    sinceLastUse = 0;
                }
            }
        }

        public void Used()  
        {
            onCooldown = true;
            sinceLastUse = 0;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
		{
			GD.Print("Right click detected");
			TrySetDestination();
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("stop"))
		{
			ResetPathfind();
			GetViewport().SetInputAsHandled();
		}
    }


	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
		{
			GD.Print("Right click detected");
			TrySetDestination();
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("stop"))
		{
			ResetPathfind();
			GetViewport().SetInputAsHandled();
		}
		// buffer system
		foreach (var ability in abilities)
		{
			if (@event.IsActionPressed(ability.actionName) && ability.onCooldown)
			{
				if (ability.canBuffer)
				{
					ability.buffered = true;
					GetViewport().SetInputAsHandled();
				}	
			}
			if (@event.IsActionReleased(ability.actionName) && ability.buffered)
			{
				ability.buffered = false;
			}
		}

		foreach (var ability in abilities)
		{
			if ((@event.IsActionPressed(ability.actionName) && !ability.onCooldown) || ability.buffered && !ability.onCooldown)
			{
				ability.buffered = false;
				GD.Print($"{ability.name} used");
				var skillHitboxInstance = ability.hitboxScene.Instantiate<SkillHitbox>();
				GetTree().CurrentScene.AddChild(skillHitboxInstance);
				skillHitboxInstance.GlobalPosition = GetCurrentMousePos();
				skillHitboxInstance.origin = this;
				ability.Used();
				GetViewport().SetInputAsHandled();
			}
		}
	}

	Vector3 GetCurrentMousePos()
	{
		Vector2 mousePos = GetViewport().GetMousePosition();

		Camera3D camera = GetViewport().GetCamera3D();

		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePos);
		Vector3 rayDirection = camera.ProjectRayNormal(mousePos);
		Vector3 rayEnd = rayOrigin + rayDirection * _rayLength;

		PhysicsRayQueryParameters3D query =
			PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);

		var result = GetWorld3D().DirectSpaceState.IntersectRay(query);

		if (result.Count > 0)
		{
			return result["position"].AsVector3();
		}
		else
		{
			Plane groundPlane = new Plane(Vector3.Up, 0f);
			Vector3? planeHit = groundPlane.IntersectsRay(rayOrigin, rayDirection);

			if (planeHit == null) return Vector3.Zero;

			return planeHit.Value;
		}
	}

	private void TrySetDestination()
	{
		Vector2 mousePos = GetViewport().GetMousePosition();

		Camera3D camera = GetViewport().GetCamera3D();

		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePos);
		Vector3 rayDirection = camera.ProjectRayNormal(mousePos);
		Vector3 rayEnd = rayOrigin + rayDirection * _rayLength;

		PhysicsRayQueryParameters3D query =
			PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);

		var result = GetWorld3D().DirectSpaceState.IntersectRay(query);

		Vector3 clickedPosition;

		if (result.Count > 0)
		{
			clickedPosition = result["position"].AsVector3();
		}
		else
		{
			Plane groundPlane = new Plane(Vector3.Up, 0f);
			Vector3? planeHit = groundPlane.IntersectsRay(rayOrigin, rayDirection);

			if (planeHit == null) return;

			clickedPosition = planeHit.Value;
		}

		Rid map = GetWorld3D().NavigationMap;
		Vector3 targetPosition =
			NavigationServer3D.MapGetClosestPoint(map, clickedPosition);

		var clickMarkerInstance = clickMarker.Instantiate<Suicidal>();
		GetTree().CurrentScene.AddChild(clickMarkerInstance);
		clickMarkerInstance.GlobalPosition = targetPosition;

		PathfindTo(targetPosition);
	}
}
