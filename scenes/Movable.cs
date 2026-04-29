using Godot;
using System;

public partial class Movable : Minion
{
	private float _rayLength = 1000f;

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
		{
			TrySetDestination();
		}
	}

	private void TrySetDestination()
	{
		Vector2 mousePos = GetViewport().GetMousePosition();

		// Convert mouse position to world coordinates
		Vector3 rayOrigin = GetViewport().GetCamera3D().ProjectRayOrigin(mousePos);
		Vector3 rayDirection = GetViewport().GetCamera3D().ProjectRayNormal(mousePos);
		Vector3 rayEnd = rayOrigin + rayDirection * _rayLength;

		// Raycast into world
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);

		var space = GetWorld3D().DirectSpaceState;
		var result = space.IntersectRay(query);

		if (result.Count == 0)
		{
			GD.Print("No collision detected");
			return;
		}

		Vector3 targetPosition = (Vector3)result["position"];
		PathfindTo(targetPosition);
	}
}
