using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class PlayerInput : PlayerSystem
{
    bool canMove = true;
	
	void Update()
	{
		if (!canMove) { return; }
		
		Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.ID.events.OnXYInput?.Invoke(moveInput);

		if (Input.GetButtonDown("Jump")) { player.ID.events.OnJumpDown?.Invoke(); }
		
		if (Input.GetButtonUp("Jump")) { player.ID.events.OnJumpUp?.Invoke(); }
		
		if (Input.GetMouseButtonDown(0)) { player.ID.events.OnGrappleDown?.Invoke(); }

        if (Input.GetMouseButtonUp(0)) { player.ID.events.OnGrappleUp?.Invoke(); }

        if (Input.GetMouseButtonDown(1)) { player.ID.events.OnPullDown?.Invoke(); }
	}

	void StopMovement()
	{
		canMove = false;
	}

	void StartMovement()
	{
		canMove = true;
	}

	void OnEnable()
	{
		player.ID.events.OnDeath += StopMovement;
		player.ID.events.OnRespawn += StartMovement;
	}

	void OnDisable()
	{
		player.ID.events.OnDeath -= StopMovement;
		player.ID.events.OnRespawn -= StartMovement;
	}
}
