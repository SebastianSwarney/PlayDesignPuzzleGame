using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushGun : PushableObject
{
	public Transform m_visual;

	public float m_pushForce;
	public float m_pushRadius;
	public LayerMask m_pushObjectMask;

	public override void PerformController()
	{
		base.PerformController();

		if (Input.GetKeyDown(KeyCode.F))
		{
			PushObjects();
		}

		m_visual.transform.rotation = Quaternion.AngleAxis(m_faceDir == -1 ? 0 : 180, Vector3.up);
	}

	private void PushObjects()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, m_pushRadius, m_pushObjectMask);

		foreach (Collider collider in colliders)
		{
			collider.attachedRigidbody.AddExplosionForce(m_pushForce, transform.position, m_pushRadius);

			collider.gameObject.GetComponent<DestroyFragments>().StartDestroy();
		}
	}
}
