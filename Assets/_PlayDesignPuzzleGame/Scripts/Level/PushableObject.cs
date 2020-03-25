using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObject : ControllerObject
{
	public float m_gravity;

	public float m_groundDeccelerationTime;

	[HideInInspector]
	public Vector3 m_velocitySmoothing;

	[HideInInspector]
	public bool m_isOnBridge;

	private bool m_hasBeenMovedThisFrame;

	public override void PerformController()
	{
		CalculateVelocity();

		base.PerformController();
	}

	private void CalculateVelocity()
	{
		m_velocity.y -= m_gravity * Time.deltaTime;

		Vector3 horizontalMovement = Vector3.SmoothDamp(m_velocity, Vector3.zero, ref m_velocitySmoothing, m_groundDeccelerationTime);

		//m_velocity = new Vector3(horizontalMovement.x, m_velocity.y, horizontalMovement.z);
	}

	private void LateUpdate()
	{
		if (!m_hasBeenMovedThisFrame && !m_isOnBridge)
		{
			m_velocity.x = 0;
			m_velocitySmoothing = Vector3.zero;
		}

		m_hasBeenMovedThisFrame = false;
	}

	public void PushObject(Vector3 p_amount)
	{
		//m_characterController.Move(p_amount);

		//m_velocity = p_amount;

		m_velocity.x = p_amount.x;

		m_isOnBridge = false;

		m_hasBeenMovedThisFrame = true;
	}
}
