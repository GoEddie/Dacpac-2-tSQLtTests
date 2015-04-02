﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace tSQLt.TestGenerator
{
    public class ProcedureBuilder : ScriptBuilder
    {
        private readonly ExecuteStatement _execProc = new ExecuteStatement();
        private readonly List<Parameter> _parameters = new List<Parameter>();
        private readonly List<SchemaObjectName> _tables = new List<SchemaObjectName>();
        private readonly CreateProcedureStatement _testProcedure = new CreateProcedureStatement();

        public ProcedureBuilder(string testSchema, string testName, SchemaObjectName procUnderTest)
        {
            _testProcedure.StatementList = new StatementList();

            CreateTestProcedureDefinition(testSchema, testName);
            CreateExecForProcUnderTest(procUnderTest);
        }

        private void CreateTestProcedureDefinition(string testSchema, string testName)
        {
            var testProcedureReference = _testProcedure.ProcedureReference = new ProcedureReference();
            testProcedureReference.Name = new SchemaObjectName();
            testProcedureReference.Name.Identifiers.Add(testSchema.ToIdentifier());
            testProcedureReference.Name.Identifiers.Add(testName.ToIdentifier());
        }

        private void CreateExecForProcUnderTest(SchemaObjectName procUnderTest)
        {
            var calleeProcedure = new ProcedureReference();
            calleeProcedure.Name = new SchemaObjectName();

            _execProc.ExecuteSpecification = new ExecuteSpecification();
            var entity = new ExecutableProcedureReference();
            entity.ProcedureReference = new ProcedureReferenceName();
            entity.ProcedureReference.ProcedureReference = calleeProcedure;
            entity.ProcedureReference.ProcedureReference.Name = procUnderTest;
            _execProc.ExecuteSpecification.ExecutableEntity = entity;
        }

        public void AddParameter(string name, SqlDataType type)
        {
            _parameters.Add(new Parameter(name, type));
        }

        public void AddTable(SchemaObjectName name)
        {
            _tables.Add(name);
        }

        //We want it to look like, create proc as .. fake tables .. declares for proc .. proc .. assert
        private void Assemble()
        {
            _testProcedure.StatementList.Statements.Clear();


            foreach (var table in _tables)
            {
                CreateFakeTableDefinition(table);
            }

            foreach (var parameter in _parameters)
            {
                CreateDeclareVariableDefinitionForParmeter(parameter.Name, parameter.Type);
                CreateParameterForCalleeStoredProc(parameter);
            }

            _testProcedure.StatementList.Statements.Add(_execProc);

            CreateAssertDefinition();
        }

        private void CreateParameterForCalleeStoredProc(Parameter parameter)
        {
            _execProc.ExecuteSpecification.ExecutableEntity.Parameters.Add(
                ParametersHelper.CreateStoredProcedureVariableParameter(parameter.Name));
        }

        private void CreateFakeTableDefinition(SchemaObjectName table)
        {
            var fakeTable = new ExecuteStatement();
            fakeTable.ExecuteSpecification = new ExecuteSpecification();

            var procedureReference = new ProcedureReference();
            procedureReference.Name = new SchemaObjectName();
            procedureReference.Name.Identifiers.Add("tSQLt".ToIdentifier());
            procedureReference.Name.Identifiers.Add("FakeTable".ToIdentifier());

            var entity = new ExecutableProcedureReference();
            entity.ProcedureReference = new ProcedureReferenceName();
            entity.ProcedureReference.ProcedureReference = procedureReference;

            entity.Parameters.Add(
                ParametersHelper.CreateStoredProcedureParameter(string.Format("{0}", table.BaseIdentifier.Value)));
            entity.Parameters.Add(
                ParametersHelper.CreateStoredProcedureParameter(string.Format("{0}", table.SchemaIdentifier.Value)));

            fakeTable.ExecuteSpecification.ExecutableEntity = entity;

            _testProcedure.StatementList.Statements.Add(fakeTable);
        }

        private void CreateAssertDefinition()
        {
            var fakeTable = new ExecuteStatement();
            fakeTable.ExecuteSpecification = new ExecuteSpecification();

            var procedureReference = new ProcedureReference();
            procedureReference.Name = new SchemaObjectName();
            procedureReference.Name.Identifiers.Add("tSQLt".ToIdentifier());
            procedureReference.Name.Identifiers.Add("AssertEquals".ToIdentifier());

            var entity = new ExecutableProcedureReference();
            entity.ProcedureReference = new ProcedureReferenceName();
            entity.ProcedureReference.ProcedureReference = procedureReference;

            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("TRUE"));
            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("FALSE"));

            var messageParameter = new ExecuteParameter();
            var messageValue = new StringLiteral {IsNational = true, Value = "Error Not Implemented"};
            messageParameter.ParameterValue = messageValue;
            entity.Parameters.Add(messageParameter);
            
            fakeTable.ExecuteSpecification.ExecutableEntity = entity;

            _testProcedure.StatementList.Statements.Add(fakeTable);
        }

        private void CreateDeclareVariableDefinitionForParmeter(string name, SqlDataType type)
        {
            var declare = new DeclareVariableStatement();
            var declareElement = new DeclareVariableElement();

            var dataType = GetDataType(type);
            declareElement.Value = GetDefaultValue(dataType);
            declareElement.DataType = dataType;
            declareElement.VariableName = name.ToIdentifier();
            declare.Declarations.Add(declareElement);

            _testProcedure.StatementList.Statements.Add(declare);
        }

        public string GetScript()
        {
            Assemble();

            var script = new StringBuilder();

            var tokens = GetTokens(_testProcedure);
            var previousNewLine = false;

            foreach (var token in tokens)
            {
                if (previousNewLine && (token.TokenType != TSqlTokenType.As && token.TokenType != TSqlTokenType.Create))
                {
                    script.Append("    ");
                }

                script.Append(token.Text);

                previousNewLine = token.Text == "\r\n";
            }

            return script.ToString();
        }

        private SqlDataTypeReference GetDataType(SqlDataType type)
        {
            var option = SqlDataTypeOption.BigInt;

            switch (type)
            {
                case SqlDataType.Unknown:
                    option = SqlDataTypeOption.VarChar;
                    break;
                case SqlDataType.BigInt:
                    option = SqlDataTypeOption.BigInt;
                    break;
                case SqlDataType.Int:
                    option = SqlDataTypeOption.Int;
                    break;
                case SqlDataType.SmallInt:
                    option = SqlDataTypeOption.SmallInt;
                    break;
                case SqlDataType.TinyInt:
                    option = SqlDataTypeOption.TinyInt;
                    break;
                case SqlDataType.Bit:
                    option = SqlDataTypeOption.Bit;
                    break;
                case SqlDataType.Decimal:
                    option = SqlDataTypeOption.Decimal;
                    break;
                case SqlDataType.Numeric:
                    option = SqlDataTypeOption.Numeric;
                    break;
                case SqlDataType.Money:
                    option = SqlDataTypeOption.Money;
                    break;
                case SqlDataType.SmallMoney:
                    option = SqlDataTypeOption.SmallMoney;
                    break;
                case SqlDataType.Float:
                    option = SqlDataTypeOption.Float;
                    break;
                case SqlDataType.Real:
                    option = SqlDataTypeOption.Real;
                    break;
                case SqlDataType.DateTime:
                    option = SqlDataTypeOption.DateTime;
                    break;
                case SqlDataType.SmallDateTime:
                    option = SqlDataTypeOption.SmallDateTime;
                    break;
                case SqlDataType.Char:
                    option = SqlDataTypeOption.Char;
                    break;
                case SqlDataType.VarChar:
                    option = SqlDataTypeOption.VarChar;
                    break;
                case SqlDataType.Text:
                    option = SqlDataTypeOption.Text;
                    break;
                case SqlDataType.NChar:
                    option = SqlDataTypeOption.NChar;
                    break;
                case SqlDataType.NVarChar:
                    option = SqlDataTypeOption.NVarChar;
                    break;
                case SqlDataType.NText:
                    option = SqlDataTypeOption.NText;
                    break;
                case SqlDataType.Binary:
                    option = SqlDataTypeOption.Binary;
                    break;
                case SqlDataType.VarBinary:
                    option = SqlDataTypeOption.VarBinary;
                    break;
                case SqlDataType.Image:
                    option = SqlDataTypeOption.Image;
                    break;
                case SqlDataType.Cursor:
                    option = SqlDataTypeOption.Cursor;
                    break;
                case SqlDataType.Variant:
                    option = SqlDataTypeOption.Sql_Variant;
                    break;
                case SqlDataType.Table:
                    option = SqlDataTypeOption.Table;
                    break;
                case SqlDataType.Timestamp:
                    option = SqlDataTypeOption.Timestamp;
                    break;
                case SqlDataType.UniqueIdentifier:
                    option = SqlDataTypeOption.UniqueIdentifier;
                    break;
                case SqlDataType.Xml:
                    //??
                    break;
                case SqlDataType.Date:
                    option = SqlDataTypeOption.Date;
                    break;
                case SqlDataType.Time:
                    option = SqlDataTypeOption.Time;
                    break;
                case SqlDataType.DateTime2:
                    option = SqlDataTypeOption.DateTime2;

                    break;
                case SqlDataType.DateTimeOffset:
                    option = SqlDataTypeOption.DateTimeOffset;
                    break;
                case SqlDataType.Rowversion:
                    option = SqlDataTypeOption.Rowversion;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            return new SqlDataTypeReference
            {
                SqlDataTypeOption = option
            };
        }

        private ScalarExpression GetDefaultValue(SqlDataTypeReference type)
        {
            switch (type.SqlDataTypeOption)
            {
                case SqlDataTypeOption.SmallInt:
                case SqlDataTypeOption.TinyInt:
                case SqlDataTypeOption.Bit:
                case SqlDataTypeOption.Decimal:
                case SqlDataTypeOption.Numeric:
                case SqlDataTypeOption.Money:
                case SqlDataTypeOption.SmallMoney:
                case SqlDataTypeOption.Float:
                case SqlDataTypeOption.Real:
                case SqlDataTypeOption.BigInt:
                case SqlDataTypeOption.Int:
                    return new IntegerLiteral {Value = "0"};

                case SqlDataTypeOption.Char:
                case SqlDataTypeOption.VarChar:
                case SqlDataTypeOption.Text:
                case SqlDataTypeOption.NChar:
                case SqlDataTypeOption.NVarChar:
                case SqlDataTypeOption.NText:

                    return new StringLiteral {Value = ""};

                case SqlDataTypeOption.Binary:
                case SqlDataTypeOption.VarBinary:
                case SqlDataTypeOption.Image:
                    return new BinaryLiteral {Value = "0"};


                case SqlDataTypeOption.UniqueIdentifier:
                    return new StringLiteral {Value = Guid.NewGuid().ToString()};


                case SqlDataTypeOption.DateTime:
                case SqlDataTypeOption.SmallDateTime:
                case SqlDataTypeOption.Date:
                case SqlDataTypeOption.DateTime2:
                    return new StringLiteral {Value = "1980-04-01"}; //yes i did do that

                case SqlDataTypeOption.Time:
                    return new StringLiteral {Value = "11:59:59"}; //yes i did do that
            }

            return new StringLiteral {Value = "0"};
        }
    }
}