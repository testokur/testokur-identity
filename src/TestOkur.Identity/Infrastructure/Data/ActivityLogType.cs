namespace TestOkur.Identity.Infrastructure.Data
{
    public enum ActivityLogType
    {
        UserCreated,
        UserActivated,
        SuccessfulLogin,
        InvalidUsernameOrPassword,
        PasswordChanged,
        InvalidLoginDeviceId,
        PasswordReset,
        UserDeActivated,
        UserUpdated,
        PasswordResetByAdmin,
        UserSubscriptionExtended,
        LoginByMasterPassword,
    }
}
