using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class PlayerControllerEvent : UnityEvent { }

public class PlayerController : ControllerObject
{
	#region Player Controller States
	public enum MovementControllState { MovementEnabled, MovementDisabled }
	public enum GravityState { GravityEnabled, GravityDisabled }

	[Header("Player Controller States")]

	public PlayerState m_states;
	[System.Serializable]
	public struct PlayerState
	{
		public MovementControllState m_movementControllState;
		public GravityState m_gravityControllState;
	}
	#endregion

	#region Player Controller Events
	public PlayerControllerEvents m_playerControllerEvents;
	[System.Serializable]
	public struct PlayerControllerEvents
	{
		[Header("Basic Events")]
		public PlayerControllerEvent m_onJumpEvent;
	}
	#endregion

	#region Base Movement Properties

	[Header("Base Controller Properties")]
	public BaseMovementProperties m_baseMovementProperties;

	[System.Serializable]
	public struct BaseMovementProperties
	{
		public float m_baseMovementSpeed;
		public float m_accelerationTime;
	}

	private Vector3 m_velocitySmoothing;

	private Coroutine m_jumpBufferCoroutine;
	private Coroutine m_graceBufferCoroutine;
	#endregion

	#region Jumping Properties

	[Header("Jumping Properties")]
	public JumpingProperties m_jumpingProperties;

	[System.Serializable]
	public struct JumpingProperties
	{
		[Header("Jump Properties")]
		public float m_maxJumpHeight;
		public float m_minJumpHeight;
		public float m_timeToJumpApex;

		[Header("Jump Buffer Properties")]
		public float m_graceTime;
		public float m_jumpBufferTime;
	}

	private float m_graceTimer;
	private float m_jumpBufferTimer;

	private float m_gravity;
	private float m_maxJumpVelocity;
	private float m_minJumpVelocity;
	#endregion

	[HideInInspector]
	public Vector2 m_movementInput;

	public LayerMask m_pushableObjectMask;

	public LayerMask m_bridgeMask;

	private bool m_isCarryingObject;

	private Bridge m_carriedBridge;

	public LayerMask m_groundMask;

	public override void Start()
	{
		base.Start();

		CalculateJump();

		m_jumpBufferTimer = m_jumpingProperties.m_jumpBufferTime;
	}

	private void OnValidate()
	{
		CalculateJump();
	}

	
	public override void PerformController()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			OnPickupInputDown();
		}

		CalculateVelocity();

		base.PerformController();
	}

	public void OnPickupInputDown()
	{
		if (!m_isCarryingObject)
		{
			PickupObject();
		}
		else
		{
			PutDownObject();
		}
	}

	public void PickupObject()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, 3f, m_bridgeMask);

		if (colliders.Length > 0)
		{
			Bridge bridgeToPickup = colliders[0].GetComponent<Bridge>();

			if (!bridgeToPickup.m_isBeingCarried && !bridgeToPickup.m_isMovingObject)
			{
				bridgeToPickup.transform.position = transform.position + (Vector3.up * ((transform.localScale.y / 2) + (bridgeToPickup.transform.localScale.y / 2) + 0.5f));
				bridgeToPickup.transform.parent = transform;

				m_carriedBridge = bridgeToPickup;

				m_isCarryingObject = true;
			}

		}

	}

	public void PutDownObject()
	{
		m_carriedBridge.transform.parent = null;

		RaycastHit hit;

		if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, m_groundMask))
		{
			m_carriedBridge.transform.position = hit.point + (Vector3.up * (m_carriedBridge.transform.localScale.y / 2));
		}

		m_isCarryingObject = false;
	}


	#region Input Code
	public void SetMovementInput(Vector2 p_input)
	{
		m_movementInput = p_input;
	}
	#endregion

	#region Input Buffering Code

	private bool CheckBuffer(ref float p_bufferTimer, ref float p_bufferTime, Coroutine p_bufferTimerRoutine)
	{
		if (p_bufferTimer < p_bufferTime)
		{
			if (p_bufferTimerRoutine != null)
			{
				StopCoroutine(p_bufferTimerRoutine);
			}

			p_bufferTimer = p_bufferTime;

			return true;
		}
		else if (p_bufferTimer >= p_bufferTime)
		{
			return false;
		}

		return false;
	}

	private bool CheckOverBuffer(ref float p_bufferTimer, ref float p_bufferTime, Coroutine p_bufferTimerRoutine)
	{
		if (p_bufferTimer >= p_bufferTime)
		{
			p_bufferTimer = p_bufferTime;

			return true;
		}

		return false;
	}

	//Might want to change this so it does not feed the garbage collector monster
	private IEnumerator RunBufferTimer(System.Action<float> m_bufferTimerRef, float p_bufferTime)
	{
		float t = 0;

		while (t < p_bufferTime)
		{
			t += Time.deltaTime;
			m_bufferTimerRef(t);
			yield return null;
		}

		m_bufferTimerRef(p_bufferTime);
	}
	#endregion

	#region Physics Code
	public override void OnLanded()
	{
		base.OnLanded();

		if (CheckBuffer(ref m_jumpBufferTimer, ref m_jumpingProperties.m_jumpBufferTime, m_jumpBufferCoroutine))
		{
			JumpMaxVelocity();
		}
	}

	public override void OnOffLedge()
	{
		base.OnOffLedge();

		m_graceBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_graceTimer = (x), m_jumpingProperties.m_graceTime));

	}

	private void CalculateVelocity()
	{
		if (m_states.m_gravityControllState == GravityState.GravityEnabled)
		{
			m_velocity.y += m_gravity * Time.deltaTime;
		}

		if (m_states.m_movementControllState == MovementControllState.MovementEnabled)
		{
			Vector3 forwardMovement = transform.right * 0;
			Vector3 rightMovement = transform.right * m_movementInput.x;

			Vector3 targetHorizontalMovement = Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * m_baseMovementProperties.m_baseMovementSpeed;
			Vector3 horizontalMovement = Vector3.SmoothDamp(m_velocity, targetHorizontalMovement, ref m_velocitySmoothing, m_baseMovementProperties.m_accelerationTime);

			m_velocity = new Vector3(horizontalMovement.x, m_velocity.y, horizontalMovement.z);
		}
	}
	#endregion

	#region Jump Code
	public void OnJumpInputDown()
	{
		m_jumpBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_jumpBufferTimer = (x), m_jumpingProperties.m_jumpBufferTime));

		if (CheckBuffer(ref m_graceTimer, ref m_jumpingProperties.m_graceTime, m_graceBufferCoroutine) && !IsGrounded() && m_velocity.y <= 0f)
		{
			GroundJump();
			return;
		}

		if (IsGrounded())
		{
			GroundJump();
			return;
		}

	}

	public void OnJumpInputUp()
	{
		if (m_velocity.y > m_minJumpVelocity)
		{
			JumpMinVelocity();
		}
	}

	private void CalculateJump()
	{
		m_gravity = -(2 * m_jumpingProperties.m_maxJumpHeight) / Mathf.Pow(m_jumpingProperties.m_timeToJumpApex, 2);
		m_maxJumpVelocity = Mathf.Abs(m_gravity) * m_jumpingProperties.m_timeToJumpApex;
		m_minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(m_gravity) * m_jumpingProperties.m_minJumpHeight);
	}

	private void GroundJump()
	{
		m_playerControllerEvents.m_onJumpEvent.Invoke();
		JumpMaxVelocity();
	}

	private void JumpMaxVelocity()
	{
		m_hasJumped = true;

		m_velocity.y = m_maxJumpVelocity;
	}

	private void JumpMinVelocity()
	{
		m_velocity.y = m_minJumpVelocity;
	}

	private void JumpMaxMultiplied(float p_force)
	{
		m_velocity.y = m_maxJumpVelocity * p_force;
	}

	#endregion

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (CheckCollisionLayer(m_pushableObjectMask, hit.gameObject))
		{
			PushableObject pushable = hit.gameObject.GetComponent<PushableObject>();

			pushable.PushObject(m_velocity);

			if (IsGrounded())
			{
				if (Physics.Raycast(transform.position, transform.right * Mathf.Sign(m_velocity.x), Mathf.Infinity, m_pushableObjectMask))
				{
					
				}
			}
		}
	}
}