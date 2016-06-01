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
        //Copy DLL from plugins folter to executable root for Windows builds
		if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows)
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
		}
		else if (target == BuildTarget.iOS)
		{
			string pathToFramework = "Assets/Plugins/iOS/GemSDK.framework";
			string pathToCopy = Path.Combine(pathToBuiltProject, "GemSDK.framework");

			if (Directory.Exists(pathToCopy))
			{
				Debug.Log("Deleting old GemSDK.framework from " +  pathToBuiltProject);

				try
				{
					Directory.Delete(pathToCopy, true);
				}
				catch(Exception e)
				{
					Debug.LogError(e.Message);
					return;
				}
			}

			Debug.Log(string.Format("Copying {0} to {1}", pathToFramework, pathToCopy));

			try
			{
				FileUtil.CopyFileOrDirectory(pathToFramework, pathToCopy);
			}
			catch(Exception e)
			{
				Debug.LogError(e.Message);
			}
		}
        else if (target != BuildTarget.Android)
        {
            Debug.LogError("GemSDK: this platform is not supported. Supported platforms: Android, Windows(x64)");
        }
    }
}
