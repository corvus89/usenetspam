//Andrew Polar, semanticsearchart.com, Mar. 20, 2012.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace Cluster
{
    class Row
    {
        public int nClusterIndex = -1;
        public List<int> m_files = new List<int>(0);
        public List<int> m_data = new List<int>(0);
        public List<int> m_pos = new List<int>(0);
        public double norm = 0.0;
    }

    class HACBuilder
    {
        public void ProcessDataFile(string fileName)
        {

            List<Row> m_rows = new List<Row>();
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
            //Console.WriteLine("m_rows: {0}", m_rows.Count);
        }

        public List<Row> readData(string datafile, int[] files, int[] tabooList)
        {
            //ProcessDataFile(datafile);
            List<Row> rows = new List<Row>();
            FileInfo fi = new FileInfo(datafile);
            long nBytes = fi.Length;
            int nNonZeros = (int)(nBytes / 12);
            int nRold = -1;
            int current_row = -1;
            bool isStarted = false;
            using (FileStream stream = new FileStream(datafile, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    for (long i = 0; i < nNonZeros; ++i)
                    {
                        int nR = reader.ReadInt32();
                        int nC = reader.ReadInt32();
                        int sum = reader.ReadInt32();
                        if (nR != nRold)
                        {
                            bool isFound = false;
                            int index = 0;
                            foreach (int k in files)
                            {
                                if (k == nR)
                                {
                                    isFound = true;
                                    break;
                                }
                                ++index;
                            }
                            if (isFound)
                            {
                                rows.Add(new Row());
                                ++current_row;
                                rows[current_row].m_files.Add(nR);
                                if (rows[current_row].nClusterIndex < 0)
                                {
                                    rows[current_row].nClusterIndex = tabooList[index];
                                }
                                rows[current_row].m_pos.Add(nC);
                                rows[current_row].m_data.Add(sum);
                                isStarted = true;
                            }
                            else
                            {
                                isStarted = false;
                            }
                            nRold = nR;
                        }
                        else
                        {
                            if (isStarted)
                            {
                                rows[current_row].m_pos.Add(nC);
                                rows[current_row].m_data.Add(sum);
                            }
                        }
                        //if (current_row == 0) Console.WriteLine("rows[{0}] m_files:{1}  m_pos:{2} m_data:{3} nClusterIndex:{4}", current_row, rows[current_row].m_files[0], rows[current_row].m_pos[0], rows[current_row].m_data[0], rows[current_row].nClusterIndex);
                    }
                    reader.Close();
                }
                stream.Close();
            }
            //Console.WriteLine("rows[{0}] m_files:{1}  m_pos:{2} m_data:{3} nClusterIndex:{4}", current_row, rows[current_row].m_files[0], rows[current_row].m_pos[0], rows[current_row].m_data[0], rows[current_row].nClusterIndex);
                    
            //compute norms
            foreach (Row row in rows)
            {
                row.norm = 0.0;
                foreach (int s in row.m_data)
                {
                    row.norm += s * s;
                }
                row.norm = Math.Sqrt(row.norm);
            }
            //end
            //Console.WriteLine("rows: {0}", rows.Count);
            return rows;
        }

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

        private bool checkTabooList(List<Row> rows, int clusterI, int clusterJ)
        {
            if (rows[clusterI].nClusterIndex < 0) return true;
            if (rows[clusterJ].nClusterIndex < 0) return true;
            if (rows[clusterI].nClusterIndex == rows[clusterJ].nClusterIndex) return true;
            return false;
        }

        private void getMaxElement(List<Row> rows, ref int nRow, ref int nCol, ref float fMax, float[,] fMatrix, int nSize)
        {
            nRow = 0;
            nCol = 0;
            fMax = fMatrix[nRow, nCol];
            for (int i = 0; i < nSize; ++i)
            {
                for (int j = i + 1; j < nSize; ++j)
                {
                    if (fMax < fMatrix[i, j] && checkTabooList(rows, i, j) == true)
                    {
                        fMax = fMatrix[i, j];
                        nRow = i;
                        nCol = j;
                    }
                }
            }
            if (nRow==0 && nCol==0)
            {
                for (int i = 0; i < nSize; ++i)
                {
                    for (int j = i + 1; j < nSize; ++j)
                    {
                        if ( checkTabooList(rows, i, j) == true)
                        {
                            fMax = fMatrix[i, j];
                            nRow = i;
                            nCol = j;
                        }
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

            if (r1.nClusterIndex >= 0 && r2.nClusterIndex >= 0)
            {
                if (r1.nClusterIndex != r2.nClusterIndex)
                {
                    Console.WriteLine("Error: merging two knowingly different clusters {0} {1}", r1.nClusterIndex, r2.nClusterIndex);
                    Environment.Exit(0);
                }
            }

            if (r1.nClusterIndex >= 0) merged.nClusterIndex = r1.nClusterIndex;
            if (r2.nClusterIndex >= 0) merged.nClusterIndex = r2.nClusterIndex;

            return merged;
        }

        public List<Row> executeHAC(List<Row> rows, int nCategories, int[] nExpected)
        {
            if (nCategories >= rows.Count)
            {
                Console.WriteLine("Number of requested categories is larger data size");
                return rows;
            }
            Console.WriteLine("Start HAC procedure...\n");
            int NN = rows.Count - nCategories;
            int nMatrixSize = rows.Count;
            float[,] fMatrix = new float[nMatrixSize, nMatrixSize];
            for (int i = 0; i < nMatrixSize; ++i)
            {
                for (int j = i + 1; j < nMatrixSize; ++j)
                {
                    fMatrix[i, j] = getCosine(rows[i], rows[j]);
                }
                
            }
            //Console.Write("\nrows {0} ", rows.Count);
            //Console.Write("\nnCategories {0} \n", nCategories);

            /*int cn1t = 0;
            foreach (Row row in rows)
            {
                Console.Write("\nCategory : {0}  ", cn1t);
                Console.Write(" row : {0} files ", row.nClusterIndex);
                foreach (int n in row.m_files)
                {
                    Console.Write("{0,5}", n);
                }
                Console.Write(" data: ");
                foreach (int n in row.m_data)
                {
                    Console.Write("{0,5}", n);
                }
                Console.Write(" pos: ");
                foreach (int n in row.m_pos)
                {
                    Console.Write("{0,5}", n);
                }
                ++cn1t;
            }*/
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"data\Debug.txt"))
            //{
                for (int loop = 0; loop <= NN; ++loop)
                {

                    nMatrixSize = rows.Count;
                    int nrow = 0;
                    int ncol = 0;
                    float fMax = 0.0f;
                    getMaxElement(rows, ref nrow, ref ncol, ref fMax, fMatrix, nMatrixSize);

                    Row r1 = rows[nrow];
                    Row r2 = rows[ncol];
                    Row merged = mergeTwoRows(r1, r2);
                    rows.Remove(r1);
                    rows.Remove(r2);
                    //file.WriteLine("{0}[{1}], ", n, nExpected[n]);
                    //file.Write("[ {0} {1} ], ", nrow, ncol);
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

                    rows.Add(merged);

                    //recompute new matrix elements
                    curRow = 0;
                    for (int i = 0; i < nMatrixSize; ++i)
                    {
                        if (i != nrow && i != ncol)
                        {
                            fMatrix[curRow, nMatrixSize - 2] = getCosine(rows[curRow], rows[nMatrixSize - 2]);
                            ++curRow;
                        }
                    }

                    Console.Write("Step {0} \r", loop);
                }
            //}

            Console.WriteLine();
            Console.WriteLine("\nList of categories\n");
            int cnt = 0;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"data\Categories.txt"))
            {
                foreach (Row row in rows)
                {
                    file.WriteLine("Category {0}\n", cnt);
                    Console.WriteLine("Category {0}\n", cnt);
                    foreach (int n in row.m_files)
                    {
                        file.WriteLine("{0}[{1}], ", n, nExpected[n]);
                        Console.Write("{0}, ", n);
                    }
                    file.WriteLine("\n");
                    Console.WriteLine("\n");
                    ++cnt;
                }
            }

            return rows;
        }

        public double computeF_measure(List<Row> rows, int[] nExpected, int[] nFiles)
        {
            //Console.WriteLine("test1");
            Dictionary<int, int> dict = new Dictionary<int, int>();
            int cnt = 0;
            int nCategories = 0;
            int nMax = nExpected[0];
            for (int i = 0; i < nExpected.Length; ++i)
            {
                if (!dict.ContainsKey(nExpected[i]))
                {
                    dict.Add(nExpected[i], cnt);
                    ++cnt;
                    ++nCategories;
                }
                if (nExpected[i] > nMax) nMax = nExpected[i];
            }
            int[] chart = new int[nMax + 1];
            //Console.WriteLine("test2");
            foreach (KeyValuePair<int, int> pair in dict)
            {
                chart[pair.Key] = pair.Value;
            }
            //Console.WriteLine("test3");
            for (int i = 0; i < nExpected.Length; ++i)
            {
                nExpected[i] = chart[nExpected[i]];
            }

            int[,] nAccuracy = new int[nCategories, nCategories];
            int nC = 0;
            //Console.WriteLine("test4");
            //Console.WriteLine("rows.Count : {0}", rows.Count);
            //Console.WriteLine("test4B");
            foreach (Row row in rows)
            {
                //Console.Write("nC : {0}, ", nC);
                //Console.WriteLine("{0}", nC);
                for (int i = 0; i < nCategories; ++i)
                {
                    nAccuracy[nC, i] = 0;
                }
                //Console.WriteLine("test4B");
                foreach (int nFile in row.m_files)
                {
                    int index = 0;
                    foreach (int n in nFiles)
                    {
                        if (n == nFile) break;
                        ++index;
                    }
                    //Console.WriteLine("test4B");
                    //Console.WriteLine("{0}", index);
                    ++nAccuracy[nC, nExpected[index]];
                }
                ++nC;
            }
            //Console.WriteLine("test5");
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"data\MatrixAccu.txt"))
            {
                for (int i = 0; i < nCategories; ++i)
                {
                    for (int j = 0; j < nCategories; ++j)
                    {
                        file.Write(" {0,4} ", nAccuracy[i, j]);
                        Console.Write(" {0,4} ", nAccuracy[i, j]);
                    }
                    file.WriteLine();
                    Console.WriteLine();
                }
                file.WriteLine();
                Console.WriteLine();
            }

            int[] sumInRows = new int[nCategories];
            int[] sumInCols = new int[nCategories];
            //Console.WriteLine("test6");
            for (int i = 0; i < nCategories; ++i)
            {
                sumInRows[i] = 0;
                sumInCols[i] = 0;
                for (int j = 0; j < nCategories; ++j)
                {
                    sumInRows[i] += nAccuracy[i, j];
                    sumInCols[i] += nAccuracy[j, i];
                }
            }
            //Console.WriteLine("test7");
            double[,] f_measure = new double[nCategories, nCategories];
            for (int i = 0; i < nCategories; ++i)
            {
                for (int j = 0; j < nCategories; ++j)
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
            //Console.WriteLine("test8");
            double total = 0.0;
            for (int loop = 0; loop < nCategories; ++loop)
            {
                int RR = 0;
                int CC = 0;
                double max = f_measure[RR, CC];
                for (int i = 0; i < nCategories; ++i)
                {
                    for (int j = 0; j < nCategories; ++j)
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
                for (int i = 0; i < nCategories; ++i)
                {
                    f_measure[RR, i] = 0.0;
                }
                for (int j = 0; j < nCategories; ++j)
                {
                    f_measure[j, CC] = 0.0;
                }
            }
            return total / (double)(nCategories);
        }
    }

    class Program
    {
        private static void initTables(string datafile)
        {

            ArrayList list_nExpected = new ArrayList();
            int counter = 0;
            int big_size = 0;
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(datafile);
            while ((line = file.ReadLine()) != null)
            {
                //string sub = line.Substring(0, line.IndexOf(" "));
                int disc_size = Convert.ToInt32(line.Substring(0, line.IndexOf(" ")));
                int disc_id = counter;
                big_size += disc_size;
                for (int i = 0; i < disc_size; i++) {
                    list_nExpected.Add(disc_id);
                }
                    //Console.WriteLine(line);
                    counter++;
            }

            file.Close();

        }

        static void Main(string[] args)
        {
            DateTime start = DateTime.Now;

            string datafile = @"data\DocWordMatrix.dat";
            string dataIndex = @"data\indeksgrup.txt";


            ArrayList list_nExpected = new ArrayList();
            int counter = -1;
            int big_size = 0;
            string line;
            //Console.WriteLine("big_size {0}", big_size);
            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(dataIndex);
            while ((line = file.ReadLine()) != null)
            {
                counter++;
                int disc_size = Convert.ToInt32(line.Substring(0, line.IndexOf(" ")));
                //Console.WriteLine("disc_size {0}", disc_size);
                int disc_id = counter;
                //Console.WriteLine("disc_id {0}", disc_id);
                big_size += disc_size;
                for (int i = 0; i < disc_size; i++)
                {
                    list_nExpected.Add(disc_id);
                }      
            }
            //Console.WriteLine("big_size {0}", big_size);
            file.Close();

            int nCategories = counter+1;// 3;//3
            Console.WriteLine("Categories : {0}", nCategories);
            int[] nFiles = new int[big_size];
            for (int i = 0; i < big_size; i++) {
                nFiles[i] = i;
            }
            Console.WriteLine("Podaj wielkość zbioru treningowego % ");

            int testset = 10;
            String readline = "";
            readline = Console.ReadLine();
            Console.WriteLine("{0}", readline);
            testset = Convert.ToInt32(readline);
            Console.WriteLine("{0}", testset);
            testset = (100 / testset);

            Console.WriteLine("{0}", testset);

            int[] nTabooList = new int[big_size];
            for (int i = 0; i < big_size; i++)
            {
                if (i % testset == 1)
                {
                    nTabooList[i] = Convert.ToInt32(list_nExpected[i]);
                }
                else
                {
                    nTabooList[i] = -1;
                }
            }

            int[] nExpected = new int[big_size];
            for (int i = 0; i < big_size; i++)
            {
                nExpected[i] = Convert.ToInt32(list_nExpected[i]);
            }
            //Console.WriteLine("nFiles[big_size-1] {0}", nFiles[big_size - 1]);
            //Console.WriteLine("nTabooList[big_size-1] {0}", nTabooList[big_size - 1]);
            //Console.WriteLine("nExpected[big_size-1] {0}", nExpected[big_size-1]);
            

          /*
            int nCategories = 3;
            int[] nFiles = {
                0, 72, 2, 3, 6, 7, 8, 9, 11, 10, 12, 13, 14, 15,
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
                64, 1, 66, 79, 68, 69, 70, 71, 65, 73, 74, 75, 76, 77, 78, 67};
            int[] nTabooList = {
                -1, -1,  9, 9, -1, -1, 9, -1, -1, -1, -1, -1, -1, -1,
                 3, -1, -1, 3,  3, -1, -1, 3, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, 5, -1, -1, -1, -1, 5, -1, -1, -1, -1, -1, -1, -1};
            int[] nExpected = {
                0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                4, 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4};*/
           /* int nCategories = 3;
            int[] nFiles = {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
                20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
                40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59};//,
                //60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
                //80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99};
            int[] nTabooList = {
                -1, -1, -1, -1, 1, 1, 1, 1, -1, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, 2, 2, 2, 2, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3, 3, 3, 3, 3, -1, -1, -1};//, 
                //-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                //-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1};
            int[] nExpected = {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2};//, 
                //3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 
                //4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4};
            */

            HACBuilder hacBuilder = new HACBuilder();
            List<Row> rows = hacBuilder.readData(datafile, nFiles, nTabooList);
            rows = hacBuilder.executeHAC(rows, nCategories, nExpected);
            double acc = hacBuilder.computeF_measure(rows, nExpected, nFiles);
            Console.WriteLine("Accuracy {0}", acc);
            using (System.IO.StreamWriter raport = new System.IO.StreamWriter(@"data\Raport.txt"))
            {
                raport.WriteLine("Accuracy {0}", acc);
                raport.WriteLine(" FileId | Lerning | Expected");
                for (int i = 0; i < big_size; i++)
                {
                    raport.WriteLine("{0,7} | {1,7} | {2,7}", nFiles[i], nTabooList[i], nExpected[i]);
                }
            
            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            double time = duration.Minutes * 60.0 + duration.Seconds + duration.Milliseconds / 1000.0;
            Console.WriteLine("Total processing time {0:########.00} seconds", time);
            raport.WriteLine("Total processing time {0:########.00} seconds", time);
            }
        }
    }
}