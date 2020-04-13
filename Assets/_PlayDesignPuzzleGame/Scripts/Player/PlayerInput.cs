using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerInput : MonoBehaviour
{
	public int m_playerId;
	private PlayerController m_playerController;
	private bool m_lockLooking;

	private Player m_playerInputController;

	private void Start()
	{
		m_playerController = GetComponent<PlayerController>();
		m_playerInputController = ReInput.players.GetPlayer(m_playerId);

	}

	private void Update()
	{
		GetInput();
	}

	public void GetInput()
	{
		Vector2 movementInput = new Vector2(m_playerInputController.GetAxis("MoveHorizontal"), m_playerInputController.GetAxis("MoveVertical"));
		m_playerController.SetMovementInput(movementInput);

		if (m_playerInputController.GetButtonDown("Jump"))
		{
			m_playerController.OnJumpInputDown();
		}
		if (m_playerInputController.GetButtonUp("Jump"))
		{
			m_playerController.OnJumpInputUp();
		}

		if (m_playerInputController.GetButtonDown("Ladder"))
		{
			m_playerController.OnLadderInputDown();
		}

		if (m_playerInputController.GetButton("Push"))
		{
			m_playerController.PushCrates();
		}
		if (m_playerInputController.GetButtonUp("Push"))
		{
			m_playerController.PushInputUp();
		}

		if (m_playerInputController.GetButtonDown("Pickup"))
		{
			m_playerController.OnPickupInputDown();
		}

		if (m_playerInputController.GetButtonDown("Use"))
		{
			m_playerController.OnUseInputDown();
		}
	}
}