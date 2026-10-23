using UnityEngine;

public class LoadingScreenRotatingIcon : MonoBehaviour
{
	private void Update()
	{
		transform.Rotate(new Vector3(0, 0, -50) * Time.deltaTime);
	}
}
