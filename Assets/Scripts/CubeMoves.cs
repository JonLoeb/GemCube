using UnityEngine;
using System.Collections;
using System.Linq;
using GemSDK.Unity;
using UnityEngine.UI;

//orgin
public class CubeMoves : MonoBehaviour {

  const int gemCount = 6;
  bool originRotate = false;
  bool useAngleMethod = true;
  bool firstRun = true;
  IGem[] gem = new IGem[gemCount];

  Quaternion[] currentState = new Quaternion[gemCount];
  Quaternion[] stabilizers = new Quaternion[gemCount];
  Quaternion cubeRotation = Quaternion.identity;
  Quaternion prevCubeRotation = Quaternion.identity;
  Quaternion[] rotationData = new Quaternion[12];
  bool smallChange = true;
  bool[] gemIsConnected = new bool[gemCount];
  public Transform cubeParent;
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

  void Start () {
    GemManager.Instance.Connect();
    // for (int i = 0; i < gemCount; i++){
    //   gem[i] = GemManager.Instance.GetGem(i);
    // }
    gem[0] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:DD");
    gem[1] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:E6");
    gem[2] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:3A");
    gem[3] =  GemManager.Instance.GetGem("D0:B5:C2:90:7C:69");
    gem[4] =  GemManager.Instance.GetGem("D0:B5:C2:90:7C:4D");
    gem[5] =  GemManager.Instance.GetGem("D0:B5:C2:90:7E:2F");




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
        gem[i].CalibrateAzimuth();
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
          if(ignoreUpdate(angleCounter[i], 26)){
            angle = (angle + 360) % 360;
            angle = Mathf.Round(angle/90)*90;
            angle = (angle + 360) % 360;
          }
          foreach (Piece c in animateUs) {
            //c.transform.rotation = Quaternion.AngleAxis(angle, cubeRotation * axis[i]) * c.transform.rotation;
            //c.transform.RotateAround(Vector3.zero, cubeRotation * axis[i], angle);
          }
        }
        else{
          annimateLayer(i);
        }
      }
      for(int i = 0; i < gemCount; i++){
        //doSpin(i);
      }
      //cubeParent.transform.rotation = cubeRotation;
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
      //if (gem[0].State == GemState.Connected && gem[1].State == GemState.Connected){
      if (gemIsConnected[0] && gemIsConnected[1]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[1] * axis[1],currentState[0] * axis[0])
        * Quaternion.AngleAxis(90, Vector3.up);
        count++;
      }
    }
    if (gemCount >= 3){
      //if (gem[0].State == GemState.Connected && gem[2].State == GemState.Connected){
      if (gemIsConnected[0] && gemIsConnected[2]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[2] * -axis[2], currentState[0] * axis[0]);
        count++;
      }

      //if (gem[1].State == GemState.Connected && gem[2].State == GemState.Connected){
      if (gemIsConnected[1] && gemIsConnected[2]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[2] * -axis[2], currentState[1] * axis[1])
        * Quaternion.AngleAxis(90, Vector3.back);
        count++;
      }
    }
    if (gemCount >= 4){
      //if (gem[0].State == GemState.Connected && gem[3].State == GemState.Connected){
      if (gemIsConnected[0] && gemIsConnected[3]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[3] * axis[3], currentState[0] * axis[0])
        * Quaternion.AngleAxis(-90, Vector3.up);
        count++;
      }

      //if (gem[2].State == GemState.Connected && gem[3].State == GemState.Connected){
      if (gemIsConnected[2] && gemIsConnected[3]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[2] * -axis[2], currentState[3] * axis[3])
        * Quaternion.AngleAxis(-90, Vector3.back);
        count++;
      }
    }
    if (gemCount >= 5){
      //if (gem[0].State == GemState.Connected && gem[4].State == GemState.Connected){
      if (gemIsConnected[0] && gemIsConnected[4]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[4] * axis[4], currentState[0] * axis[0]);
        count++;
      }

      //if (gem[1].State == GemState.Connected && gem[4].State == GemState.Connected){
      if (gemIsConnected[1] && gemIsConnected[4]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[4] * axis[4], currentState[1] * axis[1])
        * Quaternion.AngleAxis(90, Vector3.back);
        count++;
      }

      //if (gem[3].State == GemState.Connected && gem[4].State == GemState.Connected){
      if (gemIsConnected[3] && gemIsConnected[4]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[4] * axis[4], currentState[3] * axis[3])
        * Quaternion.AngleAxis(-90, Vector3.back);
        count++;
      }
    }
    if (gemCount == 6){
      //if (gem[1].State == GemState.Connected && gem[5].State == GemState.Connected){
      if (gemIsConnected[1] && gemIsConnected[5]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[1] * axis[1], currentState[5] * -axis[5])
        * Quaternion.AngleAxis(90, Vector3.up);
        count++;
      }

      //if (gem[2].State == GemState.Connected && gem[5].State == GemState.Connected){
      if (gemIsConnected[2] && gemIsConnected[5]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[2] * -axis[2], currentState[5] * -axis[5]);
        count++;
      }

      //if (gem[3].State == GemState.Connected && gem[5].State == GemState.Connected){
      if (gemIsConnected[3] && gemIsConnected[5]){
        rotationData[count] = Quaternion.LookRotation(
        currentState[3] * axis[3], currentState[5] * -axis[5])
        * Quaternion.AngleAxis(-90, Vector3.up);
        count++;
      }

      //if (gem[4].State == GemState.Connected && gem[5].State == GemState.Connected){
      if (gemIsConnected[4] && gemIsConnected[5]){
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

    if(gemIsConnected[i]){
      Quaternion q = Quaternion.Inverse(cubeRotation) * currentState[i];

      angleCounter[i] = Vector3.Angle(q * axisNorm[i], axisNorm[i]);

      angleCounter[i] *= angleSign(q * axisNorm[i], axisNorm[i], q * axis[i]);
      //angleCounter[i] *= angleSign(q * axisNorm[i], axisNorm[i], axis[i]);

      // angleCounter[i] = (angleCounter[i] + 360) % 360;

      if(!originRotate && firstRun){
        //calibrateFixer[i] = angleCounter[i];
          //calibrateFixer[i] = angleCounter[i] - Mathf.Round( angleCounter[i]/45  )*45;
      }
      //angleCounter[i] =  angleCounter[i] - calibrateFixer[i];
      angleCounter[i] = (angleCounter[i] + 360) % 360;

      if(angleIsTooBig(angle, angleCounter[i])){
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
    if(delta > 40){
      stateText[0].text = "Connecting, please wait...   ";
      return true;
    }
    //stateText[0].text = moves;
    return false;
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
      return -1;
    }
    return 1;
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
