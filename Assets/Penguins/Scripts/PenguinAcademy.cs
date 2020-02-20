using MLAgents;
using UnityEngine;

public class PenguinAcademy : MonoBehaviour {
	public float FishSpeed { get; private set; }

	public float FeedRadius { get; private set; }

	private void Awake() {

		IFloatProperties floatProperties = Academy.Instance.FloatProperties;
		floatProperties.SetProperty("fish_speed", 0f);
		floatProperties.SetProperty("feed_radius", 0f);

		floatProperties.RegisterCallback("fish_speed", f => {
			FishSpeed = f;
		});

		floatProperties.RegisterCallback("feed_radius", f => {
			FeedRadius = f;
		});
	}
}
