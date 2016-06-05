using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System;

class GemsensePluginBuildPostProcessor
{
    [PostProcessBuildAttribute()]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        
		switch (target)
		{
		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			{
				var pathToDLL = Path.Combine(Path.GetDirectoryName(pathToBuiltProject),
					Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");

				pathToDLL = Path.Combine(pathToDLL, "Plugins");
				pathToDLL = Path.Combine(pathToDLL, "GemSDK.dll");

				var pathToCopy = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileName(pathToDLL));

				Debug.Log(string.Format("Copying \"{0}\" to \"{1}\"...", pathToDLL, pathToCopy));

				try
				{
					File.Copy(pathToDLL, pathToCopy, true);
					File.Delete(pathToDLL);
				} catch (Exception ex)
				{
					Debug.Log(ex.Message);
				}

				break;
			}
		case BuildTarget.iOS:
			{
				string pathToFramework = "Assets/Plugins/iOS/GemSDK.framework";
				string pathToCopy = Path.Combine(pathToBuiltProject, "GemSDK.framework");

				if (Directory.Exists(pathToCopy))
				{
					Debug.Log("Deleting old GemSDK.framework from " + pathToBuiltProject);

					try
					{
						Directory.Delete(pathToCopy, true);
					} catch (Exception e)
					{
						Debug.LogError(e.Message);
						return;
					}
				}

				Debug.Log(string.Format("Copying {0} to {1}", pathToFramework, pathToCopy));

				try
				{
					FileUtil.CopyFileOrDirectory(pathToFramework, pathToCopy);
				} catch (Exception e)
				{
					Debug.LogError(e.Message);
				}
			}
			break;
		case BuildTarget.Android:
		case BuildTarget.StandaloneOSXIntel64:
			// Nothing to do
			break;
		case BuildTarget.StandaloneOSXIntel:
		case BuildTarget.StandaloneOSXUniversal:
			Debug.LogError("GemSDK: OSX 32 bit and universal is not supported");
			break;
		default:
			Debug.LogError("GemSDK: this platform is not supported. Supported platforms: Android, Windows(x64), iOS, OSX(x64)");
			break;
		}
    }
}
