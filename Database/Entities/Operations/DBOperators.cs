using System;

namespace Database.Entities.Operations {

    /// <summary>
    /// operators for database operations
    /// </summary>
    public static class DBOperators {
        
        /// <summary>
        /// determines whether a string is like another instance of a string
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool Like(this string lhs, string rhs) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// replaces every occurence of src in lhs with target
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string Replace(this string lhs, string src, string target) {
            throw new NotImplementedException();
        }
    }
}