namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// a prepared db operation
    /// </summary>
    public class PreparedOperation {

        /// <summary>
        /// creates a new prepared operation
        /// </summary>
        /// <param name="commandText">sql query text</param>
        /// <param name="parameters">parameters for query</param>
        public PreparedOperation(string commandText, params object[] parameters) {
            CommandText = commandText;
            Parameters = parameters;
        }

        /// <summary>
        /// text to execute
        /// </summary>
        public string CommandText { get; }

        /// <summary>
        /// parameters for command
        /// </summary>
        public object[] Parameters { get; }
    }
}