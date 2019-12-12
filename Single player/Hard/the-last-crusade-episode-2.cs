using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        var game = new Game();
        game.InitOnce();

        while (true)
        {
            game.InitTurn();

            var cmd = game.GetNextCommand();

            // if double rotation, 2 moves, 2 turns
            if (cmd.Type == CommandType.DOUBLE)
            {
                game.ProcessCommand(cmd);

                game.InitTurn();

                game.ProcessCommand(cmd);
            }
            else
            {
                game.ProcessCommand(cmd);
            }
        }
    }
}

public enum RoomState { NONE, LEFT, RIGHT, DOUBLE }
public enum MovableFrom { LEFT, RIGHT, TOP }
public enum CommandType { WAIT, LEFT, RIGHT, DOUBLE }
public enum RotationMove { LEFT, RIGHT }

public class Command
{
    public int X;
    public int Y;
    public CommandType Type;
    public Stack<RotationMove> Moves;

    public Command(int x, int y, CommandType type)
    {
        X = x;
        Y = y;
        Type = type;

        Moves = new Stack<RotationMove>();
        switch (Type)
        {
            case CommandType.WAIT:
                break;
            case CommandType.LEFT:
                Moves.Push(RotationMove.LEFT);
                break;
            case CommandType.RIGHT:
                Moves.Push(RotationMove.RIGHT);
                break;
            case CommandType.DOUBLE:
                Moves.Push(RotationMove.LEFT);
                Moves.Push(RotationMove.LEFT);
                break;
        }
    }
}

public class Game
{
    public static bool ActivateLogs = false;
    public int NbTurns = 0;

    public Indy Indy;
    public Room[,] Map;
    private int _width;
    private int _height;
    public int xEnd;
    public int yEnd;

    public List<Rock> Rocks = new List<Rock>();
    public bool HasRock => Rocks.Count() > 0;

    private bool _calculateIndyPath = true;

    public void InitOnce()
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        _width = int.Parse(inputs[0]); // number of columns.
        _height = int.Parse(inputs[1]); // number of rows.


        // init Map
        Map = new Room[_width, _height];

        for (int y = 0; y < _height; y++)
        {
            var line = Console.ReadLine();
            inputs = line.Split(' '); // each line represents a line in the grid and contains W integers T. The absolute value of T specifies the type of the room. If T is negative, the room cannot be rotated.

            for (int x = 0; x < _width; x++)
            {
                Map[x, y] = new Room(x, y, int.Parse(inputs[x]));
            }

            Console.Error.WriteLine(line);
        }
        yEnd = _height - 1;
        xEnd = int.Parse(Console.ReadLine()); // the coordinate along the X axis of the exit.
    }

    public void InitTurn()
    {
        var inputs = Console.ReadLine().Split(' ');
        int XI = int.Parse(inputs[0]);
        int YI = int.Parse(inputs[1]);
        string POSI = inputs[2];
        int R = int.Parse(Console.ReadLine()); // the number of rocks currently in the grid.

        Rocks.Clear();
        for (int i = 0; i < R; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int XR = int.Parse(inputs[0]);
            int YR = int.Parse(inputs[1]);
            string POSR = inputs[2];

            var rockState = new MovableState(XR, YR, POSR, Map[XR, YR].State, this.NbTurns, true);
            Rocks.Add(new Rock(rockState));
        }

        var indyState = new MovableState(XI, YI, POSI, Map[XI, YI].State, this.NbTurns, true);
        if (Indy == null)
            Indy = new Indy(indyState);
        else
            Indy.CurrentState = indyState;

        this.NbTurns++;
    }

    public Stack<MovableState> GetPathToEnd(Movable movable)
    {
        Console.Error.WriteLine("GetPathToEnd()");

        var path = new Stack<MovableState>();

        // simulated indy
        var state = movable.CurrentState;

        path.Push(state);

        while (path.Count > 0)
        {
            // on prend le dernier de la pile
            state = path.Peek();
            var room = Map[state.X, state.Y];

            // on move simIndy
            state = room.Move(state);

            // on vérifie si simIndy est out of bound
            if (state.X >= _width || state.Y >= _height || state.X < 0 || state.Y < 0)
                state.Alive = false;

            // si simIndy est mort
            if (state.Alive == false)
            {
                // si la room ne peut pas tourner, ou si toutes les combinaisons de rotation ont été faites
                if (room.CanRotate == false)
                {
                    // delete rooms from path if sealed or all rotations have been made
                    // go back to last available room
                    while (path.Count > 0)
                    {
                        state = path.Peek();
                        room = Map[state.X, state.Y];

                        if (room.CanRotate == false)
                        {
                            room.Reset();
                            path.Pop();
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                room.Rotate();
            }
            else
            {
                path.Push(state);

                // si simIndy est arrivé c'est cool
                if (state.X == xEnd && state.Y == yEnd)
                {
                    Console.Error.WriteLine($"YOUPI {state.X} {state.Y}");
                    break;
                }
            }
        }

        Console.Error.WriteLine("GetPathToEnd() ends");

        /*var list = path.ToList();
        list.Reverse();
        list.ForEach(s => Console.Error.WriteLine($"{s.X} {s.Y} {s.From} {s.RoomState}"));*/

        return path;
    }

    public Stack<Command> BuildCommands(Stack<MovableState> path)
    {
        var commands = new Stack<Command>();

        foreach (var state in path)
        {
            var cmd = state.GetCommand();

            if (cmd != null)
                commands.Push(cmd);
        }

        return commands;
    }

    public List<Command> GetKillCommands(Stack<MovableState> path)
    {
        var commands = new List<Command>();

        var list = path.ToList();
        list.Reverse();

        for (int i = 1; i < list.Count(); i++)
        {
            var state = list[i];
            var room = Map[state.X, state.Y];

            room.Reset();
            if (room.CanRotate)
            {
                for (int r = 0; r < 4; r++)
                {
                    var newState = room.Move(state);

                    if (newState.Alive == false)
                    {
                        var cmd = state.GetCommand();

                        if (cmd != null)
                        {
                            Console.Error.WriteLine($"add {cmd.X} {cmd.Y} {cmd.Type}");
                            commands.Add(cmd);
                        }
                        // no need to turn
                        else
                        {
                            break;
                        }
                    }

                    room.Rotate();
                }
            }
        }

        return commands;
    }

    public void ResetMap()
    {
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                Map[i, j].Reset();
            }
        }
    }

    public void ProcessCommand(Command cmd)
    {
        switch (cmd.Type)
        {
            case CommandType.LEFT:
            case CommandType.RIGHT:
            case CommandType.DOUBLE:
                {
                    var move = cmd.Moves.Pop();
                    this.processMove(cmd.X, cmd.Y, move);
                    Console.WriteLine($"{cmd.X} {cmd.Y} {move}");
                    break;
                }
            case CommandType.WAIT:
                {
                    Console.WriteLine("WAIT");
                    break;
                }
        }
    }

    private void processMove(int x, int y, RotationMove move)
    {
        var room = Map[x, y];
        room.Update(move);
    }

    public Command GetNextCommand()
    {
        Console.Error.WriteLine("GetNextCommand()");

        Command output = null;
        if (_calculateIndyPath)
        {
            Indy.Path = this.GetPathToEnd(Indy);
            Indy.Commands = this.BuildCommands(Indy.Path);
            this.ResetMap();
            _calculateIndyPath = false;
        }

        var indyState = Indy.CurrentState;
        var indyCommand = Indy.Commands.Count() != 0 ? Indy.Commands.Peek() : null;
        bool canKill = Rocks.Count() > 0 &&
                        (indyCommand == null ||    //if no command left for Indy
                        Math.Abs(indyCommand.X - indyState.X) + Math.Abs(indyCommand.Y - indyState.Y) > 1); // if next room to move is not adjacent to indy position

        if (canKill == true)
        {
            int count = 1;
            foreach (var rock in Rocks)
            {
                Console.Error.WriteLine($"process rock {count++} {rock.CurrentState.Y}");
                rock.Path = this.GetPathToEnd(rock);
                rock.Commands = this.GetKillCommands(rock.Path);
                this.ResetMap();

                //check if commands are valid
                var toRemove = new List<Command>();
                for (int i = 0; i < rock.Commands.Count(); i++)
                {
                    Console.Error.WriteLine($"count {rock.Commands.Count()}");
                    var cmd = rock.Commands[i];
                    Console.Error.WriteLine($"Command {cmd.X} {cmd.Y} {cmd.Type}");

                    // if command not on Indy's path OK
                    if (Indy.Path.Any(s => s.X == cmd.X && s.Y == cmd.Y) == false)// check if command is not on indypath
                    {

                    }
                    // check if command is on Indy current room
                    else if(cmd.X == Indy.CurrentState.X && cmd.Y == Indy.CurrentState.Y)
                    {
                        Console.Error.WriteLine($"Command on indy {cmd.X} {cmd.Y} {cmd.Type} invalid");
                        toRemove.Add(cmd);
                    }
                    else
                    {
                        // check if new command will kill indy
                        var room = Map[cmd.X, cmd.Y];

                        foreach (var mv in cmd.Moves)
                        {
                            room.Rotate(mv);
                        }

                        var stateAtRoom = Indy.Path.First(s => s.X == cmd.X && s.Y == cmd.Y);
                        var newState = room.Move(stateAtRoom);

                        if (newState.Alive == false)
                        {
                            Console.Error.WriteLine($"Command {cmd.X} {cmd.Y} {cmd.Type} invalid");
                            toRemove.Add(cmd);
                        }
                        else
                        {
                            Console.Error.WriteLine($"Command {cmd.X} {cmd.Y} {cmd.Type} valid");
                        }

                        room.Reset();
                    }
                }

                rock.Commands.RemoveAll(c => toRemove.Contains(c));
            }

            // choose most urging rock to kill
            int turnLeftToMove = int.MaxValue;
            Rock rockToKill = null;

            foreach (var rock in Rocks)
            {
                var path = rock.Path.ToList();
                path.Reverse();

                int last = int.MaxValue;
                for (int i = 0; i < path.Count; i++)
                {
                    var state = path[i];
                    if (rock.Commands.Any(c => c.X == state.X && c.Y == state.Y)
                        && Indy.Path.Any(s => s.X == state.X && s.Y == state.Y) == false)
                        last = i;
                }
                if (last < turnLeftToMove)
                {
                    turnLeftToMove = last;
                    rockToKill = rock;

                    Console.Error.WriteLine($"select rock {rockToKill.CurrentState.X} {rockToKill.CurrentState.Y} {last}");
                }
            }

            // choose most urging rock to kill among ones on indy's path
            turnLeftToMove = int.MaxValue;

            if (rockToKill == null)
            {
                foreach (var rock in Rocks)
                {
                    var path = rock.Path.ToList();
                    path.Reverse();

                    int last = int.MaxValue;
                    for (int i = 0; i < path.Count; i++)
                    {
                        var state = path[i];
                        if (rock.Commands.Any(c => c.X == state.X && c.Y == state.Y))
                            last = i;
                    }
                    if (last < turnLeftToMove)
                    {
                        turnLeftToMove = last;
                        rockToKill = rock;

                        Console.Error.WriteLine($"select rock {rockToKill.CurrentState.X} {rockToKill.CurrentState.Y} {last}");
                        _calculateIndyPath = true;
                    }
                }
            }

            if (rockToKill != null)
                output = rockToKill.Commands.First();
        }
        else
        {
            // select and delete next command
            if (indyCommand != null)
                output = Indy.Commands.Pop();
        }


        if (output == null)
            output = new Command(-1, -1, CommandType.WAIT);

        return output;
    }
}

public class Indy : Movable
{
    public Stack<Command> Commands;

    public Indy(MovableState state)
       : base(state)
    {

    }
}

public class Rock : Movable
{
    public List<Command> Commands;

    public Rock(MovableState state)
       : base(state)
    {

    }
}

public class MovableState
{
    public int X;
    public int Y;
    public bool Alive;
    public MovableFrom From;
    public RoomState RoomState;
    public int NbTurns;

    public MovableState(int x, int y, string sFrom, RoomState rState, int nbTurns, bool alive)
    {
        X = x;
        Y = y;
        RoomState = rState;
        NbTurns = nbTurns;
        Alive = alive;

        switch (sFrom)
        {
            case "TOP":
                From = MovableFrom.TOP;
                break;
            case "LEFT":
                From = MovableFrom.LEFT;
                break;
            case "RIGHT":
                From = MovableFrom.RIGHT;
                break;
        }
    }

    public MovableState(int x, int y, MovableFrom from, RoomState rState, int nbTurns, bool alive)
    {
        X = x;
        Y = y;
        From = from;
        RoomState = rState;
        Alive = alive;
        NbTurns = nbTurns;
    }

    private MovableState() { }

    public MovableState Clone()
    {
        var clone = new MovableState();
        clone.X = X;
        clone.Y = Y;
        clone.Alive = Alive;
        clone.NbTurns = NbTurns;
        clone.From = From;
        clone.RoomState = RoomState.NONE;

        return clone;
    }

    public Command GetCommand()
    {
        Command cmd = null;
        switch (this.RoomState)
        {
            case RoomState.NONE:
                break;
            case RoomState.LEFT:
                cmd = new Command(X, Y, CommandType.LEFT);
                break;
            case RoomState.RIGHT:
                cmd = new Command(X, Y, CommandType.RIGHT);
                break;
            case RoomState.DOUBLE:
                cmd = new Command(X, Y, CommandType.DOUBLE);
                break;
        }

        return cmd;
    }
}

public class Movable
{
    public MovableState CurrentState;
    public Stack<MovableState> Path;

    public Movable(MovableState state)
    {
        CurrentState = state;
    }
}

public class Room
{
    public int X;
    public int Y;
    private int _initialRoomType;
    public int RoomType;
    private bool _initialCanRotate;
    public bool CanRotate;

    public RoomState State = RoomState.NONE;

    public Room(int x, int y, int roomType)
    {
        X = x;
        Y = y;
        _initialRoomType = RoomType = Math.Abs(roomType);
        _initialCanRotate = CanRotate = roomType > 0;
    }

    public MovableState Move(MovableState state)
    {
        state.RoomState = State;
        var newState = state.Clone();
        switch (RoomType)
        {
            case 0:
                this.Kill(newState);
                break;
            case 1:
                this.MoveDown(newState);
                break;
            case 2:
                if (newState.From == MovableFrom.TOP)
                    this.Kill(newState);
                else if (newState.From == MovableFrom.LEFT)
                    this.MoveRight(newState);
                else if (newState.From == MovableFrom.RIGHT)
                    this.MoveLeft(newState);
                break;
            case 3:
                if (newState.From == MovableFrom.LEFT || newState.From == MovableFrom.RIGHT)
                    this.Kill(newState);
                else
                    this.MoveDown(newState);
                break;
            case 4:
                if (newState.From == MovableFrom.LEFT)
                    this.Kill(newState);
                else if (newState.From == MovableFrom.TOP)
                    this.MoveLeft(newState);
                else if (newState.From == MovableFrom.RIGHT)
                    this.MoveDown(newState);
                break;
            case 5:
                if (newState.From == MovableFrom.RIGHT)
                    this.Kill(newState);
                if (newState.From == MovableFrom.TOP)
                    this.MoveRight(newState);
                else if (newState.From == MovableFrom.LEFT)
                    this.MoveDown(newState);
                break;
            case 6:
                if (newState.From == MovableFrom.TOP)
                    this.Kill(newState);
                else if (newState.From == MovableFrom.LEFT)
                    this.MoveRight(newState);
                else if (newState.From == MovableFrom.RIGHT)
                    this.MoveLeft(newState);
                break;
            case 7:
                if (newState.From == MovableFrom.LEFT)
                    this.Kill(newState);
                else
                    this.MoveDown(newState);
                break;
            case 8:
                if (newState.From == MovableFrom.TOP)
                    this.Kill(newState);
                else
                    this.MoveDown(newState);
                break;
            case 9:
                if (newState.From == MovableFrom.RIGHT)
                    this.Kill(newState);
                else
                    this.MoveDown(newState);
                break;
            case 10:
                if (newState.From == MovableFrom.LEFT || newState.From == MovableFrom.RIGHT)
                    this.Kill(newState);
                else
                    this.MoveLeft(newState);
                break;
            case 11:
                if (newState.From == MovableFrom.LEFT || newState.From == MovableFrom.RIGHT)
                    this.Kill(newState);
                else
                    this.MoveRight(newState);
                break;
            case 12:
                if (newState.From == MovableFrom.LEFT || newState.From == MovableFrom.TOP)
                    this.Kill(newState);
                else
                    this.MoveDown(newState);
                break;
            case 13:
                if (newState.From == MovableFrom.RIGHT || newState.From == MovableFrom.TOP)
                    this.Kill(newState);
                else
                    this.MoveDown(newState);
                break;
        }
        newState.NbTurns++;
        return newState;
    }

    private void Kill(MovableState state)
    {
        state.Alive = false;
    }

    private void MoveDown(MovableState state)
    {
        state.Y++;
        state.From = MovableFrom.TOP;
    }

    private void MoveLeft(MovableState state)
    {
        state.X--;
        state.From = MovableFrom.RIGHT;
    }

    private void MoveRight(MovableState state)
    {
        state.X++;
        state.From = MovableFrom.LEFT;
    }

    public void Reset()
    {
        RoomType = _initialRoomType;
        State = RoomState.NONE;
        CanRotate = _initialCanRotate;
    }

    public void Rotate()
    {
        switch (State)
        {
            case RoomState.NONE:
                this.Rotate(RotationMove.LEFT);
                State = RoomState.LEFT;
                break;
            case RoomState.LEFT:
                this.Rotate(RotationMove.LEFT);
                State = RoomState.DOUBLE;
                break;
            case RoomState.DOUBLE:
                this.Rotate(RotationMove.LEFT);
                State = RoomState.RIGHT;
                CanRotate = false;
                break;
            case RoomState.RIGHT:
                // on ne peut plus tourner
                break;
        }
    }

    public void Update(RotationMove move)
    {
        this.Rotate(move);
        _initialRoomType = RoomType;
        State = RoomState.NONE;
    }

    public void Rotate(RotationMove move)
    {
        switch (RoomType)
        {
            case 2:
                RoomType = 3;
                break;
            case 3:
                RoomType = 2;
                break;
            case 4:
                RoomType = 5;
                break;
            case 5:
                RoomType = 4;
                break;
            case 6:
                if (move == RotationMove.LEFT)
                    RoomType = 9;
                else if (move == RotationMove.RIGHT)
                    RoomType = 7;
                break;
            case 7:
                if (move == RotationMove.LEFT)
                    RoomType = 6;
                else if (move == RotationMove.RIGHT)
                    RoomType = 8;
                break;
            case 8:
                if (move == RotationMove.LEFT)
                    RoomType = 7;
                else if (move == RotationMove.RIGHT)
                    RoomType = 9;
                break;
            case 9:
                if (move == RotationMove.LEFT)
                    RoomType = 8;
                else if (move == RotationMove.RIGHT)
                    RoomType = 6;
                break;
            case 10:
                if (move == RotationMove.LEFT)
                    RoomType = 13;
                else if (move == RotationMove.RIGHT)
                    RoomType = 11;
                break;
            case 11:
                if (move == RotationMove.LEFT)
                    RoomType = 10;
                else if (move == RotationMove.RIGHT)
                    RoomType = 12;
                break;
            case 12:
                if (move == RotationMove.LEFT)
                    RoomType = 11;
                else if (move == RotationMove.RIGHT)
                    RoomType = 13;
                break;
            case 13:
                if (move == RotationMove.LEFT)
                    RoomType = 12;
                else if (move == RotationMove.RIGHT)
                    RoomType = 10;
                break;
            default:
                break;
        }
    }
}