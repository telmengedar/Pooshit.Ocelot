namespace NightlyCode.DB {

    /// <summary>
    /// command of database
    /// </summary>
    public class DBCommand {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="arguments"></param>
        public DBCommand(string text, params object[] arguments) {
            Text = text;
            Arguments = arguments;
        }

        /// <summary>
        /// text to be executed
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// arguments for command
        /// </summary>
        public object[] Arguments { get; private set; }
    }
}
