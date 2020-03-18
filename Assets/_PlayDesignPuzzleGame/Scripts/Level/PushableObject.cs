using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
	public float gravity = 12;
	CharacterController controller;
	Vector3 velocity;

	void Start()
	{
		controller = GetComponent<CharacterController>();
	}

	void Update()
	{
		Push(Vector3.forward * 10 * Time.deltaTime);

		velocity += Vector3.down * gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
		if (controller.collisionFlags == CollisionFlags.Below)
		{
			velocity = Vector3.zero;
		}

	}


	public void Push(Vector3 amount)
	{
		controller.Move(amount);
	}
}
