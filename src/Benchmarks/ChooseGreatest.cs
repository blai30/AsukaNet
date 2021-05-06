using System.Linq;
using Asuka.Models.API.TraceMoe;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    public class ChooseGreatest
    {
        private TraceMoeResponse _response1;
        private TraceMoeResponse _response2;

        [Benchmark]
        public void AggregateBenchmark()
        {
            var doc = _response1.Docs!
                .Aggregate((current, next) => next.Similarity > current.Similarity ? next : current);
        }

        [Benchmark]
        public void OrderByDescendingBenchmark()
        {
            var doc = _response2.Docs!
                .OrderByDescending(d => d.Similarity)
                .First();
        }

        [GlobalSetup]
        public void Setup()
        {
            _response1 = new TraceMoeResponse
            {
                Docs = new[]
                {
                    new TraceMoeDoc
                    {
                        Similarity = 0.89f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.89f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.41f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.65f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.21f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.87f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.99f
                    },
                }
            };

            _response2 = new TraceMoeResponse
            {
                Docs = new[]
                {
                    new TraceMoeDoc
                    {
                        Similarity = 0.89f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.89f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.41f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.65f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.21f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.87f
                    },
                    new TraceMoeDoc
                    {
                        Similarity = 0.99f
                    },
                }
            };
        }
    }
}
