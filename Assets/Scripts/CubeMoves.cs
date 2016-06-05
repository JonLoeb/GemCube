using UnityEngine;
using System.Collections;
using System.Linq;
using GemSDK.Unity;
using UnityEngine.UI;

//orgin
public class CubeMoves : MonoBehaviour {

  const int gemCount = 2;
  bool originRotate = false;
  bool useAngleMethod = true;
  bool firstRun = true;
  IGem[] gem = new IGem[6];

  Quaternion[] currentState = new Quaternion[gemCount];
  Quaternion[] stabilizers = new Quaternion[gemCount];
  Quaternion cubeRotation = Quaternion.identity;
  Quaternion prevCubeRotation = Quaternion.identity;
  Quaternion[] rotationData = new Quaternion[12];
  bool smallChange = true;
  bool[] gemIsConnected = new bool[gemCount];
  bool wasFullyConnected = false;

  Vector3[] axis = new Vector3[6];
  Vector3[] axisNorm = new Vector3[6];
  Quaternion[] sideOrientation = new Quaternion[6];

  Quaternion[] cornerPermutation = new Quaternion[8];
  Quaternion[] edgePermutation  = new Quaternion[12];
  Quaternion[] centerPermutation  = new Quaternion[6];

  Piece[] corner = new Piece[8];
  Piece[] edge  = new Piece[12];
  Piece[] center = new Piece[6];
  Piece[] animateUs = new Piece[9];

  public Text[] stateText = new Text[gemCount+1];
  bool reset = false;
  float[] angleCounter = new float[gemCount];
  float[] spinFixer = new float[gemCount];
  //float[] calibrateFixer = new float[gemCount];

  bool[] clockwiseDirection = new bool[6];
  bool[] needsUpdate = new bool[6];
  string moves = "";
  string sideOrder = "ULFRBD";

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
    {-1.4f, 20.1f, 8.8f, 5.2f, -6.5f, 3.8f},
    {-2.9f, 30.2f, 10.3f, 6.2f, -4.5f, 5.1f},
    {-2.5f, 26.8f, 8.4f, 2.9f, -1.9f, 4.4f},
    {0.5f, 16.1f, 4.7f, -1.6f, 1.1f, -3.7f},
    {6.3f, 2.8f, 0.6f, -4.2f, 4.9f, -5.3f},
    {9.0f, -1.5f, -1.1f, -3.9f, 6.4f, -6.9f},
    {12.1f, -4.7f, -2.1f, -2.8f, 7.7f, -9.5f},
    {-0.4f, -0.2f, 0.2f, -2.3f, -0.3f, 0.5f},
    {-1.7f, 4.1f, 7.3f, -4.3f, 0.6f, 2.4f},
    {-2.6f, 7.4f, 9.5f, -3.0f, 1.0f, 3.7f},
    {-4.6f, 8.2f, 10.8f, -2.9f, 0.7f, 5.1f},
    {-6.2f, 8.2f, 11.3f, -3.0f, 0.3f, 5.8f},
    {-0.8f, 2.5f, 1.0f, 0.3f, 1.8f, 0.1f},
    {-1.6f, 3.0f, 1.4f, 0.0f, 2.0f, 0.3f},
    {-1.6f, 3.0f, 1.4f, 0.1f, 1.9f, 0.3f},
    {-1.3f, 2.7f, 1.5f, -0.6f, 2.3f, -0.7f},
    {0.2f, 2.3f, 1.3f, -1.0f, 1.3f, -2.4f},
    {3.0f, -3.8f, 1.2f, -1.4f, 0.4f, -5.3f},
    {1.0f, 2.4f, 0.7f, -1.5f, -1.1f, -1.3f},
    {2.9f, 3.8f, 2.1f, -1.7f, -2.8f, -3.9f},
    {2.0f, 6.1f, 2.7f, -2.5f, -2.3f, -4.0f},
    {3.4f, 5.2f, 1.8f, -1.4f, -3.3f, -4.4f},
    {14.1f, -6.7f, 7.6f, -8.3f, -6.8f, -9.9f},
    {15.4f, -11.6f, 8.2f, -9.0f, -6.6f, -9.8f},
    {13.8f, -7.7f, 9.5f, 6.3f, -6.0f, -7.9f},
    {11.7f, 6.7f, 9.6f, 4.1f, -5.8f, -6.9f},
    {9.6f, 6.4f, 9.4f, 2.8f, -5.8f, -6.1f},
    {8.8f, 5.4f, 6.6f, -4.4f, -7.0f, -8.3f},
    {-2.4f, 1.0f, 3.0f, -2.9f, 1.0f, 0.5f},
    {-1.8f, -4.8f, 3.7f, 3.0f, -2.0f, -2.3f},
    {1.2f, -7.4f, 4.5f, -3.1f, -3.8f, -3.9f},
    {6.1f, 9.1f, 5.3f, -4.2f, -2.0f, -6.5f},
    {2.1f, -3.0f, 1.1f, -2.5f, -2.1f, -5.5f},
    {4.6f, 6.0f, 2.1f, -2.9f, 1.5f, -9.7f},
    {3.7f, 6.6f, 2.8f, 6.2f, 2.0f, -12.9f},
    {2.4f, -7.5f, 3.3f, 9.2f, 3.2f, -14.5f},
    {3.6f, -8.8f, 4.1f, 11.8f, 4.8f, -15.4f},
    {3.2f, -5.6f, 2.8f, 3.0f, -6.2f, -8.2f},
    {-1.6f, -3.9f, -2.3f, 1.7f, -4.2f, -4.8f},
    {1.8f, -1.8f, 3.2f, 2.8f, -2.5f, -1.9f},
    {2.9f, -2.5f, 5.5f, -7.1f, -2.2f, 1.6f},
    {8.1f, -3.6f, 5.7f, -8.5f, -3.3f, 3.1f},
    {13.0f, -4.2f, 6.2f, -8.6f, -4.1f, 4.1f},
    {-17.1f, -5.4f, 7.2f, -8.1f, -4.6f, 4.6f},
    {-1.4f, -10.6f, 7.8f, -3.6f, 1.7f, -11.0f},
    {2.4f, -3.1f, -1.9f, 0.5f, -2.8f, -5.1f},
    {3.4f, -4.8f, -2.7f, 1.6f, -4.7f, -7.8f},
    {4.1f, 8.6f, -2.8f, 2.3f, -4.4f, -7.3f},
    {7.6f, 6.9f, 8.9f, 4.6f, -4.6f, -5.0f},
    {9.5f, 9.5f, 8.5f, -4.9f, -5.4f, -5.4f},
    {10.4f, 8.2f, 7.1f, -2.8f, -5.3f, -5.9f},
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
    {2.2f, 4.2f, 10.4f, -12.3f, -9.9f, 1.4f},



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
    new Quaternion(0.012f, 0.278f, 0.057f, 0.959f),
    new Quaternion(0.010f, 0.480f, 0.038f, 0.877f),
    new Quaternion(-0.006f, 0.642f, 0.016f, 0.766f),
    new Quaternion(-0.030f, 0.778f, 0.002f, 0.628f),
    new Quaternion(-0.064f, 0.897f, 0.002f, 0.438f),
    new Quaternion(-0.078f, 0.932f, 0.006f, 0.355f),
    new Quaternion(-0.092f, 0.966f, 0.017f, 0.239f),
    new Quaternion(0.009f, -0.198f, 0.002f, 0.980f),
    new Quaternion(0.021f, -0.675f, 0.026f, 0.737f),
    new Quaternion(0.017f, -0.789f, 0.032f, 0.613f),
    new Quaternion(0.012f, -0.880f, 0.039f, 0.473f),
    new Quaternion(0.008f, -0.932f, 0.043f, 0.360f),
    new Quaternion(0.005f, -0.031f, -0.112f, 0.993f),
    new Quaternion(0.016f, -0.053f, -0.202f, 0.978f),
    new Quaternion(0.016f, -0.053f, -0.201f, 0.978f),
    new Quaternion(0.056f, -0.103f, -0.393f, 0.912f),
    new Quaternion(0.086f, -0.113f, -0.534f, 0.834f),
    new Quaternion(0.104f, -0.096f, -0.694f, 0.706f),
    new Quaternion(-0.233f, -0.051f, -0.012f, 0.971f),
    new Quaternion(-0.465f, -0.032f, -0.016f, 0.885f),
    new Quaternion(-0.489f, 0.091f, -0.253f, 0.829f),
    new Quaternion(-0.536f, -0.008f, -0.005f, 0.844f),
    new Quaternion(-0.521f, -0.526f, 0.511f, 0.437f),
    new Quaternion(-0.708f, -0.347f, 0.440f, 0.430f),
    new Quaternion(-0.820f, -0.231f, 0.369f, 0.372f),
    new Quaternion(-0.875f, -0.150f, 0.319f, 0.332f),
    new Quaternion(-0.906f, -0.093f, 0.268f, 0.313f),
    new Quaternion(-0.704f, -0.156f, 0.050f, 0.691f),
    new Quaternion(0.315f, -0.225f, -0.252f, 0.887f),
    new Quaternion(0.587f, -0.074f, -0.091f, 0.801f),
    new Quaternion(0.694f, -0.068f, 0.075f, 0.713f),
    new Quaternion(0.619f, -0.345f, 0.347f, 0.614f),
    new Quaternion(0.365f, -0.226f, 0.428f, 0.795f),
    new Quaternion(0.262f, -0.475f, 0.561f, 0.626f),
    new Quaternion(0.178f, -0.573f, 0.639f, 0.481f),
    new Quaternion(0.081f, -0.643f, 0.671f, 0.360f),
    new Quaternion(-0.034f, -0.701f, 0.673f, 0.234f),
    new Quaternion(-0.419f, -0.110f, 0.412f, 0.802f),
    new Quaternion(-0.252f, 0.094f, 0.522f, 0.809f),
    new Quaternion(-0.104f, 0.308f, 0.558f, 0.764f),
    new Quaternion(0.125f, 0.597f, 0.550f, 0.571f),
    new Quaternion(0.243f, 0.710f, 0.506f, 0.425f),
    new Quaternion(0.331f, 0.777f, 0.451f, 0.289f),
    new Quaternion(0.409f, 0.819f, 0.378f, 0.141f),
    new Quaternion(0.042f, 0.652f, -0.410f, 0.637f),
    new Quaternion(-0.058f, -0.130f, 0.468f, 0.872f),
    new Quaternion(-0.081f, -0.103f, 0.719f, 0.683f),
    new Quaternion(-0.251f, 0.014f, 0.682f, 0.687f),
    new Quaternion(-0.610f, 0.386f, 0.384f, 0.576f),
    new Quaternion(-0.692f, 0.499f, 0.169f, 0.493f),
    new Quaternion(-0.700f, 0.600f, -0.081f, 0.379f)
  };

  void Start () {
    GemManager.Instance.Connect();
    // for (int i = 0; i < gemCount; i++){
    //   gem[i] = GemManager.Instance.GetGem(i);
    // }
    gem[0] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:DD");
    gem[1] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:E6");
    //gem[2] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:3A");
    //gem[3] =  GemManager.Instance.GetGem("D0:B5:C2:90:7C:69");
    //gem[4] =  GemManager.Instance.GetGem("D0:B5:C2:90:7C:4D");
    //gem[5] =  GemManager.Instance.GetGem("D0:B5:C2:90:7E:2F");




    GameObject[] corner = GameObject.FindGameObjectsWithTag("corner").OrderBy(c => int.Parse(c.name)).ToArray();
    for (int i = 0; i < corner.Length; i++){
      this.corner[i] = corner[i].GetComponent<Piece>();
    }
    GameObject[] edge = GameObject.FindGameObjectsWithTag("edge").OrderBy(c => int.Parse(c.name)).ToArray();
    for (int i = 0; i < edge.Length; i++){
      this.edge[i] = edge[i].GetComponent<Piece>();
    }
    GameObject[] center = GameObject.FindGameObjectsWithTag("center").OrderBy(c => int.Parse(c.name)).ToArray();
    for (int i = 0; i < center.Length; i++){
      this.center[i] = center[i].GetComponent<Piece>();
    }
    resetAll();
  }

  void FixedUpdate () {
    if (Input.GetMouseButton(0)){
      resetAll();
    }
    //else if (allGemsConnected()){
    else {

      rotateCube();
      if(smallChange){
        stateText[0].text = moves;
        //stateText[0].text = Quaternion.Angle(Quaternion.identity, cubeRotation).ToString("#.0");
      }
      firstRun = false;
      for (int i = 0; i < gemCount; i++){
        stateText[i+1].text = sideOrder[i] + ": " + gem[i].State + ": " + (angleCounter[i]).ToString("#.0");
        //stateText[i+1].text = sideOrder[i] + ": " + gemIsConnected[i].ToString() + ": " + (angleCounter[i]).ToString("#.0");
      }
    }
  }
  void resetAll(){
    moves = "";
    firstRun = true;

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

    sideOrientation[0] = Quaternion.identity;
    sideOrientation[1] = Quaternion.AngleAxis(90, axisNorm[1]);
    sideOrientation[2] = Quaternion.AngleAxis(90, axisNorm[2]);
    sideOrientation[3] = Quaternion.AngleAxis(90, axisNorm[3]);
    sideOrientation[4] = Quaternion.AngleAxis(90, axisNorm[4]);
    sideOrientation[5] = Quaternion.AngleAxis(180, axisNorm[5]);

    setUpCenters();
    setUpCorners();
    setUpEdges();

    for (int i = 0; i < gemCount; i++){
      needsUpdate[i] = false;
      clockwiseDirection[i] = true;
      gemIsConnected[i] = false;
      //calibrateFixer[i] = 0;
      stabilizers[i] = Quaternion.identity;

      //change here
      if(originRotate){
        gem[i].CalibrateOrigin();
        currentState[i] =  Quaternion.Inverse(sideOrientation[i]) * gem[i].Rotation * sideOrientation[i];
      }
      else{
        //gem[i].CalibrateAzimuth();
        currentState[i] = gem[i].Rotation * sideOrientation[i];
      }
      stabilizers[i] = stabilizer(i);
      currentState[i] = stabilizers[i] * currentState[i];

      angleCounter[i] = 0;
      spinFixer[i] = 0;

    }

    reset = true;
    rotateCube();
    reset = false;
  }

  void rotateCube(){
    if(reset){
      cubeRotation = Quaternion.identity;
      //prevCubeRotation = Quaternion.identity;
    }
    else {
      for (int i = 0; i < gemCount; i++){
        if(originRotate){
          currentState[i] =  Quaternion.Inverse(sideOrientation[i]) * gem[i].Rotation * sideOrientation[i];
        }
        else{
          currentState[i] = gem[i].Rotation * sideOrientation[i];
        }
        currentState[i] = stabilizers[i] * currentState[i];

      }
      prevCubeRotation = cubeRotation;
      checkConnections();
      getCubeRotation();
      smallChange = true;
      if(Quaternion.Angle(prevCubeRotation, cubeRotation) > 50){
        cubeRotation = prevCubeRotation;
        smallChange = false;
        stateText[0].text = "Connecting, please wait...   ";
      }
      else{
        for(int i = 0; i< gemCount; i++){
          currentState[i] = currentState[i] * Quaternion.FromToRotation(currentState[i] * axis[i], cubeRotation * axis[i]);

          needsUpdate[i] = updateDecider(i);
        }
      }
    }

    if(fullyConnected() || wasFullyConnected || true){
      resticker();
      for(int i = 0; i < gemCount; i++){
        if(useAngleMethod){
          getLayer(i);
          float angle = angleCounter[i] + spinFixer[i];
          //float range = 6 + (20 * (Quaternion.Angle(Quaternion.identity, cubeRotation)/180));
          if(ignoreUpdate(angleCounter[i], 19)){
            angle = (angle + 360) % 360;
            angle = Mathf.Round(angle/90)*90;
            angle = (angle + 360) % 360;
          }
          foreach (Piece c in animateUs) {
            c.transform.rotation = Quaternion.AngleAxis(angle, cubeRotation * axis[i]) * c.transform.rotation;
            //c.transform.RotateAround(Vector3.zero, cubeRotation * axis[i], angle);
          }
        }
        else{
          annimateLayer(i);
        }
      }
      for(int i = 0; i < gemCount; i++){
        doSpin(i);
      }
    }
    else{
      stateText[0].text = "Connecting, please wait...   ";
    }


  }

  Quaternion stabilizer(int i){
    return  Quaternion.Inverse(Quaternion.LookRotation(
             currentState[i] * Vector3.forward,
             currentState[i] * Vector3.up));

    //return Quaternion.identity;
  }

  void resticker(){
    for (int i = 0; i < 6; i++){
      center[i].transform.rotation = cubeRotation * centerPermutation[i];
      //center[i].transform.rotation = centerPermutation[i];

    }
    for (int i = 0; i < 12; i++){
      edge[i].transform.rotation = cubeRotation * edgePermutation[i];
      //edge[i].transform.rotation = edgePermutation[i];

    }
    for (int i = 0; i < 8; i++){
      corner[i].transform.rotation = cubeRotation * cornerPermutation[i];
      //corner[i].transform.rotation = cornerPermutation[i];

    }
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
        float lengthD = 1.0f / (w*w + x*x + y*y + z*z);
        w *= lengthD;
        x *= lengthD;
        y *= lengthD;
        z *= lengthD;

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
  }

  //Changes the sign of the quaternion components. This is not the same as the inverse.
  Quaternion checkAndFixQuaternion(Quaternion newQ, Quaternion firstQ){
    float dot = Quaternion.Dot(newQ, firstQ);
    if(dot < 0.0f){
      return new Quaternion(-newQ.x, -newQ.y, -newQ.z, -newQ.w);
    }
    return newQ;
  }

  void checkConnections(){
    for (int i = 0; i < gemCount; i++){
      if(gem[i].State == GemState.Connected){
        gemIsConnected[i] = true;
      }
      else{
        gemIsConnected[i] = false;
      }
    }
  }

  bool fullyConnected(){
    for (int i = 0; i < gemCount; i++){
      if(gem[i].State != GemState.Connected){
        return false;
      }
    }
    wasFullyConnected = true;
    return true;
  }

  void doSpin(int i) {

    // if(needsUpdate[i] && ignoreUpdate(angleCounter[i])){
    //   stabilizers[i] = Quaternion.AngleAxis(angleCounter[i], axis[i]) * stabilizer(i);
    //   currentState[i] = stabilizers[i] * currentState[i];
    // }

    if(needsUpdate[i] && !ignoreUpdate(angleCounter[i], 15)){
      updateLogic(i, clockwiseDirection[i]);
      doMove(i);
    }
    needsUpdate[i] = false;
  }

  void  doMove(int layerIndex){
    float angle = 90;
    if(!clockwiseDirection[layerIndex]){
      angle *= -1;
    }

    spinFixer[layerIndex] -= angle;
    spinFixer[layerIndex] = Mathf.Round( spinFixer[layerIndex]/45  )*45;
    spinFixer[layerIndex] = (spinFixer[layerIndex] + 360) % 360;

    //stabilizers[layerIndex] = Quaternion.AngleAxis(angleCounter[layerIndex], axis[layerIndex])
    //    * stabilizer(layerIndex);
    //currentState[layerIndex] = stabilizers[layerIndex] * currentState[layerIndex];


    for (int i = 0; i < 6; i++){
      if(center[i].isOnFace(layerIndex)){
        centerPermutation[i] = Quaternion.AngleAxis(angle, axis[layerIndex]) * centerPermutation[i];
      }
    }
    for (int i = 0; i < 12; i++){
      if(edge[i].isOnFace(layerIndex)){
        edgePermutation[i] = Quaternion.AngleAxis(angle, axis[layerIndex]) * edgePermutation[i];
      }
    }
    for (int i = 0; i < 8; i++){
      if(corner[i].isOnFace(layerIndex)){
        cornerPermutation[i] = Quaternion.AngleAxis(angle, axis[layerIndex]) * cornerPermutation[i];
      }
    }
  }


  bool updateDecider(int i){
    bool needsUpdateNow = false;
    float angle = angleCounter[i];
    currentState[i] = checkAndFixQuaternion(currentState[i], cubeRotation);

    if(gemIsConnected[i]){
      Quaternion q = Quaternion.Inverse(cubeRotation) * currentState[i];
      //angleCounter[i] = Vector3.Angle(q * axisNorm[i], axisNorm[i]);
      //angleCounter[i] *= -angleSign(q * axisNorm[i], axisNorm[i], q * axis[i]);

      angleCounter[i] = Quaternion.Angle(cubeRotation, currentState[i]);
      angleCounter[i] *= angleSign(cubeRotation * axisNorm[i],currentState[i] * axisNorm[i],cubeRotation * axis[i]);

      angleCounter[i] = (angleCounter[i] + 360) % 360;

          //turn this off when getting bug data
      //angleCounter[i] -= bugFixAngle(i);
      //angleCounter[i] = (angleCounter[i] + 360) % 360;

      if(angleSignChanged(angle, angleCounter[i])){
        angleCounter[i] = 360 - angleCounter[i];
        angleCounter[i] = (angleCounter[i] + 360) % 360;
      }

      else if(angleIsTooBig(angle, angleCounter[i])){
        angleCounter[i] = angle;
      }

      if (angle % 90 < 45 && angleCounter[i] % 90 > 45){
        needsUpdateNow = true;
        clockwiseDirection[i] = true;
      }
      if (angle % 90 > 45 && angleCounter[i] % 90 < 45){
        needsUpdateNow = true;
        clockwiseDirection[i] = false;
      }

      // if(needsUpdateNow){
      //   firstRun = true;
      // }
    }

    return needsUpdateNow;
  }

  bool angleIsTooBig(float angle1, float angle2){
    float delta = Mathf.Max(angle1, angle2) - Mathf.Min(angle1, angle2);
    if (180 < delta) {
      delta = 360 - delta;
    }
    if(delta > 30){
      stateText[0].text = "Connecting, please wait...   ";
      return true;
    }
    //stateText[0].text = moves;
    return false;
  }

  bool angleSignChanged(float angle1, float angle2){
    float lowerBound = 5;
    float upperBound = 355;
    float min =  Mathf.Min(angle1, angle2);
    float max =  Mathf.Max(angle1, angle2);
    float sum = angle1 + angle2;
    sum = (sum + 360) % 360;
    if (min > lowerBound && max < upperBound && (sum > upperBound || sum < lowerBound )) {
      return true;
    }
    return false;
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

  void getLayer(int layerIndex) {
    animateUs[0] = center[layerIndex].GetComponent<Piece>();
    int animate= 1;
    foreach (Piece piece in corner) {
      if (piece.isOnFace(layerIndex)){
        animateUs[animate] = piece;
        animate++;
      }
    }
    foreach (Piece piece in edge) {
      if (piece.isOnFace(layerIndex)) {
        animateUs[animate] = piece;
        animate++;
      }
    }
  }

  void annimateLayer(int layerIndex) {
    center[layerIndex].transform.rotation =
      (Quaternion.Inverse(cubeRotation) * currentState[layerIndex])
      * center[layerIndex].transform.rotation;
      center[layerIndex].transform.RotateAround(Vector3.zero, axis[layerIndex], spinFixer[layerIndex]);
    for (int i = 0; i < 8; i++){
      if (corner[i].isOnFace(layerIndex)){
        corner[i].transform.rotation =
          (Quaternion.Inverse(cubeRotation) * currentState[layerIndex])
          * corner[i].transform.rotation;
          corner[i].transform.RotateAround(Vector3.zero, axis[layerIndex], spinFixer[layerIndex]);
      }
    }

    for (int i = 0; i < 12; i++){
      if (edge[i].isOnFace(layerIndex)){
        edge[i].transform.rotation =
          (Quaternion.Inverse(cubeRotation) * currentState[layerIndex])
           * edge[i].transform.rotation;
          edge[i].transform.RotateAround(Vector3.zero, axis[layerIndex], spinFixer[layerIndex]);
      }
    }
  }

  bool allGemsConnected(){
    for (int i = 0; i < gemCount; i++){
      //if (gem[i] == null){
      if (gem[i].State != GemState.Connected){
        return false;
      }
    }
    return true;
  }

  //TOFIX: find more effecent way to avoid false posite at quarter turn completions
  bool ignoreUpdate(float angle, float range){
    if(angle % 90 < range || angle % 90 > (90-range)){
      return true;
    }
    return false;
  }

  int angleSign (Vector3 v1, Vector3 v2, Vector3 normalVector){
    Vector3 crossProduct = Vector3.Cross(v1, v2);
    float dotProduct = (Vector3.Dot(crossProduct, normalVector));
    if (dotProduct > 0){
      return 1;
    }
    return -1;
  }

  void updateLogic(int layerToUpdate, bool clockwise){
    if (layerToUpdate == 0){
      updateLogicU(clockwise);
      if (clockwiseDirection[layerToUpdate]){
        moves += "U ";
      }
      else {
        moves += "U' ";
      }
    }
    if (layerToUpdate == 1){
      updateLogicL(clockwise);
      if (clockwiseDirection[layerToUpdate]){
        moves += "L ";
      }
      else {
        moves += "L' ";
      }
    }
    if (layerToUpdate == 2){
      updateLogicF(clockwise);
      if (clockwiseDirection[layerToUpdate]){
        moves += "F ";
      }
      else {
        moves += "F' ";
      }
    }
    if (layerToUpdate == 3){
      updateLogicR(clockwise);
      if (clockwiseDirection[layerToUpdate]){
        moves += "R ";
      }
      else {
        moves += "R' ";
      }
    }
    if (layerToUpdate == 4){
      updateLogicB(clockwise);
      if (clockwiseDirection[layerToUpdate]){
        moves += "B ";
      }
      else {
        moves += "B' ";
      }
    }
    if (layerToUpdate == 5){
      updateLogicD(clockwise);
      if (clockwiseDirection[layerToUpdate]){
        moves += "D ";
      }
      else {
        moves += "D' ";
      }
    }
  }

  void updateLogicU(bool clockwise) {
    foreach (Piece piece in corner) {
      if (piece.isOnFace(0)){
        if (clockwise){
          if (piece.isOnFace(1) && piece.isOnFace(4)) {
            piece.setFace(1,false);
            piece.setFace(3,true);
          }
          else if(piece.isOnFace(4) && piece.isOnFace(3)){
            piece.setFace(4,false);
            piece.setFace(2,true);
          }
          else if (piece.isOnFace(3) && piece.isOnFace(2)){
            piece.setFace(3,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(2) && piece.isOnFace(1)) {
            piece.setFace(2,false);
            piece.setFace(4,true);
          }
        }
        else{
          if (piece.isOnFace(1) && piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(2,true);
          }
          else if(piece.isOnFace(4) && piece.isOnFace(3)){
            piece.setFace(3,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(3) && piece.isOnFace(2)){
            piece.setFace(2,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(2) && piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(3,true);
          }
        }
      }
    }
    foreach (Piece piece in edge) {
      if (piece.isOnFace(0)) {
        if (clockwise){
          if (piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(3)) {
            piece.setFace(3,false);
            piece.setFace(2,true);
          }
          else if (piece.isOnFace(2)) {
            piece.setFace(2,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(4,true);
          }
        }
        else {
          if (piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(3)) {
            piece.setFace(3,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(2)) {
            piece.setFace(2,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(2,true);
          }
        }
      }
    }
  }
  void updateLogicL(bool clockwise) {
    foreach (Piece piece in corner) {
      if (piece.isOnFace(1)){
        if (clockwise){
          if (piece.isOnFace(4) && piece.isOnFace(0)) {
            piece.setFace(4,false);
            piece.setFace(2,true);
          }
          else if (piece.isOnFace(0) && piece.isOnFace(2)) {
            piece.setFace(0,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(2) && piece.isOnFace(5)) {
            piece.setFace(2,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(5) && piece.isOnFace(4)) {
            piece.setFace(5,false);
            piece.setFace(0,true);
          }
        }
        else{
          if (piece.isOnFace(4) && piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(0) && piece.isOnFace(2)) {
            piece.setFace(2,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(2) && piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(0,true);
          }
          else if (piece.isOnFace(5) && piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(2,true);
          }
        }
      }
    }
    foreach (Piece piece in edge) {
      if (piece.isOnFace(1)) {
        if (clockwise){
          if (piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(2,true);
          }
          else if (piece.isOnFace(2)) {
            piece.setFace(2,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(0,true);
          }
        }
        else {
          if (piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(2)) {
            piece.setFace(2,false);
            piece.setFace(0,true);
          }
          else if (piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(2,true);
          }
          else if (piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(5,true);
          }
        }
      }
    }
  }
  void updateLogicF(bool clockwise) {
    foreach (Piece piece in corner) {
      if (piece.isOnFace(2)){
        if (clockwise){
          if (piece.isOnFace(1) && piece.isOnFace(0)) {
            piece.setFace(1,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(0) && piece.isOnFace(3)) {
            piece.setFace(0,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(3) && piece.isOnFace(5)) {
            piece.setFace(3,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(5) && piece.isOnFace(1)) {
            piece.setFace(5,false);
            piece.setFace(0,true);
          }
        }
        else{
          if (piece.isOnFace(1) && piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(0) && piece.isOnFace(3)) {
            piece.setFace(3,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(3) && piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(0,true);
          }
          else if (piece.isOnFace(5) && piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(3,true);
          }
        }
      }
    }
    foreach (Piece piece in edge) {
      if (piece.isOnFace(2)) {
        if (clockwise){
          if (piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(3)) {
            piece.setFace(3,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(0,true);
          }
        }
        else {
          if (piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(3)) {
            piece.setFace(3,false);
            piece.setFace(0,true);
          }
          else if (piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(5,true);
          }
        }
      }
    }
  }
  void updateLogicR(bool clockwise) {
    foreach (Piece piece in corner) {
      if (piece.isOnFace(3)){
        if (clockwise){
          if (piece.isOnFace(2) && piece.isOnFace(0)) {
            piece.setFace(2,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(0) && piece.isOnFace(4)) {
            piece.setFace(0,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(4) && piece.isOnFace(5)) {
            piece.setFace(4,false);
            piece.setFace(2,true);
          }
          else if (piece.isOnFace(5) && piece.isOnFace(2)) {
            piece.setFace(5,false);
            piece.setFace(0,true);
          }
        }
        else{
          if (piece.isOnFace(2) && piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(0) && piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(2,true);
          }
          else if (piece.isOnFace(4) && piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(0,true);
          }
          else if (piece.isOnFace(5) && piece.isOnFace(2)) {
            piece.setFace(2,false);
            piece.setFace(4,true);
          }
        }
      }
    }
    foreach (Piece piece in edge) {
      if (piece.isOnFace(3)) {
        if (clockwise){
          if (piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(2,true);
          }
          else if (piece.isOnFace(2)) {
            piece.setFace(2,false);
            piece.setFace(0,true);
          }
        }
        else {
          if (piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(2,true);
          }
          else if (piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(0,true);
          }
          else if (piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(2)) {
            piece.setFace(2,false);
            piece.setFace(5,true);
          }
        }
      }
    }
  }
  void updateLogicB(bool clockwise) {
    foreach (Piece piece in corner) {
      if (piece.isOnFace(4)){
        if (clockwise){
          if (piece.isOnFace(3) && piece.isOnFace(0)) {
            piece.setFace(3,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(0) && piece.isOnFace(1)) {
            piece.setFace(0,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(1) && piece.isOnFace(5)) {
            piece.setFace(1,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(5) && piece.isOnFace(3)) {
            piece.setFace(5,false);
            piece.setFace(0,true);
          }
        }
        else{
          if (piece.isOnFace(3) && piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(0) && piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(1) && piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(0,true);
          }
          else if (piece.isOnFace(5) && piece.isOnFace(3)) {
            piece.setFace(3,false);
            piece.setFace(1,true);
          }
        }
      }
    }
    foreach (Piece piece in edge) {
      if (piece.isOnFace(4)) {
        if (clockwise){
          if (piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(5,true);
          }
          else if (piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(3)) {
            piece.setFace(3,false);
            piece.setFace(0,true);
          }
        }
        else {
          if (piece.isOnFace(0)) {
            piece.setFace(0,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(0,true);
          }
          else if (piece.isOnFace(5)) {
            piece.setFace(5,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(3)) {
            piece.setFace(3,false);
            piece.setFace(5,true);
          }
        }
      }
    }
  }
  void updateLogicD(bool clockwise) {
    foreach (Piece piece in corner) {
      if (piece.isOnFace(5)){
        if (clockwise){
          if (piece.isOnFace(1) && piece.isOnFace(2)) {
            piece.setFace(1,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(2) && piece.isOnFace(3)) {
            piece.setFace(2,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(3) && piece.isOnFace(4)) {
            piece.setFace(3,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(4) && piece.isOnFace(1)) {
            piece.setFace(4,false);
            piece.setFace(2,true);
          }
        }
        else{
          if (piece.isOnFace(1) && piece.isOnFace(2)) {
            piece.setFace(2,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(2) && piece.isOnFace(3)) {
            piece.setFace(3,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(3) && piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(2,true);
          }
          else if (piece.isOnFace(4) && piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(3,true);
          }
        }
      }
    }
    foreach (Piece piece in edge) {
      if (piece.isOnFace(5)) {
        if (clockwise){
          if (piece.isOnFace(2)) {
            piece.setFace(2,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(3)) {
            piece.setFace(3,false);
            piece.setFace(4,true);
          }
          else if (piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(2,true);
          }
        }
        else {
          if (piece.isOnFace(2)) {
            piece.setFace(2,false);
            piece.setFace(1,true);
          }
          else if (piece.isOnFace(3)) {
            piece.setFace(3,false);
            piece.setFace(2,true);
          }
          else if (piece.isOnFace(4)) {
            piece.setFace(4,false);
            piece.setFace(3,true);
          }
          else if (piece.isOnFace(1)) {
            piece.setFace(1,false);
            piece.setFace(4,true);
          }
        }
      }
    }
  }

  void setUpCorners() {
    foreach (Piece piece in corner) {
      piece.setAllToFalse();
    }
    corner[0].setFace(0,true); corner[0].setFace(4,true); corner[0].setFace(1,true);
    corner[1].setFace(0,true); corner[1].setFace(4,true); corner[1].setFace(3,true);
    corner[2].setFace(0,true); corner[2].setFace(2,true); corner[2].setFace(3,true);
    corner[3].setFace(0,true); corner[3].setFace(2,true); corner[3].setFace(1,true);
    corner[4].setFace(5,true); corner[4].setFace(2,true); corner[4].setFace(1,true);//start D layer
    corner[5].setFace(5,true); corner[5].setFace(2,true); corner[5].setFace(3,true);
    corner[6].setFace(5,true); corner[6].setFace(4,true); corner[6].setFace(3,true);
    corner[7].setFace(5,true); corner[7].setFace(4,true); corner[7].setFace(1,true);

    for (int i = 0; i < 8; i++){
      cornerPermutation[i] = Quaternion.identity;
    }
  }
  void setUpEdges() {
    foreach (Piece piece in edge) {
      piece.setAllToFalse();
    }
    edge[0].setFace(0,true); edge[0].setFace(4,true);//start U slice
    edge[1].setFace(0,true); edge[1].setFace(3,true);
    edge[2].setFace(0,true); edge[2].setFace(2,true);
    edge[3].setFace(0,true); edge[3].setFace(1,true);
    edge[4].setFace(1,true); edge[4].setFace(2,true);//start E slice
    edge[5].setFace(2,true); edge[5].setFace(3,true);
    edge[6].setFace(3,true); edge[6].setFace(4,true);
    edge[7].setFace(4,true); edge[7].setFace(1,true);
    edge[8].setFace(5,true); edge[8].setFace(2,true);//start D slice
    edge[9].setFace(5,true); edge[9].setFace(3,true);
    edge[10].setFace(5,true); edge[10].setFace(4,true);
    edge[11].setFace(5,true); edge[11].setFace(1,true);
    for (int i = 0; i < 12; i++){
      edgePermutation[i] = Quaternion.identity;
    }
  }
  void setUpCenters() {
    center[0].setFace(0,true);
    center[1].setFace(1,true);
    center[2].setFace(2,true);
    center[3].setFace(3,true);
    center[4].setFace(4,true);
    center[5].setFace(5,true);

    for (int i = 0; i < 6; i++){
      centerPermutation[i] = Quaternion.identity;
    }
  }

  void OnApplicationQuit(){
    GemManager.Instance.Disconnect();
  }
  //For Android to unbind Gem Service when the app is not in focus
  void OnApplicationPause(bool paused){
    if (Application.platform == RuntimePlatform.Android){
      if (paused){
        GemManager.Instance.Disconnect();
      }
      else{
        GemManager.Instance.Connect();
      }
    }
  }

}
