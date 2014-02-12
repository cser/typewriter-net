-- while a is not equal to b, loop.
while a /= b loop
  Ada.Text_IO.Put_Line ("Waiting");
end loop;
 
if a > b then
  Ada.Text_IO.Put_Line ("Condition met");
else
  Ada.Text_IO.Put_Line ("Condition not met");
end if;
 
for i in 1 .. 10 loop
  Ada.Text_IO.Put ("Iteration: ");
  Ada.Text_IO.Put (i);
  Ada.Text_IO.Put_Line;
end loop;
 
loop
  a := a + 1;
  exit when a = 10;
end loop;
 
case i is
  when 0 => Ada.Text_IO.Put ("zero");
  when 1 => Ada.Text_IO.Put ("one");
  when 2 => Ada.Text_IO.Put ("two");
  -- case statements have to cover all possible cases:
  when others => Ada.Text_IO.Put ("none of the above");  
end case;
 
for aWeekday in Weekday'Range loop               -- loop over an enumeration
   Put_Line ( Weekday'Image(aWeekday) );         -- output string representation of an enumeration
   if aWeekday in Working_Day then               -- check of a subtype of an enumeration
      Put_Line ( " to work for " & 
               Working_Hours'Image (Work_Load(aWeekday)) ); -- access into a lookup table
   end if;
end loop;