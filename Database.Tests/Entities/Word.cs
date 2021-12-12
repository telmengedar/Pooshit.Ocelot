using NightlyCode.Database.Entities.Attributes;

namespace NightlyCode.Database.Tests.Entities {
    
    /// <summary>
    /// word in a dictionary
    /// </summary>
    public class Word {

        /// <summary>
        /// id
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        /// <summary>
        /// id of language
        /// </summary>
        [Index("language")]
        [Unique("word")]
        public long LanguageId { get; set; }

        /// <summary>
        /// word text
        /// </summary>
        [Unique("word")]
        [Index("text")]
        public string Text { get; set; }

        /// <summary>
        /// word class
        /// </summary>
        [Unique("word")]
        [Index("class")]
        public int Class { get; set; }

        /// <summary>
        /// word flags
        /// </summary>
        [Index("flags")]
        public int Flags { get; set; }

        /// <summary>
        /// primary context word can be used in
        /// </summary>
        [Index("context")]
        public long? ContextId { get; set; }
    }
}