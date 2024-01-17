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
        player.events.OnXYInput?.Invoke(moveInput);

		if (Input.GetButtonDown("Jump")) { player.events.OnJumpButtonDown?.Invoke(); }
		
		if (Input.GetButtonUp("Jump")) { player.events.OnJumpButtonUp?.Invoke(); }
		
		if (Input.GetMouseButtonDown(0)) { player.events.OnGrappleButtonDown?.Invoke(); }

        if (Input.GetMouseButtonUp(0)) { player.events.OnGrappleButtonUp?.Invoke(); }

        if (Input.GetMouseButtonDown(1)) { player.events.OnPullButtonDown?.Invoke(); }
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
		player.events.OnDeath += StopMovement;
		player.events.OnRespawn += StartMovement;
	}

	void OnDisable()
	{
		player.events.OnDeath -= StopMovement;
		player.events.OnRespawn -= StartMovement;
	}
}
