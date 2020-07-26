using System;
using System.Linq;
using NightlyCode.Database.Entities.Descriptors;
using NightlyCode.Database.Entities.Operations.Fields.Sql;
using NightlyCode.Database.Entities.Operations.Prepared;
using NightlyCode.Database.Errors;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Fields.Sql {

    /// <summary>
    /// field referencing a property of an entity
    /// </summary>
    public class PropName : SqlField {

        /// <summary>
        /// creates a new <see cref="PropName"/>
        /// </summary>
        /// <param name="entity">entity of which property is to be referenced</param>
        /// <param name="propertyname">name of property to reference</param>
        /// <param name="ignorecase">ignores casing when looking up the property</param>
        public PropName(Type entity, string propertyname, bool ignorecase = false) {
            Entity = entity;
            Property = propertyname;
            IgnoreCase = ignorecase;
        }

        /// <summary>
        /// entity of which property is to be referenced
        /// </summary>
        public Type Entity { get; }

        /// <summary>
        /// name of property to reference
        /// </summary>
        /// <remarks>
        /// property has to exist, else this field will throw an exception on evaluation
        /// </remarks>
        public string Property { get; }

        /// <summary>
        /// ignores casing when looking up the property
        /// </summary>
        public bool IgnoreCase { get; }

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            EntityDescriptor model = models(Entity);
            EntityColumnDescriptor columnmodel;

            if(IgnoreCase) {
                string property = Property.ToLower();
                columnmodel = model.Columns.FirstOrDefault(cd => cd.Property.Name.ToLower() == property);
                if(columnmodel == null)
                    throw new PropertyNotFoundException(Property);
            }
            else {
                if(!model.TryGetColumnByProperty(Property, out columnmodel))
                    throw new PropertyNotFoundException(Property);
            }

            if(!string.IsNullOrEmpty(tablealias))
                preparator.AppendText($"{tablealias}.{dbinfo.MaskColumn(columnmodel.Name)}");
            else
                preparator.AppendText(dbinfo.MaskColumn(columnmodel.Name));
        }
    }

    /// <summary>
    /// references a property of an entity in a statement
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropName<T> : PropName {

        /// <summary>
        /// creates a new <see cref="PropName{T}"/>
        /// </summary>
        /// <param name="propertyname">name of property to be referenced</param>
        /// <param name="ignorecase">ignores casing when looking up property</param>
        public PropName(string propertyname, bool ignorecase = false)
            : base(typeof(T), propertyname, ignorecase) {
        }
    }
}