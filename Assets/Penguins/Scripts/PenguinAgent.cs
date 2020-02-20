using MLAgents;
using UnityEngine;

public class PenguinAgent : Agent {
	[Tooltip("How fast the agent moves forward")]
	public float moveSpeed = 5f;

	[Tooltip("How fast the agent turns")]
	public float turnSpeed = 180f;

	[Tooltip("Prefab of the heart that appears when the baby is fed")]
	public GameObject heartPrefab;

	[Tooltip("Prefab of the regurgitated fish that appears when the baby is fed")]
	public GameObject regurgitatedFishPrefab;

	private PenguinArea penguinArea;
	private PenguinAcademy penguinAcademy;
	private new Rigidbody rigidbody;
	private GameObject baby;

	private bool isFull = false; // If true, penguin has a full stomach
	private float feedRadius = 0f;

	public override void InitializeAgent() {
		base.InitializeAgent();
		penguinArea = GetComponentInParent<PenguinArea>();
		penguinAcademy = FindObjectOfType<PenguinAcademy>();
		baby = penguinArea.penguinBaby;
		rigidbody = GetComponent<Rigidbody>();
	}

	public override void AgentAction(float[] vectorAction) {
		// Convert the first action to forward movement
		float forwardAmount = vectorAction[0];

		// Convert the second action to turning left or right
		float turnAmount = 0f;
		if (vectorAction[1] == 1f) {
			turnAmount = -1f;
		} else if (vectorAction[1] == 2f) {
			turnAmount = 1f;
		}

		// Apply movement
		rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * moveSpeed * Time.fixedDeltaTime);
		transform.Rotate(transform.up * turnAmount * turnSpeed * Time.fixedDeltaTime);

		// Apply a tiny negative reward every step to encourage action
		AddReward(-1f / 5000);
	}

	public override float[] Heuristic() {
		float forwardAction = 0f;
		float turnAction = 0f;
		if (Input.GetKey(KeyCode.W)) {
			// move forward
			forwardAction = 1f;
		}
		if (Input.GetKey(KeyCode.A)) {
			// turn left
			turnAction = 1f;
		} else if (Input.GetKey(KeyCode.D)) {
			// turn right
			turnAction = 2f;
		}

		// Put the actions into an array and return
		return new float[] { forwardAction, turnAction };
	}

	public override void AgentReset() {
		isFull = false;
		penguinArea.ResetArea();
		feedRadius = penguinAcademy.FeedRadius;
	}

	public override void CollectObservations(VectorSensor sensor) {
		// Whether the penguin has eaten a fish (1 float = 1 value)
		sensor.AddObservation(isFull);

		// Distance to the baby (1 float = 1 value)
		sensor.AddObservation(Vector3.Distance(baby.transform.position, transform.position));

		// Direction to baby (1 Vector3 = 3 values)
		sensor.AddObservation((baby.transform.position - transform.position).normalized);

		// Direction penguin is facing (1 Vector3 = 3 values)
		sensor.AddObservation(transform.forward);

		// 1 + 1 + 3 + 3 = 8 total values
	}

	private void FixedUpdate() {
		// Test if the agent is close enough to to feed the baby
		if (Vector3.Distance(transform.position, baby.transform.position) < feedRadius) {
			// Close enough, try to feed the baby
			RegurgitateFish();
		}
	}

	private void OnCollisionEnter(Collision collision) {
		if (collision.transform.CompareTag("fish")) {
			// Try to eat the fish
			EatFish(collision.gameObject);
		} else if (collision.transform.CompareTag("baby")) {
			// Try to feed the baby
			RegurgitateFish();
		}
	}

	private void EatFish(GameObject fishObject) {
		if (isFull) {
			return; // Can't eat another fish while full
		}

		isFull = true;

		penguinArea.RemoveSpecificFish(fishObject);

		AddReward(1f);
	}

	private void RegurgitateFish() {
		if (!isFull) {
			return; // Nothing to regurgitate
		}

		isFull = false;

		// Spawn regurgitated fish
		GameObject regurgitatedFish = Instantiate<GameObject>(regurgitatedFishPrefab);
		regurgitatedFish.transform.parent = transform.parent;
		regurgitatedFish.transform.position = baby.transform.position;
		Destroy(regurgitatedFish, 4f);

		// Spawn heart
		GameObject heart = Instantiate<GameObject>(heartPrefab);
		heart.transform.parent = transform.parent;
		heart.transform.position = baby.transform.position + Vector3.up;
		Destroy(heart, 4f);

		AddReward(1f);

		if (penguinArea.FishRemaining <= 0) {
			Done();
		}
	}
}
