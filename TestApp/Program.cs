using System;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;
using tSQLt.TestGenerator;

namespace TestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (!CheckArgs(args))
                return;

            var model = new TSqlTypedModel(args[0]);
            var outputFolder = args[1];

            var schemaDir = CreateDirectory(outputFolder, "Schema");
            var procDir = CreateDirectory(outputFolder, "Tests");
            
            var procedures = model.GetObjects<TSqlProcedure>(DacQueryScopes.UserDefined);
            foreach (var procedure in procedures)
            {
                var schema = GenerateSchema(procedure);

                var test = GenerateTest(procedure);

                using (var writer = new StreamWriter(Path.Combine(schemaDir, procedure.Name.Parts.Last() + ".sql")))
                {
                    writer.Write(schema);
                }

                using (
                    var writer =
                        new StreamWriter(Path.Combine(procDir,
                            procedure.Name.Parts.Last() + "." + "test To Be Implemented.sql")))
                {
                    writer.Write(test);
                }
            }
        }

        private static bool CheckArgs(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.Length > 1 && arg[1] == '?')
                {
                    ShowHelp("Please pass 2 parameters, the path of the dacpac and the path of the scripts folder to place the files\r\nIf there are spaces in either path, please surround qith quotes");
                    return false;
                }
            }

            if (args.Length != 2)
            {
                ShowHelp("Please pass 2 parameters, the path of the dacpac and the path of the scripts folder to place the files");
                return false;
            }

            if (!File.Exists(args[0]))
            {
                ShowHelp("Error dacpac not found");
                return false;
            }

            if (!Directory.Exists(args[1]))
            {
                ShowHelp("Error output folder not found");
            }

            return true;
        }

        private static void ShowHelp(string message)
        {
            Console.WriteLine(message);
        }

        private static string GenerateSchema(TSqlProcedure procedure)
        {
            var builder = new SchemaBuilder(procedure.Name.Parts.Last());
            return builder.GetScript();
        }

        private static string GenerateTest(TSqlProcedure procedure)
        {
            var builder = new ProcedureBuilder(procedure.Name.Parts.Last(), "[test To Be Implemented]", procedure.Name.ToSchemaObjectName());

            foreach (var param in procedure.Parameters)
            {
                foreach (var type in param.DataType)
                {
                    builder.AddParameter(param.Name.Parts.Last(), type.SqlDataType);
                    break;
                }
            }

            foreach (var dependency in procedure.BodyDependencies)
            {
                if (dependency.ObjectType == ModelSchema.Table)
                {
                    var table = new TSqlTable(dependency);
                    builder.AddTable(table.Name.ToSchemaObjectName());
                }
            }

            return builder.GetScript();
        }

        private static string CreateDirectory(string outputFolder, string name)
        {
            var path = Path.Combine(outputFolder, name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }
}