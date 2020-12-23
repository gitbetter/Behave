using System.Threading;

[System.Serializable]
public abstract class Property
{
    public object value;
}

[BTEditor("Property/Int")] [System.Serializable]
public class IntProperty : Property {
    public new int value;
}

[BTEditor("Property/Float")] [System.Serializable]
public class FloatProperty : Property {
    public new float value;
}

[BTEditor("Property/Bool")] [System.Serializable]
public class BoolProperty : Property {
    public new bool value;
}

[BTEditor("Property/String")] [System.Serializable]
public class StringProperty : Property {
    public new string value;
}

[BTEditor("Property/Semaphore")] [System.Serializable]
public class SemaphoreProperty : Property {
    public new Semaphore value;
}
