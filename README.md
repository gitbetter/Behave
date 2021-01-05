# Behave

Behave is a simple Unity node-based editor for creating and editing behavior trees, with an intuitive C# API.

## Installation

To setup Behave in your project simply got to _Window -> Package Manager_, click the _+_ icon on the top left and select _Add package from git URL_ to add the Behave git URL.

## Usage

You can now make a new behavior tree by going to _Assets -> Create -> Behavior Tree Editor -> Behavior Tree_.
After editing your behavior tree with the editor, you can reference the generated scriptable asset file from any of your scripts and use the `Root` property to access the root of the generated behavior tree. The `Root` property is of type `BehaviorTree`, which derives indirectly from the abstract `Task` class, which provides a common interface into a behavior tree task. Functionality and extensibility is described in the sections below.

## Behavior Tree API

The behavior tree API is based on a `Task` abstract class that can be derived to provide custom functionality. You can derive from `Task` and provide a `bool Run(Blackboard board)` override to create your own task, or use one of the provided tasks. The built-in tasks are:
* Selector
* Sequence
* NonDeterministicSelector
* NonDeterministicSequence
* Decorator
* Inverter
* UntilFail
* Limit
* SemaphoreGuard
* Wait
* Parallel
* Interrupter
* PerformInterruption
* BlackboardManager
* BehaviorTree

In order to create a behavior tree programmatically, you can compose constructor calls for the desired tasks. Here's an example:
```csharp
Task behaviorTree = new Sequence(
    new NonDeterministicSelector(
        new UntilFail(
            new CustomDecorator1(),
            new CustomDecorator2()
        )
    ),
    new Limit(
        new CustomDecorator(),
        100
    )
);
```
The resulting behavior tree will automatically be generated every time you access the `Root` property of your editor generated scriptable object, so it is recommended that you cache the result of `Root` somewhere in your script.

## Extending Behavior Tree

You can derive from any of the aforementioned `Task` subclasses, or create your own `Task` subclass in order to extend the behavior tree implementation to suit your needs. In order for your task to show up as an editor context menu option when using the behavior tree editor, you need to add a `BTEditor` attribute to the top of your class definition. Here's an example:
```csharp
[BTEditor("Behavior Tree/CustomTask1")]
public class CustomTask1 : Task {
    // Do some stuff
}
```
The argument to the `BTEditor` attribute specifies what the context menu path and name of the node should be. It is recommended that you use the "Behavior Tree/..." context menu path so that all behavior tree nodes are grouped together, but you are free to follow your own conventions.
The `BTEditor` attribute can also include an additional argument to specify a path to a texture that will be used as the editor node image, if that is something you'd like to include.
```csharp
[BTEditor("Behavior Tree/CustomDecorator1", texturePath = "some/texture/path/here")]
public class CustomDecorator1 : Decorator {
    // Do some stuff
}
```
In order to have class fields show up as node fields in the behavior tree editor, you should mark them with the `BTEditable` attribute
```csharp
[BTEditor("Behavior Tree/CustomTaskWithProperties")]
public class CustomTaskWithProperties : Task {
    [BTEditable]
    public float maxSum;

    [BTEditable]
    public bool hasMaxSum;

    [BTEditable]
    float sumSoFar;
}
```

## Extending Editor Properties

The behavior tree editor also includes a set of properties that you can link into any node to provide values to fields. All of these properties extend from the abstract `Property` class, which has a single `object` field named `value`. The visual property types include:
* IntProperty
* FloatProperty
* BoolProperty
* StringProperty
* SemaphoreProperty

You can extend the `Property` abstract class to define your own visual properties. Make sure to use the `BTEditor` attribute to make them available in the editor context menu.
```csharp
[BTEditor("Property/CustomProperty")] [System.Serializable]
public class CustomProperty : Property {
    public new CustomClassType value;
}
```

## Blackboards

Behave includes a simple blackboard data structure definition to be used in saving and sharing data within your behavior trees or as a general centralized data store across your application. Simply create a new `Blackboard`, adding and accessing `BlackboardDatum` instances using the `Set` and `Get` methods, respectively. Here's an example that saves a position into the blackboard
```csharp
Blackboard blackboard = new Blackboard();
BlackboardDatum positionDatum = new BlackboardDatum("PlayerPosition", playerGO.transform.position);
blackboard.Set(positionDatum);
```
and getting the position back is just as simple
```csharp
Vector3 position = blackboard.Get<Vector3>("PlayerPosition");
```