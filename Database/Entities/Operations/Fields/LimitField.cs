namespace Database.Entities.Operations.Fields {

    /// <summary>
    /// limits the number of result rows
    /// </summary>
    public class LimitField : DBField {

        /// <summary>
        /// maximum number of rows to return
        /// </summary>
        public long? Limit { get; set; }

        /// <summary>
        /// number of rows to skip in returned result
        /// </summary>
        public long? Offset { get; set; }
    }
}