using System;
using System.Linq;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Errors;
using Pooshit.Ocelot.Info;
using Pooshit.Ocelot.Tokens;

namespace Pooshit.Ocelot.Fields.Sql {

    /// <summary>
    /// field referencing a property of an entity
    /// </summary>
    public class PropName : SqlToken {
        
        /// <summary>
        /// creates a new <see cref="PropName"/>
        /// </summary>
        /// <param name="entity">entity of which property is to be referenced</param>
        /// <param name="propertyname">name of property to reference</param>
        /// <param name="ignorecase">ignores casing when looking up the property</param>
        /// <param name="alias">alias to use</param>
        public PropName(Type entity, string propertyname, bool ignorecase = false, string alias=null) {
            Entity = entity;
            Property = propertyname;
            IgnoreCase = ignorecase;
            Alias = alias;
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

        /// <summary>
        /// alias to use
        /// </summary>
        public string Alias { get; set; }
        
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

            if(!string.IsNullOrEmpty(Alias))
                preparator.AppendText($"{Alias}.{dbinfo.MaskColumn(columnmodel.Name)}");
            else if(!string.IsNullOrEmpty(tablealias))
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
        /// <param name="alias">alias to use</param>
        public PropName(string propertyname, bool ignorecase = false, string alias=null)
            : base(typeof(T), propertyname, ignorecase, alias) {
        }
    }
}