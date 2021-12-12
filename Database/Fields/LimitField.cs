using NightlyCode.Database.Tokens;

namespace NightlyCode.Database.Fields {

    /// <summary>
    /// limits the number of result rows
    /// </summary>
    public class LimitField : DBField {

        /// <summary>
        /// maximum number of rows to return
        /// </summary>
        public ISqlToken Limit { get; set; }

        /// <summary>
        /// number of rows to skip in returned result
        /// </summary>
        public ISqlToken Offset { get; set; }
    }
}