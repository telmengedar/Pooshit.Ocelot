using Pooshit.Ocelot.CustomTypes;

namespace Pooshit.Ocelot.Schemas {
    
    /// <summary>
    /// db types which should be available for database columns
    /// </summary>
    public static class Types {

        /// <summary>
        /// <see cref="String"/>
        /// </summary>
        public const string CharacterVarying = "character varying";

        /// <summary>
        /// <see cref="DateTime"/>
        /// </summary>
        public const string DateTime = "datetime";

        /// <summary>
        /// <see cref="DateTime"/>
        /// </summary>
        public const string TimestampWithoutTimezone = "timestamp without time zone";
        
        /// <summary>
        /// <see cref="Timespan"/>
        /// </summary>
        public const string Timespan = "timespan";

        /// <summary>
        /// <see cref="Guid"/>
        /// </summary>
        public const string Guid = "guid";

        /// <summary>
        /// <see cref="string"/>
        /// </summary>
        public const string String = "string";

        /// <summary>
        /// <see cref="Version"/>
        /// </summary>
        public const string Version = "version";

        /// <summary>
        /// <see cref="Char"/>
        /// </summary>
        public const string Char = "char";

        /// <summary>
        /// <see cref="Byte"/>
        /// </summary>
        public const string Byte = "byte";

        /// <summary>
        /// <see cref="SByte"/>
        /// </summary>
        public const string SByte = "sbyte";

        /// <summary>
        /// <see cref="Short"/>
        /// </summary>
        public const string Short = "short";

        /// <summary>
        /// <see cref="Int"/>
        /// </summary>
        public const string Int = "int";

        /// <summary>
        /// <see cref="Int"/>
        /// </summary>
        public const string Int2 = "int2";

        /// <summary>
        /// <see cref="Int"/>
        /// </summary>
        public const string Int4 = "int4";

        /// <summary>
        /// <see cref="Int"/>
        /// </summary>
        public const string Int8 = "int8";

        /// <summary>
        /// <see cref="Int"/>
        /// </summary>
        public const string Integer = "integer";

        /// <summary>
        /// <see cref="Long"/>
        /// </summary>
        public const string Long = "long";

        /// <summary>
        /// <see cref="Int16"/>
        /// </summary>
        public const string Int16 = "int16";

        /// <summary>
        /// <see cref="Int32"/>
        /// </summary>
        public const string Int32 = "int32";

        /// <summary>
        /// <see cref="Int64"/>
        /// </summary>
        public const string Int64 = "int64";

        /// <summary>
        /// <see cref="Int64"/>
        /// </summary>
        public const string BigInt = "bigint";

        /// <summary>
        /// <see cref="UShort"/>
        /// </summary>
        public const string UShort = "ushort";

        /// <summary>
        /// <see cref="UInt"/>
        /// </summary>
        public const string UInt = "uint";

        /// <summary>
        /// <see cref="ULong"/>
        /// </summary>
        public const string ULong = "ulong";

        /// <summary>
        /// <see cref="UInt16"/>
        /// </summary>
        public const string UInt16 = "uint16";

        /// <summary>
        /// <see cref="UInt32"/>
        /// </summary>
        public const string UInt32 = "uint32";

        /// <summary>
        /// <see cref="UInt64"/>
        /// </summary>
        public const string UInt64 = "uint64";
        
        /// <summary>
        /// <see cref="Single"/>
        /// </summary>
        public const string Single = "single";

        /// <summary>
        /// <see cref="Single"/>
        /// </summary>
        public const string SinglePrecision = "single precision";

        /// <summary>
        /// <see cref="Single"/>
        /// </summary>
        public const string Real = "real";

        /// <summary>
        /// <see cref="Float"/>
        /// </summary>
        public const string Float = "float";

        /// <summary>
        /// <see cref="Float"/>
        /// </summary>
        public const string Float4 = "float4";

        /// <summary>
        /// <see cref="Double"/>
        /// </summary>
        public const string Float8 = "float8";

        /// <summary>
        /// <see cref="Double"/>
        /// </summary>
        public const string Double = "double";
        
        /// <summary>
        /// <see cref="Double"/>
        /// </summary>
        public const string DoublePrecision = "double precision";
        
        /// <summary>
        /// <see cref="Decimal"/>
        /// </summary>
        public const string Decimal = "decimal";

        /// <summary>
        /// <see cref="Decimal"/>
        /// </summary>
        public const string Numeric = "numeric";

        /// <summary>
        /// <see cref="Bool"/>
        /// </summary>
        public const string Bool = "bool";

        /// <summary>
        /// <see cref="Boolean"/>
        /// </summary>
        public const string Boolean = "boolean";

        /// <summary>
        /// <see cref="System.Byte"/>[]
        /// </summary>
        public const string ByteA = "bytea";

        /// <summary>
        /// <see cref="System.Byte"/>[]
        /// </summary>
        public const string ByteArray = "byte[]";

        /// <summary>
        /// <see cref="System.Byte"/>[]
        /// </summary>
        public const string Blob = "blob";

        /// <summary>
        /// <see cref="System.Numerics.BigInteger"/>
        /// </summary>
        public const string BigInteger = "biginteger";
        
        /// <summary>
        /// <see cref="Range{T}"/>
        /// </summary>
        public const string NumericRange = "numrange";
        
        /// <summary>
        /// <see cref="Range{T}"/>
        /// </summary>
        public const string IntRange = "int4range";
        
        /// <summary>
        /// <see cref="Range{T}"/>
        /// </summary>
        public const string LongRange = "int8range";
        
        /// <summary>
        /// <see cref="Range{T}"/>
        /// </summary>
        public const string DateRange = "daterange";

        /// <summary>
        /// <see cref="float"/>[]
        /// </summary>
        public const string SingleArray = "single[]";
        
        /// <summary>
        /// <see cref="float"/>[]
        /// </summary>
        public const string RealArray = "real[]";
        
        /// <summary>
        /// <see cref="float"/>[]
        /// </summary>
        public const string Float4Array = "real[]";
    }
}