using System;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;

namespace tSQLt.TestGenerator.Tests
{
    [TestFixture]
    public class TestBuilderTests
    {
        [Test]
        public void Generates_FakeTake_Statements()
        {
            var name = new SchemaObjectName();
            name.Identifiers.Add("dbo".ToIdentifier());
            name.Identifiers.Add("procedureName".ToIdentifier());

            var builder = new ProcedureBuilder("[procedureName]", "[test blah bloo blah]", name);
            var tableName = new SchemaObjectName();
            tableName.Identifiers.Add("someschema".ToIdentifier());
            tableName.Identifiers.Add("table_thingy".ToIdentifier());

            builder.AddTable(tableName);

            var script = builder.GetScript();

            const string expected = "EXECUTE tSQLt.FakeTable 'table_thingy', 'someschema'";
            Assert.True(script.IndexOf(expected) > -1, "Did not find faketable statement: \"{0}\"", script);
        }

        [Test]
        public void Generates_Declare_Variables_Statements()
        {
            var name = new SchemaObjectName();
            name.Identifiers.Add("dbo".ToIdentifier());
            name.Identifiers.Add("procedureName".ToIdentifier());

            var builder = new ProcedureBuilder("[procedureName]", "[test blah bloo blah]", name);
            builder.AddParameter("@p1", SqlDataType.Date);
            builder.AddTable(name);

            var script = builder.GetScript();

            const string expectedP1 = "DECLARE @p1 AS DATE";
            Assert.True(script.IndexOf(expectedP1) > -1, "Did not find declare statement for p1: \"{0}\"", script);
        }

        [Test]
        public void Multiple_Parameters_Generates_All_Declare_Statements()
        {
            var name = new SchemaObjectName();
            name.Identifiers.Add("dbo".ToIdentifier());
            name.Identifiers.Add("procedureName".ToIdentifier());

            var builder = new ProcedureBuilder("[procedureName]", "[test blah bloo blah]", name);
            builder.AddParameter("@p1", SqlDataType.Date);
            builder.AddParameter("@p2222222222222222222", SqlDataType.VarChar);
            builder.AddTable(name);

            var script = builder.GetScript();

            const string expectedP1 = "DECLARE @p1 AS DATE";
            const string expectedP2 = "DECLARE @p2222222222222222222 AS VARCHAR = ''";

            Assert.True(script.IndexOf(expectedP1) > -1, "Did not find declare statement for p1: \"{0}\"", script);
            Assert.True(script.IndexOf(expectedP2) > -1, "Did not find declare statement for p2: \"{0}\"", script);
        }

        [Test]
        public void Test_Proc_Body_Is_Indented()
        {
            var name = new SchemaObjectName();
            name.Identifiers.Add("dbo".ToIdentifier());
            name.Identifiers.Add("procedureName".ToIdentifier());

            var builder = new ProcedureBuilder("[procedureName]", "[test blah bloo blah]", name);
            builder.AddParameter("@p1", SqlDataType.Date);
            builder.AddParameter("@p2222222222222222222]", SqlDataType.VarChar);
            builder.AddTable(name);

            var script = builder.GetScript();

            var lines = script.Replace("\r\n", "~").Split('~');
            for (var i = 2; i < lines.Length; i++)
            {
                if (!(Char.IsWhiteSpace(lines[i][0])))
                {
                    Assert.Fail("Line does not start with whitespace: \"{0}\"", lines[i]);
                }
            }
        }
    }
}