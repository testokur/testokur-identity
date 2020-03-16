namespace Integration.Tests.Common
{
    using AutoFixture;
    using System;
    using TestOkur.Identity.Models;

    public class TestOkurCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.RepeatCount = 5;
            fixture.Customize<UpdateUserModel>(request =>
                request.With(x => x.ExpiryDateUtc, DateTime.UtcNow.AddDays(100)));
        }
    }
}