using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObject : ControllerObject
{
	public float m_gravity;

	public float m_groundDeccelerationTime;

	private Vector3 m_velocitySmoothing;

	public override void PerformController()
	{
		CalculateVelocity();

		base.PerformController();
	}

	private void CalculateVelocity()
	{
		m_velocity.y -= m_gravity * Time.deltaTime;

		Vector3 horizontalMovement = Vector3.SmoothDamp(m_velocity, Vector3.zero, ref m_velocitySmoothing, m_groundDeccelerationTime);

		m_velocity = new Vector3(horizontalMovement.x, m_velocity.y, horizontalMovement.z);
	}

	public void PushObject(Vector3 p_amount)
	{
		m_velocity = p_amount;
	}
}
