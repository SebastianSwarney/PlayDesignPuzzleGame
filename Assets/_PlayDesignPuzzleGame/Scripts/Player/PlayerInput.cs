using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
	public int m_playerId;
	private PlayerController m_playerController;
	private bool m_lockLooking;

	private void Start()
	{
		m_playerController = GetComponent<PlayerController>();
	}

	private void Update()
	{
		GetInput();
	}

	public void GetInput()
	{
		Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		m_playerController.SetMovementInput(movementInput);

		/*
		if (Input.GetKeyDown(KeyCode.Space))
		{
			m_playerController.OnJumpInputDown();
		}

		if (Input.GetKeyUp(KeyCode.Space))
		{
			m_playerController.OnJumpInputUp();
		}
		*/
	}
}