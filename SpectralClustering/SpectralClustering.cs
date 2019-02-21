using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Accord.MachineLearning;
using Accord.Statistics;

namespace SpectralClustering
{
    public class SpectClustering
    {        
        private List<string> nodes;
        private List<List<string>> all_res;
        private double[][] observations;
        public Dictionary<string, int> words_cluster;
        public int[] labels;
        
        private int inputSize = 0;          // size of points in dataset       
        public double[,] adjacency_matrix;  // affinity matrix
        private double[,] D;                // diagonal matrix        
        private double[,] L;                // laplacian: L = D - adjacency_matrix.
        private double[,] X;                // k largest eigenvectors of L.
        private double[,] Y;                // normalized k largest eigenvectors.
        private int[] KResult;              // results from k-means algorithm.
        private int NumofClusters;

        private List<string> input_file;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Constructor
        /// </summary>
        ///
        public SpectClustering(List<string> input, int NumofCluster)
        {
            this.input_file = input;
            NumofClusters = NumofCluster;
                        
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public Tuple<double[,], Dictionary<string, int>> StartClustering()
        {            
            convert_input();                // 1.  build adjacency matrix.
            diagonalMatrix();               // 2.1 build diganoal matrix.
            buildLmat();                    // 2.2 build L = D - adjacency_matrix.
            calculate_eigendecomposition(); // 3.  find k largest eigenvectors.
            //findEigenvectors();             // 3.  find k largest eigenvectors.
            normalizeVectors();             // 4.  normalize eigenvectors.
            labels = new int[inputSize];

            //////ClusterCollection clusters;
            //////clusters = KMeans.ClusterDataSet(NumofClusters, Y);
            //////int index = -1;
            //////double[] temp = new double[NumofClusters];
            //////for (int i = 0; i < clusters.Count; i++)
            //////{
            //////    //System.Console.Out.Write(clusters.Count);  
            //////    // System.Console.Out.Write(i + ") ( ");
            //////    for (int j = 0; j < clusters[i].Count; j++)
            //////    {
            //////        // System.Console.Out.Write(clusters[i].Count );
            //////        for (int k = 0; k < clusters[i][j].Length; k++) // one point (full row)
            //////        {
            //////            temp[k] = clusters[i][j][k];



            //////        }
            //////        index = findCluster(conArrayD(Y), temp);
            //////        if (index != -1) // cluster and row were matched
            //////        {
            //////            labels[index] = i;
            //////            index = -1; // reset index.
            //////        }
            //////    }
            //////}            

            observations = new double[Y.GetLength(0)][];
            for (int i = 0; i < Y.GetLength(0); i++)
            {
                observations[i] = new double[Y.GetLength(1)];
                for (int j = 0; j < Y.GetLength(1); j++)
                    observations[i][j] = Y[i, j];
            }

            ////////////////observations = new double[X.GetLength(0)][];
            ////////////////for (int i = 0; i < X.GetLength(0); i++)
            ////////////////{
            ////////////////    observations[i] = new double[X.GetLength(1)];
            ////////////////    for (int j = 0; j < X.GetLength(1); j++)
            ////////////////        observations[i][j] = X[i, j];
            ////////////////}

            //K-Means
            Accord.MachineLearning.KMeans kmeans = new Accord.MachineLearning.KMeans(k: NumofClusters);
            var clusters = kmeans.Learn(observations);
            labels = clusters.Decide(observations);

            words_cluster = new Dictionary<string, int>();
            for (int i = 0; i < labels.Length; i++)
                words_cluster.Add(nodes[i], labels[i]);
            var tuple = new Tuple<double[,], Dictionary<string, int>>(adjacency_matrix, words_cluster);
            return tuple;
        }// end StartClustering

        private void diagonalMatrix()
        {

            for (int i = 0; i < inputSize; i++)
            {
                double sum_row = 0;
                for (int j = 0; j < inputSize; j++)
                    sum_row += adjacency_matrix[i, j];
                D[i, i] = sum_row;
            }// end outer for

         }// end diagonalMatrix()

        private void buildLmat() // ** changed
        {            
            for (int i = 0; i < inputSize; i++)
            {
                for (int j = 0; j < inputSize; j++)
                {
                    L[i, j] = D[i, j] - adjacency_matrix[i, j];
                }
            }
        }// end buildLmat()        
        
        /*
         *  4. Normalize eigenvectors.
         */
        private void normalizeVectors()
        {
            int a=X.GetLength(0);
            int b = X.GetLength(1);
            double[] sum_row = new double[X.GetLength(0)];
            for (int i = 0; i < (X.GetLength(0)); i++)
            {
                for (int j = 0; j < (X.GetLength(1)); j++)
                    sum_row[i] += X[i, j] * X[i, j];
                sum_row[i] = Math.Sqrt(sum_row[i]);
            }// end outer for
            Y = new double[inputSize, NumofClusters];
            for (int i = 0; i < X.GetLength(0); i++)
                for (int j = 0; j < X.GetLength(1); j++)
                    Y[i, j] = X[i, j] / sum_row[i];
            
        }// end normalizeVectors()        
                
        /*
         *  Extract one vector from BoW.
         *  input: BoW matrix, number of Vector to extract.
         *  output: Extracted vector.
         */        
        
        private void convert_input()
        {
            string[] lines = input_file.ToArray<string>();
            
            Dictionary<string, int> words = new Dictionary<string, int>();
            
            List<string> val = new List<string>();
            double[] vector = new double[lines.Length];

            int k = 1;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] line = lines[i].Split('\t');
                if (!words.ContainsKey(line[0]))
                    words.Add(line[0], k++);
                if (!words.ContainsKey(line[1]))
                    words.Add(line[1], k++);
                val.Add(line[2]);
                vector[i] = double.Parse(line[2]);
            }

            double[][] observ = new double[vector.Length][];
            for (int i = 0; i < vector.Length; i++)
                observ[i] = new double[] { vector[i] };

            Accord.MachineLearning.KMeans kmeans = new Accord.MachineLearning.KMeans(k:2);
            var clusters = kmeans.Learn(observ);
            int[] labels = clusters.Decide(observ);

            ////double min = 0, max = 0;
            ////Sigma(vector,ref min,ref max);
            nodes = new List<string>();// { "Id" };
            List<string> edges = new List<string>();// { "Source; Target" };
            HashSet<string> tmpnodes = new HashSet<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (labels[i] == 2 || labels[i] == 1)//if (vector[i] < min || vector[i] > max)
                    continue;
                string[] tmp = lines[i].Split('\t');
                //newlines.Add(words[tmp[0]].ToString() + "," + words[tmp[1]].ToString());// + "\t" + val[i]);
                edges.Add(tmp[0] + "\t" + tmp[1] + "\t" + val[i]);
                                                 //if (!nodes.Contains(tmp[0]))
                tmpnodes.Add(tmp[0]);
                //if (!nodes.Contains(tmp[1]))
                tmpnodes.Add(tmp[1]);
            }

            nodes.AddRange(tmpnodes.ToList());

            File.WriteAllLines("nodes.csv", nodes);
            File.WriteAllLines("edges.csv", edges);

            nodes = new List<string>();
            foreach (var item in words)
            {
                nodes.Add(item.Key);
            }

            File.WriteAllLines("nodes.txt", nodes);
            inputSize = nodes.Count;
            adjacency_matrix = new double[inputSize, inputSize]; // init affinity matrix
            D = new double[inputSize, inputSize]; // init diagonal matrix
            Y = new double[inputSize, NumofClusters]; D = new double[inputSize, inputSize];
            L = new double[inputSize, inputSize];

            SortedDictionary<int, List<string>> lst = new SortedDictionary<int, List<string>>();
            List<string> all_temp = new List<string>();
            string[] file = edges.ToArray();
            for (int i = 0; i < file.Length; i++)
            {
                string[] row = file[i].Split('\t');
                int feat = 0;
                if (!lst.ContainsKey(feat))
                    lst.Add(feat, new List<string>());
                lst[feat].Add(row[0] + "\t" + row[1] + "\t" + row[2]);
            }

            for (int i = 0; i < lst.Count; i++)
                normalize(lst[i], i.ToString());

            List<string> all_results = new List<string>();
            for (int i = 0; i < lst.Count; i++)
            {
                string[] files = all_res[i].ToArray<string>();                
                for (int j = 0; j < files.Length; j++)
                    all_results.Add(files[j]);
            }
            build_adjmat(all_results);

        }

        private double Norm(double value)
        {
            double value_new = Math.Log(value + 1);
            value_new = Math.Round(value_new, 5);
            return value_new;
        }
        private List<double> Scale(List<double> ls)
        {
            double minimum = 0;// ls.Min() - 1;
            double maximum = ls.Max();
            List<double> ls_scale = new List<double>();
            for (int i = 0; i < ls.Count; i++)
            {
                double temp = ls[i] - minimum;
                double temp2 = maximum - minimum;
                double res = temp / temp2;
                ls_scale.Add(res);
            }
            return ls_scale;
        }
        //private void normalize(List<string> features, string feat_num)
        //{
        //    all_res = new List<List<string>>();
        //    string[] file = features.ToArray<string>();//File.ReadAllLines("likeFullGraph.txt");
        //    List<double> lst2 = new List<double>();
        //    List<Int32> lst22 = new List<Int32>();
        //    List<string> all_temp = new List<string>();
        //    for (int i = 0; i < file.Length; i++)
        //    {
        //        string[] row = file[i].Split('\t');
        //        lst22.Add(Convert.ToInt32(row[2]));
        //        all_temp.Add(row[0] + '\t' + row[1] + '\t' + feat_num);
        //    }

        //    double mu = (double)(lst22.Sum()) / lst22.Count;
        //    double sig = Sigma(lst22, mu);
        //    double low = mu - sig;
        //    double up = mu + sig;
        //    for (int i = 0; i < lst22.Count; i++)
        //    {
        //        if (low < lst22[i] || lst22[i] < up)
        //            lst2.Add(Norm(lst22[i]));
        //        else
        //            lst2.Add(0);
        //    }

        //    lst2 = Scale(lst2);

        //    for (int i = 0; i < all_temp.Count; i++)
        //    {
        //        all_temp[i] = all_temp[i] + '\t' + lst2[i].ToString();
        //    }
        //    all_res.Add(all_temp);
        //    //File.WriteAllLines("result_" + feat_num + ".txt", all_temp);
        //}
        private void normalize(List<string> features, string feat_num)
        {
            all_res = new List<List<string>>();
            string[] file = features.ToArray<string>();//File.ReadAllLines("likeFullGraph.txt");
            List<double> lst2 = new List<double>();
            List<string> all_temp = new List<string>();
            for (int i = 0; i < file.Length; i++)
            {
                string[] row = file[i].Split('\t');
                double log_value = Norm(Convert.ToDouble(row[2]));
                lst2.Add(log_value);
                all_temp.Add(row[0] + '\t' + row[1]);// + '\t' + feat_num);
            }

            lst2 = Scale(lst2);

            for (int i = 0; i < all_temp.Count; i++)
            {
                all_temp[i] = all_temp[i] + '\t' + lst2[i].ToString();
            }
            all_res.Add(all_temp);
            //File.WriteAllLines("result_" + feat_num + ".txt", all_temp);
        }
        //static double Sigma(List<Int32> x, double mu)
        //{
        //    double sum = 0;
        //    for (int i = 0; i < x.Count; i++)
        //        sum += Math.Pow(x[i] - mu, 2);
        //    return Math.Sqrt(sum / x.Count);
        //}
        private void build_adjmat(List<string> results)
        {
            string[] lines = results.ToArray<string>();
            adjacency_matrix = new double[nodes.Count, nodes.Count];

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                double sim = 0;                
                string[] tmp = line.Split('\t');
                int n1 = nodes.IndexOf(tmp[0]);
                int n2 = nodes.IndexOf(tmp[1]);
                //Int64 n1 = Int64.Parse(tmp[0]) - 1;
                //Int64 n2 = Int64.Parse(tmp[1]) - 1;                                
                sim = Convert.ToDouble(tmp[2]);

                adjacency_matrix[n1, n2] = sim;
                adjacency_matrix[n2, n1] = sim;                
            }               
        }
        private void calculate_eigendecomposition()
        {
            var e = new Accord.Math.Decompositions.EigenvalueDecomposition(L);
            X = new double[inputSize, NumofClusters];
            for (int i = 0; i < inputSize; i++)
            {
                for (int j = 0; j < NumofClusters; j++)
                {
                    X[i,j] = e.Eigenvectors[i, j];
                }
            }
        }
        private int findCluster(double[][] mat, double[] x)
        {
            int index = -1;
            int found = 0;
            int maxi = mat.GetLength(0);
            int maxj = mat[0].Length;

            for (int i = 0; i < maxi; i++)
            {
                found = 0; // reset
                for (int j = 0; j < maxj; j++)
                {
                    if (mat[i][j] == x[j])
                        found++;


                }// end inner for
                if (found == NumofClusters) // row found ( text found ) 
                {
                    for (int j = 0; j < NumofClusters; j++)
                        mat[i][j] = -1; // reset the found object
                    return i; // return the found index;


                }
            }// end outer for



            return -1;
        }// end conArray
        private double[][] conArrayD(double[,] arr)
        {

            double[][] mat = new double[arr.GetLongLength(0)][];
            int maxi = arr.GetLength(0);
            int maxj = arr.GetLength(1);

            for (int i = 0; i < maxi; i++)
            {
                mat[i] = new double[arr.GetLength(1)];
                for (int j = 0; j < maxj; j++)
                {
                    mat[i][j] = arr[i, j];

                }// end inner for
            }// end outer for



            return mat;
        }// end conArray

        public void creat_csv()
        {
            //Create Table
            DataTable employees = new DataTable("Employees");
            employees.Columns.Add("FirstName", typeof(String));
            employees.Columns.Add("LastName", typeof(String));
            employees.Columns.Add("Address", typeof(String));
            employees.Columns.Add("City", typeof(String));
            employees.Columns.Add("State", typeof(String));
            employees.Columns.Add("Zipcode", typeof(Int32));

            //Add rows
            employees.Rows.Add("John", "Grove", "12 Malibu Road", "Malibu", "CA", 90265);
            employees.Rows.Add("Pamela", "Anderson", "12 Malibu Road", "Malibu", "CA", 90265);
            employees.Rows.Add("Carmen", "Electra", "12 Malibu Road", "Malibu", "CA", 90265);
            employees.Rows.Add("Suzanne", "Somers", "12 Malibu Road", "Malibu", "CA", 90265);

            using (StreamWriter sw = new StreamWriter(new FileStream("Temp.csv", FileMode.Create)))
            {
                for (Int32 i = 0; i < employees.Rows.Count; i++)
                {
                    string[] s = new string[employees.Columns.Count];
                    for (Int32 j = 0; j < employees.Columns.Count; j++)
                    {
                        s[j] = employees.Rows[i].ItemArray[j].ToString();
                    }
                    sw.WriteLine(string.Join(",", s));
                }
            }
        }
        private void Sigma(double[] vector, ref double min, ref double max)
        {
            double mean = vector.Mean();
            double sigma = Math.Sqrt(vector.Variance());
            max = sigma + mean;
            min = mean - sigma;
        }


        static public List<string> getMessages(long channelIDs)
        {
            List<string> messages = new List<string>();
            Uri Server_ = new Uri("http://192.168.30.106:8200/");
            var pool_DataReader = new Elasticsearch.Net.SingleNodeConnectionPool(Server_);
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            int month = DateTime.Now.Month;
            int previousMonth = DateTime.Now.AddMonths(-1).Month;
            string index = $"telegramcrw-{DateTime.Now.Year}-{month.ToString("##")}";

            Nest.ConnectionSettings connectionSettings_DataReader =                
                new Nest.ConnectionSettings(pool_DataReader).DefaultIndex(index);
            var client_DataReader = new Nest.ElasticClient(connectionSettings_DataReader);

            string minDate = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd HH:mm:ss" + ".000");
            var result = client_DataReader.Search<telegramcrw_message>(s => s
            .Type("message")
            .MatchAll()
            //.From(0)
            .Size(100)
            .Query(q => q
                .DateRange(r => r
                    .Field(f => f.createDate)
                    .GreaterThanOrEquals(minDate)
                //.LessThan("2018-12-20 19:05:00.000")
                )
                // Added without any debug
                && q.Term("language", "102097")
                && q.Term("fK_ChannelId", channelIDs)
            )
            //.Scroll("5m")
            //.SearchType(Elasticsearch.Net.SearchType.QueryThenFetch)
            .Sort(w => w.Ascending(f => f.createDate))
            );
            for (int j = 0; j < result.Documents.Count; j++)
                messages.Add(result.Documents.ElementAt(j).messageTextCleaned);

            return messages;
        }
    } // end SpectralClustering class
} // end namespace SpectralClustering