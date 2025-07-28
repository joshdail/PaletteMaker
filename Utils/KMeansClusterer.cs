using System;
using System.Collections.Generic;
using System.Linq;

namespace PaletteMaker.Utils
{
    public class KMeansClusterer
    {
        private readonly int _k;
        private readonly int _maxIterations;
        private readonly Random _rand;

        public KMeansClusterer(int k, int maxIterations = 100, int? seed = null)
        {
            _k = k;
            _maxIterations = maxIterations;
            _rand = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public List<(double L, double A, double B)> Fit(List<(double L, double A, double B)> data)
        {
            if (data.Count < _k)
                throw new ArgumentException("Not enough data points for the number of clusters.");

            // Randomly initialize centroids
            var centroids = data.OrderBy(_ => _rand.Next()).Take(_k).ToList();
            var assignments = new int[data.Count];

            for (int iter = 0; iter < _maxIterations; iter++)
            {
                bool changed = false;

                // Assign points to closest centroid
                for (int i = 0; i < data.Count; i++)
                {
                    int bestIndex = -1;
                    double bestDist = double.MaxValue;

                    for (int j = 0; j < _k; j++)
                    {
                        var dist = ColorUtils.ColorDistance(data[i], centroids[j]);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestIndex = j;
                        }
                    }

                    if (assignments[i] != bestIndex)
                    {
                        assignments[i] = bestIndex;
                        changed = true;
                    }
                }

                // Exit early if no changes
                if (!changed)
                    break;

                // Recalculate centroids
                var newCentroids = new (double L, double A, double B)[_k];
                var counts = new int[_k];

                for (int i = 0; i < data.Count; i++)
                {
                    int cluster = assignments[i];
                    newCentroids[cluster].L += data[i].L;
                    newCentroids[cluster].A += data[i].A;
                    newCentroids[cluster].B += data[i].B;
                    counts[cluster]++;
                }

                for (int j = 0; j < _k; j++)
                {
                    if (counts[j] == 0)
                        continue; // Avoid divide-by-zero

                    centroids[j] = (
                        newCentroids[j].L / counts[j],
                        newCentroids[j].A / counts[j],
                        newCentroids[j].B / counts[j]
                    );
                }
            }

            return centroids;
        }
    }
}
