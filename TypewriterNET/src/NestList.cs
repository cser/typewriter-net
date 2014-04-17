using System;

public class NestList : NestBase
{
	public NestList()
	{
	}

	private Nest head;
	public Nest Head { get { return head; } }

	public void AddParent(Nest nest)
	{
		if (nest.Owner != null)
			nest.Owner.Remove(nest);
		if (nest != null)
		{
			SetParent(nest, null);
			SetChild(nest, head);
			if (head != null)
				SetParent(head, nest);
			head = nest;
			SetOwner(nest, this);
		}
	}

	public void Remove(Nest nest)
	{
		if (nest != null && nest.Owner == this)
		{
			if (nest == head)
			{
				head = GetChild(head);
				if (head != null)
					SetParent(head, null);
			}
			else
			{
				Nest parent = GetParent(nest);
				Nest child = GetChild(nest);
				SetChild(parent, child);
				if (child != null)
					SetParent(child, parent);
			}
			SetParent(nest, null);
			SetChild(nest, null);
			SetOwner(nest, null);
		}
	}
}
