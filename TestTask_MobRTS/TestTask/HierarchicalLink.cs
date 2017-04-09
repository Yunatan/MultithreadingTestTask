namespace TestTask
{
    public class HierarchicalLink<T>
    {
        public HierarchicalLink(T parent, T child)
        {
            Parent = parent;
            Child = child;
        }

        public T Parent { get; set; }

        public T Child { get; set; }
    }
}
