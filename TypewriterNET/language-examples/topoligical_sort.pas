(* TOPOLOGICAL SORT from Wirth's Book *)
PROGRAM topsort(input,output);
TYPE lref = ^leader;
	tref = ^trailer;
	leader = RECORD 
		   key  : INTEGER;
		   count: INTEGER;
		   trail: tref;
		   next : lref;
	         END;
	trailer = RECORD 
		   id   : lref;
		   next : tref
		 END;
VAR 	head,tail,p,q: lref;
	t: tref;
	z: INTEGER;
	x,y: INTEGER;

FUNCTION L(w: INTEGER): lref;
	(* reference to leader with key w *)
	VAR h : lref;
	BEGIN
	   h := head;
	   tail^.key := w; (*sentinel*)
	   WHILE h^.key <> w DO H := h^.next;
	   IF h = tail THEN
	      BEGIN  (* no element with key w in the list *)
		new(tail);
		z := z + 1;
		h^.count := 0;
		h^.trail := NIL;
		h^.next := tail
	      END;
	   L := h
END (*L*);

BEGIN (*initialize list of leaders with a dummy *)
	New(head);
	tail := head;
	z := 0;
(* input phase *)
	Read(x);
	WHILE x <> 0 DO
	   BEGIN
		Read(y); 
		Writeln(x,y);
		p := L(x);
		q := L(y);
		New(t);
		t^.id := q;
		t^.next := p^.trail;
		p^.trail := t;
		q^.count := q^.count + 1;
		Read(x)
	   END;
(* search for leaders with count = 0 *)
	p := head;
	head := NIL;
	WHILE p <> tail DO
	   BEGIN
		q := p;
		p := p^.next;
		IF q^.count = 0 THEN
		   BEGIN
			q^.next := head;
			head := q
		   END
	   END;
(* output phase *)
	q := head;
	WHILE q <> NIL DO
	   BEGIN
		Writeln(q^.key);
		z := z - 1;
		t := q^.trail;
		q := q^.next;
		WHILE t <> NIL DO
		   BEGIN
			p:= t^.id;
			p^.count := p^.count - 1;
			IF p^.count = 0 THEN
		  	   BEGIN (* insert p^ in q-list *)
				p^.next := q;
				q := p
			   END;
			t:= t^.next
		   END
	   END;
	IF z <> 0 THEN
	   Writeln('This Set is NOT partially Ordered')
END.