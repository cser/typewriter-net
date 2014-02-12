<?php

class Person
{
    public $firstName;
    public $lastName;
 
    public function __construct($firstName, $lastName = '') { // optional second argument
        $this->firstName = $firstName;
        $this->lastName = $lastName;
    }
 
    public function greet() {
        return 'Hello, my name is ' . $this->firstName . ' ' . $this->lastName . '.';
    }
 
    public static function staticGreet($firstName, $lastName) {
        return 'Hello, my name is ' . $firstName . ' ' . $lastName . '.';
    }
}
 
$he    = new Person('John', 'Smith');
$she   = new Person('Sally', 'Davis');
$other = new Person('iAmine');
 
echo $he->greet(); // prints "Hello, my name is John Smith."
echo '<br />';
echo $she->greet(); // prints "Hello, my name is Sally Davis."
echo '<br />';
echo $other->greet(); // prints "Hello, my name is iAmine ."
echo '<br />';
echo Person::staticGreet('Jane', 'Doe'); // prints "Hello, my name is Jane Doe."

?>