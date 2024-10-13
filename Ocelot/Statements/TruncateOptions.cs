using Pooshit.Ocelot.Clients;

namespace Pooshit.Ocelot.Statements {
    
    /// <summary>
    /// options for a truncate statement
    /// </summary>
    public class TruncateOptions {
        
        /// <summary>
        /// indicates whether to reset auto increment identity
        /// </summary>
        public bool ResetIdentity { get; set; }

        /// <summary>
        /// transaction to use
        /// </summary>
        public Transaction Transaction { get; set; }
    }
}