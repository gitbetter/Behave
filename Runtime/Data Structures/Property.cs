using System.Linq;
using System.Threading;
using UnityEngine;

[System.Serializable]
public class Property
{
    public string name;
    public string typeName;
    public string value;

    public void SetValue<T>(T val) {
        if (typeof(T)
                .GetMethods()
                .FirstOrDefault(x => x.Name == "ToString") != null) {
            value = val.ToString();
        }
    }

    public dynamic GetValue() {
        switch (typeName) {
        case "Int":
            if (value == null) {
                return 0;
            }
            return System.Int32.Parse(value);
        case "Float":
            if (value == null) {
                return 0.0f;
            }
            return float.Parse(value);
        case "Bool":
            if (value == null) {
                return false;
            }
            return bool.Parse(value);
        case "String":
            if (value == null) {
                return "";
            }
            return value;
        case "Semaphore":
            return new Semaphore(0, 1);
        case "Vector3":
            if (value == null) {
                return Vector3.zero;
            }
            int start = value.IndexOf('(');
            int end = value.IndexOf(')');
            int numToRemove = end - start;
            string segment = value.Substring(start+1, numToRemove-1);
            string[] segments = segment.Split(',');
            return new Vector3(
                (float) System.Convert.ToDouble(segments[0]),
                (float) System.Convert.ToDouble(segments[1]),
                (float) System.Convert.ToDouble(segments[2]));
        default:
            return null;
        }
    }

    public static Property FromString(string propName) {
        return (Property) System.Activator.CreateInstance(System.Type.GetType(propName + "Property"));
    }

    public static Property FromSavedProperty(Property prop) {
        Property newProp;
        switch (prop.typeName) {
        case "Int":
            newProp = new IntProperty();
            break;
        case "Float":
            newProp = new FloatProperty();
            break;
        case "Bool":
            newProp = new BoolProperty();
            break;
        case "String":
            newProp = new StringProperty();
            break;
        case "Semaphore":
            newProp = new SemaphoreProperty();
            break;
        case "Vector3":
            newProp = new Vector3Property();
            break;
        default:
            newProp = new IntProperty();
            break;
        }
        newProp.name = prop.name;
        newProp.typeName = prop.typeName;
        newProp.value = prop.value;
        return newProp;
    }
}

[BTEditor("Property/Int")] [System.Serializable]
public class IntProperty : Property {
    public IntProperty() {
        this.typeName = "Int";
    }
}

[BTEditor("Property/Float")] [System.Serializable]
public class FloatProperty : Property {
    public FloatProperty() {
        this.typeName = "Float";
    }
}

[BTEditor("Property/Bool")] [System.Serializable]
public class BoolProperty : Property {
    public BoolProperty() {
        this.typeName = "Bool";
    }
}

[BTEditor("Property/String")] [System.Serializable]
public class StringProperty : Property {
    public StringProperty() {
        this.typeName = "String";
    }
}

[BTEditor("Property/Semaphore")] [System.Serializable]
public class SemaphoreProperty : Property {
    public SemaphoreProperty() {
        this.typeName = "Semaphore";
    }
}

[BTEditor("Property/Vector3")] [System.Serializable]
public class Vector3Property : Property {
    public Vector3Property() {
        this.typeName = "Vector3";
    }
}
