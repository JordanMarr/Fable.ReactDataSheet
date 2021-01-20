# Fable.ReactDataSheet
A Fable wrapper around nadbm/react-datasheet for creating Excel-like tables

See documentation for the underlying React library here:
https://github.com/nadbm/react-datasheet

Many (but not all) of the features are supported in this wrapper, including the ability to create cells with custom editors.

## Basic Usage
```fsharp

let rows = [|
  [| Cell.Create "ABC"; Cell.Create 123 |]
  [| Cell.Create "DEF"; Cell.Create 456 |]
|]

ReactDataSheet [ 
    Data rows
]
```

## More Advanced Usage
```fsharp
ReactDataSheet [ 
    Data model.Rows
    OnCellsChanged (fun changes added -> 
        mergeChanges model.Rows changes added
        |> UpdateRows 
        |> dispatch 
    )
    SheetRenderer (fun e ->
        table [Class e.className] [
            thead [] [
                tr [] [
                    th [Class "cell"; Style [Background "#e9f0f7"; Width "32px"]] []
                    th [Class "cell"; Style [Background "#e9f0f7"; Width "250px"]] [str "Sheet Number"]
                    th [Class "cell"; Style [Background "#e9f0f7"; Width "820px"]] [str "Sheet Name"]
                ]
            ]
            tbody [] [ 
                e.children 
            ]
        ]
    )
    RowRenderer (fun e -> 
        tr [] [
            td [Class Classes.cell; Style[Background "#e9f0f7"]] []
            e.children
        ]
    )
]
```
