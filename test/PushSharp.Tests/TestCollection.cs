namespace PushSharp.Tests
{
    using Xunit;

    [CollectionDefinition(nameof(TestCollection))]
    public class TestCollection : ICollectionFixture<PushSharpFixture>
    {
    }
}
