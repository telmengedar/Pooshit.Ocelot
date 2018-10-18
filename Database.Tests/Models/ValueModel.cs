namespace NightlyCode.Database.Tests.Models {

    public class ValueModel {
        public ValueModel() { }

        public ValueModel(int integer, float single=0.0f, double d=0.0, string s=null) {
            Integer = integer;
            Single = single;
            Double = d;
            String = s;
        }

        public int Integer { get; set; }
        public float Single { get; set; }
        public double Double { get; set; }
        public string String { get; set; }
    }
}