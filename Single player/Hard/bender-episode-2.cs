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
    public class Room
    {
        public int ID;
        public int IDNextRoom1;
        public int IDNextRoom2;
        public int Sum;
    }
    
    public static List<Room> Rooms = new List<Room>();
    public static Dictionary<int,int> Computed = new Dictionary<int,int>();
    
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());

        var inputs = new List<string>();
        for (int i = 0; i < N; i++)
        {
            string input = Console.ReadLine();
            inputs.Add(input);
            Console.Error.WriteLine(input);
        }
        
        Room startRoom = null;
        foreach (var input in inputs)
        {
            var entries = input.Split(' ');
            var room = new Room()
            {
                ID = int.Parse(entries[0]),
                Sum = int.Parse(entries[1])
            };
            room.IDNextRoom1 =  entries[2] == "E" ? -1 : int.Parse(entries[2]);
            room.IDNextRoom2 =  entries[3] == "E" ? -1 : int.Parse(entries[3]);
            
            if (room.ID == 0)
                startRoom = room;
            Rooms.Add(room);
        }
        
        Console.WriteLine(GetMaxSum(startRoom));
    }
    
    public static int GetMaxSum(Room room)
    {
        int val = 0;
        if(room == null)
        {
        }
        else
        {
            var id1 = room.IDNextRoom1;
            var id2 = room.IDNextRoom2;
            
            if(Computed.TryGetValue(id1, out int val1) == false)
            {
                var nextRoom1 = Rooms.FirstOrDefault(r => id1 == r.ID);
                val1 = GetMaxSum(nextRoom1);
                Computed.Add(id1, val1);
            }
            if(Computed.TryGetValue(id2, out int val2) == false)
            {
                var nextRoom2 = Rooms.FirstOrDefault(r => id2 == r.ID);
                val2 = GetMaxSum(nextRoom2);
                Computed.Add(id2, val2);
            }
            
            val = room.Sum + Math.Max(val1, val2);
            //Console.Error.WriteLine($"room {room.ID} => {val}");
        }
        
        return val;
    }
}