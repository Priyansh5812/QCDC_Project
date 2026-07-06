using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using System;
using UnityEngine;
using Pkay.Utils;
using Firebase.Extensions;


/// <summary>
/// Wrapper and helper for Firebase authentication and Google sign-in.
/// Implements initialization of Firebase SDK, provides common sign-in
/// flows (email/password, anonymous, Google) and helper utilities for
/// handling authentication state. This is a singleton so it can be
/// accessed globally via <see cref="Singleton{T}"/> base class.
/// </summary>
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
    // Web client id used to configure Google Sign-In. Replace with your
    // project's client id when necessary.
    private readonly string WebClientID = "991491842989-a46uq429ipfht0qupq60ln1momc6lp2b.apps.googleusercontent.com";
    

    protected override void Awake()
    {
        base.Awake();
        // Start Firebase initialization on Awake so authentication is ready
        // when other systems request it.
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
                // Cache the auth instance and notify listeners that firebase
                // is initialized. Also listen for auth state changes.
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

        // Cache the GoogleSignIn instance after configuration.
        googleAuth = GoogleSignIn.DefaultInstance;

        IsGoogleSignInInitialized = true;
    }

    #region Auth
    // Forward Firebase auth state changes to external subscribers.
    private void AuthStateChanged(object sender , EventArgs args) => OnAuthStateChanged?.Invoke(sender, args);

    /// <summary>
    /// Get the currently signed in Firebase user or null if none.
    /// </summary>
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
            // Convert Firebase exceptions to user friendly messages and
            // invoke the failure callback on the main thread.
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
            // Log the raw exception and convert it to a user friendly
            // message for callbacks.
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
            // Return a friendly error for anonymous login failures.
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
            // Attempt to sign in with Google; this returns a token used to
            // authenticate with Firebase.
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
            // Exchange the Google ID token for Firebase credentials and sign
            // in to Firebase using those credentials.
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

    public async UniTask SignOutUser(Action OnOperationSuccess = null, Action OnOperationFailed = null)
    {
        // If the user signed in with Google, sign out from the Google SDK as
        // well so that future sign-ins require explicit account selection.
        if (fireAuth.CurrentUser.IsEmailVerified)
            googleAuth.SignOut();

        fireAuth.SignOut();
        OnOperationSuccess?.Invoke();
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