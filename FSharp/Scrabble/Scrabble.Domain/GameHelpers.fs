namespace Scrabble.Domain

type TileBagCreator =
    static member Default =
        let data = [(12,'e',1); //1 point: E ×12, A ×9, I ×9, O ×8, N ×6, R ×6, T ×6, L ×4, S ×4, U ×4
                    (9,'a',1);
                    (9,'i',1);
                    (8,'o',1);
                    (6,'n',1);
                    (6,'r',1);
                    (6,'t',1);
                    (4,'l',1);
                    (4,'s',1);
                    (4,'u',1);
                    
                    (4,'d',2); //2 points: D ×4, G ×3
                    (3,'g',2);
                    
                    (2,'b',3); //3 points: B ×2, C ×2, M ×2, P ×2
                    (2,'c',3);
                    (2,'m',3);
                    (2,'p',3);
                    
                    (2,'f',4); //4 points: F ×2, H ×2, V ×2, W ×2, Y ×2
                    (2,'h',4);
                    (2,'v',4);
                    (2,'w',4);
                    (2,'y',4);
                    
                    (1,'k',5); //5 points: K ×1
                    (1,'j',8); //8 points: J ×1, X ×1
                    (1,'x',8);
                    (1,'q',10); //10 points: Q ×1, Z ×1
                    (1,'z',10)]
        let letterTiles = data |> Seq.map (fun (n,chr,value) -> {1..n} |> Seq.map (fun _ -> { TileLetter = Letter(chr); Value = value }))
                               |> Seq.collect (fun x -> x) |> Seq.toList
        let blankTiles = [0..1] |> Seq.map (fun _ -> { TileLetter = Blank; Value = 0}) |> Seq.toList //2 blank tiles (scoring 0 points)
        new TileBag(List.append letterTiles blankTiles)

type BoardCreator = 
    
    static member Default =
        let size = 15
        let create w h space = { Location = { Width = w; Height = h }; State = BoardState.Free(space) }
        let array = Array2D.init size size (fun x y -> create x y Normal)
        let apply(tiles: seq<BoardLocation>) = tiles |> Seq.iter (fun x -> array.[x.Location.Width, x.Location.Height] <- x);
        
        let coMap2 = SequenceHelpers.CoMap2
        let coMap = SequenceHelpers.CoMap
        let coMapRev indA indB = Seq.append (coMap2 indA indB) (coMap2 indB indA);
        
        let createFromRange indexes state = indexes |> Seq.map (fun (w,h) -> create w h state)

        let tripWrd = createFromRange (coMap {0..7..size}) (WordMultiply 3)
        
        let dubWrdSeq = {1..4} |> Seq.map (fun n -> [ (n,n); (n, size-1-n); (size-1-n,n); (size-1-n,size-1-n)] ) |> Seq.collect (fun x -> x);
        let dubWrd = createFromRange dubWrdSeq (WordMultiply 2)

        let start = [7] |> Seq.map (fun x -> create x x (WordMultiply 2))

        let trpLet = createFromRange (coMap [1;5;9;13]) (LetterMultiply 3)
        
        let dubLetSeq = Seq.append (coMapRev [0;7;14] [3;11]) (coMap [2;6;8;12]);
        let dubLet = createFromRange dubLetSeq (LetterMultiply 2)
        
        apply ([trpLet; dubLet; tripWrd; dubWrd; start] |> Seq.collect (fun x -> x))
        
        new Board(array, size, size);

    static member PlayArray (startBoard:Board) (items: char list list) = 
        let chars = items |> Seq.mapi (fun h wList -> wList |> Seq.mapi (fun w c -> (w, h, c)))
                          |> Seq.collect (fun x -> x)
                          |> Seq.filter (fun (_, _, c) -> System.Char.IsLetter c)
                          |> Seq.toList

        let draw (tb:TileBag) (c:char) = tb.DrawFromLetter(Letter(c))
        let play (board:Board) w h c v = board.Play [ { Location = { Width = w; Height = h}; Piece = { Letter = c; Value = v } } ]
        
        let tileBag = TileBagCreator.Default
        let (board,bag) = chars |> Seq.fold (fun (brd, tb) (w,h,c) -> let (newTb, tbl) = draw tb c
                                                                      let newBoard = play brd w h c tbl.Value
                                                                      (newBoard, newTb)) (startBoard, tileBag)
        board

    static member FromArray (items: char list list) = 
        let height = items.Length;
        let width = match height with | 0 -> 0 | _ -> items.Head.Length
        let initial = Board.Empty width height
        BoardCreator.PlayArray initial items

type WordLoader = 
    
    static member LoadAllWords() =
        let location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        let raw = System.IO.File.ReadAllText (location + @"\\sowpods.txt")
        let set = raw.Split '\n' |> Seq.map (fun x -> x.ToLower().Trim())
                                 |> Seq.filter (fun x -> System.String.IsNullOrEmpty x = false)
                                 |> Set
        new WordSet(set)

type GameStateCreator =
    static member InitialiseGameFor (players : Player list) =
        let board = BoardCreator.Default
        let initialBag = TileBagCreator.Default
        let states = players |> List.map (fun p -> PlayerState.Empty p)
        { Board = board; TileBag = initialBag; PlayerStates = states; }