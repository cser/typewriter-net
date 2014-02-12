# http://habrahabr.ru/post/140362/

enum = [1, 2].each
puts enum.next #=> 1
puts enum.next #=> 2
puts enum.next #=> StopIteration exception raised

enum = Enumerator.new do |yielder|
  yielder << 1
  yielder << 2
end
puts enum.next #=> 1
puts enum.next #=> 2
puts enum.next #=> StopIteration exception raised

enum = "xy".enum_for(:each_byte)
enum.each { |b| puts b }
# => 120
# => 121

o = Object.new
def o.each
  yield
  yield 'hello'
  yield [1, 2]
end
enum = o.to_enum
p enum.next #=> nil
p enum.next #=> 'hello'
p enum.next #=> [1, 2]

enum = %w{foo bar baz}.map
puts enum.with_index { |w, i| "#{i}:#{w}" } # => ["0:foo", "1:bar", "2:baz"]

# защита данных от изменений
a = [1, 2, 3]
some_method(a.enum_for)

[1,2,3].cycle.take(10) #=> [1, 2, 3, 1, 2, 3, 1, 2, 3, 1]