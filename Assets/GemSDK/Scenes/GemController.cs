using UnityEngine;
using System.Collections;
using System.Linq;
using GemSDK.Unity;
using UnityEngine.UI;

public class GemController : MonoBehaviour{

  bool firstAngleMethod = true;
  bool normalize = true;

  const int gemCount = 6;
  IGem[] gem = new IGem[6];
  float[] angle = new float[gemCount];

  public Text stateText;
  public Text tableDataText;
  string quatData = "";
  string angleData = "";

  Vector3[] axis = new Vector3[6];
  Vector3[] axisNorm = new Vector3[6];
  Quaternion[] sideOrientation = new Quaternion[6];
  Quaternion[] currentState = new Quaternion[gemCount];
  Quaternion[] stabalizer = new Quaternion[gemCount];
  Quaternion cubeRotation = Quaternion.identity;
  Quaternion[] rotationData = new Quaternion[12];

  public GameObject ball;

  private static readonly float[,] angleTable = {
    {0.9f, 1.9f, -0.7f, -2.8f, -1.5f, -4.5f},
    {-7.9f, -12.6f, 5.3f, 5.1f, -4.4f, -8.3f},
    {3.6f, 5.3f, 3.2f, -1.4f, -6.8f, -6.6f},
    {14.6f, -7.1f, 5.4f, -8.3f, -7.2f, -10.0f},
    {3.1f, -18.2f, 13.6f, -3.8f, -3.2f, -3.3f},
    {0.9f, -0.2f, 0.2f, 0.1f, -0.1f, -2.3f},
    {0.8f, -2.0f, -0.2f, -0.8f, -3.1f, -2.9f},
    {0.8f, -2.0f, -0.2f, -0.8f, -3.1f, -2.9f},
    {3.6f, -2.8f, 6.6f, 8.1f, -4.2f, -3.4f},
    {1.2f, 9.6f, 14.7f, -9.2f, -5.2f, 6.4f},
    {10.6f, -4.7f, 6.1f, -4.4f, -5.8f, -9.2f},
    {-1.0f, -3.7f, 3.3f, -3.7f, 1.9f, -8.2f},
    {10.0f, 7.4f, 7.7f, 3.6f, -7.3f, -5.8f},
    {9.1f, 2.5f, 6.1f, -8.0f, -2.5f, 1.9f},
    {-0.8f, 4.4f, 1.2f, -4.0f, -1.0f, -2.5f},
    {9.4f, 7.5f, 1.5f, -4.7f, -7.3f, -6.9f},
    {-7.6f, 10.3f, 5.7f, -4.6f, 0.3f, -9.0f},
    {-0.9f, 0.1f, 5.1f, 2.6f, 0.3f, -1.4f},
    {-6.3f, -5.5f, 4.1f, -3.1f, -1.8f, -5.3f},
    {-0.9f, 0.2f, 0.0f, -1.4f, -0.3f, 0.3f},
    {-1.3f, 0.6f, 1.3f, -3.5f, -0.9f, 0.5f},
    {-2.4f, 0.5f, 3.4f, -4.5f, -0.8f, 1.2f},
    {-4.0f, 1.9f, 5.6f, -4.1f, -0.4f, 2.2f},
    {-6.2f, 4.5f, 7.7f, -2.7f, -0.2f, 3.3f},
    {-9.4f, 4.6f, 8.6f, -2.5f, -0.7f, 4.2f},
    {-12.0f, 3.7f, 9.1f, -2.6f, -2.3f, 3.6f},
    {-12.7f, 1.7f, 8.6f, -2.6f, -3.9f, 2.8f},
    {-2.0f, -2.4f, 4.7f, 3.0f, -0.6f, -3.1f},
    {-2.4f, -3.2f, 5.4f, 2.5f, -0.1f, -3.8f},
    {-3.9f, -5.7f, 5.3f, 0.9f, -0.9f, -5.2f},
    {-5.6f, -6.5f, 5.6f, 0.6f, -1.2f, -7.0f},
    {-7.0f, -6.5f, 5.7f, 0.8f, -1.4f, -8.3f},
    {-7.8f, -5.2f, 6.6f, 1.9f, -1.2f, -8.9f},
    {-7.9f, -4.4f, 7.1f, 2.6f, -1.1f, -9.2f},
    {-4.9f, -7.8f, 4.7f, 5.5f, 0.3f, -2.1f},
    {-3.9f, -12.5f, 3.7f, 7.5f, -2.7f, -5.5f},
    {3.8f, -15.9f, 4.1f, 7.4f, -5.3f, -7.9f},
    {3.1f, -17.5f, 5.3f, 5.2f, -5.1f, -10.9f},
    {1.5f, -18.0f, 6.3f, 4.0f, -5.0f, -13.4f},
    {1.7f, -18.0f, 6.5f, 4.2f, -5.1f, -13.5f},
    {0.3f, -17.4f, 7.0f, 4.3f, -5.1f, -14.1f},
    {-1.1f, -16.3f, 7.4f, 4.6f, -5.3f, -13.7f},
    {3.6f, 4.8f, -2.7f, 2.3f, -5.2f, -5.6f},
    {3.1f, -3.2f, 1.3f, 2.3f, -2.5f, -1.9f},
    {2.8f, -1.9f, 2.8f, 2.9f, -2.0f, -0.3f},
    {6.2f, -1.3f, 4.2f, -5.0f, -2.4f, 1.3f},
    {-15.8f, -1.7f, 4.9f, 7.3f, -3.5f, 2.4f},
    {-16.3f, -1.4f, 5.1f, 7.3f, -3.4f, 2.5f},
    {4.5f, -6.3f, 1.3f, 2.8f, -2.9f, -2.8f},
    {6.6f, -9.4f, 2.7f, 2.1f, -3.2f, -4.7f},
    {2.5f, -3.8f, -1.3f, 1.2f, -2.5f, -3.4f},
    {4.9f, -7.9f, -0.3f, 6.4f, -4.0f, -1.2f},
    {-9.4f, -13.7f, 1.5f, 8.5f, -3.7f, 2.0f},
    {-12.8f, -18.4f, 3.2f, 7.6f, -3.2f, 7.1f},
    {-15.3f, -18.9f, 4.4f, 6.7f, -2.9f, 9.7f},
    {1.5f, -7.5f, 1.3f, 3.9f, -4.8f, -5.0f},
    {4.2f, 6.9f, 1.7f, -2.9f, -4.4f, -5.9f},
    {6.2f, 8.0f, 1.5f, -2.9f, -2.4f, -7.4f},
    {13.2f, -10.9f, 5.8f, 5.6f, -6.0f, -7.8f},
    {14.2f, -12.2f, 5.7f, 5.6f, -6.3f, -7.9f},
    {22.7f, -13.6f, 7.3f, -5.9f, -7.9f, -7.8f},
    {30.5f, -13.9f, 8.0f, -4.7f, -9.8f, -7.2f},
    {34.1f, -13.3f, 7.6f, -3.5f, -10.9f, -6.6f},
    {2.0f, -12.5f, 1.6f, 7.6f, -5.2f, -6.4f},
    {2.1f, -19.8f, 3.3f, 4.4f, 5.1f, -11.1f},
    {-2.1f, -20.0f, 3.7f, 3.1f, 4.6f, -12.2f},
    {-1.8f, -19.5f, 3.9f, 2.0f, 4.0f, -12.9f},
    {-2.2f, -17.6f, 3.9f, 1.6f, 2.9f, -12.4f},
    {-3.1f, -15.8f, 3.9f, 2.0f, 2.0f, -11.2f},
    {-1.7f, -15.5f, -3.0f, 3.6f, -3.0f, -8.2f},
    {-0.9f, -14.3f, -2.8f, 5.0f, -3.8f, -6.9f},
    {2.9f, -3.7f, 7.6f, -12.2f, -6.4f, -4.0f},
    {4.6f, -1.2f, 10.2f, -13.5f, -5.5f, -4.3f},
    {5.4f, 1.8f, 13.4f, -15.3f, -4.4f, -4.3f},
    {-9.6f, 12.0f, 6.1f, 7.5f, -9.1f, -4.9f},
    {-0.1f, -3.5f, -0.8f, -2.0f, -0.8f, -4.2f},
    {2.2f, -2.3f, 0.7f, -4.3f, -1.8f, -2.8f},
    {3.9f, 1.3f, 3.6f, -6.6f, -2.7f, -2.2f},
    {5.1f, 2.8f, 7.3f, -8.5f, -2.9f, -2.3f},
    {6.5f, 5.8f, 11.6f, -9.4f, -3.4f, -3.6f},
    {7.4f, 2.4f, 9.9f, -9.0f, -4.0f, -4.4f},
    {6.7f, -1.6f, 10.0f, -13.4f, -4.4f, -3.1f},
    {4.7f, -2.3f, 9.8f, -16.3f, 4.3f, -2.5f},
    {2.0f, 7.1f, 1.9f, -4.4f, -3.7f, -2.9f},
    {-1.6f, 5.1f, 3.9f, -3.8f, -3.5f, -3.8f},
    {-4.9f, 2.9f, 6.4f, -3.1f, -3.7f, -4.8f},
    {5.0f, -3.8f, 8.7f, -8.8f, -2.6f, -3.3f},
    {3.5f, 4.0f, 9.7f, -11.5f, -4.6f, -1.2f},
    {2.2f, 4.2f, 10.4f, -12.3f, -9.9f, 1.4f}

  };

  private static readonly Quaternion[] quaternionTable = {
    new Quaternion(0.064f, -0.063f, -0.693f, 0.715f),
    new Quaternion(0.501f, 0.474f, -0.459f, 0.560f),
    new Quaternion(-0.711f, 0.002f, 0.002f, 0.704f),
    new Quaternion(-0.508f, -0.511f, 0.527f, 0.451f),
    new Quaternion(0.484f, -0.016f, -0.466f, 0.741f),
    new Quaternion(0.000f, 0.005f, 0.000f, 1.000f),
    new Quaternion(0.318f, -0.081f, 0.295f, 0.897f),
    new Quaternion(0.318f, -0.081f, 0.295f, 0.897f),
    new Quaternion(0.785f, -0.204f, -0.306f, 0.498f),
    new Quaternion(0.294f, -0.793f, -0.398f, 0.354f),
    new Quaternion(-0.768f, -0.184f, 0.362f, 0.495f),
    new Quaternion(0.116f, -0.615f, 0.440f, 0.644f),
    new Quaternion(0.993f, -0.005f, 0.002f, 0.122f),
    new Quaternion(0.169f, 0.794f, 0.452f, 0.370f),
    new Quaternion(-0.264f, -0.044f, -0.335f, 0.903f),
    new Quaternion(-0.834f, 0.008f, -0.340f, 0.434f),
    new Quaternion(0.471f, -0.536f, 0.538f, 0.449f),
    new Quaternion(-0.010f, 0.462f, 0.002f, 0.887f),
    new Quaternion(-0.011f, 0.971f, 0.001f, 0.239f),
    new Quaternion(0.004f, -0.135f, 0.001f, 0.991f),
    new Quaternion(0.011f, -0.365f, 0.007f, 0.931f),
    new Quaternion(0.015f, -0.562f, 0.018f, 0.827f),
    new Quaternion(0.015f, -0.695f, 0.025f, 0.719f),
    new Quaternion(0.012f, -0.829f, 0.031f, 0.558f),
    new Quaternion(0.006f, -0.926f, 0.036f, 0.375f),
    new Quaternion(-0.001f, -0.991f, 0.033f, 0.132f),
    new Quaternion(-0.012f, -0.999f, 0.024f, 0.047f),
    new Quaternion(-0.011f, 0.625f, 0.014f, 0.780f),
    new Quaternion(-0.015f, 0.689f, 0.013f, 0.724f),
    new Quaternion(-0.014f, 0.805f, 0.015f, 0.593f),
    new Quaternion(-0.013f, 0.898f, 0.018f, 0.440f),
    new Quaternion(-0.013f, 0.966f, 0.019f, 0.257f),
    new Quaternion(-0.012f, 0.994f, 0.020f, 0.106f),
    new Quaternion(-0.012f, 0.999f, 0.021f, 0.040f),
    new Quaternion(0.142f, 0.680f, -0.141f, 0.706f),
    new Quaternion(0.339f, 0.598f, -0.319f, 0.653f),
    new Quaternion(0.481f, 0.507f, -0.469f, 0.540f),
    new Quaternion(0.385f, 0.594f, -0.551f, 0.442f),
    new Quaternion(0.262f, 0.668f, -0.622f, 0.313f),
    new Quaternion(0.262f, 0.668f, -0.621f, 0.314f),
    new Quaternion(0.169f, 0.703f, -0.657f, 0.216f),
    new Quaternion(0.091f, 0.720f, -0.676f, 0.133f),
    new Quaternion(0.017f, -0.015f, 0.720f, 0.693f),
    new Quaternion(-0.215f, 0.173f, 0.430f, 0.860f),
    new Quaternion(-0.091f, 0.423f, 0.471f, 0.769f),
    new Quaternion(0.064f, 0.655f, 0.476f, 0.583f),
    new Quaternion(0.245f, 0.836f, 0.411f, 0.267f),
    new Quaternion(0.245f, 0.836f, 0.412f, 0.267f),
    new Quaternion(-0.260f, 0.054f, 0.455f, 0.850f),
    new Quaternion(-0.373f, -0.208f, 0.407f, 0.808f),
    new Quaternion(0.395f, -0.035f, 0.328f, 0.857f),
    new Quaternion(0.511f, 0.334f, 0.089f, 0.787f),
    new Quaternion(0.511f, 0.549f, -0.102f, 0.653f),
    new Quaternion(0.458f, 0.713f, -0.239f, 0.475f),
    new Quaternion(0.393f, 0.823f, -0.222f, 0.345f),
    new Quaternion(0.691f, -0.020f, 0.016f, 0.722f),
    new Quaternion(0.674f, -0.190f, 0.178f, 0.691f),
    new Quaternion(0.606f, -0.373f, 0.358f, 0.605f),
    new Quaternion(-0.502f, -0.509f, 0.526f, 0.461f),
    new Quaternion(-0.503f, -0.508f, 0.528f, 0.459f),
    new Quaternion(-0.603f, -0.592f, 0.414f, 0.339f),
    new Quaternion(-0.689f, -0.661f, 0.242f, 0.173f),
    new Quaternion(-0.715f, -0.677f, 0.150f, 0.088f),
    new Quaternion(0.517f, 0.075f, -0.667f, 0.531f),
    new Quaternion(0.385f, 0.327f, -0.744f, 0.437f),
    new Quaternion(0.291f, 0.375f, -0.787f, 0.394f),
    new Quaternion(0.191f, 0.418f, -0.819f, 0.344f),
    new Quaternion(0.026f, 0.470f, -0.845f, 0.255f),
    new Quaternion(-0.094f, 0.497f, -0.842f, 0.185f),
    new Quaternion(-0.094f, 0.316f, -0.923f, 0.197f),
    new Quaternion(-0.102f, 0.244f, -0.949f, 0.172f),
    new Quaternion(0.190f, -0.332f, -0.860f, 0.338f),
    new Quaternion(0.392f, -0.418f, -0.748f, 0.335f),
    new Quaternion(0.619f, -0.502f, -0.576f, 0.182f),
    new Quaternion(0.889f, 0.060f, 0.339f, 0.302f),
    new Quaternion(-0.018f, 0.028f, -0.695f, 0.718f),
    new Quaternion(0.126f, -0.113f, -0.683f, 0.710f),
    new Quaternion(0.280f, -0.268f, -0.634f, 0.669f),
    new Quaternion(0.425f, -0.417f, -0.547f, 0.589f),
    new Quaternion(0.608f, -0.616f, -0.330f, 0.376f),
    new Quaternion(-0.519f, 0.502f, 0.505f, 0.473f),
    new Quaternion(-0.407f, 0.595f, 0.599f, 0.349f),
    new Quaternion(-0.281f, 0.656f, 0.666f, 0.218f),
    new Quaternion(-0.479f, -0.060f, -0.332f, 0.810f),
    new Quaternion(-0.492f, 0.105f, -0.163f, 0.849f),
    new Quaternion(-0.509f, 0.486f, 0.085f, 0.705f),
    new Quaternion(-0.416f, 0.764f, 0.322f, 0.374f),
    new Quaternion(-0.376f, 0.806f, 0.363f, 0.277f),
    new Quaternion(-0.314f, 0.843f, 0.415f, 0.140f),
    new Quaternion(0.004f, -0.135f, 0.001f, 0.991f),
    new Quaternion(0.011f, -0.365f, 0.007f, 0.931f),
    new Quaternion(0.015f, -0.562f, 0.018f, 0.827f),
    new Quaternion(0.015f, -0.695f, 0.025f, 0.719f),
    new Quaternion(0.012f, -0.829f, 0.031f, 0.558f),
    new Quaternion(0.006f, -0.926f, 0.036f, 0.375f),
    new Quaternion(-0.001f, -0.991f, 0.033f, 0.132f),
    new Quaternion(-0.012f, -0.999f, 0.024f, 0.047f),
    new Quaternion(-0.011f, 0.625f, 0.014f, 0.780f),
    new Quaternion(-0.015f, 0.689f, 0.013f, 0.724f),
    new Quaternion(-0.014f, 0.805f, 0.015f, 0.593f),
    new Quaternion(-0.013f, 0.898f, 0.018f, 0.440f),
    new Quaternion(-0.013f, 0.966f, 0.019f, 0.257f),
    new Quaternion(-0.012f, 0.994f, 0.020f, 0.106f),
    new Quaternion(-0.012f, 0.999f, 0.021f, 0.040f),
    new Quaternion(0.142f, 0.680f, -0.141f, 0.706f),
    new Quaternion(0.339f, 0.598f, -0.319f, 0.653f),
    new Quaternion(0.481f, 0.507f, -0.469f, 0.540f),
    new Quaternion(0.385f, 0.594f, -0.551f, 0.442f),
    new Quaternion(0.262f, 0.668f, -0.622f, 0.313f),
    new Quaternion(0.262f, 0.668f, -0.621f, 0.314f),
    new Quaternion(0.169f, 0.703f, -0.657f, 0.216f),
    new Quaternion(0.091f, 0.720f, -0.676f, 0.133f),
    new Quaternion(0.017f, -0.015f, 0.720f, 0.693f),
    new Quaternion(-0.215f, 0.173f, 0.430f, 0.860f),
    new Quaternion(-0.091f, 0.423f, 0.471f, 0.769f),
    new Quaternion(0.064f, 0.655f, 0.476f, 0.583f),
    new Quaternion(0.245f, 0.836f, 0.411f, 0.267f),
    new Quaternion(0.245f, 0.836f, 0.412f, 0.267f),
    new Quaternion(-0.260f, 0.054f, 0.455f, 0.850f),
    new Quaternion(-0.373f, -0.208f, 0.407f, 0.808f),
    new Quaternion(0.395f, -0.035f, 0.328f, 0.857f),
    new Quaternion(0.511f, 0.334f, 0.089f, 0.787f),
    new Quaternion(0.511f, 0.549f, -0.102f, 0.653f),
    new Quaternion(0.458f, 0.713f, -0.239f, 0.475f),
    new Quaternion(0.393f, 0.823f, -0.222f, 0.345f),
    new Quaternion(0.691f, -0.020f, 0.016f, 0.722f),
    new Quaternion(0.674f, -0.190f, 0.178f, 0.691f),
    new Quaternion(0.606f, -0.373f, 0.358f, 0.605f),
    new Quaternion(-0.502f, -0.509f, 0.526f, 0.461f),
    new Quaternion(-0.503f, -0.508f, 0.528f, 0.459f),
    new Quaternion(-0.603f, -0.592f, 0.414f, 0.339f),
    new Quaternion(-0.689f, -0.661f, 0.242f, 0.173f),
    new Quaternion(-0.715f, -0.677f, 0.150f, 0.088f),
    new Quaternion(0.517f, 0.075f, -0.667f, 0.531f),
    new Quaternion(0.385f, 0.327f, -0.744f, 0.437f),
    new Quaternion(0.291f, 0.375f, -0.787f, 0.394f),
    new Quaternion(0.191f, 0.418f, -0.819f, 0.344f),
    new Quaternion(0.026f, 0.470f, -0.845f, 0.255f),
    new Quaternion(-0.094f, 0.497f, -0.842f, 0.185f),
    new Quaternion(-0.094f, 0.316f, -0.923f, 0.197f),
    new Quaternion(-0.102f, 0.244f, -0.949f, 0.172f),
    new Quaternion(0.190f, -0.332f, -0.860f, 0.338f),
    new Quaternion(0.392f, -0.418f, -0.748f, 0.335f),
    new Quaternion(0.619f, -0.502f, -0.576f, 0.182f),
    new Quaternion(0.889f, 0.060f, 0.339f, 0.302f),
    new Quaternion(-0.018f, 0.028f, -0.695f, 0.718f),
    new Quaternion(0.126f, -0.113f, -0.683f, 0.710f),
    new Quaternion(0.280f, -0.268f, -0.634f, 0.669f),
    new Quaternion(0.425f, -0.417f, -0.547f, 0.589f),
    new Quaternion(0.608f, -0.616f, -0.330f, 0.376f),
    new Quaternion(-0.519f, 0.502f, 0.505f, 0.473f),
    new Quaternion(-0.407f, 0.595f, 0.599f, 0.349f),
    new Quaternion(-0.281f, 0.656f, 0.666f, 0.218f),
    new Quaternion(-0.479f, -0.060f, -0.332f, 0.810f),
    new Quaternion(-0.492f, 0.105f, -0.163f, 0.849f),
    new Quaternion(-0.509f, 0.486f, 0.085f, 0.705f),
    new Quaternion(-0.416f, 0.764f, 0.322f, 0.374f),
    new Quaternion(-0.376f, 0.806f, 0.363f, 0.277f),
    new Quaternion(-0.314f, 0.843f, 0.415f, 0.140f)
  };





  // Use this for initialization
  void Start(){
    GemManager.Instance.Connect();

    //To get gem by number instead of address, on Android the Gem should be paired to Gem SDK Utility app
    //gem = GemManager.Instance.GetGem(0);

    gem[0] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:DD");
    gem[1] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:E6");
    gem[2] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:3A");
    gem[3] =  GemManager.Instance.GetGem("D0:B5:C2:90:7C:69");
    gem[4] =  GemManager.Instance.GetGem("D0:B5:C2:90:7C:4D");
    gem[5] =  GemManager.Instance.GetGem("D0:B5:C2:90:7E:2F");

    for (int i = 0; i < gemCount; i++){
      stabalizer[i] = Quaternion.identity;
    }

    axis[0] = Vector3.up;//U
    axis[1] = Vector3.left;//L
    axis[2] = Vector3.back;//F
    axis[3] = Vector3.right;//R
    axis[4] = Vector3.forward;//B
    axis[5] = Vector3.down;//D

    axisNorm[0] = Vector3.left;//U
    axisNorm[1] = Vector3.back;//L
    axisNorm[2] = Vector3.right;//F
    axisNorm[3] = Vector3.forward;//R
    axisNorm[4] = Vector3.left;//B
    axisNorm[5] = Vector3.back;//D
      //axisNorm[5] = Vector3.right;//D


    sideOrientation[0] = Quaternion.identity;
    sideOrientation[1] = Quaternion.AngleAxis(90, axisNorm[1]);
    sideOrientation[2] = Quaternion.AngleAxis(90, axisNorm[2]);
    sideOrientation[3] = Quaternion.AngleAxis(90, axisNorm[3]);
    sideOrientation[4] = Quaternion.AngleAxis(90, axisNorm[4]);
    sideOrientation[5] = Quaternion.AngleAxis(180, axisNorm[5]);
  }



  void FixedUpdate(){



    if (gemsAreNotNull()){
      if(Input.GetKeyUp(KeyCode.Space) && allGemsConnected()){
        //tableDataText.text = tableText();
        //System.IO.File.WriteAllText("../../Desktop/quatData.text", quatData);
        //System.IO.File.WriteAllText("../../Desktop/angleData.text", angleData);
        //Instantiate(ball, Vector3.zero, cubeRotation);

        firstAngleMethod = !firstAngleMethod;

      }

      if (Input.GetMouseButton(0)){
        //calibrate gems
        for (int i = 0; i < gemCount; i++){
        //  gem[i].CalibrateAzimuth();
        }
        //Use instead of CalibrateAzimuth() to calibrate also tilt and elevation
        //gem.CalibrateOrigin();

        cubeRotation = Quaternion.identity;
        calculateStabalizers();
      }

      for (int i = 0; i < gemCount; i++){
        currentState[i] = gem[i].Rotation * sideOrientation[i];
      }
      stabalizeGems();
      getCubeRotation();
      matchStateToCube();

      transform.rotation = cubeRotation;

      calculateAngles();
      float cubeAngle = Quaternion.Angle(cubeRotation, Quaternion.identity);
      stateText.text = displayText() + cubeAngle.ToString("#.0") + " " + allGemsConnected()
        + "\n"  + firstAngleMethod;

      //if(allGemsConnected() && allGemsConnected()){
      //tableDataText.text = tableText();
      //System.IO.File.WriteAllText("../../Desktop/quatData.text", quatData);
      //System.IO.File.WriteAllText("../../Desktop/angleData.text", angleData);
      //}
    }
  }

  void calculateAngles(){
    for (int i = 0; i < gemCount; i++){
        currentState[i] = checkAndFixQuaternion(currentState[i], cubeRotation);
        Quaternion q = Quaternion.Inverse(cubeRotation) * currentState[i];


        if (firstAngleMethod){
          angle[i] = Quaternion.Angle(cubeRotation, currentState[i]);
          angle[i] *= angleSign(cubeRotation * axisNorm[i],currentState[i] * axisNorm[i],cubeRotation * axis[i]);
        }
        else{
          angle[i] = Vector3.Angle(q * axisNorm[i], axisNorm[i]);
          angle[i] *= -angleSign(q * axisNorm[i], axisNorm[i], axis[i]);
        }
        angle[i] = (angle[i] + 360) % 360;

        //turn this off when getting bug data
        //angle[i] -= bugFixAngle(i);
        //angle[i] = (angle[i] + 360) % 360;
    }
  }

  bool allGemsConnected(){
    for (int i = 0; i < gemCount; i++){
      if (gem[i].State != GemState.Connected){
        return false;
      }
    }
    return true;
  }

  bool gemsAreNotNull(){
    for (int i = 0; i < gemCount; i++){
      if (gem[i] == null){
        return false;
      }
    }
    return true;
  }

  void calculateStabalizers(){
    for (int i = 0; i < gemCount; i++){
      stabalizer[i] = Quaternion.Inverse(Quaternion.LookRotation(
      gem[i].Rotation * sideOrientation[i] * Vector3.forward,
      gem[i].Rotation * sideOrientation[i] * Vector3.up));
    }



  }

  void stabalizeGems(){
    for (int i = 0; i < gemCount; i++){
      currentState[i] =  stabalizer[i] * currentState[i];
    }
  }

  void matchStateToCube(){
    for (int i = 0; i < gemCount; i++){
      currentState[i] = currentState[i] *
      Quaternion.FromToRotation(currentState[i] * axis[i], cubeRotation * axis[i]);
    }
  }

  int angleSign (Vector3 v1, Vector3 v2, Vector3 normalVector){
    Vector3 crossProduct = Vector3.Cross(v1, v2);
    float dotProduct = (Vector3.Dot(crossProduct, normalVector));
    if (dotProduct > 0){
      return 1;
    }
    return -1;
  }

  string displayText(){
    string outputMe = "";
    for (int i = 0; i < gemCount; i++){
    //  outputMe += gem[i].State.ToString() + ": " + angle[i].ToString("#.0")
    //  + ":   " + getAngleError(angle[i], 0).ToString("#.0") + "\n";

      outputMe += gem[i].State.ToString() + ": " + angle[i].ToString("#.0") + "\n";
    }
    return outputMe;
  }

  string tableText(){
    string outputMe = cubeRotation.ToString("#.00") + "\n{";
    quatData += "\n new Quaternion("
    + cubeRotation[0].ToString("#0.000") + "f, "
    + cubeRotation[1].ToString("#0.000") + "f, "
    + cubeRotation[2].ToString("#0.000") + "f, "
    + cubeRotation[3].ToString("#0.000") + "f),";
    angleData += "\n {";
    for (int i = 0; i < gemCount; i++){
      if(i != 0){
        angleData += ", ";
        outputMe += ", ";
      }
      angleData += getAngleError(angle[i], 0).ToString("#0.0") + "f";
      outputMe += getAngleError(angle[i], 0).ToString("#.0") + "f";
    }
    angleData += "},";
    outputMe += "}";
    return outputMe;
  }

  void getCubeRotation(){
    cubeRotation = Quaternion.identity;
    int count = 0;
    if (gemCount >= 2){
      if (gem[0].State == GemState.Connected && gem[1].State == GemState.Connected){
        //if (gemIsConnected[0] && gemIsConnected[1]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[1] * axis[1],currentState[0] * axis[0])
        * Quaternion.AngleAxis(90, Vector3.up);
        count++;
      }
    }
    if (gemCount >= 3){
      if (gem[0].State == GemState.Connected && gem[2].State == GemState.Connected){
        //if (gemIsConnected[0] && gemIsConnected[2]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[2] * -axis[2], currentState[0] * axis[0]);
        count++;
      }

      if (gem[1].State == GemState.Connected && gem[2].State == GemState.Connected){
        //if (gemIsConnected[1] && gemIsConnected[2]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[2] * -axis[2], currentState[1] * axis[1])
        * Quaternion.AngleAxis(90, Vector3.back);
        count++;
      }
    }
    if (gemCount >= 4){
      if (gem[0].State == GemState.Connected && gem[3].State == GemState.Connected){
        //if (gemIsConnected[0] && gemIsConnected[3]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[3] * axis[3], currentState[0] * axis[0])
        * Quaternion.AngleAxis(-90, Vector3.up);
        count++;
      }

      if (gem[2].State == GemState.Connected && gem[3].State == GemState.Connected){
        //if (gemIsConnected[2] && gemIsConnected[3]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[2] * -axis[2], currentState[3] * axis[3])
        * Quaternion.AngleAxis(-90, Vector3.back);
        count++;
      }
    }
    if (gemCount >= 5){
      if (gem[0].State == GemState.Connected && gem[4].State == GemState.Connected){
        //if (gemIsConnected[0] && gemIsConnected[4]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[4] * axis[4], currentState[0] * axis[0]);
        count++;
      }

      if (gem[1].State == GemState.Connected && gem[4].State == GemState.Connected){
        //if (gemIsConnected[1] && gemIsConnected[4]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[4] * axis[4], currentState[1] * axis[1])
        * Quaternion.AngleAxis(90, Vector3.back);
        count++;
      }

      if (gem[3].State == GemState.Connected && gem[4].State == GemState.Connected){
        //if (gemIsConnected[3] && gemIsConnected[4]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[4] * axis[4], currentState[3] * axis[3])
        * Quaternion.AngleAxis(-90, Vector3.back);
        count++;
      }
    }
    if (gemCount == 6){
      if (gem[1].State == GemState.Connected && gem[5].State == GemState.Connected){
        //if (gemIsConnected[1] && gemIsConnected[5]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[1] * axis[1], currentState[5] * -axis[5])
        * Quaternion.AngleAxis(90, Vector3.up);
        count++;
      }

      if (gem[2].State == GemState.Connected && gem[5].State == GemState.Connected){
        //if (gemIsConnected[2] && gemIsConnected[5]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[2] * -axis[2], currentState[5] * -axis[5]);
        count++;
      }

      if (gem[3].State == GemState.Connected && gem[5].State == GemState.Connected){
        //if (gemIsConnected[3] && gemIsConnected[5]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[3] * axis[3], currentState[5] * -axis[5])
        * Quaternion.AngleAxis(-90, Vector3.up);
        count++;
      }

      if (gem[4].State == GemState.Connected && gem[5].State == GemState.Connected){
        //if (gemIsConnected[4] && gemIsConnected[5]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[4] * axis[4], currentState[5] * -axis[5]);
        count++;
      }
    }


    if(count != 0){
      Vector4 addedRotation = Vector4.zero;
      for (int i = 0; i < count; i++){

        //Temporary values
        float w = 0.0f;
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        if (i != 0){
          rotationData[i] = checkAndFixQuaternion(rotationData[i], rotationData[0]);
        }

        float addDet = 1.0f / (float)(i+1);
        addedRotation.w += rotationData[i].w;
        w = addedRotation.w * addDet;
        addedRotation.x += rotationData[i].x;
        x = addedRotation.x * addDet;
        addedRotation.y += rotationData[i].y;
        y = addedRotation.y * addDet;
        addedRotation.z += rotationData[i].z;
        z = addedRotation.z * addDet;

        //Normalize. Note: experiment to see whether you
        //can skip this step.
        if (normalize){
          float lengthD = 1.0f / (w*w + x*x + y*y + z*z);
          w *= lengthD;
          x *= lengthD;
          y *= lengthD;
          z *= lengthD;
        }


        //The result is valid right away, without
        //first going through the entire array.
        cubeRotation = new Quaternion(x, y, z, w);

        //useful links
        //http://forum.unity3d.com/threads/average-quaternions.86898/
        //http://wiki.unity3d.com/index.php/Averaging_Quaternions_and_Vectors
        //http://www.acsu.buffalo.edu/~johnc/ave_quat07.pdf
        //https://github.com/Algoryx/agxUnity/blob/master/AgXUnity/Utils/AverageQuaternion.cs
        //https://gist.github.com/jankolkmeier/8543156
      }
    }

    //cubeRotation = rotationData[1];
  }

  //Changes the sign of the quaternion components. This is not the same as the inverse.
  Quaternion checkAndFixQuaternion(Quaternion newQ, Quaternion firstQ){
    float dot = Quaternion.Dot(newQ, firstQ);
    if(dot < 0.0f){
      return new Quaternion(-newQ.x, -newQ.y, -newQ.z, -newQ.w);
    }
    return newQ;
  }


  float getAngleError(float badAngle, float realAngle){
    float error = Mathf.Max(badAngle, realAngle) - Mathf.Min(badAngle, realAngle);
    if (180 < error) {
      error = error - 360;
    }
    return error;
  }



  float bugFixAngle(int gemIndex){
    float closestDistance = Quaternion.Angle(cubeRotation, Quaternion.identity);
    float bugFix = 0;

    for (int i = 0; i < quaternionTable.Length; i++){
      float distance = Quaternion.Angle(cubeRotation, quaternionTable[i]);
      if (distance < closestDistance){
        closestDistance = distance;
        bugFix = angleTable[i, gemIndex];
      }
    }

    return bugFix;
  }

  void OnApplicationQuit()
  {
    GemManager.Instance.Disconnect();
  }

  //For Android to unbind Gem Service when the app is not in focus
  void OnApplicationPause(bool paused)
  {
    if (Application.platform == RuntimePlatform.Android)
    {
      if (paused)
      GemManager.Instance.Disconnect();
      else
      GemManager.Instance.Connect();
    }
  }
}
