#light

// Game of Life, F#
// (c) ane 2009, 2011
// 2/2011: Fix for new F#

open System
open System.Windows.Forms
open System.Drawing

// Checks cell at (x, y)
let check_cell (board : bool [,]) (x, y) =
  try
    board.[x,y]
  with 
    | :? IndexOutOfRangeException -> false

// Flips (x, y)
let flip_cell (board : bool [,]) (x, y) =
  board.[x, y] <- not board.[x, y]

// Check neighbours at (x, y)
let count_neighbors board (x, y) =
  let count = ref 0 in
  for i = -1 to 1 do
    for j = -1 to 1 do
       if check_cell board (x+i, y+j) && not (i = 0 && j = 0) then 
         count := !count + 1
  !count

// Evolve the cell according to the game of life rules
// > 3 neighbours         -> dead
// = 3 neighbours & dead  -> born
// < 2 neighbours         -> dies (forever alone)
let evolve_cell board loc =
  let rec state = check_cell board loc
  and neighbors = count_neighbors board loc
  in match neighbors with
      | 2 -> if state = true then true else false
      | 3 -> if state = false then true else true
      | _ -> false

// Call the above function for each cell
let evolve_board (board : bool [,]) = 
  let new_board = Array2D.create 10 10 false in
    for x = 0 to 9 do
      for y = 0 to 9 do
        new_board.[x,y] <- evolve_cell board (x,y)
  new_board

let mutable b = Array2D.create 10 10 false
b.[5, 5] <- true
b.[6, 5] <- true
b.[7, 5] <- true
b.[5, 6] <- true
b.[6, 7] <- true

let form = new Form()
form.Width <- 250
form.Height <- 180
form.Text <- "Game of Life"
form.FormBorderStyle <- FormBorderStyle.FixedDialog

let mutable labels = Array2D.create 10 10 (new Label())

// alustaa taulukon
let create_labels =
  labels |> Array2D.mapi (fun x y label -> 
    new Label(
      BorderStyle = BorderStyle.Fixed3D, 
      Size = new Size(15, 15), 
      Location = new Point(x*15, y*15), 
      BackColor=Color.White
    )
  )

// Loop each label (blue = alive, white = dead)
let draw_labels board =
  labels |> Array2D.iteri (fun x y label -> label.BackColor <- if check_cell board (x, y) then Color.Blue else Color.White)        
       
let stepButton = new Button()
do stepButton.Text <- "Step"
do stepButton.Location <- new Point(160, 15)
do stepButton.Click.Add(fun _ -> 
    b <- evolve_board b
    draw_labels(b))
let quitButton = new Button()
do quitButton.Text <- "Quit"
do quitButton.Location <- new Point(160, 45)
do quitButton.Click.Add(fun _ -> form.Close())

form.AcceptButton <- stepButton
labels <- create_labels 
labels |> Array2D.iter (fun label -> form.Controls.Add(label))

draw_labels(b)

form.Controls.Add(stepButton)
form.Controls.Add(quitButton)

Application.Run(form)