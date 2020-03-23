using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{
	public Bridge m_otherBridge;
	public LayerMask m_bridgeMoveableObjects;

	public float m_bridgeMoveTime;

	public int m_moveableLayer;
	public int m_transparentLayer;

	public float m_endMoveTime;

	public bool m_isMovingObject;

	public bool m_isBeingCarried;

	private IEnumerator RunBridge(PushableObject p_objectToMove)
	{
		p_objectToMove.m_isOnBridge = true;

		p_objectToMove.gameObject.layer = m_transparentLayer;

		float t1 = 0;

		while (t1 < m_bridgeMoveTime)
		{
			t1 += Time.fixedDeltaTime;

			float progress = t1 / m_bridgeMoveTime;

			Vector3 targetPosition = Vector3.Lerp(transform.position, m_otherBridge.transform.position, progress);

			p_objectToMove.PhysicsSeekTo(targetPosition);

			yield return new WaitForFixedUpdate();
		}

		float t2 = 0;

		Vector3 endTargetPos = m_otherBridge.transform.position + (Vector3.up * p_objectToMove.transform.localScale.y);

		while (t2 < m_endMoveTime)
		{
			t2 += Time.fixedDeltaTime;

			float progress = t2 / m_endMoveTime;

			Vector3 targetPosition = Vector3.Lerp(m_otherBridge.transform.position, endTargetPos, progress);

			p_objectToMove.PhysicsSeekTo(targetPosition);

			yield return new WaitForFixedUpdate();
		}

		//p_objectToMove.m_velocity = Vector3.right * 30;

		p_objectToMove.gameObject.layer = m_moveableLayer;

		//p_objectToMove.m_isOnBridge = false;
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

	private void OnTriggerEnter(Collider other)
	{
		if (CheckCollisionLayer(m_bridgeMoveableObjects, other.gameObject))
		{
			PushableObject pushTarget = other.gameObject.GetComponent<PushableObject>();

			if (!m_isBeingCarried)
			{
				if (!pushTarget.m_isOnBridge)
				{
					StartCoroutine(RunBridge(pushTarget));
				}
			}
		}
	}
}
