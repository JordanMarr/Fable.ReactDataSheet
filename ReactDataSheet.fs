// fsharplint:disable RecordFieldNames
module Fable.ReactDataSheet

open Fable.Core
open Fable.React

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
    onCommit: (obj -> Browser.Types.Event -> unit)
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
