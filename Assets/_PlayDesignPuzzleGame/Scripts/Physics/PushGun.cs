using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushGun : PushableObject
{
	public Transform m_visual;

	public float m_pushForce;
	public float m_pushRadius;
	public LayerMask m_pushObjectMask;

	public void OnPickupObject()
	{
		m_isBeingCarried = true;
		m_disableGravity = true;
	}

	public void OnPlaceObject()
	{
		m_isBeingCarried = false;
		m_disableGravity = false;
	}

	public override void PerformController()
	{
		base.PerformController();

		m_visual.transform.rotation = Quaternion.AngleAxis(m_faceDir == -1 ? 0 : 180, Vector3.up);
	}

	public void PushObjects()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, m_pushRadius, m_pushObjectMask);

		foreach (Collider collider in colliders)
		{
			collider.attachedRigidbody.AddExplosionForce(m_pushForce, transform.position, m_pushRadius);

			collider.gameObject.GetComponent<DestroyFragments>().StartDestroy();
		}
	}
}
