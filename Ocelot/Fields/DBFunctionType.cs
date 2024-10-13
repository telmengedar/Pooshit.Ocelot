namespace Pooshit.Ocelot.Fields {

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

        /// <summary>
        /// id of last inserted row
        /// </summary>
        LastInsertID,

        /// <summary>
        /// all fields of type
        /// </summary>
        All,
        
        /// <summary>
        /// determines whether a value is contained in a collection
        /// </summary>
        InCollection
    }
}