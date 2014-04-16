public class NestList : NestBase
{
	public NestList()
	{
	}

	private Nest head;
	public Nest Head { get { return head; } }

	public void AddToHead(Nest nest)
	{
		SetChild(nest, head);
		if (head != null)
			SetParent(head, nest);
		head = nest;
	}

	public void Remove(Nest nest)
	{
		if (nest == head)
		{
			head = GetChild(head);
			if (head != null)
				SetParent(head, null);
		}
		else
		{
			SetChild(GetParent(nest), GetChild(nest));
			if (GetChild(nest) != null)
				SetParent(GetChild(nest), GetParent(nest));
		}
	}
}
