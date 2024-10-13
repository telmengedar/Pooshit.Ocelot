using System;
using System.Reflection;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Values {
    
    /// <summary>
    /// token using a <see cref="PropertyInfo"/> to specify a property
    /// </summary>
    public class PropertyInfoToken : SqlToken {
        
        /// <summary>
        /// creates a new <see cref="PropertyInfo"/>
        /// </summary>
        /// <param name="property">property to load</param>
        /// <param name="alias">alias to use</param>
        public PropertyInfoToken(PropertyInfo property, string alias=null) {
            Alias = alias;
            Property = property;
        }

        /// <summary>
        /// alias to use
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// property to load
        /// </summary>
        public PropertyInfo Property { get; }

        /// <inheritdoc />
        public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
            EntityDescriptor model = models(Property.ReflectedType);

            string table = !string.IsNullOrEmpty(tablealias) ? tablealias : model.TableName;
            preparator.AppendText($"{dbinfo.MaskColumn(table)}.{dbinfo.MaskColumn(model.GetColumnByProperty(Property.Name).Name)}");
        }
    }
}