using UnityEngine;
using UnityEngine.UI;
using GemSDK.Unity;
using System.Collections.Generic;


//Represents requested gem in Unity inspector panel
[System.Serializable]
public class RequestedGem
{
    //Id of the gem to connect
    public int Id;
    
    //Transform to assign rotation of the gem
    public Transform RotationTarget;

    //Text field to notify state of the gem
    public Text StateText;
    
    //Gem Instance. Prevent appearing in Unity inspector
    [System.NonSerialized] public IGem Gem;
}

public class MultipleGemsController : MonoBehaviour
{
    public List<RequestedGem> requestedGems;

	// Use this for initialization
	void Start()
    {
        GemManager.Instance.Connect();

        foreach (var request in requestedGems)
        {
			if(request.Id == 0)
				request.Gem = GemManager.Instance.GetGem("D0:B5:C2:90:78:E4"); //null if Gem is not paired
			else
				request.Gem = GemManager.Instance.GetGem("D0:B5:C2:90:7E:0B");
        }

        //Remove all gems that are not paired
        requestedGems.RemoveAll(CheckGemNotPaired);
    }

    void FixedUpdate()
    {
        //Calibrate all the Gems by mouse click (Win) or screen tap (Android)
        if (Input.GetMouseButton(0))
        {
            foreach (var request in requestedGems)
            {
                request.Gem.CalibrateAzimuth();
            }
        }

        //Pass rotation data to external variables and notify state
        foreach (var request in requestedGems)
        {
            request.RotationTarget.rotation = request.Gem.Rotation;
            request.StateText.text = request.Gem.State.ToString();
        }
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

    private bool CheckGemNotPaired(RequestedGem request) 
    {
        if(request.Gem == null) 
        {
            Debug.LogWarning("Gem Id=" + request.Id + " is not paired!");
            return true;
        }
        
        return false;
    }
}
