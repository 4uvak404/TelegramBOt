using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace TelegramBOt
{
    internal class TicTacToeMap
    {
        private int winLength = 3;
        public int Id { get; set; }
        public string StupidStringValue { get; set; }
        [NotMapped]
        public int[,] Values 
        {
            get
            {
                string[] temp = StupidStringValue.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                int[,] values = new int[height, width];
                for (int i = 0; i < height; i++)
                {
                    string[] temp2 = temp[i].Split(' ');
                    for (int j = 0; j < width; j++)
                    {
                        values[i, j] = int.Parse(temp2[j]);
                    }
                }
                
                return values;
            }
            set
            {
                StupidStringValue = "";
                int counter = 0;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (j == width - 1)
                        {
                            StupidStringValue += value[i, j].ToString() + "\n";
                        }
                        else
                        {
                            StupidStringValue += value[i, j].ToString() + " ";
                        }
                    }
                }
            }
        }
        public int height { get; set; }
        public int width { get; set; }
        public int turn { get; set; }
        private int onesInARowMax { get; set; }
        private int twosInARowMax { get; set; }
        private int onesInARow { get; set; }
        private int twosInARow { get; set; }
        public int WinLength
        {
            get { return winLength; }
            set
            {
                int min = Math.Min(height, width);
                if (value <= min) winLength = value;
            }
        }
        private TicTacToeMap() { }
        public TicTacToeMap(int height, int width, int winLength)
        {
            this.height = height;
            this.width = width;
            WinLength = winLength;
            turn = 0;
            Values = new int[height, width];
        }
        public bool SetElement(int row, int column)
        {
            
            if (Values[row, column] == 0)
            {
                int[,] temp = Values;
                temp[row, column] = turn % 2 == 0 ? 1 : 2;
                Values = temp;
                turn++;
                return true;
            }
            else return false;
        }
        public int CheckWin()
        {
            onesInARowMax = 0;
            twosInARowMax = 0;

            for (int i = 0; i  < height; i++)
            {
                onesInARow = 0;
                twosInARow = 0;
                for (int j = 0 ; j < width; j++)
                {
                    CheckOnesAndTwos(Values[i, j]);
                }
                if (onesInARowMax >= winLength) return 1;
                if (twosInARowMax >= winLength) return 2;
            }
            for (int j = 0; j < width; j++)
            {
                onesInARow = 0;
                twosInARow = 0;
                for (int i = 0; i < height; i++)
                {
                    CheckOnesAndTwos(Values[i, j]);
                }
                if (onesInARowMax >= winLength) return 1;
                if (twosInARowMax >= winLength) return 2;
            }
            for (int i = 0; i <= height - winLength; i++)
            {
                CountDiagonalRight(i, 0);
                if (onesInARowMax >= winLength) return 1;
                if (twosInARowMax >= winLength) return 2;
            }
            for (int i = 0; i <= width - winLength; i++)
            {
                CountDiagonalRight(0, i);
                if (onesInARowMax >= winLength) return 1;
                if (twosInARowMax >= winLength) return 2;
            }
            for (int i = 0; i <= height - winLength; i++)
            {
                CountDiagonalLeft(i, width - 1);
                if (onesInARowMax >= winLength) return 1;
                if (twosInARowMax >= winLength) return 2;
            }
            for (int i = width - 1; i >= winLength - 1; i--)
            {
                CountDiagonalLeft(0, i);
                if (onesInARowMax >= winLength) return 1;
                if (twosInARowMax >= winLength) return 2;
            }
            if (turn >= height * width) return -1;
            return 0;
        }
        public void Reset()
        {
            Values = new int[height,width];
        }
        private void CountDiagonalRight(int row, int column)
        {
            onesInARow = 0;
            twosInARow = 0;
            int min = Math.Min(height - row, width - column);
            for (int i = 0; i < min; i++)
            {
                CheckOnesAndTwos(Values[row + i, column + i]);
            }
        }
        private void CountDiagonalLeft(int row, int column)
        {
            onesInARow = 0;
            twosInARow = 0;
            int min = Math.Min(height - row, column + 1);
            for (int i = 0; i < min; i++)
            {
                CheckOnesAndTwos(Values[row + i, column - i]);
            }
        }
        private void CheckOnesAndTwos(int value)
        {
            switch (value)
            {
                case 0:
                    onesInARow = 0;
                    twosInARow = 0;
                    break;
                case 1:
                    onesInARow++;
                    twosInARow = 0;
                    if (onesInARow > onesInARowMax) onesInARowMax = onesInARow;
                    break;
                case 2:
                    twosInARow++;
                    onesInARow = 0;
                    if (twosInARow > twosInARowMax) twosInARowMax = twosInARow;
                    break;
            }
        }
            //private int CheckWinner()
            //{
            //    if (ones >= winLength) return 1;
            //    if (twos >= winLength) return 2;
            //    return 0;
            //}
    }
}
