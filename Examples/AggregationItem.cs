using DynamicData.Binding;

namespace DynamicDataSamplzCopy.Examples
{
    public class AggregationItem : AbstractNotifyPropertyChanged
    {
        public int Number { get; }

        private bool _includeInTotal = true;

        public AggregationItem(int number)
        {
            Number = number;
        }

        public bool IncludeInTotal
        {
            get => _includeInTotal;
            set { SetAndRaise(ref _includeInTotal, value); }
        }
    }
}