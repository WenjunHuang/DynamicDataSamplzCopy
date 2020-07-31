using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;

namespace DynamicDataSamplzCopy.Examples
{
    public sealed class AggregationViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<AggregationItem> _items;

        public ReadOnlyObservableCollection<AggregationItem> Items => _items;

        private int _count;
        private int _max;
        private double _stdDev;
        private double _avg;
        private int _min;
        private double _sumOfOddNumbers;
        private int _sum;
        private int _countIncluded;

        public AggregationViewModel()
        {
            var sourceList = new SourceList<AggregationItem>();
            sourceList.AddRange(Enumerable.Range(1, 15).Select(i => new AggregationItem(i)));

            var listLoader = sourceList.Connect()
                .Sort(SortExpressionComparer<AggregationItem>.Ascending(vm => vm.Number))
                .ObserveOnDispatcher()
                .Bind(out _items)
                .Subscribe();

            var aggregatable = sourceList.Connect()
                .FilterOnProperty(vm => vm.IncludeInTotal, vm => vm.IncludeInTotal)
                .Publish();

            var sumOfOddNumbers = aggregatable.ToCollection()
                .Select(collection => collection.Where(i => i.Number % 2 == 1)
                    .Select(ai => ai.Number)
                    .Sum())
                .Subscribe(sum => SumOfOddNumbers = sum);

            _cleanUp = new CompositeDisposable(sourceList,
                listLoader,
                Observable.Count(aggregatable).Subscribe(count => Count = count),
                aggregatable.Sum(ai => ai.Number).Subscribe(sum => Sum = sum),
                aggregatable.Avg(ai => ai.Number).Subscribe(average => Avg = Math.Round(average, 2)),
                aggregatable.Minimum(ai => ai.Number).Subscribe(min => Min = min),
                aggregatable.Maximum(ai => ai.Number).Subscribe(max => Max = max),
                aggregatable.StdDev(ai => ai.Number).Subscribe(std => StdDev = Math.Round(std, 2)),
                sumOfOddNumbers,
                aggregatable.Connect()
            );
        }

        public int Count
        {
            get => _count;
            set { SetAndRaise(ref _count, value); }
        }

        public int CountIncluded
        {
            get => _countIncluded;
            set { SetAndRaise(ref _countIncluded, value); }
        }

        public int Sum
        {
            get => _sum;
            set { SetAndRaise(ref _sum, value); }
        }

        public int Min
        {
            get => _min;
            set { SetAndRaise(ref _min, value); }
        }

        public int Max
        {
            get => _max;
            set { SetAndRaise(ref _max, value); }
        }

        public double StdDev
        {
            get => _stdDev;
            set { SetAndRaise(ref _stdDev, value); }
        }

        public double Avg
        {
            get => _avg;
            set { SetAndRaise(ref _avg, value); }
        }

        public double SumOfOddNumbers
        {
            get => _sumOfOddNumbers;
            set { SetAndRaise(ref _sumOfOddNumbers, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}