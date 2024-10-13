using System;
using System.IO;

namespace Pooshit.Ocelot.Entities.Attributes {

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

        /// <summary>
        /// get view definition text for a type
        /// </summary>
        /// <param name="type">type to check for view definition sql</param>
        /// <returns>view definition text</returns>
        public static string GetViewDefinition(Type type) {
            ViewAttribute viewdef = (ViewAttribute)GetCustomAttribute(type, typeof(ViewAttribute));
            if(viewdef == null)
                return null;

            Stream definitionstream = type.Assembly.GetManifestResourceStream(viewdef.Definition);
            if(definitionstream == null)
                throw new ArgumentException($"View definition resource '{viewdef.Definition}' does not exist");

            using(StreamReader sr = new StreamReader(definitionstream)) {
                return sr.ReadToEnd();
            }
        }
    }
}