module Point

    type Point2D() = 
        
        let mutable x = 0.0

        member this.X   with get() = x
                        and  set(v) = x <- v
        
        member val Y = 0.0 with get, set


    let oExamplePoint = Point2D()
    oExamplePoint.Y <- 5.0
