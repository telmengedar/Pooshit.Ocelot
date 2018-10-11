using NightlyCode.Database.Entities.Operations.Fields;

namespace NightlyCode.Database.Entities.Operations {

    /// <summary>
    /// criteria used for ordering of results
    /// </summary>
    public class OrderByCriteria {

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