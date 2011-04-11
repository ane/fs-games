#light

(* 
   Inkre (c) ane 2009, 2011 - a small game written in F#
   
   Some screenshots
     http://users.jyu.fi/~anhekalm/misc/ohje1.png
     http://users.jyu.fi/~anhekalm/misc/ohje2.png
     
   Revision history:
     * 2/2011 Fix for the changed F#
     * 2/2009 Write the original
*)

open System
open System.Windows.Forms
open System.Drawing

type MainForm() as form = 
  inherit Form()
  // Create the controls
  let newgame = new Button(Location = new Point(244, 84), Size = new Size(81, 25), Text = "New Game") :> Control
  let undo = new Button(Location = new Point(244, 55), Size = new Size(81, 25), Text = "Undo") :> Control
  let title = new Label(Location = new Point(250, 9), Font = new Font("Verdana", float32 14.25, FontStyle.Bold), Text = "Inkre") :> Control 
  let lbl1 = new Label(Location = new Point(262, 130), Font = new Font("Verdana", float32 9.75, FontStyle.Bold), Text = "Next") :> Control
  let lbl2 = new Label(Location = new Point(262, 32), Text = "by ane") :> Control
  let lbl3 = new Label(Location = new Point(245, 200), Text = "", Font = new Font("Verdana", float32 11.25, FontStyle.Bold)) :> Control
  
  // The label displaying the upcoming move
  let next = 
    new Label(
      BorderStyle = BorderStyle.Fixed3D, 
      Location = new Point(262, 155),
      Text = "0",
      Size = new Size(45, 40),
      TextAlign = ContentAlignment.MiddleCenter,
      Font = new Font("Arial", float32 14.25, FontStyle.Regular)
    )
    :> Control // Reinterpret cast to control
    
  // Border colors etc
  let backcolor = Color.PowderBlue
  let mutable blocks = Array2D.create 5 5 (new Label()) // shouldn't be mutable!
  let count = ref 0
  
  // Call the constructor
  do form.InitializeForm
   
  member this.InitializeForm = 
    this.ClientSize <- new Size(337, 225)
    this.FormBorderStyle <- FormBorderStyle.FixedDialog
    this.MaximizeBox <- false
    this.Text <- "Inkre v0.1" 
    blocks <- this.make_blocks()
    this.draw_blocks()
    next.Text <- string (!count + 1)
    blocks |> Array2D.iter (fun label -> 
      label.Click.AddHandler(new EventHandler(fun x args -> this.click_label x))
      label.MouseHover.AddHandler(new EventHandler(fun x args -> this.hover_label x))
      label.MouseLeave.AddHandler(new EventHandler(fun x args -> this.leave_label x))
      this.Controls.Add(label)
      )
    
    undo.Click.Add(fun _ -> this.undo_click())
    newgame.Click.Add(fun _ -> this.newgame_click())  
    
    // Add the controls
    this.Controls.AddRange([| undo; newgame; next; title; lbl1; lbl2; lbl3 |])
  
  // Draw white blocks
  member this.draw_blocks () = 
    blocks |> Array2D.iter (fun blk -> blk.BackColor <- Color.White)

  // Resets
  member this.reset_blocks () =
    blocks |> Array2D.iter (fun blk ->
      blk.Text <- ""
      blk.BackColor <- Color.White
    )

  // Create a two-dimensional array
  member this.make_blocks () = 
    blocks |> Array2D.mapi (fun x y blk ->
      new Label(
        BorderStyle = BorderStyle.Fixed3D,
        Size = new Size(45, 45),
        Location = new Point(x * 45, y * 45),
        Font = new Font("Arial", float32 14.25, FontStyle.Regular),
        Tag = new Point(x, y),
        TextAlign = ContentAlignment.MiddleCenter
      )
    )

  // Returns the block at (x, y)
  member this.get_block (x, y) =
    try
      blocks.[x, y]
    with
      | :? IndexOutOfRangeException -> null

  // Highlights blocks
  member this.hilight_blocks (block : Point) =
    let hilighted = ref 0
    let n = this.get_block (block.X, block.Y - 3) in
    let s = this.get_block (block.X, block.Y + 3) in
    let w = this.get_block (block.X - 3, block.Y) in
    let e = this.get_block (block.X + 3, block.Y) in
    let nw = this.get_block (block.X - 2, block.Y - 2) in
    let ne = this.get_block (block.X + 2, block.Y - 2) in
    let sw = this.get_block (block.X - 2, block.Y + 2) in
    let se = this.get_block (block.X + 2, block.Y + 2) in
    [n; s; w; e; nw; ne; sw; se] |> Seq.iter (fun blk -> if (blk <> null) && (blk.Text = "") then blk.BackColor <- backcolor; incr hilighted)
    !hilighted // Return the number of highlighted blocks
    
  member this.click_label (label : Object) = 
    let tmp = label :?> Label in
    if (tmp.BackColor <> backcolor && !count <> 0) then () else
    tmp.Text <- string (!count + 1)
    incr count
    this.draw_blocks()
    if this.hilight_blocks (tmp.Tag :?> Point) = 0 then lbl3.Text <- "owned :("
    next.Text <- string (!count + 1)
  
  member this.leave_label (blk : Object) =
    // Cast
    let tmp = blk :?> Label in
    tmp.BorderStyle <- BorderStyle.Fixed3D

  member this.hover_label (blk : Object) =
    let tmp = blk :?> Label in
    if (tmp.BackColor = backcolor && tmp.Text = "") || !count = 0 then tmp.BorderStyle <- BorderStyle.FixedSingle 

  member this.newgame_click () =
    this.reset_blocks()
    this.draw_blocks()
    count := 0
    next.Text <- "1"

  member this.undo_click () = 
    if !count = 0 then () else
    if !count = 1 then this.newgame_click() else
    blocks |> Array2D.iter (fun blk ->
      if blk.Text = string !count then blk.Text <- ""; blk.BackColor <- backcolor
      if blk.Text = string (!count - 1) then this.draw_blocks(); this.hilight_blocks(blk.Tag :?> Point) |> ignore
    )
    decr count
    next.Text <- string (!count + 1)

Application.Run(new MainForm())
