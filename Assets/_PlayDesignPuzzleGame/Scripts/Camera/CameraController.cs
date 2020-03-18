using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public CharacterController target;
	private PlayerController m_player;
	//public PlayerController shit;
	public float m_xOffset;
	public float verticalOffset;
	public float lookAheadDstX;
	public float lookSmoothTimeX;
	public float verticalSmoothTime;
	public Vector2 focusAreaSize;

	FocusArea focusArea;

	float currentLookAheadX;
	float targetLookAheadX;
	float lookAheadDirX;
	float smoothLookVelocityX;
	float smoothVelocityY;

	bool lookAheadStopped;

	void Start()
	{
		focusArea = new FocusArea(target.bounds, focusAreaSize);

		m_player = target.GetComponent<PlayerController>();
	}

	void LateUpdate()
	{
		focusArea.Update(target.bounds);

		Vector3 focusPosition = focusArea.centre + Vector3.up * verticalOffset;

		if (focusArea.velocity.z != 0)
		{
			lookAheadDirX = Mathf.Sign(focusArea.velocity.z);

			if (Mathf.Sign(m_player.m_movementInput.x) == Mathf.Sign(focusArea.velocity.z) && m_player.m_movementInput.x != 0)
			{
				lookAheadStopped = false;
				targetLookAheadX = lookAheadDirX * lookAheadDstX;
			}
			else
			{
				if (!lookAheadStopped)
				{
					lookAheadStopped = true;
					targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
				}
			}
		}

		currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);
		focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);

		focusPosition += Vector3.forward * currentLookAheadX;

		transform.position = focusPosition + Vector3.right * m_xOffset;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 0, 0, .5f);
		Gizmos.DrawCube(focusArea.centre, new Vector3(0, focusAreaSize.y, focusAreaSize.x));
	}

	[System.Serializable]
	struct FocusArea
	{
		public Vector3 centre;
		public Vector3 velocity;
		float left, right;
		float top, bottom;


		public FocusArea(Bounds targetBounds, Vector2 size)
		{
			left = targetBounds.center.x - size.x / 2;
			right = targetBounds.center.x + size.x / 2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;

			velocity = Vector3.zero;

			Vector2 flatPoint = new Vector2((left + right) / 2, (top + bottom) / 2);
			centre = new Vector3(0, flatPoint.y, flatPoint.x);
		}

		public void Update(Bounds targetBounds)
		{
			float shiftX = 0;

			if (targetBounds.min.z < left)
			{
				shiftX = targetBounds.min.z - left;
			}
			else if (targetBounds.max.z > right)
			{
				shiftX = targetBounds.max.z - right;
			}
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if (targetBounds.min.y < bottom)
			{
				shiftY = targetBounds.min.y - bottom;
			}
			else if (targetBounds.max.y > top)
			{
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;

			Vector2 flatCentre = new Vector2((left + right) / 2, (top + bottom) / 2);
			centre = new Vector3(0, flatCentre.y, flatCentre.x);

			Vector2 flatVelocity = new Vector2(shiftX, shiftY);
			velocity = new Vector3(0, flatVelocity.y, flatVelocity.x);
		}
	}
}
