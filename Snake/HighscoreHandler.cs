using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Snake
{
    public class HighscoreHandler
    {
        public string DB_Path
        {
            get { return Path.Combine(Environment.CurrentDirectory, "db.txt"); }
        }
        public int GetHighScore()
        {
            if (!File.Exists(DB_Path))
            {
                SetHighScore(0);
            }
            using (StreamReader fileStream = new StreamReader(DB_Path))
            {
                return Convert.ToInt32(fileStream.ReadLine());
            }
        }

        public void SetHighScore(int highscore)
        {
            using (StreamWriter fileStream = new StreamWriter(DB_Path, false))
            {
                fileStream.Write(highscore.ToString());
            }
        }
        
    }
}
