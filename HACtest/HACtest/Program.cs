using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace HACtest
{
    class Program
    {
        static StopWordFilter stopWordFilter = new StopWordFilter();
        static EnglishStemmer englishStemmer = new EnglishStemmer();
        static EverGrowingDictionary dictionary = new EverGrowingDictionary();
        static System.Text.Encoding enc = System.Text.Encoding.ASCII;
        private const int nWordSize = 256;
        static byte[] word = new byte[nWordSize];

        static byte[] charFilter = {
	        0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, //spaces  8
	        0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, //spaces  16
	        0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, //spaces  24
	        0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, //spaces  32
	        0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, //spaces  40
	        0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, //spaces  48
	        0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, //numbers  56
	        0x38, 0x39, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, //2 numbers and spaces  64
	        0x20, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, //upper case to lower case 72
	        0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F, //upper case to lower case 80
	        0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, //upper case to lower case 88
	        0x78, 0x79, 0x7A, 0x20, 0x20, 0x20, 0x20, 0x20, //96
	        0x20, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, //this must be lower case 104
	        0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F, //lower case 112
	        0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, //120
	        0x78, 0x79, 0x7A, 0x20, 0x20, 0x20, 0x20, 0x20  //128
        };

        private static byte[] GetFileData(string fileName)
        {
            FileStream fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            int length = (int)fStream.Length;
            byte[] data = new byte[length];
            int count;
            int sum = 0;
            while ((count = fStream.Read(data, sum, length - sum)) > 0)
            {
                sum += count;
            }
            fStream.Close();
            return data;
        }

        private static void enumerateFiles(List<string> files, string folder, string extension)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folder);
            FileInfo[] fiArray = dirInfo.GetFiles(extension, SearchOption.AllDirectories);
            foreach (FileInfo fi in fiArray)
            {
                files.Add(fi.FullName);
            }
        }

        private static void processFiles(ref List<string> files)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dataFolder = Path.Combine(path, "data");
            Directory.CreateDirectory(dataFolder);
            string fileList = Path.Combine(dataFolder, "ProcessedFilesList.txt");
            string docWordMatrix = Path.Combine(dataFolder, "DocWordMatrix.dat");
            if (File.Exists(fileList)) File.Delete(fileList);
            if (File.Exists(docWordMatrix)) File.Delete(docWordMatrix);

            StreamWriter fileListStream = new StreamWriter(fileList);
            BinaryWriter docWordStream = new BinaryWriter(File.Open(docWordMatrix, FileMode.Create));

            ArrayList wordCounter = new ArrayList();
            int nFileCounter = 0;
            foreach (string file in files)
            {
                wordCounter.Clear();
                int nWordsSoFar = dictionary.GetNumberOfWords();
                wordCounter.Capacity = nWordsSoFar;
                for (int i = 0; i < nWordsSoFar; ++i)
                {
                    wordCounter.Add(0);
                }

                byte[] data = GetFileData(file);
                int counter = 0;
                for (int i = 0; i < data.Length; ++i)
                {
                    byte b = data[i];
                    if (b < 128)
                    {
                        b = charFilter[b];
                        if (b != 0x20 && counter < nWordSize)
                        {
                            word[counter] = b;
                            ++counter;
                        }
                        else
                        {
                            if (counter > 0)
                            {
                                if (!stopWordFilter.isThere(word, counter))
                                {
                                    string strWord = enc.GetString(word, 0, counter);
                                   /* englishStemmer.SetCurrent(strWord);
                                    if (englishStemmer.Stem())
                                    {
                                        strWord = englishStemmer.GetCurrent();
                                    }*/
                                    int nWordIndex = dictionary.GetWordIndex(strWord);

                                    //we check errors
                                    if (nWordIndex < 0)
                                    {
                                        if (nWordIndex == -1)
                                        {
                                            Console.WriteLine("Erorr: word = NULL");
                                            Environment.Exit(1);
                                        }
                                        if (nWordIndex == -2)
                                        {
                                            Console.WriteLine("Error: word length > 255");
                                            Environment.Exit(1);
                                        }
                                        if (nWordIndex == -3)
                                        {
                                            Console.WriteLine("Error: uknown");
                                            Environment.Exit(1);
                                        }
                                        if (nWordIndex == -4)
                                        {
                                            Console.WriteLine("Error: memory buffer for dictionary is too short");
                                            Environment.Exit(1);
                                        }
                                        if (nWordIndex == -5)
                                        {
                                            Console.WriteLine("Error: word length = 0");
                                            Environment.Exit(1);
                                        }
                                    }

                                    if (nWordIndex < nWordsSoFar)
                                    {
                                        int element = (int)wordCounter[nWordIndex];
                                        wordCounter[nWordIndex] = element + 1;
                                    }
                                    else
                                    {
                                        wordCounter.Add(1);
                                        ++nWordsSoFar;
                                    }
                                }
                                counter = 0;
                            } //word processed
                        }
                    }
                }//file processed
                Console.WriteLine("File: " + file + ", words: " + dictionary.GetNumberOfWords() + ", size: " + dictionary.GetDictionarySize());
                fileListStream.WriteLine(nFileCounter.ToString() + " " + file);

                int pos = 0;
                foreach (int x in wordCounter)
                {
                    if (x > 0)
                    {
                        docWordStream.Write(nFileCounter);
                        docWordStream.Write(pos);
                        //short value = (short)(x);
                        //int value = (int)(x);
                        docWordStream.Write(x);
                    }
                    ++pos;
                }

                ++nFileCounter;
            }//end foreach block, all files are processed
            fileListStream.Flush();
            fileListStream.Close();
            docWordStream.Flush();
            docWordStream.Close();
        }

        static void Main(string[] args)
        {
            DateTime start = DateTime.Now;

            string rootFolder = "input";//@"..//..//..//..//PATENTCORPUS128";
            string extension = "*";

            DirectoryInfo di = new DirectoryInfo(rootFolder);
            if (!di.Exists)
            {
                Console.WriteLine("The data folder not found, please correct the path");
                return;
            }
            //
            List<string> files = new List<string>();
            files.Clear();
            enumerateFiles(files, rootFolder, extension);
            processFiles(ref files);
            //
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "data");
            string docWordMatrix = Path.Combine(path, "DocWordMatrix.dat");
            HAC hac = new HAC();
            if (!hac.ProcessDataFile(docWordMatrix, 8))
            {
                Console.WriteLine("Failed to process file {0}", docWordMatrix);
            }
            //
            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            double time = duration.Minutes * 60.0 + duration.Seconds + duration.Milliseconds / 1000.0;
            Console.WriteLine("Total processing time {0:########.00} seconds", time);
        }
    }
}
