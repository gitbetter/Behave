using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class Task {
    public abstract bool Run(Blackboard blackboard);
    public virtual void Terminate() { }
    public virtual void AddChild(Task child) { }
    public virtual string ToString(string prefix = "") {
        string output = prefix + this.GetType().Name + System.Environment.NewLine;
        FieldInfo[] fields = this.GetType().GetFields().Where(
            field => System.Attribute.IsDefined(field, typeof(BTEditableAttribute))
        ).ToArray();
        foreach (FieldInfo fInfo in fields) {
            output += "  - " + fInfo.Name + ": " + fInfo.GetValue(this) + System.Environment.NewLine;
        }
        return output;
    }
}

[BTEditor("Behavior Tree/Selector")]
public class Selector : Task {
    List<Task> children = new List<Task>();

    public Selector() { }

    public Selector(params Task[] tasks) {
        children = new List<Task>(tasks);
    }

    public override bool Run(Blackboard blackboard)
    {
        foreach (Task child in children) {
            if (child.Run(blackboard))
                return true;
        }
        return false;
    }

    public override void AddChild(Task child) {
        children.Add(child);
    }

    public override string ToString(string prefix = "") {
        string output = base.ToString(prefix);
        foreach (Task child in children) {
            output += child.ToString(prefix + "  ");
        }
        return output;
    }
}

[BTEditor("Behavior Tree/Sequence")]
public class Sequence : Task {
    List<Task> children = new List<Task>();

    public Sequence() { }

    public Sequence(params Task[] tasks) {
        children = new List<Task>(tasks);
    }

    public override bool Run(Blackboard blackboard)
    {
        foreach (Task child in children) {
            if (!child.Run(blackboard))
                return false;
        }
        return true;
    }

    public override void AddChild(Task child) {
        children.Add(child);
    }

    public override string ToString(string prefix) {
        string output = base.ToString(prefix);
        foreach (Task child in children) {
            output += child.ToString(prefix + "  ");
        }
        return output;
    }
}

[BTEditor("Behavior Tree/Non-Deterministic Selector")]
public class NonDeterministicSelector : Task {
    List<Task> children = new List<Task>();

    public NonDeterministicSelector() { }

    public NonDeterministicSelector(params Task[] tasks) {
        children = new List<Task>(tasks);
    }

    public override bool Run(Blackboard blackboard)
    {
        List<Task> shuffled = children.Shuffle();
        foreach (Task child in shuffled) {
            if (child.Run(blackboard))
                return true;
        }
        return false;
    }

    public override void AddChild(Task child) {
        children.Add(child);
    }

    public override string ToString(string prefix = "") {
        string output = base.ToString(prefix);
        foreach (Task child in children) {
            output += child.ToString(prefix + "  ");
        }
        return output;
    }
}

[BTEditor("Behavior Tree/Non-Deterministic Sequence")]
public class NonDeterministicSequence : Task {
    List<Task> children = new List<Task>();

    public NonDeterministicSequence() { }

    public NonDeterministicSequence(params Task[] tasks) {
        children = new List<Task>(tasks);
    }

    public override bool Run(Blackboard blackboard)
    {
        List<Task> shuffled = children.Shuffle();
        foreach (Task child in shuffled) {
            if (!child.Run(blackboard))
                return false;
        }
        return true;
    }

    public override void AddChild(Task child) {
        children.Add(child);
    }

    public override string ToString(string prefix = "") {
        string output = base.ToString(prefix);
        foreach (Task child in children) {
            output += child.ToString(prefix + "  ");
        }
        return output;
    }
}

[BTEditor("Behavior Tree/Decorator")]
public class Decorator : Task {
    public Task child;

    public Decorator() { }

    public Decorator(Task task) {
        child = task;
    }

    public override bool Run(Blackboard blackboard)
    {
        return child.Run(blackboard);
    }

    public override void AddChild(Task child) {
        this.child = child;
    }

    public override string ToString(string prefix = "") {
        string output = base.ToString(prefix);
        if (child != null) {
            output += child.ToString(prefix + "  ");
        }
        return output;
    }
}

[BTEditor("Behavior Tree/Inverter")]
public class Inverter : Decorator {
    public Inverter() { }

    public Inverter(Task task) : base(task) { }

    public override bool Run(Blackboard blackboard)
    {
        return !child.Run(blackboard);
    }
}

[BTEditor("Behavior Tree/Until Fail")]
public class UntilFail : Decorator {
    public UntilFail() { }

    public UntilFail(Task task) : base(task) { }

    public override bool Run(Blackboard blackboard)
    {
        while (true) {
            bool result = child.Run(blackboard);
            if (!result) break;
        }
        return true;
    }
}

[BTEditor("Behavior Tree/Limit")]
public class Limit : Decorator {
    [BTEditable]
    public int runLimit;

    [BTEditable]
    int runSoFar = 0;

    public Limit() { }

    public Limit(Task task) : base(task) { }

    public override bool Run(Blackboard blackboard)
    {
        if (runSoFar >= runLimit)
            return false;
        runSoFar++;
        return child.Run(blackboard);
    }
}

[BTEditor("Behavior Tree/Semaphore Guard")]
public class SemaphoreGuard : Decorator {
    [BTEditable]
    public Semaphore semaphore;

    public SemaphoreGuard() { }

    public SemaphoreGuard(Task task) : base(task) { }

    public override bool Run(Blackboard blackboard)
    {
        if (semaphore.WaitOne(1000)) {
            bool result = child.Run(blackboard);
            semaphore.Release();
            return result;
        }
        return false;
    }
}

[BTEditor("Behavior Tree/Wait")]
public class Wait : Task {
    [BTEditable]
    public int duration;

    public override bool Run(Blackboard blackboard)
    {
        Thread.Sleep(duration);
        return true;
    }
}

[BTEditor("Behavior Tree/Parallel")]
public class Parallel : Task {
    public List<Task> children;

    List<Task> runningChildren = new List<Task>();
    EventWaitHandle waitHandle;

    public Parallel() { }

    public Parallel(params Task[] tasks) {
        children = new List<Task>(tasks);
    }

    public override bool Run(Blackboard blackboard) {
        waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        void RunChild(Task child, Blackboard bb) {
            runningChildren.Add(child);
            bool returned = child.Run(bb);
            runningChildren.Remove(child);

            if (!returned) {
                waitHandle.Set();
                Terminate();
            } else if (runningChildren.Count == 0) {
                waitHandle.Set();
            }
        }
        void Terminate() {
            foreach (Task child in runningChildren) {
                child.Terminate();
            }
        }

        foreach (Task child in children) {
            Thread thread = new Thread(() => RunChild(child, blackboard));
            thread.Start();
        }

        waitHandle.WaitOne();

        return true;
    }

    public override void AddChild(Task child) {
        children.Add(child);
    }

    public override string ToString(string prefix = "") {
        string output = base.ToString(prefix);
        foreach (Task child in children) {
            output += child.ToString(prefix + "  ");
        }
        return output;
    }
}

[BTEditor("Behavior Tree/Interrupter")]
public class Interrupter : Decorator {
    bool isRunning = false;
    bool? result = null;

    public Interrupter() { }

    public Interrupter(Task task) : base(task) { }

    public override bool Run(Blackboard blackboard) {
        void RunChild(Task child, Blackboard bb) {
            isRunning = true;
            result = child.Run(bb);
            isRunning = false;
        }

        Thread thread = new Thread(() => RunChild(child, blackboard));
        thread.Start();

        while (result == null) {
            Thread.Sleep(500);
        }

        return result ?? false;
    }

    public override void Terminate()
    {
        if (isRunning) {
            child.Terminate();
        }
    }

    public void SetResult(bool desired) {
        result = desired;
    }
}

[BTEditor("Behavior Tree/Perform Interruption")]
public class PerformInterruption : Task {
    [BTEditable]
    public Interrupter interrupter;

    [BTEditable]
    public bool desiredResult;

    public override bool Run(Blackboard blackboard)
    {
        interrupter.SetResult(desiredResult);
        return true;
    }
}

[BTEditor("Behavior Tree/Blackboard Manager")]
public class BlackboardManager : Decorator {
    public BlackboardManager(Task task = null) : base(task) { }

    public override bool Run(Blackboard blackboard)
    {
        Blackboard newBlackboard = new Blackboard();
        newBlackboard.parent = blackboard;
        bool result = child.Run(newBlackboard);
        newBlackboard = null;
        return result;
    }
}

public class BehaviorTree : Decorator
{
    public BehaviorTree() { }

    public BehaviorTree(Task task) : base(task) { }
}
