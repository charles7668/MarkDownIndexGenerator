namespace MarkDownIndexGenerator.Models;

public class Tree<T>
{
    /// <summary>
    ///     child nodes
    /// </summary>
    public List<Tree<T>>? Childs { get; set; }

    /// <summary>
    ///     value
    /// </summary>
    public T Value { get; set; }
}