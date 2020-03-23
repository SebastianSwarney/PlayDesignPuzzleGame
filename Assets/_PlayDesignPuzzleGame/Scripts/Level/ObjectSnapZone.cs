using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ObjectSnapZoneEvent : UnityEvent { }

public class ObjectSnapZone : MonoBehaviour
{
	public PushableObject m_targetObject;

	public float m_snapTime;

	public ObjectSnapZoneEvent m_onSnapCompleteEvent;

	private bool m_hasObject;

	private Collider m_collider;

	private void Start()
	{
		m_collider = GetComponent<Collider>();
	}

	private IEnumerator SnapObject()
	{
		m_hasObject = true;

		float t = 0;

		Vector3 startPos = m_targetObject.transform.position;

		Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, m_collider.bounds.center.z);

		while (t < m_snapTime)
		{
			t += Time.fixedDeltaTime;

			float progress = t / m_snapTime;

			Vector3 targetPosition = Vector3.Lerp(startPos, targetPos, progress);

			m_targetObject.PhysicsSeekTo(targetPosition);

			yield return new WaitForFixedUpdate();
		}

		m_targetObject.m_velocity = Vector3.zero;

		m_targetObject.m_velocitySmoothing = Vector3.zero;

		m_targetObject.transform.position = targetPos;

		m_onSnapCompleteEvent.Invoke();
	}

	private void OnTriggerEnter(Collider other)
	{

		if (other.gameObject == m_targetObject.gameObject)
		{
			StartCoroutine(SnapObject());
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == m_targetObject.gameObject)
		{
			m_hasObject = false;
		}
	}
}
