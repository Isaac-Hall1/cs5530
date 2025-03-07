using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace ChessBrowser.Components.Pages
{
    public partial class ChessBrowser
    {
    /// <summary>
    /// Bound to the Unsername form input
    /// </summary>
    private string Username = "";

    /// <summary>
    /// Bound to the Password form input
    /// </summary>
    private string Password = "";

    /// <summary>
    /// Bound to the Database form input
    /// </summary>
    private string Database = "";

    /// <summary>
    /// Represents the progress percentage of the current
    /// upload operation. Update this value to update 
    /// the progress bar.
    /// </summary>
    private int    Progress = 0;

    /// <summary>
    /// This method runs when a PGN file is selected for upload.
    /// Given a list of lines from the selected file, parses the 
    /// PGN data, and uploads each chess game to the user's database.
    /// </summary>
    /// <param name="PGNFileLines">The lines from the selected file</param>
    private async Task InsertGameData(string[] PGNFileLines)
    {
        
        // This will build a connection string to your user's database on atr,
        // assuimg you've filled in the credentials in the GUI
        string connection = GetConnectionString();
        // TODO:
        //   Parse the provided PGN data
        //   We recommend creating separate libraries to represent chess data and load the file
        List<ChessGame> games = PgnParser.readPGN(PGNFileLines);

        using (MySqlConnection conn = new MySqlConnection(connection))
        {
            try
            {
                    // Open a connection
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    // TODO:
                    //   Iterate through your data and generate appropriate insert commands
                    foreach (ChessGame game in games)
                    {
                        cmd.CommandText = "insert ignore into Events (Name, Site, Date)" +
                            " values(@eName, @Site, @Date);";
                        cmd.CommandText += "insert ignore into Players (Name, Elo)" +
                            " values(@wName, @wElo) ON DUPLICATE KEY UPDATE Elo = IF(@wElo > Elo, @wElo, Elo);";
                        cmd.CommandText += "insert ignore into Players (Name, Elo)" +
                            " values(@bName, @bElo) ON DUPLICATE KEY UPDATE Elo = IF(@bElo > Elo, @bElo, Elo);";
                        cmd.CommandText += "insert ignore into Games (Round, Result, Moves, BlackPlayer, WhitePlayer, eID)" +
                            " values(@Round,@Result,@Moves,(Select pID from Players Where Players.Name = @BlackPlayer),(Select pID from Players Where Players.Name = @WhitePlayer)," +
                            "(Select eID from Events where Events.Name = @eName and Events.Date = @Date and Events.Site = @Site));";
                        cmd.Parameters.AddWithValue("@eName", game.Event);
                        cmd.Parameters.AddWithValue("@Site", game.Site);
                        cmd.Parameters.AddWithValue("@Date", game.EventDate);
                        cmd.Parameters.AddWithValue("@bName", game.Black);
                        cmd.Parameters.AddWithValue("@bElo", int.TryParse(game.BlackElo, out int _));
                        cmd.Parameters.AddWithValue("@wName", game.White);
                        cmd.Parameters.AddWithValue("@wElo", int.TryParse(game.WhiteElo, out int _));
                        cmd.Parameters.AddWithValue("@Round", game.Round);
                        cmd.Parameters.AddWithValue("@Result", game.Result);
                        cmd.Parameters.AddWithValue("@Moves", game.Moves);
                        cmd.Parameters.AddWithValue("@BlackPlayer", game.Black);
                        cmd.Parameters.AddWithValue("@WhitePlayer", game.White);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        Progress += 1;
                        await InvokeAsync(StateHasChanged);
                    }   

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }


    /// <summary>
    /// Queries the database for games that match all the given filters.
    /// The filters are taken from the various controls in the GUI.
    /// </summary>
    /// <param name="white">The white player, or "" if none</param>
    /// <param name="black">The black player, or "" if none</param>
    /// <param name="opening">The first move, e.g. "1.e4", or "" if none</param>
    /// <param name="winner">The winner as "W", "B", "D", or "" if none</param>
    /// <param name="useDate">true if the filter includes a date range, false otherwise</param>
    /// <param name="start">The start of the date range</param>
    /// <param name="end">The end of the date range</param>
    /// <param name="showMoves">true if the returned data should include the PGN moves</param>
    /// <returns>A string separated by newlines containing the filtered games</returns>
    private string PerformQuery(string white, string black, string opening,
    string winner, bool useDate, DateTime start, DateTime end, bool showMoves)
    {
        // This will build a connection string to your user's database on atr,
        // assuimg you've typed a user and password in the GUI
        string connection = GetConnectionString();

        // Build up this string containing the results from your query
        string parsedResult = "";

        // Use this to count the number of rows returned by your query
        // (see below return statement)
        int numRows = 0;

        using (MySqlConnection conn = new MySqlConnection(connection))
        {
            try
            {
                // Open a connection
                conn.Open();

                // TODO:
                //   Generate and execute an SQL command,
                //   then parse the results into an appropriate string and return it.
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

       return numRows + " results\n" + parsedResult;
    }


    private string GetConnectionString()
    {
            return "server=atr.eng.utah.edu;database=" + Database + ";uid=" + Username + ";password=" + Password;
        }


    /// <summary>
    /// This method will run when the file chooser is used.
    /// It loads the files contents as an array of strings,
    /// then invokes the InsertGameData method.
    /// </summary>
    /// <param name="args">The event arguments, which contains the selected file name</param>
    private async void HandleFileChooser(EventArgs args)
    {
        try
        {
            string fileContent = string.Empty;

            InputFileChangeEventArgs eventArgs = args as InputFileChangeEventArgs ?? throw new Exception("unable to get file name");
            if (eventArgs.FileCount == 1)
            {
                var file = eventArgs.File;
                if (file is null)
                {
                    return;
                }

                // load the chosen file and split it into an array of strings, one per line
                using var stream = file.OpenReadStream(1000000); // max 1MB
                using var reader = new StreamReader(stream);                   
                fileContent = await reader.ReadToEndAsync();
                string[] fileLines = fileContent.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                // insert the games, and don't wait for it to finish
                // _ = throws away the task result, since we aren't waiting for it
                _ = InsertGameData(fileLines);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("an error occurred while loading the file..." + e);
        }
        }

    }

}
