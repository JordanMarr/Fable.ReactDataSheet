// fsharplint:disable RecordFieldNames
module Fable.ReactDataSheet

open System
open Fable.Core
open Fable.React

type ParseResult = string[][]

let toParseResult (results: string seq) = 
    results 
    |> Seq.map (fun s -> [| s |])
    |> Seq.toArray

type ReactDatasheetProps = 
    /// An array of cell arrays.
    | Data of Row []
    /// Provides the ability to override specific cells view mode.
    | ValueRenderer of (Cell -> RowIndex -> ColumnIndex -> string)
    /// Provides the ability to override specific cells edit mode.
    | DataRenderer of (Cell -> RowIndex -> ColumnIndex -> string)
    /// Provides the ability to handle changes in specific cells.
    | OnCellsChanged of (CellsChangedArgs array -> CellsAddedArgs array -> unit)
    /// Determines whether the keyboard can navigate to a cell.
    | IsCellNavigable of (Cell -> RowIndex -> ColumnIndex -> bool)
    | Overflow of Overflow
    | Selected of Selection
    | OnSelect of (Selection -> unit)
    /// A global cell viewer override (affects all cells).
    | ValueViewer of (ValueViewerProps -> string)
    /// A global cell editor override (affects all cells).
    | DataEditor of (DataEditorProps -> ReactElement)
    | SheetRenderer of (SheetRendererProps -> ReactElement)
    | RowRenderer of (RowRendererProps -> ReactElement)
    /// Provides the ability to transform pasted data. Usage ex: [ "1"; "2" ] |> toParseResult
    | ParsePaste of (string -> ParseResult)

and Row = Cell []
and ColumnIndex = int
and RowIndex = int
and 
    [<RequireQualifiedAccess>] 
    [<StringEnum>] 
    Overflow = | Wrap | NoWrap | Clip

and Cell = {
    value: obj
    ``component``: ReactElement option
}
with
    static member Create(value, ?cmp) =
        { Cell.value = value
          Cell.``component`` = cmp }

and CellsChangedArgs = {
    cell: Cell
    row: RowIndex
    col: ColumnIndex
    /// The new value
    value: obj
}

and CellsAddedArgs = {
    row: RowIndex
    col: ColumnIndex
    /// The new value
    value: obj
}

and Location = {
    i: RowIndex
    j: ColumnIndex
}

and Selection = {
    start: Location
    ``end``: Location
}
with
    static member Create(startRow, startCol, endRow, endCol) = 
        { start = {i = startRow; j = startCol}
          ``end`` = {i = endRow; j = endCol} }

and DataEditorProps = {
    value: obj
    row: RowIndex
    col: ColumnIndex
    cell: Cell
    onChange: (string -> unit)
    onKeyDown: (Browser.Types.KeyboardEvent -> unit)
    onCommit: (obj -> unit)
    onRevert: (unit -> unit)
}

and ValueViewerProps = {
    value: obj
    row: RowIndex
    col: ColumnIndex
    cell: obj
}

and SheetRendererProps = {
    data: Row []
    className: string
    children: ReactElement
}

and RowRendererProps = {
    row: int
    cells: Cell []
    children: ReactElement
}

module Classes = 
    let cell = "cell"

let defaultValueRenderer (cell: Cell) (row: RowIndex) (col: ColumnIndex) =
    match cell.value with
    | null -> ""
    | value -> value.ToString()

let prepareProps (props: ReactDatasheetProps seq) = 
    // Default the ValueRenderer (so the user doesn't have to add it every time)
    let props = 
        if props |> Seq.exists(function | ValueRenderer _ -> true | _ -> false) 
        then props
        else Seq.append (seq { ValueRenderer defaultValueRenderer }) props
    
    JsInterop.keyValueList CaseRules.LowerFirst props

let ReactDataSheet props = 
    ofImport "default" "react-datasheet" (prepareProps props) []

let defaultDataEditor (props: DataEditorProps) =
    ofImport "DataEditor" "react-datasheet" props []

open System
open Microsoft.FSharp.Collections

/// A helper function that can be used within OnCellsChanged to update Data with new and edited cells.
let mergeChanges (rows: Row[]) (changes: CellsChangedArgs []) (added: CellsAddedArgs[]) =
    let rows = rows |> Array.copy

    for c in changes do
        rows.[c.row].[c.col] <- 
            match c.cell.``component`` with
            | Some comp -> Cell.Create(c.value, comp)
            | None -> Cell.Create(c.value)

    let added = if added |> isNull then [||] else added // React lib returns null if empty

    let maxRowIdx = 
        if added = Array.empty then -1
        else added |> Array.map (fun a -> a.row) |> Array.max
    
    let newRows = 
        if maxRowIdx > (rows.Length - 1) then
            let startRowIdx = rows.Length
            [|for rowIdx in [startRowIdx .. maxRowIdx] do
                match rows |> Array.tryHead with
                | Some firstRow -> 
                    firstRow |> Array.map (fun c -> { c with value = "" })
                | None -> 
                    [||]
            |]
        else
            [||]


    let data = Array.append rows newRows

    for a in added do 
        if a.row < data.Length then
            let row = data.[a.row]
            if a.col < row.Length then
                let cell = data.[a.row].[a.col]
                data.[a.row].[a.col] <-
                    { cell with value = a.value }

    data
            
    

