namespace Database.Entities.Operations {

    /// <summary>
    /// a prepared db operation
    /// </summary>
    public class PreparedOperation {

        /// <summary>
        /// creates a new prepared operation
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        public PreparedOperation(string commandText, params DBParameter[] parameters) {
            CommandText = commandText;
            Parameters = parameters;
        }

        /// <summary>
        /// text to execute
        /// </summary>
        public string CommandText { get; private set; }

        /// <summary>
        /// parameters for command
        /// </summary>
        public DBParameter[] Parameters { get; private set; }
    }
}