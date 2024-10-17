using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Models
{

    /// <summary>
    /// index value for an object
    /// </summary>
    public abstract class ObjectIndex<T>
    {

        /// <summary>
        /// id of object for which index is stored
        /// </summary>
        [PrimaryKey]
        public long Object { get; set; }

        /// <summary>
        /// key under which index is stored
        /// </summary>
        //[PrimaryKey]
        public string Key { get; set; }

        /// <summary>
        /// index value
        /// </summary>
        public T Value { get; set; }
    }

    /// <summary>
    /// decimal values used as index for an object
    /// </summary>
    [Table("objectIndexDecimal")]
    public class ObjectIndexDecimal : ObjectIndex<decimal>
    {

    }

    /// <summary>
    /// string values used as index for an object
    /// </summary>
    [Table("objectIndexString")]
    public class ObjectIndexString : ObjectIndex<string>
    {

    }
}
