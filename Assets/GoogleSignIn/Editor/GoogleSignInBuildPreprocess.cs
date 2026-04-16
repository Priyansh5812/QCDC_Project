using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
public class GoogleSignInBuildPreprocess : IPreprocessBuildWithReport
{
    public int callbackOrder
    {
        get => -1;
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        EnsureGoogleSignInGradlePath();
    }

    private void EnsureGoogleSignInGradlePath()
    {
        try
        {

            string settingsPath = Path.Combine(Application.dataPath, "Plugins/Android/settingsTemplate.gradle");

            if (File.Exists(settingsPath))
            {
                string content = File.ReadAllText(settingsPath);
                content = content.Replace(
                    "/Assets/GoogleSignIn/Editor/m2repository",
                    "/Assets/GeneratedLocalRepo/GoogleSignIn/Editor/m2repository"
                );
                File.WriteAllText(settingsPath, content);
                Debug.Log("Fixed Google Sign In repository path.");
            }
        }
        catch
        { 
            //noop
        }
    }

}
