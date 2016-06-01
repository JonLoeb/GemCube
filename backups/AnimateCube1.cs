using UnityEngine;
using System.Collections;

public class AnimateCube : MonoBehaviour
{
    public bool IsInProgress { get; private set; }
    private Quaternion original;
    private Quaternion target;
    private float progress;
    const float rate = 0.1f;

	void Start()
    {
        IsInProgress = false;
    }

	void Update()
    {
        if(IsInProgress)
        {
            progress += rate;
            transform.rotation = Quaternion.Slerp(original, target, progress);

            if(progress >= 1.0f)
            {
                IsInProgress = false;
                transform.rotation = target; // to eliminate float math error?
            }
        }
	}

    public void StartAnimation(Vector3 axis, float angle)
    {
        if (IsInProgress)
            return;

        original = transform.rotation;
        target = transform.rotation * Quaternion.AngleAxis(angle, axis);
        progress = 0.0f;
        IsInProgress = true;
    }
}
