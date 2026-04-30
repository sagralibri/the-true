using Godot;
using System;

public partial class Movable : Minion
{
	[Export] PackedScene clickMarker;
	[Export] PackedScene abilityOne;
	private float _rayLength = 1000f;

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
		{
			TrySetDestination();
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("stop"))
		{
			ResetPathfind();
			GetViewport().SetInputAsHandled();
		}

		if (@event.IsActionPressed("ability_1"))
		{
			GD.Print("Ability 1 used");
			var skillHitboxInstance = abilityOne.Instantiate<SkillHitbox>();
			GetTree().CurrentScene.AddChild(skillHitboxInstance);
			skillHitboxInstance.GlobalPosition = GetCurrentMousePos();
			skillHitboxInstance.origin = this;
			GetViewport().SetInputAsHandled();
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
