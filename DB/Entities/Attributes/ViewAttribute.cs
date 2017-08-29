using System;

namespace NightlyCode.DB.Entities.Attributes {

    /// <summary>
    /// used for entities to mark them as views
    /// </summary>
    public class ViewAttribute : Attribute {

        /// <summary>
        /// Creates a new view attribute
        /// </summary>
        /// <param name="definition"></param>
        public ViewAttribute(string definition) {
            Definition = definition;
        }

        /// <summary>
        /// definition of view
        /// </summary>
        public string Definition { get; set; }
    }
}