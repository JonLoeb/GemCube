using UnityEngine;
using System.Collections;
using System.Linq;
using GemSDK.Unity;
using UnityEngine.UI;
using System;

public class CubeLogic : MonoBehaviour {

	const int gemCount = 6;
	IGem[] gem = new IGem[6];

	enum TurnDirection {Clockwise, CounterClockwise, NoTurn};

	Quaternion[] faceRotation = new Quaternion[gemCount];

	Quaternion cubeRotation = Quaternion.identity;

	static readonly Vector3[] axis = {Vector3.up, Vector3.left, Vector3.back, Vector3.right, Vector3.forward, Vector3.down};
	static readonly Vector3[] axisNorm = {Vector3.left, Vector3.back, Vector3.right, Vector3.forward, Vector3.left, Vector3.back};

	Quaternion[] sideOrientation = new Quaternion[6];
	Quaternion[] stabalizer = new Quaternion[gemCount];

	bool firstRun = true;

	Quaternion[] cornerPermutation = new Quaternion[8];
	Quaternion[] edgePermutation  = new Quaternion[12];
	Quaternion[] centerPermutation  = new Quaternion[6];



	public Transform[] corner = new Transform[8];
	public Transform[] edge = new Transform[12];
	public Transform[] center = new Transform[6];
	//Transform[] animateUs = new Transform[9];

	int[] cornerOrder = new int[] {0,1,2,3,4,5,6,7};
	int[] edgeOrder = new int[] {0,1,2,3,4,5,6,7,8,9,10,11};

	//CONVENTION: direction defined by new rotation
	private static readonly Quaternion[] cubeRotationTable = {
		Quaternion.LookRotation(Vector3.forward, Vector3.up),//This is equal to Quaternion.identity
		Quaternion.LookRotation(Vector3.left, Vector3.up), //y'
		Quaternion.LookRotation(Vector3.back, Vector3.up),//y2
		Quaternion.LookRotation(Vector3.right, Vector3.up),//y
		Quaternion.LookRotation(Vector3.forward, Vector3.left),//z'
		Quaternion.LookRotation(Vector3.forward, Vector3.down),//z2
		Quaternion.LookRotation(Vector3.forward, Vector3.right),//z
		Quaternion.LookRotation(Vector3.up, Vector3.back),//x'
		Quaternion.LookRotation(Vector3.back, Vector3.down),//x2
		Quaternion.LookRotation(Vector3.down, Vector3.forward),//x
		Quaternion.LookRotation(Vector3.up, Vector3.left),//z' x'
		Quaternion.LookRotation(Vector3.back, Vector3.left),//z y2
		Quaternion.LookRotation(Vector3.down, Vector3.left),//z' x'
		Quaternion.LookRotation(Vector3.left, Vector3.back),//z' y'
		Quaternion.LookRotation(Vector3.right, Vector3.back),//z y
		Quaternion.LookRotation(Vector3.down, Vector3.back),//z2 x
		Quaternion.LookRotation(Vector3.up, Vector3.right),//z x'
		Quaternion.LookRotation(Vector3.back, Vector3.right),//z' y2
		Quaternion.LookRotation(Vector3.down, Vector3.right),//z x
		Quaternion.LookRotation(Vector3.up, Vector3.forward),//z2 x'
		Quaternion.LookRotation(Vector3.left, Vector3.forward),//z y'
		Quaternion.LookRotation(Vector3.right, Vector3.forward),//z' y
		Quaternion.LookRotation(Vector3.left, Vector3.down),//z2 y'
		Quaternion.LookRotation(Vector3.right, Vector3.down)//z2 y
	};

	string[] cubeRotationText = new string[] {
		"",
		"y' ",
		"y2 ",
		"y ",
		"z' ",
		"z2 ",
		"z ",
		"x' ",
		"x2 ",
		"x ",
		"z' x' ",
		"z y2 ",
		"z' x ",
		"z' y' ",
		"z y ",
		"z2 x ",
		"z x' ",
		"z y2 ",
		"z x ",
		"z2 x' ",
		"z y' ",
		"z' y ",
		"z2 y' ",
		"z2 y "
	};

	private static readonly char[,] sideOrderTable = {
		{'U', 'L', 'F', 'R', 'B', 'D'}, //NO MOVE
		{'U', 'B', 'L', 'F', 'R', 'D'}, //y'
		{'U', 'R', 'B', 'L', 'F', 'D'}, //y2
		{'U', 'F', 'R', 'B', 'L', 'D'}, //y
		{'R', 'U', 'F', 'D', 'B', 'L'}, //z'
		{'D', 'R', 'F', 'L', 'B', 'U'}, //z2
		{'L', 'D', 'F', 'U', 'B', 'R'}, //z
		{'B', 'L', 'U', 'R', 'D', 'F'}, //x'
		{'D', 'L', 'B', 'R', 'F', 'U'}, //x2
		{'F', 'L', 'D', 'R', 'U', 'B'}, //x
		{'B', 'U', 'R', 'D', 'L', 'F'}, //z' x'
		{'L', 'U', 'B', 'D', 'F', 'R'}, //z y2
		{'F', 'U', 'L', 'D', 'R', 'B'}, //z' x
		{'R', 'B', 'U', 'F', 'D', 'L'}, //z' y'
		{'L', 'F', 'U', 'B', 'D', 'R'}, //z y
		{'F', 'R', 'U', 'L', 'D', 'B'}, //z2 x
		{'B', 'D', 'L', 'U', 'R', 'F'}, //z x'
		{'R', 'D', 'B', 'U', 'F', 'L'}, //z' y2
		{'F', 'D', 'R', 'U', 'L', 'B'}, //z x
		{'B', 'R', 'D', 'L', 'U', 'F'}, //z2 x'
		{'L', 'B', 'D', 'F', 'U', 'R'}, //z y'
		{'R', 'F', 'D', 'B', 'U', 'L'}, //z' y
		{'D', 'B', 'R', 'F', 'L', 'U'}, //z2 y'
		{'D', 'F', 'L', 'B', 'R', 'U'} //z2 y
	};



	public Text[] layerText = new Text[gemCount];
	public Text stateText;

	float[] angleCounter = new float[gemCount];
	float[] prevAngleCounter = new float[gemCount];
	float[] spinFixer = new float[gemCount];

	string moves = "";
	char[] sideOrder = new char[] {'U', 'L', 'F', 'R', 'B', 'D'};

	//runs once at start of program
	void Start () {

		GemManager.Instance.Connect();
		// for (int i = 0; i < gemCount; i++){
		//   gem[i] = GemManager.Instance.GetGem(i);
		// }
		gem[3] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:DD");//white
		gem[1] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:E6");//orange
		gem[2] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:3A");//green
		gem[0] =  GemManager.Instance.GetGem("D0:B5:C2:90:78:E4");//red
		gem[4] =  GemManager.Instance.GetGem("D0:B5:C2:90:7C:4D");//blue
		gem[5] =  GemManager.Instance.GetGem("98:7B:F3:5A:5C:6D");//yellow



		if(gemsAreNotNull()){
			resetAll();
		}


	}

	// Update is called once per frame
	void FixedUpdate () {
		if(gemsAreNotNull()){
			if (Input.GetMouseButton(0)){
				resetAll();
			}
			else {

				rotateCube(false);
				moves = useSlices(moves);
				//moves = useWideTurns(moves);
				stateText.text = moves;

				string[] sideColor = new string[] {"White", "Orange", "Green", "Red", "Blue", "Yellow"};
				for (int i = 0; i < gemCount; i++){
					layerText[i].text = gem[i].State + ": " + (angleCounter[i]).ToString("#.0");
					if (gem[i].State == GemState.Connected){
						layerText[i].text = sideColor[i] + ": " + (angleCounter[i]).ToString("#.0") + '°';
					}
				}
			}
		}
	}

	//resets all variables to make cube solved
	void resetAll(){
		moves = "";
		firstRun = true;


		sideOrientation[0] = Quaternion.identity;
		sideOrientation[1] = Quaternion.AngleAxis(90, axisNorm[1]);
		sideOrientation[2] = Quaternion.AngleAxis(90, axisNorm[2]);
		sideOrientation[3] = Quaternion.AngleAxis(90, axisNorm[3]);
		sideOrientation[4] = Quaternion.AngleAxis(90, axisNorm[4]);
		sideOrientation[5] = Quaternion.AngleAxis(180, axisNorm[5]);

		//resetCenters();
		//resetCorners();
		//resetEdges();
		cornerOrder = new int[] {0,1,2,3,4,5,6,7};
		edgeOrder = new int[] {0,1,2,3,4,5,6,7,8,9,10,11};
		sideOrder = new char[] {'U', 'L', 'F', 'R', 'B', 'D'};
		for (int i = 0; i < 6; i++){
			centerPermutation[i] = Quaternion.identity;
		}
		for (int i = 0; i < 12; i++){
			edgePermutation[i] = Quaternion.identity;
		}
		for (int i = 0; i < 8; i++){
			cornerPermutation[i] = Quaternion.identity;
		}


		for (int i = 0; i < gemCount; i++){

			gem[i].CalibrateAzimuth();
			faceRotation[i] = gem[i].Rotation * sideOrientation[i];


			prevAngleCounter[i] = 0;
			angleCounter[i] = 0;
			spinFixer[i] = 0;

		}
		calculateStabalizers();
		stabalizeGems();

		cubeRotation = Quaternion.identity;
		rotateCube(true);
	}



	//gets data from gems and calls other methods to make the cube move
	void rotateCube (bool reset){
		//cubeRotation = Quaternion.identity;

		if(!reset){
			for (int i = 0; i < gemCount; i++){
				faceRotation[i] = gem[i].Rotation * sideOrientation[i];
			}
			stabalizeGems();

		}
		Quaternion prevCubeRotation = cubeRotation;

		//checkConnections();
		cubeRotation = getCubeRotation();
		if(Quaternion.Angle(prevCubeRotation, cubeRotation) > 50){
			cubeRotation = prevCubeRotation;
		}
		else{
			for(int i = 0; i< gemCount; i++){
				matchRotationToCube(i);

				prevAngleCounter[i] = angleCounter[i];
				getAngle(i);
			}
		}

		//if ((allGemsConnected() || true) && !reset){
		if (allGemsConnected() && !reset){


			//calibrates the gems after they are all connected
			if(firstRun){
				resetAll();
				firstRun = false;
			}

			resticker();
			transform.rotation = cubeRotation;
			for(int i = 0; i < gemCount; i++){
				rotateSide(i);
			}
			moves += printCubeRotations(prevCubeRotation);

			for(int i = 0; i < gemCount; i++){
				doSpin(i);
			}
		}

	}

	//rotates side i of the cube by the angle it calculates (rounds to 90 when close to 90)
	void rotateSide(int i){
		Transform[] animateUs = getLayer(i);
		float angle = angleCounter[i] + spinFixer[i];
		//float range = 6 + (20 * (Quaternion.Angle(Quaternion.identity, cubeRotation)/180));
		if(ignoreUpdate(angleCounter[i], 24)){
			angle = (angle + 360) % 360;


			// If angle is close to 90 make it 90
			angle = Mathf.Round(angle/90)*90;
			angle = (angle + 360) % 360;
		}
		foreach (Transform c in animateUs) {

			//c.rotation = Quaternion.AngleAxis(angle, cubeRotation * axis[i]) * c.rotation;
			c.localRotation = Quaternion.AngleAxis(angle, axis[i]) * c.localRotation;

			//c.transform.RotateAround(Vector3.zero, cubeRotation * axis[i], angle);
		}
	}

	//after all sides are rotated doSpin is called to update the logic of where peices are,
	//and to update the start configuration for what the cube looks like before rotateSide is called
	void doSpin(int i) {

		//if(needsUpdate[i] && !ignoreUpdate(angleCounter[i], 2)){
		TurnDirection t = updateDirection(	prevAngleCounter[i], 	angleCounter[i]);
		if(t != TurnDirection.NoTurn){
			updateLogic(i, t == TurnDirection.Clockwise);
			doMove(i, t == TurnDirection.Clockwise);
		}
	}

	//updates the quaternion for each pieces start rotation before a side rotation is done.
	//Bascially it generates the data for restickering
	void  doMove(int layerIndex, bool isClockwise){
		float angle = 90;
		if(!isClockwise){
			angle *= -1;
		}

		spinFixer[layerIndex] -= angle;
		spinFixer[layerIndex] = Mathf.Round( spinFixer[layerIndex]/45  )*45;
		spinFixer[layerIndex] = (spinFixer[layerIndex] + 360) % 360;

		//stabilizers[layerIndex] = Quaternion.AngleAxis(angleCounter[layerIndex], axis[layerIndex])
		//    * stabilizer(layerIndex);
		//faceRotation[layerIndex] = stabilizers[layerIndex] * faceRotation[layerIndex];



		centerPermutation[layerIndex] = Quaternion.AngleAxis(angle, axis[layerIndex]) * centerPermutation[layerIndex];

		for (int i = 0; i < 12; i++){
			if(edgeIsOnLayer(i,layerIndex)){
				edgePermutation[i] = Quaternion.AngleAxis(angle, axis[layerIndex]) * edgePermutation[i];
			}
		}
		for (int i = 0; i < 8; i++){
			if(cornerIsOnLayer(i,layerIndex)){
				cornerPermutation[i] = Quaternion.AngleAxis(angle, axis[layerIndex]) * cornerPermutation[i];
			}
		}
	}

	void reassignFaceLetters(int currentRotationIndex){
		for (int i = 0; i < sideOrder.Length; i++){
			sideOrder[i] = sideOrderTable[currentRotationIndex, i];
		}
	}

	string printCubeRotations(Quaternion prevCubeRotation){
		Quaternion closestRotationToCubeRotation =  cubeRotationTable[nearestCubeRotationIndex(cubeRotation)];
		Quaternion relativeRotation =	closestRotationToCubeRotation * Quaternion.Inverse(prevCubeRotation);
		string rotationText = cubeRotationText[nearestCubeRotationIndex(relativeRotation)];
		reassignFaceLetters(nearestCubeRotationIndex(Quaternion.Inverse(cubeRotation)));

		return rotationText;
	}

	string useSlices(string s){
		//M Moves
		s = s.Replace("x R' L", "M'");
		s = s.Replace("x L R'", "M'");
		s = s.Replace("R' x L", "M'");
		s = s.Replace("R' L x", "M'");
		s = s.Replace("L R' x", "M'");
		s = s.Replace("L x R'", "M'");

		s = s.Replace("x' R L'", "M");
		s = s.Replace("x' L' R", "M");
		s = s.Replace("R x' L'", "M");
		s = s.Replace("R L' x'", "M");
		s = s.Replace("L' R x'", "M");
		s = s.Replace("L' x' R", "M");

		//E Moves
		s = s.Replace("y' U D'", "E");
		s = s.Replace("y' D' U", "E");
		s = s.Replace("U y' D'", "E");
		s = s.Replace("U D' y'", "E");
		s = s.Replace("D' U y'", "E");
		s = s.Replace("D' y' U", "E");

		s = s.Replace("y U' D", "E'");
		s = s.Replace("y D U'", "E'");
		s = s.Replace("U' y D", "E'");
		s = s.Replace("U' D y", "E'");
		s = s.Replace("D U' y", "E'");
		s = s.Replace("D y U'", "E'");

		//S Moves
		s = s.Replace("z F' B", "S");
		s = s.Replace("z B F'", "S");
		s = s.Replace("F' z B", "S");
		s = s.Replace("F' B z", "S");
		s = s.Replace("B F' z", "S");
		s = s.Replace("B z F'", "S");

		s = s.Replace("z' F B'", "S'");
		s = s.Replace("z' B' F", "S'");
		s = s.Replace("F z' B'", "S'");
		s = s.Replace("F B' z'", "S'");
		s = s.Replace("B' F z'", "S'");
		s = s.Replace("B' z' F", "S'");

		return s;
	}

	string useWideTurns(string s){
		//u Moves
		s = s.Replace("y D", "u");
		s = s.Replace("U y", "u");

		//l Moves
		s = s.Replace("x' R", "l");
		s = s.Replace("R x'", "l");

		//f Moves
		s = s.Replace("z B", "f");
		s = s.Replace("B z", "f");

		//r Moves
		s = s.Replace("x L", "r");
		s = s.Replace("L x", "r");

		//b Moves
		s = s.Replace("z' F", "b");
		s = s.Replace("F z'", "b");

		//d Moves
		s = s.Replace("y' U", "d");
		s = s.Replace("U y'", "d");

		return s;
	}

	int nearestCubeRotationIndex(Quaternion rotation){
		int index = 0;
		float closestDistance = Quaternion.Angle(rotation, cubeRotationTable[0]);

		for (int i = 1; i < cubeRotationTable.Length; i++){
			float distance = Quaternion.Angle(rotation, cubeRotationTable[i]);
			if (distance < closestDistance){
				closestDistance = distance;
				index = i;
			}
		}
		return index;
	}


	bool cornerIsOnLayer(int cornerIndex, int layerIndex){
		if(layerIndex == 0){
			if (cornerOrder[0] == cornerIndex || cornerOrder[1] == cornerIndex || cornerOrder[2] == cornerIndex || cornerOrder[3] == cornerIndex){
				return true;
			}
		}
		else if(layerIndex == 1){
			if (cornerOrder[0] == cornerIndex || cornerOrder[3] == cornerIndex || cornerOrder[4] == cornerIndex || cornerOrder[7] == cornerIndex){
				return true;
			}
		}
		else if(layerIndex == 2){
			if (cornerOrder[3] == cornerIndex || cornerOrder[2] == cornerIndex || cornerOrder[5] == cornerIndex || cornerOrder[4] == cornerIndex){
				return true;
			}
		}
		else if(layerIndex == 3){
			if (cornerOrder[2] == cornerIndex || cornerOrder[1] == cornerIndex || cornerOrder[6] == cornerIndex || cornerOrder[5] == cornerIndex){
				return true;
			}
		}
		else if(layerIndex == 4){
			if (cornerOrder[1] == cornerIndex || cornerOrder[0] == cornerIndex || cornerOrder[7] == cornerIndex || cornerOrder[6] == cornerIndex){
				return true;
			}
		}
		else if(layerIndex == 5){
			if (cornerOrder[4] == cornerIndex || cornerOrder[5] == cornerIndex || cornerOrder[6] == cornerIndex || cornerOrder[7] == cornerIndex){
				return true;
			}
		}

		return false;
	}
	bool edgeIsOnLayer(int edgeIndex, int layerIndex){
		if(layerIndex == 0){
			if (edgeOrder[0] == edgeIndex || edgeOrder[1] == edgeIndex || edgeOrder[2] == edgeIndex || edgeOrder[3] == edgeIndex){
				return true;
			}
		}
		else if(layerIndex == 1){
			if (edgeOrder[3] == edgeIndex || edgeOrder[4] == edgeIndex || edgeOrder[11] == edgeIndex || edgeOrder[7] == edgeIndex){
				return true;
			}
		}
		else if(layerIndex == 2){
			if (edgeOrder[2] == edgeIndex || edgeOrder[5] == edgeIndex || edgeOrder[8] == edgeIndex || edgeOrder[4] == edgeIndex){
				return true;
			}
		}
		else if(layerIndex == 3){
			if (edgeOrder[1] == edgeIndex || edgeOrder[6] == edgeIndex || edgeOrder[9] == edgeIndex || edgeOrder[5] == edgeIndex){
				return true;
			}
		}
		else if(layerIndex == 4){
			if (edgeOrder[0] == edgeIndex || edgeOrder[7] == edgeIndex || edgeOrder[10] == edgeIndex || edgeOrder[6] == edgeIndex){
				return true;
			}
		}
		else if(layerIndex == 5){
			if (edgeOrder[8] == edgeIndex || edgeOrder[9] == edgeIndex || edgeOrder[10] == edgeIndex || edgeOrder[11] == edgeIndex){
				return true;
			}
		}

		return false;
	}

	//if a move is done (side rotated past 45 degrees) this will print the move to a string,
	//and then call updateLogic for that side so each peice now knows which new faces it is touching
	void updateLogic(int layerToUpdate, bool isClockwise){
		moves += sideOrder[layerToUpdate];
		int stop = 1;
		if(isClockwise){
			moves += " ";
		}
		else{
			moves += "' ";
			stop = 3;
		}

		//calling clockwise 3x is the same as doing counterclockwise
		for (int i = 0; i< stop; i++){
			if (layerToUpdate == 0){
				updateLogicU();
			}
			if (layerToUpdate == 1){
				updateLogicL();

			}
			if (layerToUpdate == 2){
				updateLogicF();

			}
			if (layerToUpdate == 3){
				updateLogicR();

			}
			if (layerToUpdate == 4){
				updateLogicB();

			}
			if (layerToUpdate == 5){
				updateLogicD();
			}
		}
	}

	void updateLogicU(){
		//U Layer
		//corners: 0, 1, 2, 3
		//edges: 0, 1, 2, 3

		int temp = cornerOrder[0];
		cornerOrder[0] = cornerOrder[3];
		cornerOrder[3] = cornerOrder[2];
		cornerOrder[2] = cornerOrder[1];
		cornerOrder[1] = temp;

		temp = edgeOrder[0];
		edgeOrder[0] = edgeOrder[3];
		edgeOrder[3] = edgeOrder[2];
		edgeOrder[2] = edgeOrder[1];
		edgeOrder[1] = temp;
	}
	void updateLogicL(){
		//L Layer
		//corners: 0, 3, 4, 7
		//edges: 3, 4, 11, 7

		int temp = cornerOrder[0];
		cornerOrder[0] = cornerOrder[7];
		cornerOrder[7] = cornerOrder[4];
		cornerOrder[4] = cornerOrder[3];
		cornerOrder[3] = temp;

		temp = edgeOrder[3];
		edgeOrder[3] = edgeOrder[7];
		edgeOrder[7] = edgeOrder[11];
		edgeOrder[11] = edgeOrder[4];
		edgeOrder[4] = temp;
	}
	void updateLogicF(){
		//F Layer
		//corners: 3, 2, 5, 4
		//edges: 2, 5, 8, 4

		int temp = cornerOrder[3];
		cornerOrder[3] = cornerOrder[4];
		cornerOrder[4] = cornerOrder[5];
		cornerOrder[5] = cornerOrder[2];
		cornerOrder[2] = temp;

		temp = edgeOrder[2];
		edgeOrder[2] = edgeOrder[4];
		edgeOrder[4] = edgeOrder[8];
		edgeOrder[8] = edgeOrder[5];
		edgeOrder[5] = temp;
	}
	void updateLogicR(){
		//R Layer
		//corners: 2, 1, 6, 5
		//edges: 1, 6, 9, 5

		int temp = cornerOrder[2];
		cornerOrder[2] = cornerOrder[5];
		cornerOrder[5] = cornerOrder[6];
		cornerOrder[6] = cornerOrder[1];
		cornerOrder[1] = temp;

		temp = edgeOrder[1];
		edgeOrder[1] = edgeOrder[5];
		edgeOrder[5] = edgeOrder[9];
		edgeOrder[9] = edgeOrder[6];
		edgeOrder[6] = temp;
	}
	void updateLogicB(){
		//B Layer
		//corners: 1, 0, 7, 6
		//edges: 0, 7, 10, 6

		int temp = cornerOrder[1];
		cornerOrder[1] = cornerOrder[6];
		cornerOrder[6] = cornerOrder[7];
		cornerOrder[7] = cornerOrder[0];
		cornerOrder[0] = temp;

		temp = edgeOrder[0];
		edgeOrder[0] = edgeOrder[6];
		edgeOrder[6] = edgeOrder[10];
		edgeOrder[10] = edgeOrder[7];
		edgeOrder[7] = temp;
	}
	void updateLogicD(){
		//D Layer
		//corners: 4, 5, 6, 7
		//edges: 8, 9, 10, 11

		int temp = cornerOrder[4];
		cornerOrder[4] = cornerOrder[7];
		cornerOrder[7] = cornerOrder[6];
		cornerOrder[6] = cornerOrder[5];
		cornerOrder[5] = temp;

		temp = edgeOrder[8];
		edgeOrder[8] = edgeOrder[11];
		edgeOrder[11] = edgeOrder[10];
		edgeOrder[10] = edgeOrder[9];
		edgeOrder[9] = temp;
	}

	//returns an array of peices on a the same face to be animated when that face spins
	Transform[] getLayer(int layerIndex) {
		Transform[] animateUs = new Transform[9];
		animateUs[0] = center[layerIndex];

		if(layerIndex == 0){
			//U Layer
			//corners: 0, 1, 2, 3
			//edges: 0, 1, 2, 3

			animateUs[1] = corner[cornerOrder[0]];
			animateUs[2] = corner[cornerOrder[1]];
			animateUs[3] = corner[cornerOrder[2]];
			animateUs[4] = corner[cornerOrder[3]];

			animateUs[5] = edge[edgeOrder[0]];
			animateUs[6] = edge[edgeOrder[1]];
			animateUs[7] = edge[edgeOrder[2]];
			animateUs[8] = edge[edgeOrder[3]];

		}
		else if (layerIndex == 1){
			//L Layer
			//corners: 0, 3, 4, 7
			//edges: 3, 4, 11, 7

			animateUs[1] = corner[cornerOrder[0]];
			animateUs[2] = corner[cornerOrder[3]];
			animateUs[3] = corner[cornerOrder[4]];
			animateUs[4] = corner[cornerOrder[7]];

			animateUs[5] = edge[edgeOrder[3]];
			animateUs[6] = edge[edgeOrder[4]];
			animateUs[7] = edge[edgeOrder[11]];
			animateUs[8] = edge[edgeOrder[7]];

		}
		else if (layerIndex == 2){
			//F Layer
			//corners: 3, 2, 5, 4
			//edges: 2, 5, 8, 4

			animateUs[1] = corner[cornerOrder[3]];
			animateUs[2] = corner[cornerOrder[2]];
			animateUs[3] = corner[cornerOrder[5]];
			animateUs[4] = corner[cornerOrder[4]];

			animateUs[5] = edge[edgeOrder[2]];
			animateUs[6] = edge[edgeOrder[5]];
			animateUs[7] = edge[edgeOrder[8]];
			animateUs[8] = edge[edgeOrder[4]];

		}
		else if (layerIndex == 3){
			//R Layer
			//corners: 2, 1, 6, 5
			//edges: 1, 6, 9, 5

			animateUs[1] = corner[cornerOrder[2]];
			animateUs[2] = corner[cornerOrder[1]];
			animateUs[3] = corner[cornerOrder[6]];
			animateUs[4] = corner[cornerOrder[5]];

			animateUs[5] = edge[edgeOrder[1]];
			animateUs[6] = edge[edgeOrder[6]];
			animateUs[7] = edge[edgeOrder[9]];
			animateUs[8] = edge[edgeOrder[5]];

		}
		else if (layerIndex == 4){
			//B Layer
			//corners: 1, 0, 7, 6
			//edges: 0, 7, 10, 6

			animateUs[1] = corner[cornerOrder[1]];
			animateUs[2] = corner[cornerOrder[0]];
			animateUs[3] = corner[cornerOrder[7]];
			animateUs[4] = corner[cornerOrder[6]];

			animateUs[5] = edge[edgeOrder[0]];
			animateUs[6] = edge[edgeOrder[7]];
			animateUs[7] = edge[edgeOrder[10]];
			animateUs[8] = edge[edgeOrder[6]];

		}
		else if (layerIndex == 5){
			//D Layer
			//corners: 4, 5, 6, 7
			//edges: 8, 9, 10, 11
			animateUs[1] = corner[cornerOrder[4]];
			animateUs[2] = corner[cornerOrder[5]];
			animateUs[3] = corner[cornerOrder[6]];
			animateUs[4] = corner[cornerOrder[7]];

			animateUs[5] = edge[edgeOrder[8]];
			animateUs[6] = edge[edgeOrder[9]];
			animateUs[7] = edge[edgeOrder[10]];
			animateUs[8] = edge[edgeOrder[11]];
		}

		return animateUs;
	}

	//before each side is rotated resticker is called to bring the rotate each peice to the side that it is on
	void resticker(){
		for (int i = 0; i < 6; i++){
			//center[i].rotation = cubeRotation * centerPermutation[i];
			center[i].localRotation = centerPermutation[i];

		}
		for (int i = 0; i < 12; i++){
			//edge[i].rotation = cubeRotation * edgePermutation[i];
			edge[i].localRotation = edgePermutation[i];

		}
		for (int i = 0; i < 8; i++){
			//corner[i].rotation = cubeRotation * cornerPermutation[i];
			corner[i].localRotation = cornerPermutation[i];

		}
	}

	Quaternion fixRounding (Quaternion a){
		float aL = Mathf.Sqrt(a.x*a.x + a.y*a.y + a.z*a.z + a.w*a.w);

		a.x /= aL;
		a.y /= aL;
		a.z /= aL;
		a.w /= aL;

		return a;
	}

	void matchRotationToCube(int i){
		//match state to cube
		Quaternion q = faceRotation[i] * Quaternion.FromToRotation(faceRotation[i] * axis[i], cubeRotation * axis[i]);
		//Quaternion q =  Quaternion.FromToRotation(faceRotation[i] * axis[i], cubeRotation * axis[i]) * faceRotation[i];

			faceRotation[i] = q;
		//faceRotation[i] = Quaternion.Slerp(faceRotation[i], q, 0.5f);
	}

	//sets angleCounter[i] = to the angle that the side is rotated with respect to the core of the cube
	void getAngle(int sideIndex){
		float angle = angleCounter[sideIndex];

		cubeRotation = fixRounding(cubeRotation);
		faceRotation[sideIndex] = fixRounding(faceRotation[sideIndex]);
		faceRotation[sideIndex] = checkAndFixQuaternion(faceRotation[sideIndex], cubeRotation);

		if(gem[sideIndex].State == GemState.Connected){
			//angleCounter[sideIndex] = Quaternion.Angle(cubeRotation, faceRotation[sideIndex]);
			//angleCounter[sideIndex] = Vector3.Angle(cubeRotation * axisNorm[sideIndex], faceRotation[sideIndex] * axisNorm[sideIndex]);
			//angleCounter[sideIndex] *= angleSign(cubeRotation * axisNorm[sideIndex], faceRotation[sideIndex] * axisNorm[sideIndex], cubeRotation * axis[sideIndex]);

			angleCounter[sideIndex] = AngleSigned(cubeRotation * axisNorm[sideIndex], faceRotation[sideIndex] * axisNorm[sideIndex], cubeRotation * axis[sideIndex]);

			angleCounter[sideIndex] = (angleCounter[sideIndex] + 360) % 360;

			//turn this off when getting bug data
			//angleCounter[sideIndex] -= bugFixAngle(sideIndex);
			//angleCounter[sideIndex] = (angleCounter[sideIndex] + 360) % 360;

				//angleCounter[sideIndex] = closestToLastAngle(angle, angleCounter[sideIndex]);

				// if(angleSignChanged(angle, angleCounter[sideIndex])){
				// 	angleCounter[sideIndex] = 360 - angleCounter[sideIndex];
				// 	angleCounter[sideIndex] = (angleCounter[sideIndex] + 360) % 360;
				// }

				//check for jump in angle
				if(angleIsTooBig(angle, angleCounter[sideIndex])){
					angleCounter[sideIndex] = angle;
				}


		}
	}

	//checks to see if the difference between two angles is greater than some threashhold (30)
	bool angleIsTooBig(float angle1, float angle2){
		float delta = Mathf.Max(angle1, angle2) - Mathf.Min(angle1, angle2);
		if (180 < delta) {
			delta = 360 - delta;
		}
		if(delta > 40){
			//stateText.text = "Connecting, please wait...   ";
			return true;
		}
		//stateText[0].text = moves;
		return false;
	}

	//checks to see if two angles angle1 is close to -angle2
	//Example (15 degrees is close to -345 degrees)
	bool angleSignChanged(float angle1, float angle2){
		//float delta = 0.01f;
		float delta = 0.2f;
		//float delta = 10.01f;

		float min =  Mathf.Min(angle1, angle2);
		float max =  Mathf.Max(angle1, angle2);
		float sum = angle1 + angle2;
		sum = (sum + 360) % 360;

		//FIX THIS!!!!!
		if (min > delta && min < (180 - delta) && max < (360 - delta) && max > (180 + delta)) {
			if  (sum > (360 - delta) ||  sum < delta ) {
				return true;
			}
		}
		return false;
	}

	float closestToLastAngle(float prevAngle, float currentAngle){
		float margin = 2.2f;

		float min =  Mathf.Min(prevAngle, currentAngle);
		float max =  Mathf.Max(prevAngle, currentAngle);
		float sum = prevAngle + currentAngle;
		sum = (sum + 360) % 360;

		float delta = max - min;
		if (180 < delta) {
			delta = 360 - delta;
		}

		if ((prevAngle < 180 && currentAngle > 180) || (prevAngle > 180 && currentAngle < 180)){
			if (min > margin && min < (180 - margin) && max < (360 - margin) && max > (180 + margin)) {
				float newAngle = 360 - currentAngle;
				newAngle = (newAngle + 360) % 360;

				float delta2 = Mathf.Max(prevAngle, newAngle) - Mathf.Min(prevAngle, newAngle);
				if (180 < delta2) {
					delta2 = 360 - delta2;
				}

				if(delta2 < delta){
					return newAngle;
				}


			}
		}

		return currentAngle;
	}

	//returns the direction a layer spin is done
	//returns "NoTurn" if layer spin does not pass 45 degrees
	TurnDirection updateDirection(float prevAngle, float currentAngle){
		TurnDirection result = TurnDirection.NoTurn;

		prevAngle = (prevAngle + 360) % 360;
		currentAngle = (currentAngle + 360) % 360;


		if (prevAngle % 90 < 45 && currentAngle % 90 >= 45){
			result = TurnDirection.Clockwise;
		}
		if (prevAngle % 90 > 45 && currentAngle % 90 <= 45){
			result = TurnDirection.CounterClockwise;
		}


		//do this to check if we are passing 90 and not 45, avoid false positives
		if(result != TurnDirection.NoTurn){
			if((prevAngle >= 0 && prevAngle <= 45 && currentAngle >= 315 && currentAngle <= 360) ||   (currentAngle >= 0 && currentAngle <= 45 && prevAngle >= 315 && prevAngle <= 360)){
				result = TurnDirection.NoTurn;
			}
			if((prevAngle >= 45 && prevAngle <= 90 && currentAngle >= 90 && currentAngle <= 135) ||   (currentAngle >= 45 && currentAngle <= 90 && prevAngle >= 90 && prevAngle <= 135)){
				result = TurnDirection.NoTurn;
			}
			if((prevAngle >= 135 && prevAngle <= 180 && currentAngle >= 180 && currentAngle <= 225) ||   (currentAngle >= 135 && currentAngle <= 180 && prevAngle >= 180 && prevAngle <= 225)){
				result = TurnDirection.NoTurn;
			}
			if((prevAngle >= 225 && prevAngle <= 270 && currentAngle >= 270 && currentAngle <= 315) ||   (currentAngle >= 225 && currentAngle <= 270 && prevAngle >= 270 && prevAngle <= 315)){
				result = TurnDirection.NoTurn;
			}
		}

		return result;
	}


	//averages data from all gems to get the rotation of the cube
	Quaternion  getCubeRotation(){
		Quaternion[] rotationData = new Quaternion[12];
		cubeRotation = Quaternion.identity;
		int count = 0;
		if (gemCount >= 2){
			if (gem[0].State == GemState.Connected && gem[1].State == GemState.Connected){
				//if (gemIsConnected[0] && gemIsConnected[1]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[1] * axis[1],faceRotation[0] * axis[0])
				* Quaternion.AngleAxis(90, Vector3.up);
				count++;
			}
		}
		if (gemCount >= 3){
			if (gem[0].State == GemState.Connected && gem[2].State == GemState.Connected){
				//if (gemIsConnected[0] && gemIsConnected[2]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[2] * -axis[2], faceRotation[0] * axis[0]);
				count++;
			}

			if (gem[1].State == GemState.Connected && gem[2].State == GemState.Connected){
				//if (gemIsConnected[1] && gemIsConnected[2]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[2] * -axis[2], faceRotation[1] * axis[1])
				* Quaternion.AngleAxis(90, Vector3.back);
				count++;
			}
		}
		if (gemCount >= 4){
			if (gem[0].State == GemState.Connected && gem[3].State == GemState.Connected){
				//if (gemIsConnected[0] && gemIsConnected[3]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[3] * axis[3], faceRotation[0] * axis[0])
				* Quaternion.AngleAxis(-90, Vector3.up);
				count++;
			}

			if (gem[2].State == GemState.Connected && gem[3].State == GemState.Connected){
				//if (gemIsConnected[2] && gemIsConnected[3]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[2] * -axis[2], faceRotation[3] * axis[3])
				* Quaternion.AngleAxis(-90, Vector3.back);
				count++;
			}
		}
		if (gemCount >= 5){
			if (gem[0].State == GemState.Connected && gem[4].State == GemState.Connected){
				//if (gemIsConnected[0] && gemIsConnected[4]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[4] * axis[4], faceRotation[0] * axis[0]);
				count++;
			}

			if (gem[1].State == GemState.Connected && gem[4].State == GemState.Connected){
				//if (gemIsConnected[1] && gemIsConnected[4]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[4] * axis[4], faceRotation[1] * axis[1])
				* Quaternion.AngleAxis(90, Vector3.back);
				count++;
			}

			if (gem[3].State == GemState.Connected && gem[4].State == GemState.Connected){
				//if (gemIsConnected[3] && gemIsConnected[4]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[4] * axis[4], faceRotation[3] * axis[3])
				* Quaternion.AngleAxis(-90, Vector3.back);
				count++;
			}
		}
		if (gemCount == 6){
			if (gem[1].State == GemState.Connected && gem[5].State == GemState.Connected){
				//if (gemIsConnected[1] && gemIsConnected[5]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[1] * axis[1], faceRotation[5] * -axis[5])
				* Quaternion.AngleAxis(90, Vector3.up);
				count++;
			}

			if (gem[2].State == GemState.Connected && gem[5].State == GemState.Connected){
				//if (gemIsConnected[2] && gemIsConnected[5]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[2] * -axis[2], faceRotation[5] * -axis[5]);
				count++;
			}

			if (gem[3].State == GemState.Connected && gem[5].State == GemState.Connected){
				//if (gemIsConnected[3] && gemIsConnected[5]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[3] * axis[3], faceRotation[5] * -axis[5])
				* Quaternion.AngleAxis(-90, Vector3.up);
				count++;
			}

			if (gem[4].State == GemState.Connected && gem[5].State == GemState.Connected){
				//if (gemIsConnected[4] && gemIsConnected[5]){
				rotationData[count] = Quaternion.LookRotation(
				faceRotation[4] * axis[4], faceRotation[5] * -axis[5]);
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
		return cubeRotation;
	}

	//returns true if ANGLE is within RANGE of a multiple of 90
	//Example (94 is within 5 of 90)
	bool ignoreUpdate(float angle, float range){
		if(angle % 90 < range || angle % 90 > (90-range)){
			return true;
		}
		return false;
	}

	//Changes the sign of the quaternion components. This is not the same as the inverse.
	Quaternion checkAndFixQuaternion(Quaternion newQ, Quaternion firstQ){
		float dot = Quaternion.Dot(newQ, firstQ);
		if(dot < 0.0f){
			return new Quaternion(-newQ.x, -newQ.y, -newQ.z, -newQ.w);
		}
		return newQ;
	}

	float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n){
    return Mathf.Atan2(
        Vector3.Dot(n, Vector3.Cross(v1, v2)),
        Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
}

	//returns -1 or 1 based off the direction v2 is rotated from v1 with respect to normalVector
	int angleSign (Vector3 v1, Vector3 v2, Vector3 normalVector){
		Vector3 crossProduct = Vector3.Cross(v1, v2);
		float dotProduct = (Vector3.Dot(crossProduct, normalVector));
		if (dotProduct > 0){
			return 1;
		}
		return -1;
	}

	//called when gems are first receivng data,
	//used to calculate the correction quaternion for gem data to match what would be expected
	void calculateStabalizers(){
		for (int i = 0; i < gemCount; i++){
			stabalizer[i] = Quaternion.Inverse(Quaternion.LookRotation(
			gem[i].Rotation * sideOrientation[i] * Vector3.forward,
			gem[i].Rotation * sideOrientation[i] * Vector3.up));
			//gem[i].Rotation * Vector3.forward,
			//gem[i].Rotation * Vector3.up));
		}
	}

	//uses the data calculated by calculateStabalizers() to calibrate the gems
	void stabalizeGems(){
		for (int i = 0; i < gemCount; i++){
			faceRotation[i] =  stabalizer[i] * faceRotation[i];
		}
	}

	//Old way of updating logic of each corner to solved state
	//NOT USED ANYMORE
	void resetCorners(){
		for (int i = 0; i < 8; i++){
			for (int j = 0; j < 8; j++){
				if(cornerOrder[j] == i){
					Transform tempCorner = corner[i];
					int tempOrderValue = cornerOrder[i];
					corner[i] = corner[j];
					cornerOrder[i] = cornerOrder[j];
					corner[j] = tempCorner;
					cornerOrder[j] = tempOrderValue;
				}
			}

		}

	}

	//Old way of updating logic of each edge to solved state
	//NOT USED ANYMORE
	void resetEdges(){
		for (int i = 0; i < 12; i++){
			for (int j = 0; j < 12; j++){
				if(edgeOrder[j] == i){
					Transform tempEdge = edge[i];
					int tempOrderValue = edgeOrder[i];
					edge[i] = edge[j];
					edgeOrder[i] = edgeOrder[j];
					edge[j] = tempEdge;
					edgeOrder[j] = tempOrderValue;
				}
			}

		}

	}

	//returns true if all gems are discoverd by the computer
	bool gemsAreNotNull(){
		for (int i = 0; i < gemCount; i++){
			if (gem[i] == null){
				return false;
			}
		}
		return true;
	}

	//returns false if any gem is "conneting" or "disconnected"
	bool allGemsConnected(){
		for (int i = 0; i < gemCount; i++){
			//if (gem[i] == null){
			if (gem[i].State != GemState.Connected){
				return false;
			}
		}
		//wasFullyConnected = true;
		return true;
	}

	//let bluetooth relax after you quit the program
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
