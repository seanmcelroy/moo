using System.Collections.Generic;
using System.Text;

public class PropertyDirectory : Dictionary<string, Property>
{
    public static string Serialize(PropertyDirectory value)
    {
        if (value == null)
            return $"<propdir/>";

        var sb = new StringBuilder();
        sb.Append("<propdir>");
        foreach (var kvp in value)
        {
            sb.AppendFormat($"<key>{kvp.Key}</key><value>{Property.Serialize(kvp.Value)}</value>");
        }
        sb.Append("</propdir>");
        return sb.ToString();
    }

    public void Add(string name, string value) {
        this.Add(name, new Property(name, value));
    }

    public void Add(string name, int value) {
        this.Add(name, new Property(name, value));
    }

    public void Add(string name, Dbref value) {
        this.Add(name, new Property(name, value));
    }

    public void Add(string name, float value) {
        this.Add(name, new Property(name, value));
    }

    public void Add(string name, PropertyDirectory value) {
        this.Add(name, new Property(name, value));
    }
}