using System;
using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Models;

public class ValueModel {
    public ValueModel() { }

    public ValueModel(int integer, float single=0.0f, double d=0.0, string s=null) {
        Integer = integer;
        Single = single;
        Double = d;
        String = s;
    }

    [DefaultValue(0)]
    public int Integer { get; set; }

    [DefaultValue(0)]
    public float Single { get; set; }

    [DefaultValue(0)]
    public double Double { get; set; }

    public string String { get; set; }

    public DateTime? NDatetime { get; set; }

    public TimeSpan Timespan { get; set; }
    
    public byte[] Blob { get; set; }
}