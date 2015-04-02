using Microsoft.SqlServer.Dac.Model;

namespace tSQLt.TestGenerator
{
    public class Parameter
    {
        public Parameter(string name, SqlDataType type)
        {
            Name = name;
            Type = type;
        }

        public string Name;
        public SqlDataType Type;
    }
}