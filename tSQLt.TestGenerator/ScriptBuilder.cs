using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace tSQLt.TestGenerator
{
    public class ScriptBuilder
    {
        private readonly Sql120ScriptGenerator _generator = new Sql120ScriptGenerator(new SqlScriptGeneratorOptions
        {
            AlignClauseBodies = true,
            IncludeSemicolons = true,
            IndentViewBody = true,
            AsKeywordOnOwnLine = true,
            IndentationSize = 4,
            AlignColumnDefinitionFields = true
        });

        protected string GenerateScript(TSqlFragment fragment)
        {
            string script;
            _generator.GenerateScript(fragment, out script);
            return script;
        }

        protected IList<TSqlParserToken> GetTokens(TSqlFragment fragment)
        {
            return _generator.GenerateTokens(fragment);
        }
    }
}