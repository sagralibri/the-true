using Godot;
using System;
using System.Collections.Generic;

public partial class Movable : Minion
{
	[Export] PackedScene clickMarker;
	[Export] PackedScene abilityOne;
	[Export] PackedScene abilityTwo;
	private float _rayLength = 1000f;
	private bool _canInput = true;
	private Vector3 _bufferDestination;
	private bool _hasBufferedDestination = false;
	public List<AbilityInfo> abilities = new List<AbilityInfo>();

	public override void _Ready()
	{
		GameManager.Instance.player = this;
		base._Ready();
		abilities.Add(new AbilityInfo
		{
			name = "Fireball",
			actionName = "ability_1",
			cooldown = 2.0,
			hitboxScene = abilityOne,
			abilityType = AbilityType.Fireball
		});
		abilities.Add(new AbilityInfo
		{
			name = "Slash",
			actionName = "ability_2",
			cooldown = 4.0,
			hitboxScene = abilityTwo,
			abilityType = AbilityType.Slash
		});
		abilities.Add(new AbilityInfo
		{
			name = "Dash",
			actionName = "ability_3",
			cooldown = 1.0,
			hitboxScene = null,
			abilityType = AbilityType.Dash,
			canBuffer = false
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

	public enum AbilityType
	{
		Fireball,
		Slash,
		Dash
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
		public AbilityType abilityType;

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
		if (!_canInput)
		{
			if (@event is InputEventMouseButton mouseEvent2 && mouseEvent2.Pressed && mouseEvent2.ButtonIndex == MouseButton.Right)
			{
				_hasBufferedDestination = true;
				_bufferDestination = TryGetClickDestination();
			}
			else if (@event.IsActionPressed("stop"))
			{
				_hasBufferedDestination = false;
			}
			return;
		}
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

	private void RestrictMovement()
	{
		_canInput = false;
		_hasBufferedDestination = false;
	}

	private bool IsBehindAfterDash(Vector3 targetPos, Vector3 oldPos, Vector3 dashAngle)
	{
		return (targetPos - oldPos).Normalized().Dot(dashAngle.Normalized()) > 0;
	}


	public override void _UnhandledInput(InputEvent @event)
	{
		if (!_canInput) return;
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
				if (ability.abilityType == AbilityType.Dash)
				{
					// TEMPORARY
					Vector3 cacheDestination = _finalTarget;
					ResetPathfind();
					Vector3 dashDirection = (GetCurrentMousePos() - GlobalPosition).Normalized();
					float dashDistance = 10f;
					float dashSpeed = 20f;
					Vector3 desiredPosition = GlobalPosition + dashDirection * dashDistance;
					Rid map = GetWorld3D().NavigationMap;
					Vector3 closestPoint = NavigationServer3D.MapGetClosestPoint(map, desiredPosition);
					


					RestrictMovement();
					var tween = CreateTween();
					tween.TweenProperty(this, "global_position", closestPoint, dashDistance / dashSpeed).SetTrans(Tween.TransitionType.Linear);
					tween.TweenCallback(Callable.From(() => _canInput = true));
					tween.TweenCallback(Callable.From(() =>
					{
						if (!IsBehindAfterDash(GlobalPosition, cacheDestination, dashDirection) && !_hasBufferedDestination)
						{
							PathfindTo(cacheDestination);
						}
						else if (_hasBufferedDestination)
						{
							PathfindTo(_bufferDestination);
						}
					}));


					ability.Used();
					GetViewport().SetInputAsHandled();
					return;
				}
				ability.buffered = false;
				GD.Print($"{ability.name} used");
				var skillHitboxInstance = ability.hitboxScene.Instantiate<SkillHitbox>();
				GetTree().CurrentScene.AddChild(skillHitboxInstance);
				skillHitboxInstance.origin = this;
				ability.Used();
				GetViewport().SetInputAsHandled();


				switch (ability.abilityType)
				{
					case AbilityType.Fireball:
						skillHitboxInstance.GlobalPosition = GetCurrentMousePos();
						break;
					case AbilityType.Slash:
						// get angle to mouse position and set rotation of hitbox accordingly
						skillHitboxInstance.GlobalPosition = this.GlobalPosition;
						skillHitboxInstance.LookAt(GetCurrentMousePos(), Vector3.Up);
						skillHitboxInstance.Rotation = new Vector3(0, skillHitboxInstance.Rotation.Y, 0);
						skillHitboxInstance.FollowOriginPos();
						break;
				}
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
		GD.Print($"GM player: {GameManager.Instance.player.GlobalPosition}");
		GD.Print($"Debug player: {GlobalPosition}");
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

		GD.Print($"Clicked: {clickedPosition}");
		GD.Print($"Closest: {targetPosition}");

		PathfindTo(targetPosition);
	}

	private Vector3 TryGetClickDestination()
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

			if (planeHit == null) return Vector3.Zero;

			clickedPosition = planeHit.Value;
		}

		Rid map = GetWorld3D().NavigationMap;
		Vector3 targetPosition =
			NavigationServer3D.MapGetClosestPoint(map, clickedPosition);

		return targetPosition;
	}
}
