module Exception

let failIf isTrue message =
    if isTrue then
        failwith message