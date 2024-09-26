using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;

namespace ContentAwareResize
{
  // *****************************************
  // DON'T CHANGE CLASS OR FUNCTION NAME
  // YOU CAN ADD FUNCTIONS IF YOU NEED TO
  // *****************************************
  public class ContentAwareResize
  {
    public struct coord
    {
      public int row;
      public int column;
    }
    //========================================================================================================
    // Your Code is Here:
    //===================
    /// <summary>
    /// Develop an efficient algorithm to get the minimum vertical seam to be removed
    /// </summary>
    /// <param name="energyMatrix">2D matrix filled with the calculated energy for each pixel in the image</param>
    /// <param name="Width">Image's width</param>
    /// <param name="Height">Image's height</param>
    /// <param name="minSeamValue"></param>
    /// <param name="seamPathCoord"></param>
    /// <returns>BY REFERENCE: The min total value (energy) of the selected seam in "minSeamValue" & List of points of the selected min vertical seam in seamPathCoord</returns>
    public void CalculateSeamsCost(int[,] energyMatrix, int Width, int Height, ref int minSeamValue, ref List<coord> seamPathCoord)
    {
         //throw new NotImplementedException(); // comment this line 
         // Write your code here 
         int[,] dp = new int [Height, Width];
         for (int i = 0; i < Height; i++)
         {
           for (int j = 0; j < Width; j++)
           {
             if (i == 0)
             {
               dp[i, j] = energyMatrix[i, j];
             }
             else
             {
               int mid = dp[i - 1, j];
               if (j > 0 && j < Width - 2)
               {
                 int right = dp[i - 1, j + 1];;
                 int left = dp[i - 1, j - 1];
                 dp[i, j] = energyMatrix[i, j] + Math.Min(mid, Math.Min(left, right));
               }else if (j < Width - 2)
               {
                 int right = dp[i - 1, j + 1];
                 dp[i, j] = energyMatrix[i, j] + Math.Min(mid,right);
               }
               else 
               {
                 int left = dp[i - 1, j - 1];
                 dp[i, j] = energyMatrix[i, j] + Math.Min(mid, left);
               }
             }
           }
         }

         minSeamValue = dp[Height - 1, 0];
         int index = 0;
         for (int j = 1; j < Width; j++)
         {
           if (dp[Height - 1, j] < minSeamValue)
           {
             minSeamValue = dp[Height - 1, j];
             index = j;
           }
         }

         seamPathCoord = new List<coord>();
         seamPathCoord.Add(new coord{row = Height-1,column=index});
         for (int i = Height - 1; i > 0; i--)
         {
           int j = seamPathCoord.Last().column;
           int mid = dp[i - 1, j];
           if (j > 0 && j < Width - 1)
           {
             int left = dp[i - 1, j - 1];
             int right = dp[i - 1, j + 1];
             if (left < mid && left < right)
             {
               seamPathCoord.Add(new coord { row = i - 1, column = j - 1 });
             }
             else if (right < mid && right < left)
             {
               seamPathCoord.Add(new coord{ row = i - 1, column = j + 1 });
             }
             else
             {
               seamPathCoord.Add(new coord{ row = i - 1, column = j});
             }
           }
           else if (j < Width - 1)
           {
             int right = dp[i - 1, j + 1];
             if (right < mid)
             {
               seamPathCoord.Add(new coord{ row = i - 1, column = j + 1 });
             }
             else
             {
               seamPathCoord.Add( new coord{ row = i - 1, column = j });
             }
           }
           else
           {
             int left = dp[i - 1, j - 1];
             if (left < mid)
             {
               seamPathCoord.Add(new coord { row = i - 1, column = j - 1 });
             }
             else
             {
               coord value = new coord{ row = i - 1, column = j };
               seamPathCoord.Add(value);
             }
           }
         }

         seamPathCoord.Reverse();
    }

    // *****************************************
    // DON'T CHANGE CLASS OR FUNCTION NAME
    // YOU CAN ADD FUNCTIONS IF YOU NEED TO 
    // *****************************************
    #region DON'TCHANGETHISCODE
    public MyColor[,] _imageMatrix;
    public int[,] _energyMatrix;
    public int[,] _verIndexMap;
    public ContentAwareResize(string ImagePath)
    {
      _imageMatrix = ImageOperations.OpenImage(ImagePath);
      _energyMatrix = ImageOperations.CalculateEnergy(_imageMatrix);
      int _height = _energyMatrix.GetLength(0);
      int _width = _energyMatrix.GetLength(1);
    }
    public void CalculateVerIndexMap(int NumberOfSeams, ref int minSeamValueFinal, ref List<coord> seamPathCoord)
    {
      int Width = _imageMatrix.GetLength(1);
      int Height = _imageMatrix.GetLength(0);

      int minSeamValue = -1;
      _verIndexMap = new int[Height, Width];
      for (int i = 0; i < Height; i++)
        for (int j = 0; j < Width; j++)
          _verIndexMap[i, j] = int.MaxValue;

      bool[] RemovedSeams = new bool[Width];
      for (int j = 0; j < Width; j++)
        RemovedSeams[j] = false;

      for (int s = 1; s <= NumberOfSeams; s++)
      {
        CalculateSeamsCost(_energyMatrix, Width, Height, ref minSeamValue, ref seamPathCoord);
        minSeamValueFinal = minSeamValue;

        //Search for Min Seam # s
        int Min = minSeamValue;

        //Mark all pixels of the current min Seam in the VerIndexMap
        if (seamPathCoord.Count != Height)
          throw new Exception("You selected WRONG SEAM");
        for (int i = Height - 1; i >= 0; i--)
        {
          if (_verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column] != int.MaxValue)
          {
            string msg = "overalpped seams between seam # " + s + " and seam # " + _verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column];
            throw new Exception(msg);
          }
          _verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column] = s;
          //remove this seam from energy matrix by setting it to max value
          _energyMatrix[seamPathCoord[i].row, seamPathCoord[i].column] = 100000;
        }

        //re-calculate Seams Cost in the next iteration again
      }
    }
    public void RemoveColumns(int NumberOfCols)
    {
      int Width = _imageMatrix.GetLength(1);
      int Height = _imageMatrix.GetLength(0);
      _energyMatrix = ImageOperations.CalculateEnergy(_imageMatrix);

      int minSeamValue = 0;
      List<coord> seamPathCoord = null;
      //CalculateSeamsCost(_energyMatrix,Width,Height,ref minSeamValue, ref seamPathCoord);
      CalculateVerIndexMap(NumberOfCols, ref minSeamValue, ref seamPathCoord);

      MyColor[,] OldImage = _imageMatrix;
      _imageMatrix = new MyColor[Height, Width - NumberOfCols];
      for (int i = 0; i < Height; i++)
      {
        int cnt = 0;
        for (int j = 0; j < Width; j++)
        {
          if (_verIndexMap[i, j] == int.MaxValue)
            _imageMatrix[i, cnt++] = OldImage[i, j];
        }
      }

    }
    #endregion
  }
}
