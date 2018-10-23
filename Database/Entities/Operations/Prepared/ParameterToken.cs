using System;
using NightlyCode.Database.Info;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// token representing a parameter
    /// </summary>
    public class ParameterToken : IOperationToken {

        /// <summary>
        /// creates a new parameter token
        /// </summary>
        /// <param name="isArray">true if parameter will contain an array, false otherwise</param>
        public ParameterToken(bool isArray) {
            IsArray = isArray;
            IsConstant = false;
            Value = null;
        }

        /// <summary>
        /// creates a new <see cref="ParameterToken"/>
        /// </summary>
        /// <param name="value">value of constant parameter</param>
        public ParameterToken(object value) {
            Value = value;
            IsConstant = true;
            IsArray = value is Array && !(value is byte[]);
        }

        /// <summary>
        /// determines whether the parameter value is an array
        /// </summary>
        public bool IsArray { get; }

        /// <summary>
        /// determines whether the value is predefined or to be specified on execution
        /// </summary>
        public bool IsConstant { get; }

        /// <summary>
        /// value of constant
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// index for parameter
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// get text for database command
        /// </summary>
        /// <param name="dbinfo">database specific information</param>
        /// <returns>text representing this token</returns>
        public string GetText(IDBInfo dbinfo) {
            if(IsArray)
                return $"[{Index}]";
            return $"{dbinfo.Parameter}{Index}";
        }
    }
}