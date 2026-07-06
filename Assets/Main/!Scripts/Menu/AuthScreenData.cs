using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct AuthScreenData
{
    public CanvasGroup cgMain;
    public TMP_InputField emailField , passField;
    public Button guest, loginSignup;
    public Button googleLogin;
    public Button loginToggle;
    public TextMeshProUGUI authStatus;
    public TextMeshProUGUI ToggleText;
    public TextMeshProUGUI ToggleButtonText;
    public string emailAuthAsLoginMsg, emailAuthAsSignupMsg;
}
