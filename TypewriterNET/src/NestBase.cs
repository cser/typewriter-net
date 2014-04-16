public class NestBase
{
	protected Nest child;
	protected Nest parent;

	protected static Nest GetChild(Nest nest)
	{
		return nest.child;
	}

	protected static void SetChild(Nest nest, Nest child)
	{
		nest.child = child;
	}

	protected static Nest GetParent(Nest nest)
	{
		return nest.parent;
	}

	protected static void SetParent(Nest nest, Nest parent)
	{
		nest.parent = parent;
	}
}
