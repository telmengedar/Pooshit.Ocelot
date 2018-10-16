using NightlyCode.Database.Entities.Operations.Fields;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// criteria used for ordering of results
    /// </summary>
    public class OrderByCriteria {

        /// <summary>
        /// creates a new <see cref="OrderByCriteria"/>
        /// </summary>
        /// <param name="field">field by which to order result set</param>
        /// <param name="ascending">whether to sort ascending</param>
        public OrderByCriteria(IDBField field, bool @ascending=true) {
            Field = field;
            Ascending = @ascending;
        }

        /// <summary>
        /// fields by which to order result
        /// </summary>
        public IDBField Field { get; set; }

        /// <summary>
        /// whether to order ascending or descending
        /// </summary>
        public bool Ascending { get; set; }
    }
}