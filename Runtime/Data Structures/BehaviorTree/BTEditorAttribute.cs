using System.Linq;

public class BTEditorAttribute : System.Attribute
{
    public string menuPath;
    public string title;
    public string texturePath;

    public BTEditorAttribute(string path) {
        this.menuPath = path;
        this.title = this.menuPath.Split('/').Last();
    }
}

public class BTEditableAttribute : System.Attribute { }
