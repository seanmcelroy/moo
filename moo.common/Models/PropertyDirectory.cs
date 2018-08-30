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

    public Property GetPropertyPathValue(string path)
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
                    return default(Property);
                }
            }
            else
            {
                // Does not exist.
                return default(Property);
            }
        }
        else
        {
            // This property directory!
            return this.GetValueOrDefault(path);
        }
    }

    private PropertyDirectory FindPropertyPathForSet(string path)
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
                    return firstSegmentPropertyDirectory.FindPropertyPathForSet(path.Substring(firstSeparator + 1));
                }
                else
                {
                    // Exists and is NOT a directory.  Blow it away, recreate.
                    this.Remove(firstSegmentName);
                    var firstSegmentPropertyDirectory = new PropertyDirectory();
                    this.Add(firstSegmentName, firstSegmentPropertyDirectory);
                    return firstSegmentPropertyDirectory.FindPropertyPathForSet(path.Substring(firstSeparator + 1));
                }
            }
            else
            {
                // Does not exist.  Create.
                var firstSegmentPropertyDirectory = new PropertyDirectory();
                this.Add(firstSegmentName, firstSegmentPropertyDirectory);
                return firstSegmentPropertyDirectory.FindPropertyPathForSet(path.Substring(firstSeparator + 1));
            }
        }
        else
        {
            // This property directory!
            return this;
        }
    }

    public void ClearPropertyPathValue(string path)
    {
        var directory = this.FindPropertyPathForSet(path);
        directory.Clear();
    }

    public void SetPropertyPathValue(string path, PropertyType type, object value)
    {
        var pathDirectory = path.Substring(0, path.LastIndexOf('/'));
        var directory = this.FindPropertyPathForSet(path);

        // This property directory!
        path = path.TrimStart('/').TrimEnd('/');
        var lastPathPartIndex = path.LastIndexOf('/');
        var lastPathPart = path.Substring(lastPathPartIndex + 1);

        if (directory.ContainsKey(lastPathPart))
            directory.Remove(lastPathPart);

        switch (type)
        {
            case PropertyType.DbRef:
                directory.Add(lastPathPart, (Dbref)value);
                break;
            case PropertyType.String:
                directory.Add(lastPathPart, (string)value);
                break;
            case PropertyType.Integer:
                directory.Add(lastPathPart, (int)value);
                break;
            case PropertyType.Float:
                directory.Add(lastPathPart, (float)value);
                break;
            default:
                throw new System.InvalidOperationException($"Unable to handle property type: {type}");
        }
    }

    public void SetPropertyPathValue(string path, ForthVariable value)
    {
        var directory = this.FindPropertyPathForSet(path);

        // This property directory!
        path = path.TrimStart('/').TrimEnd('/');
        var lastPathPartIndex = path.LastIndexOf('/');
        var lastPathPart = path.Substring(lastPathPartIndex + 1);

        if (directory.ContainsKey(lastPathPart))
            directory.Remove(lastPathPart);

        switch (value.Type)
        {
            case VariableType.DbRef:
                directory.Add(lastPathPart, (Dbref)value.Value);
                break;
            case VariableType.String:
                directory.Add(lastPathPart, (string)value.Value);
                break;
            case VariableType.Integer:
                directory.Add(lastPathPart, (int)value.Value);
                break;
            case VariableType.Float:
                directory.Add(lastPathPart, (float)value.Value);
                break;
            default:
                throw new System.InvalidOperationException($"Unable to handle property type: {value.Type}");
        }
    }
}