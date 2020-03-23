using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ControllerObjectEvent : UnityEvent { }

public class ControllerObject : MonoBehaviour
{
	#region Events
	[Header("Base Controller Events")]

	public ControllerObjectEvents m_controllerObjectEvents;
	[System.Serializable]
	public struct ControllerObjectEvents
	{
		[Header("Basic Events")]
		public ControllerObjectEvent m_onLandedEvent;
	}
	#endregion

	#region Base Controller Properties
	[Header("Base Controller Properties")]

	public ControllerProperties m_controllerProperties;
	[System.Serializable]
	public struct ControllerProperties
	{
		public float m_maxDownardSlopeAngle;
		public float m_slopeFriction;
		public float m_slopeForce;
		public float m_slopeRayLengthGround;
		public float m_slopeRayLengthAir;
	}

	[HideInInspector]
	public Vector3 m_velocity;

	private Vector3 m_slopeNormal;
	private bool m_isLanded;
	private bool m_offLedge;

	private bool m_onMaxDownardSlope;
	private bool m_onSlope;
	#endregion

	[HideInInspector]
	public CharacterController m_characterController;

	[HideInInspector]
	public bool m_hasJumped;

	[HideInInspector]
	public int m_faceDir = -1;

	public virtual void Start()
	{
		m_characterController = GetComponent<CharacterController>();
	}

	private void Update()
	{
		PerformController();
	}

	public virtual void PerformController()
	{
		m_characterController.Move(m_velocity * Time.deltaTime);

		if ((int)Mathf.Sign(m_velocity.x) != m_faceDir)
		{
			m_faceDir = (int)Mathf.Sign(m_velocity.x);
		}

		/*
		if (m_velocity.x != 0)
		{
			m_faceDir = (int)Mathf.Sign(m_velocity.x);
		}
		*/

		ZeroVelocityOnGround();

		CalculateLanded();
		CalculateLedge();

		SlopePhysics();
	}

	public bool IsGrounded()
	{
		if (m_characterController.collisionFlags == CollisionFlags.Below)
		{
			return true;
		}

		return false;
	}

	public bool IsTouchingCeiling()
	{
		if (m_characterController.collisionFlags == CollisionFlags.Above)
		{
			return true;
		}

		return false;
	}
	
	public bool OnSlope()
	{
		if (m_velocity.y > 0)
		{
			return false;
		}

		if (m_hasJumped)
		{
			return false;
		}

		float currentRayLength = 0;

		if (m_isLanded)
		{
			currentRayLength = m_controllerProperties.m_slopeRayLengthGround;
		}
		else
		{
			currentRayLength = m_controllerProperties.m_slopeRayLengthAir;
		}

		RaycastHit firstHit;
		Vector3 bottom = m_characterController.transform.position - new Vector3(0, m_characterController.height / 2, 0);

		if (Physics.Raycast(bottom, Vector3.down, out firstHit, currentRayLength))
		{
			if (firstHit.normal != Vector3.up)
			{
				if (Vector3.Angle(Vector3.up, firstHit.normal) > m_controllerProperties.m_maxDownardSlopeAngle)
				{
					m_onMaxDownardSlope = true;
					m_velocity.x += (1f - firstHit.normal.y) * firstHit.normal.x * (m_controllerProperties.m_slopeFriction);
					m_velocity.z += (1f - firstHit.normal.y) * firstHit.normal.z * (m_controllerProperties.m_slopeFriction);
				}
				return true;
			}
		}
		return false;
	}
	
	public void SlopePhysics()
	{
		if (OnSlope())
		{
			m_characterController.Move(Vector3.down * (m_characterController.height / 2) * m_controllerProperties.m_slopeForce * Time.deltaTime);
		}
	}

	public void ZeroVelocityOnGround()
	{
		if (IsGrounded())
		{
			m_velocity.y = 0;
		}
	}

	public void CalculateLanded()
	{
		if (IsGrounded() && !m_isLanded)
		{
			OnLanded();
		}
		if (!IsGrounded())
		{
			m_isLanded = false;
		}
	}

	public virtual void OnLanded()
	{
		m_hasJumped = false;
		m_isLanded = true;
		m_controllerObjectEvents.m_onLandedEvent.Invoke();
	}

	public void CalculateLedge()
	{
		if (!IsGrounded() && !m_offLedge && m_velocity.y < 0)
		{
			OnOffLedge();
		}
		if (IsGrounded())
		{
			m_offLedge = false;
		}
	}

	public virtual void OnOffLedge()
	{
		m_offLedge = true;
	}

	public void PhysicsSeekTo(Vector3 p_targetPosition)
	{
		Vector3 deltaPosition = p_targetPosition - transform.position;
		m_velocity = deltaPosition / Time.deltaTime;
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
