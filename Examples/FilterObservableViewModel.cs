using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using System.Xml.Serialization;
using DynamicData;
using DynamicDataSamplzCopy.Infrastructure;

namespace DynamicDataSamplzCopy.Examples
{
    public class FilterObservableViewModel : IDisposable
    {
        private readonly IDisposable _cleanUp;

        private readonly ReadOnlyObservableCollection<FootballPlayer> _availablePlayers;
        private readonly ReadOnlyObservableCollection<FootballPlayer> _myTeamPeople;

        public ReadOnlyObservableCollection<FootballPlayer> AvailablePlayers => _availablePlayers;
        public ReadOnlyObservableCollection<FootballPlayer> MyTeam => _myTeamPeople;

        public FilterObservableViewModel()
        {
            var people = CreateFootballerList();
            var sharedDataSource = people.Connect().Publish();

            var allPeopleLoader = sharedDataSource
                .FilterOnObservable(person => person.IncludedChanged, included => !included)
                .ObserveOnDispatcher()
                .Bind(out _availablePlayers)
                .Subscribe();

            var includedPeopleLoader = sharedDataSource
                .FilterOnObservable(person => person.IncludedChanged, included => included)
                .ObserveOnDispatcher()
                .Bind(out _myTeamPeople)
                .Subscribe();

            _cleanUp = new CompositeDisposable(people, allPeopleLoader, includedPeopleLoader,
                sharedDataSource.Connect());
        }

        private ISourceList<FootballPlayer> CreateFootballerList()
        {
            var people = new SourceList<FootballPlayer>();
            people.AddRange(new[]
            {
                new FootballPlayer("Hennessey"),
                new FootballPlayer("Chester"),
                new FootballPlayer("Williams"),
                new FootballPlayer("Davies"),
                new FootballPlayer("Gunter"),
                new FootballPlayer("Allen"),
                new FootballPlayer("Ledley"),
                new FootballPlayer("Ramsey"),
                new FootballPlayer("Taylor"),
                new FootballPlayer("Bale"),
                new FootballPlayer("King"),
                new FootballPlayer("Hennessey"),
                new FootballPlayer("Collins"),

                new FootballPlayer("Courtois"),
                new FootballPlayer("Meunier"),
                new FootballPlayer("Alderweireld"),
                new FootballPlayer("Denayer"),
                new FootballPlayer("J Lukaku"),
                new FootballPlayer("Nainggolan"),
                new FootballPlayer("Witsel"),
                new FootballPlayer("Carrasco"),
                new FootballPlayer("De Bruyne"),
                new FootballPlayer("Hazard"),
                new FootballPlayer("R Lukaku"),
                new FootballPlayer("Merten"),
                new FootballPlayer("Fellain"),
                new FootballPlayer("Batshuayiat"),
            });
            return people;
        }

        public void Dispose()
        {
            _cleanUp?.Dispose();
        }
    }

    public class FootballPlayer
    {
        public string Name { get; }
        public ICommand IncludeCommand { get; }
        public ICommand ExcludeCommand { get; }
        public IObservable<bool> IncludedChanged { get; }

        public FootballPlayer(string name)
        {
            var includeChanged = new BehaviorSubject<bool>(false);

            Name = name;
            IncludeCommand = new Command(() => includeChanged.OnNext(true));
            ExcludeCommand = new Command(() => includeChanged.OnNext(false));
            IncludedChanged = includeChanged.AsObservable();
        }
    }

    public static class DynamicDataEx
    {
        public static IObservable<IChangeSet<TObject>> FilterOnObservable<TObject, TValue>(
            this IObservable<IChangeSet<TObject>> source,
            Func<TObject, IObservable<TValue>> observableSelector,
            Func<TValue, bool> predicate)
        {
            return Observable.Create<IChangeSet<TObject>>(observer =>
            {
                var locker = new object();
                var resultList = new SourceList<TObject>();

                var observableChangedMonitor = source.SubscribeMany(item =>
                {
                    return observableSelector(item)
                        .Synchronize(locker)
                        .Subscribe(value =>
                        {
                            var isMatched = predicate(value);
                            if (isMatched)
                            {
                                if (!resultList.Items.Contains(item))
                                    resultList.Add(item);
                            }
                            else
                            {
                                resultList.Remove(item);
                            }
                        });
                }).Subscribe(t => { }, observer.OnError);
                var publisher = resultList.Connect().SubscribeSafe(observer);

                return new CompositeDisposable(observableChangedMonitor, resultList, publisher);
            });
        }
    }
}