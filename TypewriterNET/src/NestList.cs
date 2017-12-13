using System;

public class NestList
{
	private MainForm mainForm;

	public NestList(MainForm mainForm)
	{
		this.mainForm = mainForm;
	}

	private Nest head;
	public Nest Head { get { return head; } }

	public bool needResize;

	public Nest AddParent()
	{
		Nest nest = new Nest(this, mainForm, RemoveNest);
		nest.parent = null;
		nest.child = head;
		if (head != null)
			head.parent = nest;
		head = nest;
		needResize = true;
		return nest;
	}

	private void RemoveNest(Nest nest)
	{
		if (nest != null && nest.Owner == this)
		{
			if (nest == head)
			{
				head = head.child;
				if (head != null)
					head.parent = null;
			}
			else
			{
				Nest parent = nest.parent;
				Nest child = nest.child;
				parent.child = child;
				if (child != null)
					child.parent = parent;
			}
			nest.parent = null;
			nest.child = null;
			needResize = true;
		}
	}
}
