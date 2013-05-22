using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace HACtest
{
    class HAC
    {
        class Row
        {
            public List<int> m_files = new List<int>(0);
            public List<int> m_data = new List<int>(0);
            public List<int> m_pos = new List<int>(0);
            public double norm = 0.0;
        }

        List<Row> m_rows = new List<Row>(0);

        private float getCosine(Row r1, Row r2)
        {
            double product = 0.0f;
            int index1 = 0;
            int index2 = 0;
            while (true)
            {
                if (r1.m_pos[index1] == r2.m_pos[index2])
                {
                    product += r1.m_data[index1] * r2.m_data[index2];
                    ++index1;
                    ++index2;
                }
                else if (r1.m_pos[index1] > r2.m_pos[index2])
                {
                    ++index2;
                }
                else if (r1.m_pos[index1] < r2.m_pos[index2])
                {
                    ++index1;
                }
                if (index1 >= r1.m_pos.Count) break;
                if (index2 >= r2.m_pos.Count) break;
            }
            product /= r1.norm;
            product /= r2.norm;
            double min = r1.m_files.Count;
            if (min > r2.m_files.Count) min = r2.m_files.Count;
            product /= min;
            return (float)(product);
        }

        private void getMaxElement(ref int nRow, ref int nCol, ref float fMax, float[,] fMatrix, int nSize)
        {
            nRow = 0;
            nCol = 0;
            fMax = fMatrix[nRow, nCol];
            for (int i = 0; i < nSize; ++i)
            {
                for (int j = i + 1; j < nSize; ++j)
                {
                    if (fMax < fMatrix[i, j])
                    {
                        fMax = fMatrix[i, j];
                        nRow = i;
                        nCol = j;
                    }
                }
            }
        }

        private Row mergeTwoRows(Row r1, Row r2)
        {
            Row merged = new Row();

            for (int i = 0; i < r1.m_files.Count; ++i)
            {
                merged.m_files.Add(r1.m_files[i]);
            }
            for (int i = 0; i < r2.m_files.Count; ++i)
            {
                merged.m_files.Add(r2.m_files[i]);
            }

            int index1 = 0;
            int index2 = 0;
            while (true)
            {
                if (r1.m_pos[index1] == r2.m_pos[index2])
                {
                    merged.m_pos.Add(r1.m_pos[index1]);
                    int sum = r1.m_data[index1];
                    sum += r2.m_data[index2];
                    merged.m_data.Add(sum); 
                    ++index1;
                    ++index2;
                }
                else if (r1.m_pos[index1] > r2.m_pos[index2])
                {
                    merged.m_pos.Add(r2.m_pos[index2]);
                    merged.m_data.Add(r2.m_data[index2]);
                    ++index2;
                }
                else if (r1.m_pos[index1] < r2.m_pos[index2])
                {
                    merged.m_pos.Add(r1.m_pos[index1]);
                    merged.m_data.Add(r1.m_data[index1]);
                    ++index1;
                }
                if (index1 >= r1.m_pos.Count) break;
                if (index2 >= r2.m_pos.Count) break;
            }

            for (int i = index1; i < r1.m_pos.Count; ++i)
            {
                merged.m_pos.Add(r1.m_pos[i]);
                merged.m_data.Add(r1.m_data[i]);
            }

            for (int i = index2; i < r2.m_pos.Count; ++i)
            {
                merged.m_pos.Add(r2.m_pos[i]);
                merged.m_data.Add(r2.m_data[i]);
            }

            merged.norm = 0.0;
            foreach (short s in merged.m_data)
            {
                merged.norm += s * s;
            }
            merged.norm = Math.Sqrt(merged.norm);

            return merged;
        }

        //this function works only when number of files in
        //each category is equal and if in the list of processed
        //files categories follow each other sequentially
        public double computeF_measure(int nCat, int nFiles)
        {
            int[,] nAccuracy = new int[nCat, nCat];
            int cnt = 1;
            foreach (Row row in m_rows)
            {
                for (int k = 0; k < nCat; ++k)
                {
                    nAccuracy[cnt - 1, k] = 0;
                }
                foreach (int n in row.m_files)
                {
                    int nLower = 0;
                    int nUpper = nFiles;
                    for (int k = 0; k < nCat; ++k)
                    {
                        if (n < nUpper && n >= nLower) ++nAccuracy[cnt - 1, k];
                        nLower += nFiles;
                        nUpper += nFiles;
                    }
                }
                ++cnt;
            }

            //for (int i = 0; i < nCat; ++i)
            //{
            //    for (int j = 0; j < nCat; ++j)
            //    {
            //        Console.Write(" {0,4} ", nAccuracy[i, j]);
            //    }
            //    Console.WriteLine();
            //}
            //Console.WriteLine();

            int[] sumInRows = new int[nCat];
            int[] sumInCols = new int[nCat];
            for (int i = 0; i < nCat; ++i)
            {
                sumInRows[i] = 0;
                sumInCols[i] = 0;
                for (int j = 0; j < nCat; ++j)
                {
                    sumInRows[i] += nAccuracy[i, j];
                    sumInCols[i] += nAccuracy[j, i];
                }
            }

            double[,] f_measure = new double[nCat, nCat];
            for (int i = 0; i < nCat; ++i)
            {
                for (int j = 0; j < nCat; ++j)
                {
                    if (nAccuracy[i, j] > 0)
                    {
                        double f = (double)nAccuracy[i, j];
                        double prec = f / (double)(sumInRows[i]);
                        double recall = f / (double)(sumInCols[j]);

                        f_measure[i, j] = 2.0 * prec * recall / (prec + recall);
                    }
                    else
                    {
                        f_measure[i, j] = 0.0;
                    }
                }
            }

            double total = 0.0;
            for (int loop = 0; loop < nCat; ++loop)
            {
                int RR = 0;
                int CC = 0;
                double max = f_measure[RR, CC];
                for (int i = 0; i < nCat; ++i)
                {
                    for (int j = 0; j < nCat; ++j)
                    {
                        if (f_measure[i, j] > max)
                        {
                            max = f_measure[i, j];
                            RR = i;
                            CC = j;
                        }
                    }
                }
                total += max;
                for (int i = 0; i < nCat; ++i)
                {
                    f_measure[RR, i] = 0.0;
                }
                for (int j = 0; j < nCat; ++j)
                {
                    f_measure[j, CC] = 0.0;
                }
            }
            return total / (double)(nCat);
        }

        public bool ProcessDataFile(string fileName, int nCategories)
        {
            if (nCategories < 2)
            {
                Console.WriteLine("Wrong number of categories");
                return false;
            }
            if (!File.Exists(fileName))
            {
                return false;
            }

            FileInfo fi = new FileInfo(fileName);
            long nBytes = fi.Length;
            int nNonZeros = (int)(nBytes / 12);
            ArrayList list = new ArrayList();
            list.Clear();

            //read data
            int nR = 0;
            int nC = 0;
            //short 
            int sum = 0;
            int current_row = 0;
            m_rows.Add(new Row());
            m_rows[current_row].m_files.Add(current_row);
            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    for (long i = 0; i < nNonZeros; ++i)
                    {
                        int nRnew = reader.ReadInt32();
                        nC = reader.ReadInt32();
                        sum = reader.ReadInt32(); //ReadInt16();
                        if (nRnew != nR)
                        {
                            m_rows.Add(new Row());
                            ++current_row;
                            m_rows[current_row].m_files.Add(current_row);
                        }
                        nR = nRnew;
                        m_rows[current_row].m_pos.Add(nC);
                        m_rows[current_row].m_data.Add(sum);
                    }
                    reader.Close();
                }
                stream.Close();
            }
            //end
            //Console.WriteLine("rows[{0}] m_files:{1}  m_pos:{2} m_data:{3} ", current_row, m_rows[current_row].m_files[0], m_rows[current_row].m_pos[0], m_rows[current_row].m_data[0]);
            //compute norms
            foreach (Row row in m_rows)
            {
                row.norm = 0.0;
                foreach (int s in row.m_data) //short
                {
                    row.norm += s * s;
                }
                row.norm = Math.Sqrt(row.norm);
            }
            //end

            Console.WriteLine("Start HAC procedure...\n");
            int NN = m_rows.Count - nCategories;
            int nMatrixSize = m_rows.Count;
            float[,] fMatrix = new float[nMatrixSize, nMatrixSize];
            for (int i = 0; i < nMatrixSize; ++i)
            {
                for (int j = i + 1; j < nMatrixSize; ++j)
                {
                    fMatrix[i, j] = getCosine(m_rows[i], m_rows[j]);
                }
            }

            for (int loop = 0; loop < NN; ++loop)
            {
                nMatrixSize = m_rows.Count;
                int nrow = 0;
                int ncol = 0;
                float fMax = 0.0f;
                getMaxElement(ref nrow, ref ncol, ref fMax, fMatrix, nMatrixSize);

                Row r1 = m_rows[nrow];
                Row r2 = m_rows[ncol];
                Row merged = mergeTwoRows(r1, r2);
                m_rows.Remove(r1);
                m_rows.Remove(r2);

                //reassign repeated matrix elements
                int curRow = 0;
                int curCol = 0;
                for (int i = 0; i < nMatrixSize; ++i)
                {
                    if (i != nrow && i != ncol)
                    {
                        curCol = curRow + 1;
                        for (int j = i + 1; j < nMatrixSize; ++j)
                        {
                            if (j != nrow && j != ncol)
                            {
                                fMatrix[curRow, curCol] = fMatrix[i, j];
                                ++curCol;
                            }
                        }
                        ++curRow;
                    }
                }

                m_rows.Add(merged);

                //recompute new matrix elements
                curRow = 0;
                for (int i = 0; i < nMatrixSize; ++i)
                {
                    if (i != nrow && i != ncol)
                    {
                        fMatrix[curRow, nMatrixSize - 2] = getCosine(m_rows[curRow], m_rows[nMatrixSize - 2]);
                        ++curRow;
                    }
                }

                Console.Write("Step {0} \r", loop);
            }

            Console.WriteLine();
            Console.WriteLine("\nList of categories\n");
            int cnt = 0;
            /*foreach (Row row in m_rows)
            {
                Console.WriteLine("Category {0}\n", cnt);
                foreach (int n in row.m_files)
                {
                    Console.Write("{0,4}", n);
                }
                Console.WriteLine("\n");
                ++cnt;
            }*/

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"data\CategoriesRandom.txt"))
            {
                foreach (Row row in m_rows)
                {
                    file.WriteLine("Category {0}\n", cnt);
                    Console.WriteLine("Category {0}\n", cnt);
                    foreach (int n in row.m_files)
                    {
                        file.WriteLine("{0}, ", n);
                        Console.Write("{0}, ", n);
                    }
                    file.WriteLine("\n");
                    Console.WriteLine("\n");
                    ++cnt;
                }
            }


            double f_measure = computeF_measure(8, 16);
            Console.WriteLine("Accuracy {0}", f_measure);
            using (System.IO.StreamWriter raport = new System.IO.StreamWriter(@"data\RaportRandom.txt"))
            {
                raport.WriteLine("Accuracy {0}", f_measure);
            }
            return true;
        }
    }
}
