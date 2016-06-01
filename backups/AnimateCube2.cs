using UnityEngine;
using System.Collections;

public class AnimateCube : MonoBehaviour
{
  public bool IsInProgress { get; private set; }
  private float progress;
	private Quaternion target;
  const float slowfactor = 0.02f;
	private Vector3 axis;
	private float angle;

	void Start()
    {
        IsInProgress = false;
    }

	void Update()
    {
        if(IsInProgress)
        {
			progress += slowfactor;
			transform.RotateAround(Vector3.zero, axis, angle * slowfactor);
            //transform.rotation = Quaternion.Slerp(original, target, progress);


            if(progress >= 1.0f)
            {
                IsInProgress = false;
                //transform.RotateAround(startVector, axis, angle);
               	//transform.rotation = target; // to eliminate float math error?
            }
        }
	}

    public void StartAnimation(Vector3 axis, float angle)
    {
        if (IsInProgress)
            return;
		target = transform.rotation * Quaternion.AngleAxis(angle, axis);
		this.axis = axis;
		this.angle = angle;
        progress = 0.0f;
        IsInProgress = true;
    }
}
