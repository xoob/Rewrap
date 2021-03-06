﻿module Rewrap.Core

open Extensions


let findLanguage name filePath : string =
    Parsing.Documents.findLanguage name filePath
        |> Option.map (fun l -> l.name)
        |> Option.defaultValue null


let languages : string[] =
    Parsing.Documents.languages
        |> Array.map (fun l -> l.name)

let rewrap 
    (language: string)
    (filePath: string)
    (selections: seq<Selection>)
    (settings: Settings) 
    (lines: seq<string>) =

    let parser = 
        Parsing.Documents.select language filePath

    let originalLines =
        List.ofSeq lines |> Nonempty.fromListUnsafe

    originalLines
        |> parser settings
        |> Selections.applyToBlocks selections settings
        |> Wrapping.wrapBlocks settings originalLines
