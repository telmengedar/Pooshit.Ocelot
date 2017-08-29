using System;
using System.Linq.Expressions;
using NightlyCode.DB.Entities.Descriptors;
using NightlyCode.DB.Info;

namespace NightlyCode.DB.Entities.Operations {

    /// <summary>
    /// function of db
    /// </summary>
    public class DBFunction : DBField {

        DBFunction(DBFunctionType function, Expression parameter=null) {
            Type = function;
            Parameter = parameter;
        }

        /// <summary>
        /// type of function
        /// </summary>
        public DBFunctionType Type { get; }

        /// <summary>
        /// expression describing the field
        /// </summary>
        public Expression Parameter { get; }

        public override void PrepareCommand(OperationPreparator preparator, IDBInfo dbinfo, Func<Type, EntityDescriptor> descriptorgetter) {
            switch(Type) {
            case DBFunctionType.Random:
                preparator.CommandBuilder.Append("RANDOM()");
                break;
            case DBFunctionType.Count:
                preparator.CommandBuilder.Append("COUNT(*)");
                break;
            case DBFunctionType.RowID:
                switch(dbinfo.Type) {
                case DBType.SQLite:
                    preparator.CommandBuilder.Append("ROWID");
                    break;
                case DBType.Postgre:
                    preparator.CommandBuilder.Append("OID");
                    break;
                }
                break;
            case DBFunctionType.Length:
                switch(dbinfo.Type) {
                case DBType.SQLite:
                    preparator.CommandBuilder.Append("LENGTH(");
                    CriteriaVisitor.GetCriteriaText(Parameter, descriptorgetter, dbinfo, preparator);
                    preparator.CommandBuilder.Append(")");
                    break;
                case DBType.Postgre:
                    preparator.CommandBuilder.Append("char_length(");
                    CriteriaVisitor.GetCriteriaText(Parameter, descriptorgetter, dbinfo, preparator);
                    preparator.CommandBuilder.Append(")");
                    break;
                }
                break;
            }
        }

        /// <summary>
        /// random value
        /// </summary>
        public static DBFunction Random => new DBFunction(DBFunctionType.Random);

        /// <summary>
        /// count the rows of the result
        /// </summary>
        public static DBFunction Count => new DBFunction(DBFunctionType.Count);

        /// <summary>
        /// table wide unique id of row
        /// </summary>
        public static DBFunction RowID => new DBFunction(DBFunctionType.RowID);

        /// <summary>
        /// length of a text or blob
        /// </summary>
        /// <typeparam name="T">type of entity containing the text or blob</typeparam>
        /// <param name="fieldexpression">expression targeting the field to measure</param>
        /// <returns></returns>
        public static DBFunction Length<T>(Expression<Func<T, object>> fieldexpression) {
            return new DBFunction(DBFunctionType.Length, fieldexpression);
        }
    }
}