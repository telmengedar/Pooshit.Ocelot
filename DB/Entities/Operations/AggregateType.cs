namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// type of aggregate function
    /// </summary>
    public enum AggregateType {

        /// <summary>
        /// sums up several values
        /// </summary>
        Sum,

        /// <summary>
        /// minimum of several values
        /// </summary>
        Min,

        /// <summary>
        /// maximum of several values
        /// </summary>
        Max,

        /// <summary>
        /// average of several values
        /// </summary>
        Average
    }
}