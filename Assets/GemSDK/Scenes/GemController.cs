using UnityEngine;
using UnityEngine.UI;
using GemSDK.Unity;
using System.Collections;

public class GemController : MonoBehaviour
{
    public Text stateText;
    private IGem gem;
    
	// Use this for initialization
	void Start()
    {
		GemManager.Instance.Connect();


        //To get gem by number instead of address, on Android the Gem should be paired to Gem SDK Utility app
        //gem = GemManager.Instance.GetGem(0);
		gem = GemManager.Instance.GetGem("D0:B5:C2:90:78:E4");
    }

    void FixedUpdate()
    {
        if (gem != null)
        {
            if (Input.GetMouseButton(0))
            {
                gem.CalibrateAzimuth();

                //Use instead of CalibrateAzimuth() to calibrate also tilt and elevation
                //gem.ColibrateOrigin(); 
            }

            transform.rotation = gem.Rotation;
            stateText.text = gem.State.ToString();
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
}
