namespace Ben.Example

module PersonModule =

    // The declaration creates a constructor that takes two values, name and age. 
    [<AutoOpen>]
    type Person(name:string, age:int) =
        // A Person object's age can be changed. The mutable keyword in the 
        // declaration makes that possible. 
        let mutable internalAge = age

        // Declare a second constructor that takes only one argument, a name. 
        // This constructor calls the constructor that requires two arguments, 
        // sending 0 as the value for age. 
        new(name:string) = Person(name, 0)

        // A read-only property. 
        member this.Name = name
        // A read/write property. 
        member this.Age
            with get() = internalAge
            and set(value) = internalAge <- value

        // Instance methods. 
        // Increment the person's age. 
        member this.HasABirthday () = internalAge <- internalAge + 1

        // Check current age against some threshold. 
        member this.IsOfAge targetAge = internalAge >= targetAge

        // Display the person's name and age. 
        override this.ToString () = 
            "Name:  " + name + "\n" + "Age:   " + (string)internalAge
    
    let RunPerson () = 
        // The following let expressions are not part of the Person class. Make sure 
        // they begin at the left margin. 
        let person1 = Person("John", 43)
        let person2 = Person("Mary")

        // Send a new value for Mary's mutable property, Age.
        person2.Age <- 15
        // Add a year to John's age.
        person1.HasABirthday()

        // Display results.
        System.Console.WriteLine(person1.ToString())
        System.Console.WriteLine(person2.ToString())
        // Is Mary old enough to vote?
        System.Console.WriteLine(person2.IsOfAge(18))
        System.Console.WriteLine(person2.IsOfAge(15))
        0