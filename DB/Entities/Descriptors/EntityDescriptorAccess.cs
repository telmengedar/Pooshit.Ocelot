using System;
using System.Linq;
using System.Linq.Expressions;
using NightlyCode.DB.Entities.Operations.Expressions;

namespace NightlyCode.DB.Entities.Descriptors {

    /// <summary>
    /// allows to modify entity model descriptions
    /// </summary>
    /// <remarks>
    /// this is useful if description can't be specified using attributes
    /// if entity descriptions are modified after the model has been updated in database (usually after application start)
    /// the behavior is at least undefined (if anything works at all)
    /// </remarks>
    public class EntityDescriptorAccess<T> {
        readonly EntityDescriptor descriptor;
        readonly ColumnVisitor visitor;

        /// <summary>
        /// creates a new <see cref="EntityDescriptorAccess{T}"/>
        /// </summary>
        /// <param name="descriptor">descriptor to modify</param>
        public EntityDescriptorAccess(EntityDescriptor descriptor) {
            this.descriptor = descriptor;
            visitor = new ColumnVisitor(descriptor);
        }

        /// <summary>
        /// descriptor modified by the accessor
        /// </summary>
        public EntityDescriptor Descriptor => descriptor;

        /// <summary>
        /// sets name of table in which model is stored
        /// </summary>
        /// <param name="tablename">name of table</param>
        public EntityDescriptorAccess<T> Table(string tablename)
        {
            descriptor.TableName = tablename;
            return this;
        }

        /// <summary>
        /// sets primary key of <see cref="EntityDescriptor"/>
        /// </summary>
        /// <param name="column">column to use as primary key</param>
        public EntityDescriptorAccess<T> PrimaryKey(Expression<Func<T, object>> column) {
            if(descriptor.PrimaryKeyColumn != null)
                descriptor.PrimaryKeyColumn.PrimaryKey = false;
            descriptor.PrimaryKeyColumn = descriptor.GetColumn(visitor.GetColumnName(column));
            descriptor.GetColumn(visitor.GetColumnName(column)).PrimaryKey = true;
            return this;
        }

        /// <summary>
        /// adds an index to the <see cref="EntityDescriptor"/>
        /// </summary>
        /// <param name="name">name of index</param>
        /// <param name="columns">columns to be included in index</param>
        public EntityDescriptorAccess<T> Index(string name, params Expression<Func<T, object>>[] columns) {
            descriptor.AddIndex(new IndexDescriptor(name, columns.Select(visitor.GetColumnName)));
            return this;
        }

        /// <summary>
        /// adds an unique index to the <see cref="EntityDescriptor"/>
        /// </summary>
        /// <param name="columns">columns to be included in index</param>
        public EntityDescriptorAccess<T> Unique(params Expression<Func<T, object>>[] columns)
        {
            if (columns.Length == 0)
                return this;

            if (columns.Length > 1)
            {
                descriptor.AddUnique(new UniqueDescriptor(columns.Select(visitor.GetColumnName)));
            }
            else
            {
                descriptor.GetColumn(visitor.GetColumnName(columns[0])).IsUnique = true;
            }
            
            return this;
        }

        /// <summary>
        /// drops an unique index from the <see cref="EntityDescriptor"/>
        /// </summary>
        /// <param name="columns">columns which make up the index</param>
        public EntityDescriptorAccess<T> DropUnique(params Expression<Func<T, object>>[] columns)
        {
            if (columns.Length == 0)
                return this;

            if (columns.Length == 1)
                descriptor.GetColumn(visitor.GetColumnName(columns[0])).IsUnique = false;
            else descriptor.RemoveUnique(columns.Select(visitor.GetColumnName).ToArray());

            return this;
        }

        /// <summary>
        /// flags columns not nullable
        /// </summary>
        /// <param name="columns">columns to flag not nullable</param>
        public EntityDescriptorAccess<T> NotNull(params Expression<Func<T, object>>[] columns) {
            foreach (EntityColumnDescriptor column in columns.Select(c => descriptor.GetColumn(visitor.GetColumnName(c))))
                column.NotNull = true;
            return this;
        }

        /// <summary>
        /// flags columns nullable
        /// </summary>
        /// <param name="columns">columns to flag nullable</param>
        public EntityDescriptorAccess<T> Nullable(params Expression<Func<T, object>>[] columns)
        {
            foreach (EntityColumnDescriptor column in columns.Select(c => descriptor.GetColumn(visitor.GetColumnName(c))))
                column.NotNull = false;
            return this;
        }

        /// <summary>
        /// flags columns to auto increment their values on insert
        /// </summary>
        /// <param name="columns">columns to flag as auto increment</param>
        public EntityDescriptorAccess<T> AutoIncrement(params Expression<Func<T, object>>[] columns)
        {
            foreach (EntityColumnDescriptor column in columns.Select(c => descriptor.GetColumn(visitor.GetColumnName(c))))
                column.AutoIncrement = true;
            return this;
        }

        /// <summary>
        /// sets a default value for a column
        /// </summary>
        /// <param name="column">column for which to set a default value</param>
        /// <param name="value">value to use as default</param>
        public EntityDescriptorAccess<T> Default(Expression<Func<T, object>> column, object value) {
            descriptor.GetColumn(visitor.GetColumnName(column)).DefaultValue = value;
            return this;
        }

        /// <summary>
        /// changes the default column name
        /// </summary>
        /// <param name="column">column of which to change the name</param>
        /// <param name="name">new column name</param>
        public EntityDescriptorAccess<T> Column(Expression<Func<T, object>> column, string name)
        {
            descriptor.ChangeColumnName(descriptor.GetColumn(visitor.GetColumnName(column)), name);
            return this;
        }
    }
}