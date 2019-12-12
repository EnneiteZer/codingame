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
class Solution
{
    static void Main(string[] args)
    {
        var game = new Game();

        game.Init();
        var sum = game.GetSum();

        Console.WriteLine(sum);
    }

    public class Game
    {
        public enum GameState { PRELOOP, LOOP, POSTLOOP }
        GameState _state = GameState.PRELOOP;

        int _maxPlaces;
        int _maxTurns;
        int _countGroups;

        int _turn = 0;

        long _sum = 0;
        int _firstGroupIndex = 0;

        List<Group> _waiting = new List<Group>();
        List<Result> _results = new List<Result>();

        public void Init()
        {
            string[] inputs = Console.ReadLine().Split(' ');
            _maxPlaces = int.Parse(inputs[0]);
            _maxTurns = int.Parse(inputs[1]);
            _countGroups = int.Parse(inputs[2]);

            Console.Error.WriteLine($"max : {_maxPlaces}, turns : {_maxTurns}, count : {_countGroups}");

            for (int i = 0; i < _countGroups; i++)
            {
                long value = long.Parse(Console.ReadLine());
                _waiting.Add(new Group(value));
            }
        }

        public long GetSum()
        {
            _sum = 0;
            while (_turn < _maxTurns)
            {
                _sum += playTurn();
            }

            return _sum;
        }

        private long playTurn()
        {
            var first = _waiting[_firstGroupIndex % _countGroups];

            // check if result already exists
            var res = _results.FirstOrDefault(r => r.Group.Equals(first));
            if (_state == GameState.PRELOOP && res != null)
            {
                _state = GameState.LOOP;
            }

            long sum = 0;

            switch (_state)
            {
                case GameState.PRELOOP:
                    {
                        sum = playStandardTurn(first);
                        break;
                    }
                case GameState.LOOP:
                    {
                        sum = playLoop(res);
                        _state = GameState.POSTLOOP;
                        break;
                    }
                case GameState.POSTLOOP:
                    {
                        sum = playStandardTurn(first);
                        break;
                    }
                default:
                    break;
            }

            return sum;
        }

        private long playLoop(Result res)
        {
            long cycleValue = _sum - res.Sum;
            int cycleDuration = _turn - res.Turns;

            int nbCycle = (_maxTurns - _turn) / cycleDuration;

            _turn += nbCycle * cycleDuration;

            return nbCycle * cycleValue;
        }

        private long playStandardTurn(Group first)
        {
            var next = first;
            int initIndex = _firstGroupIndex;

            long count = 0;
            _results.Add(new Result(first, _turn, _sum));

            while (count + next.Count <= _maxPlaces)
            {
                count += next.Count;
                _firstGroupIndex++;

                next = _waiting[_firstGroupIndex % _countGroups];

                if (_firstGroupIndex % _countGroups == initIndex % _countGroups)
                    break;
            }

            _turn++;

            return count;
        }
    }
}

public class Result
{
    public Group Group;
    public int Turns;
    public long Sum;

    public Result(Group grp, int turns, long sum)
    {
        Group = grp;
        Turns = turns;
        Sum = sum;
    }
}

public class Group
{
    public long Count = 0;

    public Group(long count)
    {
        Count = count;
    }
}