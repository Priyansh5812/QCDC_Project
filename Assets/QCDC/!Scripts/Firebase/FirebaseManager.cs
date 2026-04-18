using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using System;
using UnityEngine;
using Pkay.Utils;
using Firebase.Extensions;


public class FirebaseManager : Singleton<FirebaseManager>
{
    private FirebaseAuth fireAuth;
    private GoogleSignIn googleAuth;
    public static bool IsFirebaseActive
    {
        get; private set;
    }

    public static bool IsGoogleSignInInitialized
    {
        get; private set;
    } = false;

    public static event Action OnFirebaseInitialized;
    public static event Action OnFirebaseDependenciesError;
    public static event Action<object, System.EventArgs> OnAuthStateChanged;
    private readonly string WebClientID = "991491842989-a46uq429ipfht0qupq60ln1momc6lp2b.apps.googleusercontent.com";
    

    protected override void Awake()
    {
        base.Awake();
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        if (IsFirebaseActive)
            return;


        Firebase.FirebaseApp.LogLevel = LogLevel.Debug;

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(async task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                fireAuth = FirebaseAuth.DefaultInstance;
                IsFirebaseActive = true;
                OnFirebaseInitialized?.Invoke();
                OnFirebaseInitialized = null;
                fireAuth.StateChanged += AuthStateChanged;
                Utils.Info("Firebase Initialized");
                InitializeGoogleSignIn();

            }
            else
            {
                Utils.Error(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                OnFirebaseDependenciesError?.Invoke();
            }
        });
    }

    private void InitializeGoogleSignIn()
    {   
        if(IsGoogleSignInInitialized)
            return;

        GoogleSignIn.Configuration = new GoogleSignInConfiguration()
        {   
            RequestEmail = true,
            RequestIdToken = true,
            WebClientId = WebClientID,
            RequestAuthCode = true
        };

        googleAuth = GoogleSignIn.DefaultInstance;

        IsGoogleSignInInitialized = true;
    }

    #region Auth
    private void AuthStateChanged(object sender , EventArgs args) => OnAuthStateChanged?.Invoke(sender, args);

    public FirebaseUser GetCurrentFirebaseUser() => fireAuth.CurrentUser;

    public async UniTask TryEmailPassLogin(string email, string pass, Action OnSignInSuccess = null, Action<string> OnSignInFailed = null)
    {
        try
        {
            await fireAuth.SignInWithEmailAndPasswordAsync(email, pass).AsUniTask();

            await UniTask.SwitchToMainThread();

            if (fireAuth.CurrentUser == null)
            {
                OnSignInFailed?.Invoke("Something went wrong");
            }
            else
            {
                OnSignInSuccess?.Invoke();
            }
        }
        catch (FirebaseException ex) 
        {
            await UniTask.SwitchToMainThread();
            OnSignInFailed?.Invoke(GetFirebaseErrorMessage(ex));
        }
    }


    public async UniTask TryEmailPassSignUp(string email , string pass, Action OnSignUpSuccess = null, Action<string> OnSignUpFailed = null)
    {
        try
        {
            await fireAuth.CreateUserWithEmailAndPasswordAsync(email, pass).AsUniTask();
            await UniTask.SwitchToMainThread();

            if (fireAuth.CurrentUser == null)
            {
                OnSignUpFailed?.Invoke("Something went wrong");
            }
            else
            {
                Utils.Info(("Is User loged in : "+fireAuth.CurrentUser != null).ToString());
                OnSignUpSuccess?.Invoke();
            }
        }
        catch (FirebaseException ex) 
        {
            Debug.Log(ex.Message);

            await UniTask.SwitchToMainThread();
            
            OnSignUpFailed?.Invoke(GetFirebaseErrorMessage(ex));
        }
    }

    public async UniTask TryAnonymousLogin(Action OnAuthInitiated = null , Action OnLoginSuccess = null , Action<string> OnLoginFailed = null)
    {
        OnAuthInitiated?.Invoke();

        try
        {   
            AuthResult res = await fireAuth.SignInAnonymouslyAsync().AsUniTask();
            await UniTask.SwitchToMainThread();

            Utils.Info("Sign In Anony Completed");
            if (res.User == null)
            {
                Utils.Info("failed");
                OnLoginFailed?.Invoke("Something went wrong");
            }
            else
            {
                Utils.Info("Success");
                OnLoginSuccess?.Invoke();
            }
        }
        catch (FirebaseException e)
        {
            OnLoginFailed?.Invoke(GetFirebaseErrorMessage(e));
        }
    }

    public async void TryGoogleSignIn(Action OnSignInSuccess = null, Action<string> OnSignInFailed = null)
    {       

#if UNITY_EDITOR
            Utils.Error("Google sign in is not supported in Editor");
            OnSignInFailed?.Invoke("Unauthorized Platform");
            return;
#endif
        GoogleSignInUser user = null;
        FirebaseUser fireUser = null;
        Utils.Info("Initiated");
        try
        {
            user = await googleAuth.SignIn().AsUniTask();
            await UniTask.SwitchToMainThread();
        }
        catch (GoogleSignIn.SignInException e)
        {
            Utils.Warn("Failed to get User ID Token... \n Reason : "+e.Message);
            Utils.Warn("Status Code: "+e.Status);
            OnSignInFailed?.Invoke("Google Sign In Failed");
            return;
        }
        

        try
        {
            Credential userCredentials = GoogleAuthProvider.GetCredential(user.IdToken, null);
            fireUser = await fireAuth.SignInWithCredentialAsync(userCredentials).AsUniTask();
            await UniTask.SwitchToMainThread();
        }
        catch (Exception e)
        {
            Utils.Warn("Failed to perform Google Sign In...");
            Utils.ExceptWarn(e);
            OnSignInFailed?.Invoke("Google Sign In Failed");
            return;
        }

        OnSignInSuccess?.Invoke();
    }

    [ContextMenu("SignOut")]
    public void DebugSignout()
    {
        SignOutUser();
    }

    public async UniTask SignOutUser(bool forceDeletionIfGuest = true , Action OnOperationSuccess = null, Action OnOperationFailed = null)
    {
        if (fireAuth.CurrentUser.IsEmailVerified)
            googleAuth.SignOut();

        if (fireAuth.CurrentUser.IsAnonymous && forceDeletionIfGuest)
        {
            await DeleteUserAccount(OnOperationSuccess, OnOperationFailed);
            return;
        }

        fireAuth.SignOut();
        OnOperationSuccess?.Invoke();
    }

    public async UniTask DeleteUserAccount(Action OnOperationSuccess = null, Action OnOperationFailed = null)
    {
        // TODO
        
    }



    #endregion


    private string GetFirebaseErrorMessage(FirebaseException ex)
    {
        Debug.Log(ex.ErrorCode.ToString());

        switch ((AuthError)ex.ErrorCode)
        {
            case AuthError.Failure:
                return "Something went wrong while signing you in. Please try again.";

            case AuthError.UserDisabled:
                return "This account has been disabled. Please contact support for help.";

            case AuthError.EmailAlreadyInUse:
                return "This email is already associated with another account. Try signing in instead.";

            case AuthError.CredentialAlreadyInUse:
                return "This account is already linked to another sign-in method.";

            case AuthError.InvalidEmail:
                return "Please enter a valid email address.";

            case AuthError.WrongPassword:
                return "The password you entered is incorrect. Please try again.";

            case AuthError.TooManyRequests:
                return "Too many attempts in a short time. Please wait and try again later.";

            case AuthError.UserNotFound:
                return "No account was found with these details.";

            case AuthError.NetworkRequestFailed:
                return "Network error. Please check your internet connection and try again.";

            case AuthError.MissingEmail:
                return "Please enter your email address to continue.";

            case AuthError.MissingPassword:
                return "Please enter your password to continue.";

            case AuthError.InvalidCredential:
            case AuthError.InvalidUserToken:
            case AuthError.UserTokenExpired:
                return "This sign-in method is no longer valid. Please sign in using Google.";

            default:
                return "Something went wrong while signing you in. Please try again.";

        }


    }

 
    private void OnDisable()
    {
        try 
        {
            fireAuth.StateChanged -= AuthStateChanged;

        }
        catch 
        {
            //...
        }

    }

}