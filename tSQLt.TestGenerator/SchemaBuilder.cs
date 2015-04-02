using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace tSQLt.TestGenerator
{
    public class SchemaBuilder : ScriptBuilder
    {
        private readonly string _procedureName;

        public SchemaBuilder(string procedureName)
        {
            _procedureName = procedureName;
        }

        public string GetScript()
        {
            return string.Format("{0}\r\nGO\r\n{1}", GetSchema(), GetExtendedProperty());
        }

        private string GetSchema()
        {
            var createSchema = new CreateSchemaStatement();
            createSchema.Name = new Identifier(){Value = _procedureName};
            createSchema.Owner = new Identifier(){Value = "dbo"};

            return GenerateScript(createSchema);
        }

        private string GetExtendedProperty()
        {
            var execExtendedProperty = new ExecuteStatement();
            execExtendedProperty.ExecuteSpecification = new ExecuteSpecification();

            var name = new ChildObjectName();
            name.Identifiers.Add(new Identifier(){Value = _procedureName});
            
            var procedureReference = new ProcedureReference();
            procedureReference.Name = "sp_addextendedproperty".ToSchemaObjectName();

            var entity = new ExecutableProcedureReference();
            entity.ProcedureReference = new ProcedureReferenceName();
            entity.ProcedureReference.ProcedureReference = procedureReference;


            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("@name", "tSQLt.TestClass"));
            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("@value", 1));
            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("@level0type", "SCHEMA"));
            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("@level0name", _procedureName));

            execExtendedProperty.ExecuteSpecification.ExecutableEntity = entity;
            
            return GenerateScript(execExtendedProperty);
        }
    }
}