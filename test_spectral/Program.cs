using System;
using System.Collections.Generic;
using System.IO;
using SpectralClustering;

namespace test_spectral
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> test_in = new List<string>();
            
            //Edges including Source, Target and weight with tab seperator
            string[] lines = File.ReadAllLines("Edges.txt");

            for (int i = 0; i < lines.Length; i++)
                test_in.Add(lines[i]);

            SpectClustering sc = new SpectClustering(test_in, 50);
            
            var res = sc.StartClustering();
            Dictionary<string, int> words_clusters = res.Item2;
            List<string> lab = new List<string>();
            foreach (var item in words_clusters)
            {
                lab.Add(item.Key + "\t" + item.Value);
            }
            File.WriteAllLines("labels.txt", lab);
        }

    }
}
