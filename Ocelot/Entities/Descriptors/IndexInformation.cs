using System.Collections.Generic;

namespace Pooshit.Ocelot.Entities.Descriptors; 

public class IndexInformation {
    
    public IndexInformation(string type) {
        Type = type;
    }

    public string Type { get; set; }
    public List<string> Columns { get; } = new List<string>();
}