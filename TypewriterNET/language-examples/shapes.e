-- from http://www.angelfire.com/tx4/cus/shapes/eiffel.html

class TRYME inherit ANY
creation make

feature
   make is
      local
         i: INTEGER
         ashape: SHAPE
         scribble: ARRAY[SHAPE]
         arect: RECTANGLE

      do
         -- create some shape instances
         !!scribble.make(0, 1)
         !RECTANGLE!ashape.make_rectangle(10, 20, 5, 6)
         scribble.put(ashape, 0)
         !CIRCLE!ashape.make_circle(15, 25, 8)
         scribble.put(ashape, 1)

         -- iterate through the list and handle shapes polymorphically
         from
            i := scribble.lower
         until
            i > scribble.upper
         loop
            scribble.item(i).draw
            scribble.item(i).rmoveto(100, 100)
            scribble.item(i).draw
            i := i + 1
         end

         -- call a rectangle specific function
         !!arect.make_rectangle(0, 0, 15, 15)
         arect.setwidth(30)
         arect.draw
      end
end
