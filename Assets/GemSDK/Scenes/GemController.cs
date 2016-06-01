using UnityEngine;
using System.Collections;
using System.Linq;
using GemSDK.Unity;
using UnityEngine.UI;

public class GemController : MonoBehaviour{

  const int gemCount = 6;
  IGem[] gem = new IGem[6];
  float[] angle = new float[gemCount];

  public Text stateText;
  public Text tableDataText;
  string quatData = "";
  string angleData = "";
  private bool isKeyPressed = false;

  Vector3[] axis = new Vector3[6];
  Vector3[] axisNorm = new Vector3[6];
  Quaternion[] sideOrientation = new Quaternion[6];
  Quaternion[] currentState = new Quaternion[gemCount];
  Quaternion[] stabalizer = new Quaternion[gemCount];
  Quaternion cubeRotation = Quaternion.identity;
  //Quaternion prevCubeRotation = Quaternion.identity;
  Quaternion[] rotationData = new Quaternion[12];

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
    {-6.3f, -5.5f, 4.1f, -3.1f, -1.8f, -5.3f}
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
    new Quaternion(-0.011f, 0.971f, 0.001f, 0.239f)
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

    sideOrientation[0] = Quaternion.identity;
    sideOrientation[1] = Quaternion.AngleAxis(90, axisNorm[1]);
    sideOrientation[2] = Quaternion.AngleAxis(90, axisNorm[2]);
    sideOrientation[3] = Quaternion.AngleAxis(90, axisNorm[3]);
    sideOrientation[4] = Quaternion.AngleAxis(90, axisNorm[4]);
    sideOrientation[5] = Quaternion.AngleAxis(180, axisNorm[5]);
  }

  void FixedUpdate(){



    if (gemsAreNotNull()){
      if(Input.GetKeyUp(KeyCode.Space)){
        isKeyPressed = false;
        tableDataText.text = tableText();
        System.IO.File.WriteAllText("../quatData.text", quatData);
        System.IO.File.WriteAllText("../angleData.text", angleData);
      }

      if (Input.GetMouseButton(0)){

        //calibrate gems
        for (int i = 0; i < gemCount; i++){
          gem[i].CalibrateAzimuth();
        }
        //Use instead of CalibrateAzimuth() to calibrate also tilt and elevation
        //gem.ColibrateOrigin();

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

      stateText.text = displayText() + cubeAngle.ToString("#.0");
    }
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

  void calculateAngles(){
    for (int i = 0; i < gemCount; i++){
      Quaternion q = Quaternion.Inverse(cubeRotation) * currentState[i];
      angle[i] = Vector3.Angle(q * axisNorm[i],  axisNorm[i]);
      angle[i] *= angleSign(q * axisNorm[i], axisNorm[i], q * axis[i]);
      //angle[i] *= angleSign(q * axisNorm[i], axisNorm[i], axis[i]);
      angle[i] = (angle[i] + 360) % 360;

      //turn this off when getting bug data
      //angle[i] -= bugFixAngle(i);
      //angle[i] = (angle[i] + 360) % 360;
    }
  }

  int angleSign (Vector3 v1, Vector3 v2, Vector3 normalVector){
    Vector3 crossProduct = Vector3.Cross(v1, v2);
    float dotProduct = (Vector3.Dot(crossProduct, normalVector));
    if (dotProduct > 0){
      return -1;
    }
    return 1;
  }

  string displayText(){
    string outputMe = "";
    for (int i = 0; i < gemCount; i++){
      outputMe += gem[i].State.ToString() + ": " + angle[i].ToString("#.0")
      + ":   " + getAngleError(angle[i], 0).ToString("#.0") + "\n";
    }
    return outputMe;
  }

  string tableText(){
    string outputMe = cubeRotation.ToString("#.00") + "\n{";
    quatData += "\n new Quaternion" + cubeRotation.ToString("#0.000") + ",";
    angleData += "\n {";
    for (int i = 0; i < gemCount; i++){
      angleData += getAngleError(angle[i], 0).ToString("#0.0") + "f, ";
      outputMe += getAngleError(angle[i], 0).ToString("#.0") + ", ";
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
      float ratio = 1 / (float)count;
      for (int i = 0; i < count; i++){
        //cubeRotation = Quaternion.Lerp(Quaternion.identity, rotationData[i], ratio) * cubeRotation;
        cubeRotation = Quaternion.Slerp(Quaternion.identity, rotationData[i], ratio) * cubeRotation;
      }
    }

    // cubeRotation = Quaternion.LookRotation(
    //   currentState[1] * axis[1],currentState[0] * axis[0])
    //   * Quaternion.AngleAxis(90, Vector3.up);
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
