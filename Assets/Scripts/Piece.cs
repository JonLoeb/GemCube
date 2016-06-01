using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour {


  bool[] isOn = new bool[6];

  public void setAllToFalse(){
    for (int i = 0 ; i < 6; i++){
      isOn[i] = false;
    }

  }
  public bool isOnFace(int i){
    return isOn[i];
  }
  public void setFace(int i, bool value){
    isOn[i] = value;
  }



  // Update is called once per frame
  void Update () {
  }
}
