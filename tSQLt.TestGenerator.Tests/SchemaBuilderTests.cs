using NUnit.Framework;

namespace tSQLt.TestGenerator.Tests
{
    [TestFixture]
    public class SchemaBuilderTests
    {
        [Test]
        public void Generates_Extended_Property_String()
        {
            var builder = new SchemaBuilder("procName");
            var script = builder.GetScript();

            const string expected =
                @"EXECUTE sp_addextendedproperty @name = 'tSQLt.TestClass', @value = 1, @level0type = 'SCHEMA', @level0name = 'procName'";

            Assert.IsTrue(script.IndexOf(expected) >= 0, script);
        }

        [Test]
        public void Generates_Create_Schema_Statement()
        {
            var builder = new SchemaBuilder("[procName]");
            var script = builder.GetScript();

            const string expected =
                @"CREATE SCHEMA [procName]";

            Assert.IsTrue(script.IndexOf(expected) >= 0);
        }
    }
}