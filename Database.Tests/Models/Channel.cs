using NightlyCode.Database.Entities.Attributes;

 namespace NightlyCode.Database.Tests.Models {
    
    /// <summary>
    /// channel of a chat service
    /// </summary>
    public class Channel {

        /// <summary>
        /// id of channel
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        
        /// <summary>
        /// name of service
        /// </summary>
        [Unique("channel")]
        public string Service { get; set; }
        
        /// <summary>
        /// name or id of channel
        /// </summary>
        [Unique("channel")]
        public string Name { get; set; }

        /// <summary>
        /// flag whether bot is connected to channel
        /// </summary>
        [Index("joined")]
        public bool Joined { get; set; }
    }
}