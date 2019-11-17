using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WobbleAnimation : MonoBehaviour {

	[SerializeField]
	float wobbleRange = 0.5f;
	[SerializeField]
	float wobbleMinDistance = 0f;
	[SerializeField]
	float wobbleMaxSpeed = 0.01f;
	[SerializeField]
	float wobbleMinSpeed = 0.009f;
	[SerializeField]
	float wobbleAcceleration = 0.01f;

	[SerializeField]
	bool ignoreXAxis = false;
	[SerializeField]
	bool ignoreYAxis = false;
	[SerializeField]
	bool ignoreZAxis = false;
	[SerializeField]
	bool useMinDistance = true;

	float currentSpeed = 0f;
	Vector3 startPosition = Vector3.zero;
	Vector3 lastPosition = Vector3.zero;
	Vector3 targetPosition = Vector3.zero;
	float t = 0f;

	// Use this for initialization
	void Start () {
		wobbleMinDistance = wobbleRange / 2;
		startPosition = transform.position;
		lastPosition = startPosition;
		targetPosition = PickNewTarget();
	}
	
	// Update is called once per frame
	void Update () {

		t += currentSpeed;

		if (t < 0.5f)
			currentSpeed += wobbleAcceleration;
		else
			currentSpeed -= wobbleAcceleration;

		if (currentSpeed < wobbleMinSpeed)
			currentSpeed = wobbleMinSpeed;
		else if (currentSpeed > wobbleMaxSpeed)
			currentSpeed = wobbleMaxSpeed;

		transform.position = Vector3.Lerp(lastPosition, targetPosition, t);

		if (t > 0.99f)
		{
			Reset();
		}

	}

	private void Reset()
	{
		lastPosition = targetPosition;
		targetPosition = PickNewTarget();
		currentSpeed = 0;
		t = 0;
	}

	Vector3 PickNewTarget()
	{
		float x = startPosition.x;
		float y = startPosition.y;
		float z = startPosition.z;

		if (!ignoreXAxis)
		{
			x += RandomValue();
		}
		if (!ignoreYAxis)
		{
			y += RandomValue();
		}
		if (!ignoreZAxis)
		{
			z += RandomValue();
		}

		return new Vector3(x, y, z);

	}

	float RandomValue()
	{
		if (useMinDistance)
		{
			if (Random.Range(0, 1) > 0)
				return Random.Range(wobbleMinDistance, wobbleRange);
			else
				return -(Random.Range(wobbleMinDistance, wobbleRange));
			
		}else
		{
			return Random.Range(-wobbleRange, wobbleRange);
		}

	}
}
