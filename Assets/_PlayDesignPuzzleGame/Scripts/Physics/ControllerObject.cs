using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerObject : MonoBehaviour
{
	public float m_maxDownardSlopeAngle;

	public bool m_onMaxDownardSlope;

	public float m_slopeFriction;

	private CharacterController m_characterController;
	private Vector3 m_velocity;

	private Vector3 m_slopeNormal;

	private bool m_onSlope;

	public virtual void Start()
	{
		m_characterController = GetComponent<CharacterController>();
	}

	/*
	private void Update()
	{
		PerformController();
	}
	*/

	public virtual void PerformController()
	{
		SlopePhysics();

		m_characterController.Move(m_velocity * Time.deltaTime);

		CalculateGroundPhysics();
	}

	public bool IsGrounded()
	{
		if (m_characterController.collisionFlags == CollisionFlags.Below)
		{
			return true;
		}

		return false;
	}

	public void SlopePhysics()
	{
		RaycastHit hit;
		Vector3 bottom = m_characterController.transform.position - new Vector3(0, m_characterController.height / 2, 0);

		if (Physics.Raycast(bottom, Vector3.down, out hit, 0.5f))
		{
			if (hit.normal != Vector3.up)
			{
				if (Vector3.Angle(Vector3.up, hit.normal) > m_maxDownardSlopeAngle)
				{
					m_onMaxDownardSlope = true;

					m_velocity.x += (1f - hit.normal.y) * hit.normal.x * (m_slopeFriction);
					m_velocity.z += (1f - hit.normal.y) * hit.normal.z * (m_slopeFriction);
				}

				m_characterController.Move(new Vector3(0, -(hit.distance), 0));

				m_onSlope = true;
			}
		}
	}

	private void CalculateGroundPhysics()
	{
		if (IsGrounded())
		{
			m_velocity.y = 0;
		}
	}

	public bool CheckCollisionLayer(LayerMask p_layerMask, GameObject p_object)
	{
		if (p_layerMask == (p_layerMask | (1 << p_object.layer)))
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
