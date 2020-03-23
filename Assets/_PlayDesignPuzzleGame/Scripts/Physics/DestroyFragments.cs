using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyFragments : MonoBehaviour
{
	private bool m_isDespawning;
	public float m_despawnTime;

	public void StartDestroy()
	{
		if (!m_isDespawning)
		{
			StartCoroutine(DestroySelf());
		}
	}

	private IEnumerator DestroySelf()
	{
		m_isDespawning = true;

		yield return new WaitForSeconds(m_despawnTime);

		Destroy(gameObject);
	}
}
