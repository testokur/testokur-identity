namespace TestOkur.Identity
{
    public static class ErrorCodes
    {
        public const string UserNotActive = "UserNotActive";

        public const string UserExpired = "UserExpired";

        public const string WaitingForApproval = "WaitingForApproval";

        public const string InvalidUsernameOrPassword = "InvalidUsernameOrPassword";

        public const string UserExists = "UserExists";

        public const string UserDoesNotExists = "UserDoesNotExists";

        public const string MaxAllowedDeviceExceeded = "MaxAllowedDeviceExceeded";
    }
}
