namespace Integration.Tests.Common
{
    using AutoFixture;
    using AutoFixture.AutoNSubstitute;
    using AutoFixture.Xunit2;

    public class TestOkurDataAttribute : AutoDataAttribute
    {
        public TestOkurDataAttribute()
            : base(() => new Fixture()
                .Customize(new TestOkurCustomization())
                .Customize(new AutoNSubstituteCustomization()))
        {
        }
    }
}