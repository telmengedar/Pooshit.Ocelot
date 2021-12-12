using System.Text;
using System.Text.RegularExpressions;

namespace NightlyCode.Database.Info.Postgre {
    
    /// <summary>
    /// helper for postgres statement processing
    /// </summary>
    public static class PostgresStatementHelper {

        /// <summary>
        /// processes a postgres create statement retrieved from <see cref="PostgreInfo.GenerateCreateStatement"/>
        /// to be usable in clean databases
        /// </summary>
        /// <param name="statement">statement to process</param>
        /// <returns>processed statement</returns>
        public static string ProcessCreateStatement(this string statement) {
            string[] statementLines = statement.Split('\n');
            StringBuilder finalStatement = new StringBuilder();
            foreach (string statementLine in statementLines) {
                Match match = Regex.Match(statementLine, "^(?<column>[a-zA-Z0-9]+) bigint DEFAULT nextval");
                if (match.Success)
                    finalStatement.AppendLine($"{match.Groups["column"].Value} serial8 NOT NULL,");
                else finalStatement.AppendLine(statementLine);
            }

            finalStatement.Length--;
            return finalStatement.ToString();
        }
    }
}