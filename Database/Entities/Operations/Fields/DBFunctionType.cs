namespace NightlyCode.Database.Entities.Operations.Fields {

    /// <summary>
    /// functions of db
    /// </summary>
    public enum DBFunctionType {

        /// <summary>
        /// random value
        /// </summary>
        Random,

        /// <summary>
        /// row count
        /// </summary>
        Count,

        /// <summary>
        /// unique id of row (oid)
        /// </summary>
        RowID,

        /// <summary>
        /// length of a string or text
        /// </summary>
        Length,
    }
}