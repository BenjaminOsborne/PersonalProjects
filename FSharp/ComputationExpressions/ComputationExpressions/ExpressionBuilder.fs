module ExpressionBuilder

    type YourWorkflowBuilder() = 
        member this.Bind(m, f) = Option.bind f m
        member this.Return(x) = Some x

    let runExpression =
        let strToInt (str:string) =
            match System.Int32.TryParse str with 
            | (true,int) -> Some(int)
            | _ -> None


        let yourWorkflow = new YourWorkflowBuilder()

        let stringAddWorkflow x y z =
            yourWorkflow 
                {
                let! a = strToInt x
                let! b = strToInt y
                let! c = strToInt z
                return a + b + c
                }

        // test
        let good = stringAddWorkflow "12" "3" "2"
        let bad = stringAddWorkflow "12" "xyz" "2"

        0