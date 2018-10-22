using NightlyCode.Database.Info;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// token representing a raw command text
    /// </summary>
    public class CommandTextToken : IOperationToken {

        /// <summary>
        /// creates a new <see cref="CommandTextToken"/>
        /// </summary>
        /// <param name="text">raw command text</param>
        public CommandTextToken(string text) {
            Text = text;
        }

        /// <summary>
        /// command text
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// get text for database command
        /// </summary>
        /// <param name="dbinfo">database specific information</param>
        /// <returns>text representing this token</returns>
        public string GetText(IDBInfo dbinfo) {
            return Text;
        }
    }
}