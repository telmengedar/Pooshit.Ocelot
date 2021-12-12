namespace NightlyCode.Database.Statements {
    
    /// <summary>
    /// options for a truncate statement
    /// </summary>
    public class TruncateOptions {
        
        /// <summary>
        /// indicates whether to reset auto increment identity
        /// </summary>
        public bool ResetIdentity { get; set; }
    }
}