using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileMap.Connection
{
    class Decoder
    {
        public string[] splitByColon(string s)
        {
            return s.Split(new char[] { ':' });
        }

        public string[] splitBySemiColon(string[] tokens, int a)
        {
            return tokens[a].Split(new char[] { ';' });
        }

        public string[] splitByComma(string[] tokens, int a)
        {
            return tokens[a].Split(new char[] { ',' });
        }
    }
}
