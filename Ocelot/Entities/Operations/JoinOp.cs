namespace Pooshit.Ocelot.Entities.Operations {

    /// <summary>
    /// operation type of join
    /// </summary>
    public enum JoinOp {

        /// <summary>
        /// default join type
        /// </summary>
        Inner,

        /// <summary>
        /// left join
        /// </summary>
        Left,

        /// <summary>
        /// cross lateral join (inner-equivalent correlated subquery join).
        /// Postgres/MySQL: <c>INNER JOIN LATERAL ... ON TRUE</c>.
        /// MSSQL: <c>CROSS APPLY</c>.
        /// SQLite: not supported — throws <see cref="System.NotSupportedException"/> at Prepare time.
        /// </summary>
        CrossLateral,

        /// <summary>
        /// left lateral join (outer-equivalent correlated subquery join).
        /// Postgres/MySQL: <c>LEFT JOIN LATERAL ... ON TRUE</c>.
        /// MSSQL: <c>OUTER APPLY</c>.
        /// SQLite: not supported — throws <see cref="System.NotSupportedException"/> at Prepare time.
        /// </summary>
        LeftLateral
    }
}