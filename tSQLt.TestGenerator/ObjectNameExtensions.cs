using System;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace tSQLt.TestGenerator
{
    public static class ObjectNameExtensions
    {
        public static Identifier ToIdentifier(this string src)
        {
            var id = new Identifier {Value = src};
            return id;
        }

        public static SchemaObjectName ToSchemaObjectName(this string src)
        {
            var name = new SchemaObjectName();
            name.Identifiers.Add(src.ToIdentifier());
            return name;
        }

        public static SchemaObjectName ToSchemaObjectName(this ObjectIdentifier src)
        {
            var name = new SchemaObjectName();

            var items = src.Parts.Count;

            if (items == 0)
            {
                throw new NameConversionException("Didn't find any name parts on ObjectIdentifier");
            }

            if (items == 1)
            {
                name.Identifiers.Add(src.Parts[0].ToIdentifier());
                return name;
            }

            if (items == 2)
            {
                name.Identifiers.Add(src.Parts[0].ToIdentifier());
                name.Identifiers.Add(src.Parts[1].ToIdentifier());
                return name;
            }

            name.Identifiers.Add(src.Parts[items - 2].ToIdentifier());
            name.Identifiers.Add(src.Parts[items - 1].ToIdentifier());
            return name;
        }
    }

    public class NameConversionException : Exception
    {
        public NameConversionException(string message)
        {
        }
    }
}