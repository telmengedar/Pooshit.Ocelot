using System;

namespace Pooshit.Ocelot.Errors {

    /// <summary>
    /// thrown when a caller-supplied sql identifier does not match the accepted grammar
    /// </summary>
    public class InvalidIdentifierException : ArgumentException {

        /// <summary>
        /// creates a new <see cref="InvalidIdentifierException"/>
        /// </summary>
        /// <param name="value">offending identifier value</param>
        /// <param name="role">role the identifier was used in (table, column, alias, function, index, constraint, index type, column type, default value)</param>
        /// <param name="hint">optional message-only hint appended as a final sentence; does not affect <see cref="Role"/></param>
        public InvalidIdentifierException(string value, string role, string hint = null)
            : base(BuildMessage(value, role, hint)) {
            Value = value;
            Role = role;
        }

        static string BuildMessage(string value, string role, string hint) {
            string message = $"'{value}' is not a valid {role}, expected an unquoted identifier matching [A-Za-z_][A-Za-z0-9_]* (optionally dot-separated)";
            return string.IsNullOrEmpty(hint) ? message : $"{message} ({hint})";
        }

        /// <summary>
        /// offending identifier value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// role the identifier was used in
        /// </summary>
        public string Role { get; }
    }
}
