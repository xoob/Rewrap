﻿module rec Nonempty

open Extensions

type Nonempty<'T> =
    | Nonempty of 'T * List<'T>
    with
    // Overloading `@` seems not to be allowed
    static member (+) (a, b) =
        append a b



//-----------------------------------------------------------------------------
// CREATING NON-EMPTY LISTS 
//-----------------------------------------------------------------------------


let fromList list =
    match list with
        | [] ->
            None
        | x :: xs ->
            Some(Nonempty(x, xs))


let fromListUnsafe<'T> (list: List<'T>) : Nonempty<'T> =
    Nonempty (List.head list, List.tail list)


let singleton head =
    Nonempty (head, [])


let cons head neList =
    Nonempty(head, toList neList)

let snoc last (Nonempty(head, tail)) =
    Nonempty(head, tail @ [last])

let append (Nonempty(head, tail)) b =
    Nonempty(head, tail @ toList b)


//-----------------------------------------------------------------------------
// GETTING FROM NON-EMPTY LISTS 
//-----------------------------------------------------------------------------


let head (Nonempty (head, tail)) =
    head


let tail (Nonempty (head, tail)) =
    tail


let length<'T> =
    tail >> List.length<'T> >> (+) 1


let tryFind predicate =
    toList<'T> >> List.tryFind<'T> predicate


//-----------------------------------------------------------------------------
// TRANSFORMING NON-EMPTY LISTS 
//-----------------------------------------------------------------------------


let toList<'T> (Nonempty (head, tail)) =
    List.Cons (head, tail)


let rev<'T> =
    toList >> List.rev<'T> >> fromListUnsafe


let map fn (Nonempty(head, tail)) =
    Nonempty(fn head, List.map fn tail)


let mapHead fn (Nonempty (head, tail)) =
    Nonempty (fn head, tail)


let mapTail fn (Nonempty (head, tail)) =
    Nonempty (head, List.map fn tail)
    

let mapInit fn =
    rev >> mapTail fn >> rev


let replaceHead newHead =
    mapHead (fun _ -> newHead)


// B  =  Nonempty<'T>
let collect (fn: 'T -> Nonempty<'U>) (neList: Nonempty<'T>) : Nonempty<'U> =
    let rec loop output input =
        match input with
            | x :: xs ->
                loop (fn x + output) xs
            | [] ->
                output

    rev neList |> (fun (Nonempty(head, tail)) -> loop (fn head) tail)
        

let span predicate: Nonempty<'a> -> Option<Nonempty<'a> * Option<Nonempty<'a>>> =
    let rec loop output maybeRemaining =
        match maybeRemaining with
            | None ->
                Nonempty.fromList (List.rev output)
                    |> Option.map(fun o -> (o, maybeRemaining))
            | Some(Nonempty(head, tail)) ->
                if predicate head then
                    loop (head :: output) (Nonempty.fromList tail)
                else
                     Nonempty.fromList (List.rev output)
                        |> Option.map (fun o -> (o, maybeRemaining))
     
    Some >> loop []


let splitAfter predicate: Nonempty<'a> -> Nonempty<'a> * Option<Nonempty<'a>> =
    let rec loop output (Nonempty(head, tail)) =
        let maybeNextList =
            Nonempty.fromList tail

        if predicate head then
            (Nonempty(head, output), maybeNextList)
        else
            match maybeNextList with
                | Some(nextList) ->
                    loop (head :: output) nextList
                | None ->
                    (Nonempty(head, output), None)

    loop [] >> Tuple.mapFirst Nonempty.rev
        

let unfold (fn: 'B -> 'A * Option<'B>): ('B -> Nonempty<'A>) =
    let rec loop output input =
        match fn input with
            | (res, Some(nextInput)) -> 
                loop (res :: output) nextInput

            | (res, None) ->
                Nonempty(res, output)
        in
            loop [] >> Nonempty.rev