using System.Collections.Generic;
using System.Text;
using static ForthVariable;
using static Property;

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

    public void Add(string name, string value)
    {
        this.Add(name, new Property(name, value));
    }

    public void Add(string name, int value)
    {
        this.Add(name, new Property(name, value));
    }

    public void Add(string name, Dbref value)
    {
        this.Add(name, new Property(name, value));
    }

    public void Add(string name, float value)
    {
        this.Add(name, new Property(name, value));
    }

    public void Add(string name, PropertyDirectory value)
    {
        this.Add(name, new Property(name, value));
    }

    public Property? GetPropertyPathValue(string path)
    {
        path = path.TrimStart('/').TrimEnd('/');

        var firstSeparator = path.IndexOf('/');
        if (firstSeparator >= 0)
        {
            // Subdirectory needed
            var firstSegmentName = path.Substring(0, firstSeparator);
            if (this.ContainsKey(firstSegmentName))
            {
                var firstSegmentProperty = this.GetValueOrDefault(firstSegmentName);
                if (firstSegmentProperty.Type == PropertyType.Directory)
                {
                    // Exists and is a directory
                    var firstSegmentPropertyDirectory = (PropertyDirectory)firstSegmentProperty.Value;
                    return firstSegmentPropertyDirectory.GetPropertyPathValue(path.Substring(firstSeparator + 1));
                }
                else
                {
                    // Exists and is NOT a directory.  This isn't what they asked for.
                    return null;
                }
            }
            else
            {
                // Does not exist.
                return null;
            }
        }
        else
        {
            // This property directory!
            return this.GetValueOrDefault(path);
        }
    }

    public void SetPropertyPathValue(string path, ForthVariable value)
    {
        path = path.TrimStart('/').TrimEnd('/');

        var firstSeparator = path.IndexOf('/');
        if (firstSeparator >= 0)
        {
            // Subdirectory needed
            var firstSegmentName = path.Substring(0, firstSeparator);
            if (this.ContainsKey(firstSegmentName))
            {
                var firstSegmentProperty = this.GetValueOrDefault(firstSegmentName);
                if (firstSegmentProperty.Type == PropertyType.Directory)
                {
                    // Exists and is a directory
                    var firstSegmentPropertyDirectory = (PropertyDirectory)firstSegmentProperty.Value;
                    firstSegmentPropertyDirectory.SetPropertyPathValue(path.Substring(firstSeparator + 1), value);
                    return;
                }
                else
                {
                    // Exists and is NOT a directory.  Blow it away, recreate.
                    this.Remove(firstSegmentName);
                    var firstSegmentPropertyDirectory = new PropertyDirectory();
                    this.Add(firstSegmentName, firstSegmentPropertyDirectory);
                    firstSegmentPropertyDirectory.SetPropertyPathValue(path.Substring(firstSeparator + 1), value);
                    return;
                }
            }
            else
            {
                // Does not exist.  Create.
                var firstSegmentPropertyDirectory = new PropertyDirectory();
                this.Add(firstSegmentName, firstSegmentPropertyDirectory);
                firstSegmentPropertyDirectory.SetPropertyPathValue(path.Substring(firstSeparator + 1), value);
                return;
            }
        }
        else
        {
            // This property directory!
            if (this.ContainsKey(path))
                this.Remove(path);

            switch (value.Type)
            {
                case VariableType.DbRef:
                    this.Add(path, (Dbref)value.Value);
                    break;
                case VariableType.String:
                    this.Add(path, (string)value.Value);
                    break;
                case VariableType.Integer:
                    this.Add(path, (int)value.Value);
                    break;
                case VariableType.Float:
                    this.Add(path, (float)value.Value);
                    break;
                default:
                    throw new System.InvalidOperationException($"Unable to handle property type: {value.Type}");
            }
        }
    }
}