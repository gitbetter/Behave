using System.Collections;
using System.Collections.Generic;

public class BlackboardDatum 
{
    public string id;
    public object value;

    public BlackboardDatum(string id, object value) {
        this.id = id;
        this.value = value;
    }

    public T ConvertedValue<T>() {
        if (value is T) {
            return (T) value;
        }
        return default(T);
    }
}

public class Blackboard
{
    public Blackboard parent = null;

    Dictionary<string, BlackboardDatum> entries = new Dictionary<string, BlackboardDatum>();

    public void Set(BlackboardDatum datum) {
        entries[datum.id] = datum;
    }

    public T Get<T>(string id) {
        if (entries.ContainsKey(id)) {
            BlackboardDatum datum = entries[id];
            return datum.ConvertedValue<T>();
        } else if (parent != null) {
            return parent.Get<T>(id);
        }
        return default(T);
    }
}