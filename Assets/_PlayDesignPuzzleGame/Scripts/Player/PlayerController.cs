using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class PlayerControllerEvent : UnityEvent { }

public class PlayerController : ControllerObject
{
	public enum MovementControllState { MovementEnabled, MovementDisabled }
	public enum GravityState { GravityEnabled, GravityDisabled }
	public enum DamageState { Vulnerable, Invulnerable }
	public enum InputState { InputEnabled, InputDisabled }
	public enum AliveState { IsAlive, IsDead }
	public PlayerState m_states;

	#region Movement Events
	public PlayerMovementEvents m_movementEvents;
	[System.Serializable]
	public struct PlayerMovementEvents
	{
		[Header("Basic Events")]
		public PlayerControllerEvent m_onLandedEvent;
		public PlayerControllerEvent m_onJumpEvent;
	}
	#endregion

	#region Base Movement Properties
	[System.Serializable]
	public struct BaseMovementProperties
	{
		public float m_baseMovementSpeed;
		public float m_accelerationTime;
	}

	[Header("Base Movement Properties")]
	public BaseMovementProperties m_baseMovementProperties;

	private float m_currentMovementSpeed;
	[HideInInspector]
	public Vector3 m_velocity;
	private Vector3 m_velocitySmoothing;
	private CharacterController m_characterController;
	private Coroutine m_jumpBufferCoroutine;
	private Coroutine m_graceBufferCoroutine;
	#endregion

	#region Jumping Properties
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

	[Header("Jumping Properties")]
	public JumpingProperties m_jumpingProperties;

	private float m_graceTimer;
	private float m_jumpBufferTimer;

	private float m_gravity;
	private float m_maxJumpVelocity;
	private float m_minJumpVelocity;
	private bool m_isLanded;
	private bool m_offLedge;
	#endregion

	[HideInInspector]
	public Vector2 m_movementInput;

	private PlayerInput m_input;

	private Animator m_animator;

	public override void Start()
	{
		base.Start();

		m_characterController = GetComponent<CharacterController>();

		m_animator = GetComponentInChildren<Animator>();

		CalculateJump();

		m_currentMovementSpeed = m_baseMovementProperties.m_baseMovementSpeed;
		m_jumpBufferTimer = m_jumpingProperties.m_jumpBufferTime;
	}
	private void OnValidate()
	{
		CalculateJump();
	}

	private void Update()
	{
		PerformController();
	}

	public override void PerformController()
	{
		CalculateVelocity();

		SlopePhysics();

		m_characterController.Move(m_velocity * Time.deltaTime);

		//CalculateGroundPhysics();
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

	#region Player State Code
	[System.Serializable]
	public struct PlayerState
	{
		public MovementControllState m_movementControllState;
		public GravityState m_gravityControllState;
		public DamageState m_damageState;
		public InputState m_inputState;
		public AliveState m_aliveState;
	}

	/*
	public bool IsGrounded()
	{
		if (m_characterController.collisionFlags == CollisionFlags.Below)
		{
			return true;
		}

		return false;
	}

	private void SlopePhysics()
	{
		RaycastHit hit;
		Vector3 bottom = m_characterController.transform.position - new Vector3(0, m_characterController.height / 2, 0);

		if (Physics.Raycast(bottom, Vector3.down, out hit, 0.5f))
		{
			if (hit.normal != Vector3.up)
			{
				if (Vector3.Angle(Vector3.up, hit.normal) > m_maxSlopeAngle)
				{
					//m_onMaxDownardSlope = true;

					m_velocity.x += (1f - hit.normal.y) * hit.normal.x * (m_slopeFriction);
					m_velocity.z += (1f - hit.normal.y) * hit.normal.z * (m_slopeFriction);
				}

				m_characterController.Move(new Vector3(0, -(hit.distance), 0));
			}
		}
	}
	*/

	private void OnLanded()
	{
		m_isLanded = true;

		if (CheckBuffer(ref m_jumpBufferTimer, ref m_jumpingProperties.m_jumpBufferTime, m_jumpBufferCoroutine))
		{
			JumpMaxVelocity();
		}

		m_movementEvents.m_onLandedEvent.Invoke();
	}

	private void OnOffLedge()
	{
		m_offLedge = true;

		m_graceBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_graceTimer = (x), m_jumpingProperties.m_graceTime));

	}
	#endregion

	#region Physics Calculation Code
	/*
	private void CalculateGroundPhysics()
	{
		if (IsGrounded() && !OnSlope())
		{
			m_velocity.y = 0;
		}

		if (OnSlope())
		{
			RaycastHit hit;

			Vector3 bottom = m_characterController.transform.position - new Vector3(0, m_characterController.height / 2, 0);

			if (Physics.Raycast(bottom, Vector3.down, out hit))
			{
				m_characterController.Move(new Vector3(0, -(hit.distance), 0));
			}
		}

		if (!IsGrounded() && !m_offLedge)
		{
			OnOffLedge();
		}
		if (IsGrounded())
		{
			m_offLedge = false;
		}

		if (IsGrounded() && !m_isLanded)
		{
			OnLanded();
		}
		if (!IsGrounded())
		{
			m_isLanded = false;
		}
	}
	*/

	private void CalculateVelocity()
	{
		if (m_states.m_gravityControllState == GravityState.GravityEnabled)
		{
			m_velocity.y += m_gravity * Time.deltaTime;
		}

		if (m_states.m_movementControllState == MovementControllState.MovementEnabled)
		{
			Vector3 forwardMovement = transform.right * 0;
			Vector3 rightMovement = transform.forward * m_movementInput.x;

			Vector3 targetHorizontalMovement = Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * m_currentMovementSpeed;
			Vector3 horizontalMovement = Vector3.SmoothDamp(m_velocity, targetHorizontalMovement, ref m_velocitySmoothing, m_baseMovementProperties.m_accelerationTime);

			m_velocity = new Vector3(horizontalMovement.x, m_velocity.y, horizontalMovement.z);
		}
		else
		{
			Vector3 forwardMovement = transform.right;
			Vector3 rightMovement = transform.forward;

			Vector3 targetHorizontalMovement = Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * m_currentMovementSpeed;
			Vector3 horizontalMovement = Vector3.SmoothDamp(m_velocity, targetHorizontalMovement, ref m_velocitySmoothing, m_baseMovementProperties.m_accelerationTime);

			m_velocity = new Vector3(horizontalMovement.x, m_velocity.y, horizontalMovement.z);
		}

	}

	public void PhysicsSeekTo(Vector3 p_targetPosition)
	{
		Vector3 deltaPosition = p_targetPosition - transform.position;
		m_velocity = deltaPosition / Time.deltaTime;
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
		m_movementEvents.m_onJumpEvent.Invoke();
		JumpMaxVelocity();
	}

	private void JumpMaxVelocity()
	{
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
}