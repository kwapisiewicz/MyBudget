using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism.Mvvm;
using MyBudget.Core.DataContext;
using MyBudget.Model;
using MyBudget.UI.Core.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace MyBudget.UI.Operations
{
    public class StatisticsGroupSorter : IComparer<StatisticsGroup>
    {
        private Dictionary<string, int> ItemsOrder = new Dictionary<string, int>
        {
            { "Dochody",1 },
            { "Opłaty",2 },
            { "Jedzenie",3 },
            { "Samochód",4 },
            { "Zakupy",5 }
        };

        public int Compare(StatisticsGroup x, StatisticsGroup y)
        {
            if (ItemsOrder.Keys.Contains(x.Key) && ItemsOrder.Keys.Contains(y.Key))
            {
                return ItemsOrder[x.Key] - ItemsOrder[y.Key];
            }

            if (ItemsOrder.Keys.Contains(x.Key) && !ItemsOrder.Keys.Contains(y.Key))
            {
                return -1;
            }
            
            if (!ItemsOrder.Keys.Contains(x.Key) && ItemsOrder.Keys.Contains(y.Key))
            {
                return 1;
            }

            return x.Key.CompareTo(y.Key);
        }
    }

    public class StatisticsViewModel : BindableBase
    {
        IRepository<BankOperation> _bankOperationRepository;

        public StatisticsViewModel(IContext context)
        {
            _bankOperationRepository = context.GetRepository<IRepository<BankOperation>>();
            //ReloadView();
        }

        private Func<DateTime, bool> _filterFunction;
        public Func<DateTime, bool> FilterFunction
        {
            get
            {
                return _filterFunction;
            }
            set
            {
                _filterFunction = value;
                OnPropertyChanged(() => FilterFunction);
                ReloadView();
            }
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime Month { get; set; }
        public DateRangeType DateRangeType { get; set; }

        private bool? _cleared;
        public bool? Cleared
        {
            get
            {
                return _cleared;
            }
            set
            {
                _cleared = value;
                OnPropertyChanged(() => Cleared);
                ReloadView();
            }
        }

        private bool _showSeparate = true;
        public bool ShowSeparate
        {
            get
            {
                return _showSeparate;
            }
            set
            {
                _showSeparate = value;
                OnPropertyChanged(() => ShowSeparate);
                ReloadView();
            }
        }

        private string _SeparateCategories = "Wewnetrzne, Oszczędności";
        public string SeparateCategories
        {
            get
            {
                return _SeparateCategories;
            }
            set
            {
                _SeparateCategories = value;
                OnPropertyChanged(() => SeparateCategories);
                ReloadView();
            }
        }

        private void ReloadView()
        {
            var operations = FilterOperations();
            var previousOps = GetOperationsForPreviousMonth();

            var roots = GetGroup(operations.GroupBy(l1 => l1.Category ?? string.Empty));

            var sorted = roots.Where(a => !string.IsNullOrEmpty(a.Key)).OfType<StatisticsGroup>();

            foreach (var item in sorted)
            {
                var subGroupping = item.Elements.GroupBy(a => a.SubCategory ?? string.Empty)
                    .Where(a => !string.IsNullOrEmpty(a.Key));
                item.SubGroups = GetGroup(subGroupping);
            }

            AddSummary(roots, operations);

            AddSeparateSummaries(roots, operations);

            Items = roots;
        }

        private void AddSeparateSummaries(ObservableCollection<IGroupItem> roots, IEnumerable<BankOperation> operations)
        {
            if (ShowSeparate)
            {
                var separateSumaries = SeparateCategories.Split(',')
                    .Select(a => a.Trim());

                var sumWithoutSeparate = operations
                    .Where(op => !separateSumaries.Contains(op.Category))
                    .Sum(a => a.Amount);

                roots.Add(new StatisticsGroup()
                {
                    Key = Resources.Translations.WithoutSparateCategoriesText,
                    Sum = sumWithoutSeparate
                });

                roots.Add(new Splitter() { Key = "==============", Sum = "==============" });
                foreach (var summary in separateSumaries)
                {
                    var summarySum = operations
                        .Where(op => summary == op.Category)
                        .Sum(a => a.Amount);
                    roots.Add(new StatisticsGroup()
                    {
                        Key = summary,
                        Sum = summarySum
                    });
                }
            }
        }

        private static void AddSummary(ObservableCollection<IGroupItem> roots, IEnumerable<BankOperation> operations)
        {
            var sumAll = operations.Sum(a => a.Amount);
            roots.Add(new Splitter() { Key = "==============", Sum = "==============" });
            roots.Add(new StatisticsGroup()
            {
                Key = Resources.Translations.SumText,
                Sum = sumAll
            });
        }

        private IEnumerable<BankOperation> FilterOperations()
        {
            var itemsToDisplay = _bankOperationRepository.GetAll();
            if (Cleared.HasValue)
            {
                itemsToDisplay = itemsToDisplay.Where(a => a.Cleared == Cleared.Value);
            }
            if (FilterFunction != null)
            {
                itemsToDisplay = itemsToDisplay.Where(a => FilterFunction(a.OrderDate));
            }
            return itemsToDisplay;
        }

        private IEnumerable<BankOperation> GetOperationsForPreviousMonth()
        {
            var itemsToDisplay = _bankOperationRepository.GetAll();
            if (Cleared.HasValue)
            {
                itemsToDisplay = itemsToDisplay.Where(a => a.Cleared == Cleared.Value);
            }

            Func<DateTime, bool> filterByDate;

            if(DateRangeType == DateRangeType.ByMonth)
            {
                var previousMonth = new DateTime(Month.Year, Month.Month, 1).AddDays(-1);
                filterByDate = (date) => date.Year == previousMonth.Year && date.Month == previousMonth.Month;
            }
            else
            {
                int length = (Start - End).Days;
                var prevStart = Start.AddDays(-length);
                var prevEnd = End.AddDays(-length);
                filterByDate = (date) => date >= prevStart && date <= prevEnd;
            }

            if (FilterFunction != null)
            {
                itemsToDisplay = itemsToDisplay.Where(a => filterByDate(a.OrderDate));
            }
            return itemsToDisplay;
        }

        private ObservableCollection<IGroupItem> GetGroup(IEnumerable<IGrouping<string, BankOperation>> groupping)
        {
            return new ObservableCollection<IGroupItem>(groupping.Select(group =>
                    new StatisticsGroup()
                    {
                        Key = group.Key,
                        Elements = group,
                        Sum = group.Sum(el1 => el1.Amount)
                    }).OrderBy(x => x, new StatisticsGroupSorter()));
        }

        private IEnumerable<IGroupItem> _items { get; set; }
        public IEnumerable<IGroupItem> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                OnPropertyChanged(() => Items);
            }
        }
    }
}
