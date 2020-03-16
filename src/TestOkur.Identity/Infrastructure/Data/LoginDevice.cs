namespace TestOkur.Identity.Infrastructure.Data
{
    public class LoginDevice
    {
        public LoginDevice()
        {
        }

        public LoginDevice(string deviceId)
        {
            DeviceId = deviceId;
        }

        public int Id { get; set; }

        public string DeviceId { get; set; }
    }
}
