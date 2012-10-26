
class Animal {
    constructor (private name:string) {
    }

    move(meters: number) {
        alert(name + " moved " + meters + "m");
    }
}

class Snake extends Animal {
    move() {
        alert("Slithering...");
        super.move(5);
    }
}

class Horse extends Animal {
    constructor (private name: string) {
        super("HORSE: " + name);
    }

    move() {
        alert("Galloping...");
        super.move(45);
    }
}

var sam = new Snake("Sammy the Python")
var tom = new Horse("Tommy the Palomino")

sam.move()
tom.move()