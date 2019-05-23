namespace NightlyCode.Database.Entities.Operations.Tables {

    /// <summary>
    /// criteria for a <see cref="LoadDataOperation"/>
    /// </summary>
    public class LoadCriteria {

        /// <summary>
        /// creates a new <see cref="LoadCriteria"/>
        /// </summary>
        /// <param name="column">column for criteria</param>
        /// <param name="operator">operator used to compare column to</param>
        /// <param name="value">value to compare against</param>
        /// <param name="type">type how criteria is linked</param>
        public LoadCriteria(string column, string @operator, string value, CriteriaOperator type=CriteriaOperator.AND) {
            Column = column;
            Operator = @operator;
            Value = value;
            Type = type;
        }

        /// <summary>
        /// column to use
        /// </summary>
        public string Column { get; }

        /// <summary>
        /// operator for criteria
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// criteria value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// how criteria is linked to other criterias
        /// </summary>
        public CriteriaOperator Type { get; }
    }
}