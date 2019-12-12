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

        // game loop
        while (true)
        {
            game.InitRound();

            string cmd = game.Solve();
            Console.WriteLine(cmd);
        }
    }

    public class Game
    {
        public static List<string> MoveTypes = new List<string>() { "SPEED", "SLOW", "JUMP", "UP", "DOWN" };

        public List<Bike> Bikes = new List<Bike>();
        public int BikeCount;
        public int SurviveCount;
        public List<string> Road = new List<string>();

        public int InvalidPenalty;
        public int DeadBikePenalty;

        public List<Combo> Combos = new List<Combo>();

        public Game() { }

        public string Solve()
        {
            Combo best = null;

            int maxScore = Int32.MinValue;
            foreach (var combo in Combos)
            {
                var score = Simulate(combo);

                if (score > maxScore)
                {
                    maxScore = score;
                    best = combo;
                }
            }

            this.Simulate(best, false);
            Console.Error.WriteLine($"best: {best.Moves[0]} {best.Moves[1]} {best.Moves[2]} {maxScore}");
            Bikes.ForEach(b => Console.Error.WriteLine($"{b.X} {b.Y} {b.Speed}  {b.Alive}"));
            Bikes.ForEach(b => b.Reload());

            return best.Moves.First();
        }

        private int Simulate(Combo combo, bool reload = true)
        {
            bool isValid = true;
            foreach (var move in combo.Moves)
            {
                switch (move)
                {
                    case "SPEED":
                        isValid = this.Speed();
                        break;
                    case "SLOW":
                        isValid = this.Slow();
                        break;
                    case "JUMP":
                        isValid = this.Jump();
                        break;
                    case "UP":
                        isValid = this.Up();
                        break;
                    case "DOWN":
                        isValid = this.Down();
                        break;
                }

                if (!isValid)
                    break;
            }

            int score = 0;
            if (!isValid)
            {
                score -= InvalidPenalty;
            }
            else
            {
                var X = Bikes.First(b => b.Alive).X;
                var speed = Bikes.First(b => b.Alive).Speed;
                var aliveCount = Bikes.Count(b => b.Alive);

                foreach (var bike in Bikes)
                {
                    if (bike.Alive_old == true)
                    {
                        if (bike.Alive == false || bike.Speed == 0)
                            score -= DeadBikePenalty;
                        else
                            score += X;
                    }
                }
            }

            if (reload)
            {
                //reload bikes
                Bikes.ForEach(b => b.Reload());
            }

            return score;
        }

        private bool Up()
        {
            var speed = Bikes.First(b => b.Alive).Speed;
            var X = Bikes.First(b => b.Alive).X;

            // si moto first lane => mouvement inutile
            if (Bikes.Any(b => b.Y == 0))
                return false;

            foreach (var bike in Bikes)
            {
                if (bike.Alive)
                {
                    var Y = bike.Y;
                    bool alive = !this.HasRoadHole(X, X + speed - 1, Y);
                    alive &= !this.HasRoadHole(X, X + speed, Y - 1);

                    bike.Update(X + speed, Y - 1, speed, alive);
                }
            }

            // si aucune moto alive, commande invalide
            return Bikes.Any(b => b.Alive);
        }

        private bool Down()
        {
            var speed = Bikes.First(b => b.Alive).Speed;
            var X = Bikes.First(b => b.Alive).X;

            // si moto last lane => mouvement inutile
            if (Bikes.Any(b => b.Y == 3))
                return false;

            foreach (var bike in Bikes)
            {
                if (bike.Alive)
                {
                    var Y = bike.Y;
                    bool alive = !this.HasRoadHole(X, X + speed - 1, Y);
                    alive &= !this.HasRoadHole(X, X + speed, Y + 1);

                    bike.Update(X + speed, Y + 1, speed, alive);
                }
            }

            // si aucune moto alive, commande invalide
            return Bikes.Any(b => b.Alive);
        }

        private bool Jump()
        {
            var speed = Bikes.First(b => b.Alive).Speed;
            var X = Bikes.First(b => b.Alive).X;

            foreach (var bike in Bikes)
            {
                if (bike.Alive)
                {
                    var Y = bike.Y;
                    bool alive = !this.HasRoadHole(X + speed, X + speed, Y); ;
                    bike.Update(X + speed, Y, speed, alive);
                }
            }

            // si aucune moto alive, commande invalide
            return Bikes.Any(b => b.Alive);
        }

        private bool Slow()
        {
            var speed = Bikes.First(b => b.Alive).Speed - 1; // currentspeed - 1
            var X = Bikes.First(b => b.Alive).X;

            // on ne s'arrête pas
            if (speed <= 0)
                return false;

            foreach (var bike in Bikes)
            {
                if (bike.Alive)
                {
                    var Y = bike.Y;
                    bool alive = !this.HasRoadHole(X, X + speed, Y);

                    bike.Update(X + speed, Y, speed, alive);
                }
            }

            // si aucune moto alive, commande invalide
            return Bikes.Any(b => b.Alive);

        }

        private bool Speed()
        {
            var speed = Bikes.First(b => b.Alive).Speed + 1; // currentspeed + 1
            var X = Bikes.First(b => b.Alive).X;

            // on évite d'aller trop rapidement
            if (speed > 10)
                return false;

            foreach (var bike in Bikes)
            {
                if (bike.Alive)
                {
                    var Y = bike.Y;
                    bool alive = !this.HasRoadHole(X, X + speed, Y);

                    bike.Update(X + speed, Y, speed, alive);
                }
            }

            // si aucune moto alive, commande invalide
            return Bikes.Any(b => b.Alive);
        }

        private bool HasRoadHole(int Xstart, int Xend, int Y)
        {
            try
            {
                bool hasHole = false;

                for (int x = Xstart; x <= Xend; x++)
                {
                    if (x < Road[Y].Length && Road[Y][x] == '0')
                    {
                        hasHole = true;
                        break;
                    }
                }

                return hasHole;
            }
            catch (Exception)
            {
                Console.Error.WriteLine($"{Xstart} {Xend} {Y}");
                throw;
            }
        }

        public void InitRound()
        {
            int S = int.Parse(Console.ReadLine()); // the motorbikes' speed
            for (int i = 0; i < BikeCount; i++)
            {
                Bikes[i].Load(S);
            }
        }

        public void InitOnce()
        {

            BikeCount = int.Parse(Console.ReadLine()); // the amount of motorbikes to control
            SurviveCount = int.Parse(Console.ReadLine()); // the minimum amount of motorbikes that must survive
            Road.Add(Console.ReadLine()); // L0 to L3 are lanes of the road. A dot character . represents a safe space, a zero 0 represents a hole in the road.
            Road.Add(Console.ReadLine());
            Road.Add(Console.ReadLine());
            Road.Add(Console.ReadLine());

            for (int i = 0; i < BikeCount; i++)
            {
                Bikes.Add(new Bike());
            }

            // generate all possibles combo for NbTurns turns
            foreach (var mv1 in MoveTypes)
            {
                foreach (var mv2 in MoveTypes)
                {
                    foreach (var mv3 in MoveTypes)
                    {
                        foreach (var mv4 in MoveTypes)
                        {
                            foreach (var mv5 in MoveTypes)
                            {
                                Combos.Add(new Combo(mv1, mv2, mv3, mv4, mv5));
                            }
                        }
                    }
                }
            }

            InvalidPenalty = 10000;
            DeadBikePenalty = 10 * Road.First().Length;
        }

    }

    public class Bike
    {
        public int X;
        public int Y;
        public int Speed;
        public bool Alive;

        public int X_old;
        public int Y_old;
        public int Speed_old;
        public bool Alive_old;
        public void Load(int speed)
        {
            var line = Console.ReadLine();
            string[] inputs = line.Split(' ');
            Console.Error.WriteLine(line);
            X = int.Parse(inputs[0]); // x coordinate of the motorbike
            Y = int.Parse(inputs[1]); // y coordinate of the motorbike
            Alive = int.Parse(inputs[2]) == 1; // indicates whether the motorbike is activated "1" or detroyed "0"
            Speed = speed;

            X_old = X;
            Y_old = Y;
            Speed_old = Speed;
            Alive_old = Alive;

        }
        public void Reload()
        {
            X = X_old;
            Y = Y_old;
            Speed = Speed_old;
            Alive = Alive_old;
        }

        public void Update(int x, int y, int speed, bool alive)
        {
            X = x;
            Y = y;
            Speed = speed;
            Alive = alive;
        }
    }

    public class Combo
    {
        public List<string> Moves;

        public Combo(string mv1, string mv2, string mv3, string mv4, string mv5)
        {
            Moves = new List<string>() { mv1, mv2, mv3, mv4, mv5 };
        }
    }
}