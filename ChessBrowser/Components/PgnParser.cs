


using Microsoft.AspNetCore.Mvc.Diagnostics;
using MySqlX.XDevAPI.Common;

namespace ChessBrowser.Components
{
    static class PgnParser
    {
        /// <summary>
        /// This method takes in a pgn file and returns a list of chess games
        /// </summary>
        public static List<ChessGame> readPGN(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            // hello
            string pgnContent = File.ReadAllText(filePath);
            List<ChessGame> games = parseGames(pgnContent);

            // returns list<chessgames>
            return games;
        }

        /**
         * [Event "4. IIFL Wealth Mumbai Op"]
           [Site "Mumbai IND"]
           [Date "2018.12.31"]
           [Round "2.9"]
           [White "Sundararajan, Kidambi"]
           [Black "Ziatdinov, Raset"]
           [Result "1/2-1/2"]
           [WhiteElo "2458"]
           [BlackElo "2252"]
           [ECO "A25"]
           [EventDate "2018.?.30"]
            
           1.c4 e5 2.Nc3 Nc6 3.e3 Nf6 4.a3 Be7 5.Nf3 O-O 6.Be2 d6 7.d4 exd4 8.Nxd4 
           Nxd4 9.Qxd4 Be6 10.Nd5 c5 11.Nxe7+ Qxe7 12.Qh4 d5 13.cxd5 Bxd5 14.f3 Qe6 
           15.O-O Nd7 16.Bd2 f5 17.Rac1 Rac8 18.Rfe1 Ne5 19.Bc3 Ng6 20.Qf2 Bb3 21.Bf1
           a6 22.Qg3 Qe7 23.Bd3 Rc6 24.Qf2 Re6 25.g3 h5 26.h4 b5 27.f4 Bd5 28.Be2 Kf7
           29.Bxh5 Rh8 30.Rcd1 Bb3 31.Bf3 Bxd1 32.Rxd1 Rd8 33.Rd5 Kg8 34.Rxf5 Rxe3 
           35.Bd5+ Rxd5 36.Rxd5 Qe4 37.Rd1 Re2 38.Re1 Nxf4 39.Rxe2 Nxe2+ 40.Kh2 Nd4 
           41.Qf4 Qxf4 42.gxf4 Nc6 43.Kg3 b4 44.Bd2 a5 45.Kf3 c4 46.Ke4 1/2-1/2
            
         */
        /// <summary>
        /// This method returns a list of games
        /// </summary>
        /// <param name="pgnContent"></param>
        /// <returns>List<ChessGame></returns>
        static List<ChessGame> parseGames(string pgnContent)
        {
            // all of the possible tags
            HashSet<string> tags = new HashSet<string> { "Event", "Site", "Date", "Round", "White", "Black", "Result", "WhiteElo", "BlackElo", "EventDate" };
            int emptyLineCount = 0;
            string Moves = "";
            List<ChessGame> games = new List<ChessGame>();
            ChessGame game = new ChessGame();

            foreach (string line in pgnContent.Split('\n'))
            {
                if (line.StartsWith("["))
                {
                    var (tag, value) = StripStartTags(line, tags);
                    if (tag != "NonExistent")
                    {
                        typeof(ChessGame).GetProperty(tag)?.SetValue(game, value);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    emptyLineCount += 1;
                }
                else
                {
                    Moves += line;
                }
                if (emptyLineCount == 2)
                {
                    emptyLineCount = 0;
                    game.Moves = Moves;
                    CleanDirtyData(game);
                    games.Add(game);
                    game = new ChessGame();
                }
            }
            return games;
        }
        /// <summary>
        /// This method strings the unnecissary information from tags
        /// </summary>
        /// <param name="item"></param>
        /// <param name="tags"></param>
        /// <returns>(tag,string)</returns>
        private static (string, string) StripStartTags(string info, HashSet<string> tags)
        {
            // look at item do substring from [---- () ->
            int firstSpace = info.IndexOf(' ');
            string tag = info.Substring(1, firstSpace - 1);
            if (!tags.Contains(tag))
            {
                return ("NonExistent", "");
            }
            int lastLocation = info.IndexOf("]");
            info = info.Substring(0, lastLocation - 1).Substring(firstSpace + 2);
            return (tag, info);
        }
        public static void CleanDirtyData(ChessGame game)
        {
            if (game.EventDate != null)
            {
                if (game.EventDate.Contains("?"))
                {
                    game.EventDate = "0000-00-00";
                }
            }
            // Convert Result: "1-0" -> "W", "0-1" -> "B", "1/2-1/2" -> "D"
            game.Result = game.Result switch
            {
                "1-0" => "W",
                "0-1" => "B",
                "1/2-1/2" => "D",
                _ => throw new NotImplementedException()
            };
        }
    }
}
