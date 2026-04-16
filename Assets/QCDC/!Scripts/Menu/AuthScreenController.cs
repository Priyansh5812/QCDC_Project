using System;

public class AuthScreenController
{
    // Moved from AuthScreenView: validates email and password and returns an error message when invalid.
    public bool PerformEmailPassChecks(string email, string password, out string errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrEmpty(email))
        {
            errorMessage = "Please enter your email address.";
            return false;
        }

        if (!Helpers.IsValidEmail(email))
        {
            errorMessage = "Please enter a valid email address.";
            return false;
        }

        // ---- Password checks ----
        if (string.IsNullOrEmpty(password))
        {
            errorMessage = "Please enter a password.";
            return false;
        }

        if (password.Length < 6)
        {
            errorMessage = "Password must be at least 6 characters long.";
            return false;
        }

        return true;
    }
}
