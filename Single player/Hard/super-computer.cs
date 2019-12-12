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
    static List<Session> _askedSessions = new List<Session>();

    static void Main(string[] args)
    {
        var authSessions = new List<Session>();

        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int J = int.Parse(inputs[0]);
            int D = int.Parse(inputs[1]);

            _askedSessions.Add(new Session(J, D));
        }

        _askedSessions = _askedSessions.OrderBy(s => s.End).ToList();

        var curr = _askedSessions.First();
        authSessions.Add(curr);

        foreach (var elt in _askedSessions)
        {
            if(elt.Start > curr.End)
            {
                authSessions.Add(elt);
                curr = elt;
            }
        }

        Console.WriteLine(authSessions.Count());
    }
   
}
public class Session
{
    public int Start;
    public int End;
    public int Duration;

    public Session(int start, int duration)
    {
        Start = start;
        Duration = duration;
        End = start + duration - 1;
    }
}