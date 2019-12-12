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
    public static int width, height;
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        width = int.Parse(inputs[0]); // width of the firewall grid
        height = int.Parse(inputs[1]); // height of the firewall grid

        var grid = new List<string>();
        for (int i = 0; i < height; i++)
        {
            string mapRow = Console.ReadLine(); // one line of the firewall grid
            grid.Add(mapRow);
            Console.Error.WriteLine(mapRow);
        }

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int rounds = int.Parse(inputs[0]); // number of rounds left before the end of the game
            int bombs = int.Parse(inputs[1]); // number of bombs left

            int iFinal = -1, jFinal = -1;
            bool ok = false;
            // test de la future action
            var eliminatedPos = new List<(int I, int J)>();
            while (!ok)
            {
                var tmpGrid = new List<string>();
                grid.ForEach(s => tmpGrid.Add(s));

                // on évalue la grille pour trouver les bombes à portée en chaque point
                var tab = EvaluateGrid(tmpGrid);

                // on trouve les points avec le max de bombes à proximité
                int max = 0;
                var pts = new List<Tuple<int, int>>();
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        var val = tab[i, j];

                        if (eliminatedPos.Contains((i, j)))
                            break;

                        if (val > max)
                        {
                            max = val;

                            pts.Clear();
                            pts.Add(new Tuple<int, int>(i, j));
                        }
                        else if (val == max)
                        {
                            pts.Add(new Tuple<int, int>(i, j));
                        }
                    }
                }

                // on prend le point max où il n'y a pas de bombe
                int iBomb = -1, jBomb = -1;
                foreach (var pt in pts)
                {
                    if (tmpGrid[pt.Item1][pt.Item2] == '.')
                    {
                        iBomb = pt.Item1;
                        jBomb = pt.Item2;
                        break;
                    }
                }

                // on update les countdowns dans tous les cas
                UpdateCountDown(tmpGrid);

                if (iBomb != -1)
                {
                    // on calcule la nouvelle grille suite à la bombe qu'on va poser
                    UpdateGrid(tmpGrid, iBomb, jBomb);
                    // on compte les bombes seules
                    var count = CountAloneTargets(tmpGrid);

                    // si trop de bombes isolés => mauvaise solution
                    Console.Error.WriteLine($"alone = {count}, bombsLeft = {bombs - 1}");
                    if (count > bombs - 1)
                    {
                        eliminatedPos.Add((iBomb, jBomb));
                        ok = false;
                    }
                    else
                    {
                        iFinal = iBomb;
                        jFinal = jBomb;
                        ok = true;
                    }
                }
                else
                {
                    ok = true;
                }
            }

            // on update les countdowns dans tous les cas
            UpdateCountDown(grid);

            if (iFinal == -1)
            {
                Console.WriteLine("WAIT");
            }
            else
            {
                // on calcule la nouvelle grille suite à la bombe qu'on va poser
                UpdateGrid(grid, iFinal, jFinal);
                Console.WriteLine($"{jFinal} {iFinal}");

                // on compte les bombes seules
                CountAloneTargets(grid);
            }

            grid.ForEach(l => Console.Error.WriteLine(l));
        }
    }

    public static void UpdateGrid(List<string> grid, int iBomb, int jBomb)
    {

        // on update avec la bome
        for (int k = 1; k <= 3; k++)
        {
            int iTmp = iBomb + k;
            int jTmp = jBomb;
            if (iTmp < height)
            {
                if (grid[iTmp][jTmp] == '@')
                    grid[iTmp] = ReplaceChar(grid[iTmp], jTmp, '2');
                if (grid[iTmp][jTmp] == '#')
                    break;
            }
        }

        for (int k = 1; k <= 3; k++)
        {
            int iTmp = iBomb - k;
            int jTmp = jBomb;
            if (iTmp >= 0)
            {
                if (grid[iTmp][jTmp] == '@')
                    grid[iTmp] = ReplaceChar(grid[iTmp], jTmp, '2');

                if (grid[iTmp][jTmp] == '#')
                    break;
            }
        }

        for (int k = 1; k <= 3; k++)
        {
            int iTmp = iBomb;
            int jTmp = jBomb + k;
            if (jTmp < width)
            {
                if (grid[iTmp][jTmp] == '@')
                    grid[iTmp] = ReplaceChar(grid[iTmp], jTmp, '2');

                if (grid[iTmp][jTmp] == '#')
                    break;
            }
        }

        for (int k = 1; k <= 3; k++)
        {
            int iTmp = iBomb;
            int jTmp = jBomb - k;
            if (jTmp >= 0)
            {
                if (grid[iTmp][jTmp] == '@')
                    grid[iTmp] = ReplaceChar(grid[iTmp], jTmp, '2');
                if (grid[iTmp][jTmp] == '#')
                    break;
            }
        }
    }

    public static int CountAloneTargets(List<string> grid)
    {
        int count = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (grid[i][j] == '@')
                {
                    bool ok = false;
                    int iup = i - 6, idown = i + 6;
                    int jLeft = j - 3, jRight = j + 3;

                    for (int k = iup; k <= idown; k++)
                    {
                        for (int l = jLeft; l <= jRight; l++)
                        {
                            if (i == k && j == l ||
                                k < 0 || k >= height || l < 0 || l >= width)
                                continue;

                            if (grid[k][l] == '@')
                            {
                                ok = true;
                            }
                        }
                    }

                    if (ok == false)
                    {
                        Console.Error.WriteLine($"alone {i} {j}");
                        count++;
                    }
                }
            }
        }
        return count;
    }

    public static void UpdateCountDown(List<string> grid)
    {
        // on update les timers de toute la grille
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (grid[i][j] == '1')
                    grid[i] = ReplaceChar(grid[i], j, '.');

                if (grid[i][j] == '2')
                    grid[i] = ReplaceChar(grid[i], j, '1');
            }
        }
    }

    public static string ReplaceChar(string line, int index, char newChar)
    {
        StringBuilder sb = new StringBuilder(line);
        sb[index] = newChar;
        return sb.ToString();
    }

    public static int[,] EvaluateGrid(List<string> grid)
    {
        var tab = new int[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var c = grid[i][j];

                // bombe => -1
                if (c == '@')
                    tab[i, j] = -1;
                // mur => -2
                else if (c == '#')
                    tab[i, j] = -2;
                // sinon nombre de bombes à portée
                else //if (c == '.')
                {
                    int bombsAround = 0;

                    for (int k = 1; k <= 3; k++)
                    {
                        int iTmp = i + k;
                        int jTmp = j;
                        if (iTmp < height)
                        {
                            if (grid[iTmp][jTmp] == '@')
                                bombsAround++;
                            if (grid[iTmp][jTmp] == '#')
                                break;
                        }
                    }

                    for (int k = 1; k <= 3; k++)
                    {
                        int iTmp = i - k;
                        int jTmp = j;
                        if (iTmp >= 0)
                        {
                            if (grid[iTmp][jTmp] == '@')
                                bombsAround++;
                            if (grid[iTmp][jTmp] == '#')
                                break;
                        }
                    }

                    for (int k = 1; k <= 3; k++)
                    {
                        int iTmp = i;
                        int jTmp = j + k;
                        if (jTmp < width)
                        {
                            if (grid[iTmp][jTmp] == '@')
                            {
                                bombsAround++;
                            }
                            if (grid[iTmp][jTmp] == '#')
                            {
                                break;
                            }
                        }
                    }

                    for (int k = 1; k <= 3; k++)
                    {
                        int iTmp = i;
                        int jTmp = j - k;
                        if (jTmp >= 0)
                        {
                            if (grid[iTmp][jTmp] == '@')
                                bombsAround++;
                            if (grid[iTmp][jTmp] == '#')
                                break;
                        }
                    }

                    tab[i, j] = bombsAround;
                }
            }
        }

        // print tab
        /*for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Console.Error.Write(tab[i, j] + " ");
            }
            Console.Error.Write("\n");
        }*/

        return tab;
    }
}