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
            sb.Append($"<key>{kvp.Key}</key><value>{Property.Serialize(kvp.Value)}</value>");
        }
        sb.Append("</propdir>");
        return sb.ToString();
    }

    public Property Add(string name, string value)
    {
        if (name.Contains('/'))
        {
            var parts = name.Split('/');
            if (parts.Length > 1 && parts[0].Length > 0 && parts[1].Length > 1)
            {
                var propdirTitle = parts[0];
                PropertyDirectory subdir;
                if (this.ContainsKey(propdirTitle))
                {
                    subdir = (PropertyDirectory)this[propdirTitle].directory!;
                    if (subdir == null)
                        throw new System.InvalidOperationException();
                }
                else
                {
                    subdir = new PropertyDirectory();
                    this.Add(propdirTitle, subdir);
                }

                var remainingTitle = name.Substring(propdirTitle.Length + 1);
                return subdir.Add(remainingTitle, value);
            }
        }

        var newProp = new Property(name, value);
        if (this.ContainsKey(name))
            this[name] = newProp;
        else
            this.Add(name, newProp);
        return newProp;
    }

    public void Add(string name, int value)
    {
        this.Add(name, new Property(name, value));
    }

    public void Add(string name, Lock value)
    {
        this.Add(name, new Property(name, value));
    }

    public Property Add(string name, Dbref value)
    {
        if (name.Contains('/'))
        {
            var parts = name.Split('/');
            if (parts.Length > 1 && parts[0].Length > 0 && parts[1].Length > 1)
            {
                var propdirTitle = parts[0];
                PropertyDirectory subdir;
                if (this.ContainsKey(propdirTitle))
                {
                    subdir = (PropertyDirectory)this[propdirTitle].directory!;
                    if (subdir == null)
                        throw new System.InvalidOperationException();
                }
                else
                {
                    subdir = new PropertyDirectory();
                    this.Add(propdirTitle, subdir);
                }

                var remainingTitle = name.Substring(propdirTitle.Length + 1);
                return subdir.Add(remainingTitle, value);
            }
        }

        var newProp = new Property(name, value);
        if (this.ContainsKey(name))
            this[name] = newProp;
        else
            this.Add(name, newProp);
        return newProp;
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
                    var firstSegmentPropertyDirectory = (PropertyDirectory)firstSegmentProperty.Value!;
                    var nextPathPortion = path.Substring(firstSeparator + 1);
                    return firstSegmentPropertyDirectory.GetPropertyPathValue(nextPathPortion);
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

    public bool SetPropertyPathValue(string path, PropertyType type, string value)
    {
        // Coerce string value into designated type
        switch (type)
        {
            case PropertyType.DbRef:
                {
                    if (Dbref.TryParse(value, out Dbref result))
                    {
                        SetPropertyPathValue(path, type, result);
                        return true;
                    }
                    return false;
                }
            case PropertyType.Float:
                {
                    if (float.TryParse(value, out float result))
                    {
                        SetPropertyPathValue(path, type, result);
                        return true;
                    }
                    return false;
                }
            case PropertyType.Integer:
                {
                    if (int.TryParse(value, out int result))
                    {
                        SetPropertyPathValue(path, type, result);
                        return true;
                    }
                    return false;
                }
            case PropertyType.Lock:
                {
                    if (Lock.TryParse(value, out Lock result))
                    {
                        SetPropertyPathValue(path, type, result);
                        return true;
                    }
                    return false;
                }
            case PropertyType.String:
                SetPropertyPathValue(path, type, (object)value);
                return true;
            default:
                throw new System.InvalidOperationException($"Unable to handle property type: {type}");
        }
    }

    public void SetPropertyPathValue(string path, PropertyType type, object value)
    {
        var pathDirectory = path.LastIndexOf('/') == -1 ? path : path.Substring(0, path.LastIndexOf('/'));
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
            case PropertyType.Lock:
                directory.Add(lastPathPart, (Lock)value);
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
                directory.Add(lastPathPart, value.Value == null ? Dbref.NOT_FOUND : (Dbref)value.Value);
                break;
            case VariableType.String:
                if (value.Value != null)
                    directory.Add(lastPathPart, (string)value.Value);
                break;
            case VariableType.Integer:
                if (value.Value != null)
                    directory.Add(lastPathPart, (int)value.Value);
                break;
            case VariableType.Float:
                if (value.Value != null)
                    directory.Add(lastPathPart, (float)value.Value);
                break;
            default:
                throw new System.InvalidOperationException($"Unable to handle property type: {value.Type}");
        }
    }
}