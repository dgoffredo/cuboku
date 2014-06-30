using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudokudos
{
    class SudokudosServer
    {
        public class Finished
        {
            public string player {get; set;}
            public string puzzle {get; set;}
            public string solution { get; set; }
            public int durationSeconds { get; set; }
            public string httpQuery { get; set; }
        }
        static public Finished finished(string player, string puzzle, string solution, int durationSeconds)
        {
            return new Finished
            {
                player = player,
                puzzle = puzzle,
                solution = solution,
                durationSeconds = durationSeconds,
                httpQuery = String.Format("http://www.sudokudos.com/finished/?player={0};puzzle={1};solution={2};seconds={3}",
                                        puzzle, solution, durationSeconds)
            };
        }
    }
}
