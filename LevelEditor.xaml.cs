using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Entities;

namespace PosnaiSQLauncher;

public partial class LevelEditor : Window
{
    private readonly AppDbContext _appDbContext;
    private List<Option> _options;
    private Option? _selectedOption;
    private readonly LinkedList<int> _displayedOptionIds = new();

    public LevelEditor()
    {
        InitializeComponent();
        CommandBindings.Add(new CommandBinding(EditorCommands.SaveVariant, OnSaveVariant));
        InputBindings.Add(new KeyBinding(EditorCommands.SaveVariant, Key.S, ModifierKeys.Control));
        _appDbContext = new AppDbContext();
        SelectedQueryRadio.IsChecked = true;

        LoadOptions();
        InitDisplayedQueue();
        LoadOptionsTabs();
        LoadDatabases();

        SaveOptionMenu.Click += SaveOptionMenu_Click;
        CloseAccountMenu.Click += CloseAccountMenu_Click;
        CloseAppMenu.Click += CloseAppMenu_Click;
        ExportAppMenu.Click += ExportAppMenu_Click;
        
        if (OptionsTabControl.Items.Count > 0)
        {
            OptionsTabControl.SelectedIndex = 0;
            
            OptionsTabControl_SelectionChanged(OptionsTabControl, null);
        }
    }

    private void LoadOptions()
    {
        _options = _appDbContext.Options
            .Include(o => o.IdLocationNavigation)
            .Include(o => o.IdQueryNavigation)
            .ThenInclude(q => q.IdDatabaseNavigation)
            .ToList();

    }

    private void InitDisplayedQueue()
    {
        _displayedOptionIds.Clear();
        var last5 = _options
            .OrderByDescending(o => o.IdOption)
            .Take(5)
            .Select(o => o.IdOption);

        foreach (var id in last5)
            _displayedOptionIds.AddFirst(id);
    }
    private void LoadOptionsTabs()
    {
        OptionsTabControl.Items.Clear();

        foreach (var id in _displayedOptionIds)
        {
            var option = _options.FirstOrDefault(o => o.IdOption == id);
            if (option == null) continue;

            var tab = new TabItem
            {
                Header = $"Вариант {option.IdOption}",
                Tag = option.IdOption,
                Width = 200,
            };

            var menu = new ContextMenu();

            var closeItem = new MenuItem { Header = "Закрыть вариант" };
            closeItem.Click += (s, e) => CloseOptionTab(option.IdOption);
            menu.Items.Add(closeItem);

            var deleteItem = new MenuItem { Header = "Удалить вариант" };
            deleteItem.Click += (s, e) => RemoveOption(option.IdOption);
            menu.Items.Add(deleteItem);

            tab.ContextMenu = menu;
            OptionsTabControl.Items.Add(tab);

        }

        var addTab = new TabItem
        {
            Header = "+",
            Width = 40
        };

        var addMenu = new ContextMenu();
        var selectItem = new MenuItem { Header = "Выбрать из списка" };
        selectItem.Click += (s, e) => ShowOptionsListDialog();
        var addNewOption = new MenuItem { Header = "Добавить новый" };
        addNewOption.Click += (s, e) => AddOption();
        addMenu.Items.Add(selectItem);
        addMenu.Items.Add(addNewOption);

        addTab.PreviewMouseLeftButtonDown += (s, e) =>
        {
            addMenu.PlacementTarget = addTab;
            addMenu.IsOpen = true;
            e.Handled = true;
        };

        addTab.PreviewMouseRightButtonDown += (s, e) =>
        {
            e.Handled = true;
        };

        OptionsTabControl.Items.Add(addTab);
    }
    
    private List<Database> GetDatabasesWithAll()
    {
        var databases = _appDbContext.Databases.ToList();

        // Фейковая база должна иметь валидный int IdDatabase
        var allDatabasesItem = new Database
        {
            IdDatabase = -1,
            Name = "-",
        };

        databases.Insert(0, allDatabasesItem);
        return databases;
    }


    private void LoadDatabases()
    {
        DatabaseComboBox.ItemsSource = GetDatabasesWithAll();
        DatabaseComboBox.DisplayMemberPath = "Name";
        DatabaseComboBox.SelectedValuePath = "IdDatabase";
        DatabaseComboBox.SelectedValue = -1; // выбираем "-"
    }


    private void OptionsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (OptionsTabControl.SelectedItem is not TabItem tab) return;

        if (tab.Tag is int optionId)
        {
            _selectedOption = _options.FirstOrDefault(o => o.IdOption == optionId);
            if (_selectedOption != null)
            {
                SelectedQueryRadio.IsChecked = true;
                LoadOptionData();
            }
        }
    }

    private void AddOption()
    {
        EnsureSeedData();

        var newOption = new Option();
        _appDbContext.Options.Add(newOption);
        _appDbContext.SaveChanges();
        
        _options.Add(newOption);

        if (_displayedOptionIds.Contains(newOption.IdOption))
            _displayedOptionIds.Remove(newOption.IdOption);

        _displayedOptionIds.AddFirst(newOption.IdOption);

        if (_displayedOptionIds.Count > 5)
            _displayedOptionIds.RemoveLast();

        LoadOptionsTabs();

        var tab = OptionsTabControl.Items
            .OfType<TabItem>()
            .FirstOrDefault(t => t.Tag is int id && id == newOption.IdOption);

        if (tab != null)
        {
            OptionsTabControl.SelectedItem = tab;
            _selectedOption = newOption;
            NewQueryRadio.IsChecked = true;
            ClearQueryFields();
        }
    }
    
    private void RemoveOption(int optionId)
    {
        var option = _options.FirstOrDefault(o => o.IdOption == optionId);
        if (option == null) return;

        _appDbContext.Options.Remove(option);
        _appDbContext.SaveChanges();
        _selectedOption = null;
        _options.Remove(option);

        if (_displayedOptionIds.Contains(optionId))
            _displayedOptionIds.Remove(optionId);

        LoadOptionsTabs();
        LoadOptionData();
    }

    private void CloseOptionTab(int optionId)
    {
        var tabToRemove = OptionsTabControl.Items
            .OfType<TabItem>()
            .FirstOrDefault(t => t.Tag is int id && id == optionId);

        if (tabToRemove != null)
            OptionsTabControl.Items.Remove(tabToRemove);
        _selectedOption = null;
        if (_displayedOptionIds.Contains(optionId))
            _displayedOptionIds.Remove(optionId);

        var firstId = _displayedOptionIds.FirstOrDefault();
        var firstTab = OptionsTabControl.Items
            .OfType<TabItem>()
            .FirstOrDefault(t => t.Tag is int id && id == firstId);

        if (firstTab != null)
        {
            OptionsTabControl.SelectedItem = firstTab;
            _selectedOption = _options.FirstOrDefault(o => o.IdOption == firstId);
            if (_selectedOption != null)
                LoadOptionData();
        }
        else
            ClearQueryFields();
    }


    private void ShowOptionsListDialog()
    {
        var dialog = new SelectOptionDialog(_options);
        if (dialog.ShowDialog() == true)
        {
            var selectedOptions = dialog.SelectedOptions;
            if (selectedOptions.Count > 0)
            {
                _displayedOptionIds.Clear();

                foreach (var opt in selectedOptions.Take(5))
                {
                    _displayedOptionIds.AddLast(opt.IdOption);
                }

                LoadOptionsTabs();

                var firstSelected = selectedOptions.First();
                _selectedOption = _options.FirstOrDefault(o => o.IdOption == firstSelected.IdOption);

                var firstTab = OptionsTabControl.Items
                    .OfType<TabItem>()
                    .FirstOrDefault(t => t.Tag is int id && id == firstSelected.IdOption);
                if (firstTab != null)
                    OptionsTabControl.SelectedItem = firstTab;

                if (_selectedOption != null)
                    LoadOptionData();
            }
        }
    }

    private void EnsureSeedData()
    {
        if (!_appDbContext.Databases.Any())
        {
            var defaultDb = new Database
            {
                Name = "БД",
            };
            _appDbContext.Databases.Add(defaultDb);
            _appDbContext.SaveChanges();
        }
        else if (_appDbContext.Databases.Count() > 1)
        {
            var defaults = _appDbContext.Databases
                .ToList()
                .Where(d => d.Name == "БД")
                .ToList();

            if (defaults.Any())
            {
                _appDbContext.Databases.RemoveRange(defaults);
                _appDbContext.SaveChanges();
            }
        }

        if (!_appDbContext.Locations.Any())
        {
            var defaultLocation = new Location
            {
                Name = "Локация",
            };
            _appDbContext.Locations.Add(defaultLocation);
            _appDbContext.SaveChanges();
        }
        else if (_appDbContext.Locations.Count() > 1)
        {
            var defaults = _appDbContext.Locations
                .ToList()
                .Where(l => l.Name == "Локация")
                .ToList();

            if (defaults.Any())
            {
                _appDbContext.Locations.RemoveRange(defaults);
                _appDbContext.SaveChanges();
            }
        }

        if (!_appDbContext.Queries.Any())
        {
            var baseQuery = new Query
            {
                Name = CryptoManager.Encrypt("Название"),
                Condition = CryptoManager.Encrypt("Условие"),
                QueryString = CryptoManager.Encrypt("Запрос"),
                Difficulty = 1,
                IdDatabase = _appDbContext.Databases.First().IdDatabase
            };
            _appDbContext.Queries.Add(baseQuery);
            _appDbContext.SaveChanges();
        }
    }
    private void LoadOptionData()
    {
        LocationComboBox.ItemsSource = _appDbContext.Locations.ToList();
        LocationComboBox.SelectedValuePath = "IdLocation";
        LocationComboBox.DisplayMemberPath = "Name";

        if (_selectedOption == null)
        {
            LocationComboBox.SelectedIndex = -1;
            DatabaseComboBox.SelectedValue = -1; // вместо "-"
            QueryComboBox.SelectedIndex = -1;
            TimeLimitTimePicker.Value = DateTime.Today.AddMinutes(5);
            QueryStringTextBox.Text = "";
            QueryConditionTextBox.Text = "";
            QueryDifficultySlider.Value = 1;
            return;
        }

        LocationComboBox.SelectedValue = _selectedOption.IdLocation;

        DatabaseComboBox.ItemsSource = GetDatabasesWithAll();
        DatabaseComboBox.DisplayMemberPath = "Name";
        DatabaseComboBox.SelectedValuePath = "IdDatabase";

        int dbId = _selectedOption.IdQueryNavigation?.IdDatabaseNavigation?.IdDatabase ?? -1;
        DatabaseComboBox.SelectedValue = dbId;

        var database = _appDbContext.Databases.FirstOrDefault(db => db.IdDatabase == dbId);
        QueryComboBox.ItemsSource = SortQueries(database);
        QueryComboBox.DisplayMemberPath = "DecryptedName";
        QueryComboBox.SelectedValuePath = "IdQuery";

        QueryComboBox.SelectedValue = _selectedOption.IdQuery;

        TimeLimitTimePicker.Value = DateTime.Today.AddSeconds(_selectedOption.TimeLimit);

        if (_selectedOption.IdQueryNavigation != null)
        {
            QueryStringTextBox.Text = CryptoManager.SafeDecrypt(_selectedOption.IdQueryNavigation.QueryString);
            QueryConditionTextBox.Text = CryptoManager.SafeDecrypt(_selectedOption.IdQueryNavigation.Condition);
            QueryNameTextBox.Text = CryptoManager.SafeDecrypt(_selectedOption.IdQueryNavigation.Name);
        }
        else
        {
            QueryStringTextBox.Text = "";
            QueryConditionTextBox.Text = "";
            QueryNameTextBox.Text = "";
        }

    }



    private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DatabaseComboBox.SelectedItem is Database selectedDb)
        {
            if (selectedDb.IdDatabase == -1)
            {
                QueryComboBox.ItemsSource = _appDbContext.Queries.ToList();
                QueryComboBox.SelectedIndex = -1;
                ClearQueryFields();
            }
            else
            {
                QueryComboBox.ItemsSource = SortQueries(selectedDb);
            }
        }
    }


    private List<Query> SortQueries(Database selectedDb)
    {
        if (selectedDb == null)
            return new List<Query>();

        return _appDbContext.Queries
            .Where(q => q.IdDatabase == selectedDb.IdDatabase)
            .ToList();
    }






    private void QueryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (QueryComboBox.SelectedItem is Query selectedQuery)
        {
            var database = _appDbContext.Databases
                .FirstOrDefault(db => db.IdDatabase == selectedQuery.IdDatabase);

            LoadSelectedQueryData();

            if (database != null)
                DatabaseComboBox.SelectedItem = database;
        }
    }

    private void LoadSelectedQueryData()
    {
        if (QueryComboBox.SelectedItem is Query selectedQuery)
        {
            QueryStringTextBox.Text = CryptoManager.SafeDecrypt(selectedQuery.QueryString);
            QueryConditionTextBox.Text = CryptoManager.SafeDecrypt(selectedQuery.Condition);
            QueryDifficultySlider.Value = Convert.ToInt32(selectedQuery.Difficulty);
        }
    }


    private void ClearQueryFields()
    {
        QueryNameTextBox.Text = "";
        QueryStringTextBox.Text = "";
        QueryConditionTextBox.Text = "";
        LocationComboBox.SelectedIndex = -1;
        DatabaseComboBox.SelectedIndex = -1;
        QueryComboBox.SelectedIndex = -1;
        TimeLimitTimePicker.Value = DateTime.Today.AddMinutes(5);
        QueryDifficultySlider.Value = 1;
    }

    private void SelectQueryRadio_Checked(object sender, RoutedEventArgs e)
    {
        ClearQueryFields();
        QueryComboBox.Visibility = Visibility.Visible;
        QueryNameTextBox.Visibility = Visibility.Collapsed;

        QueryStringTextBox.IsEnabled = false;
        QueryConditionTextBox.IsEnabled = false;

        QueryDifficultySlider.IsEnabled = false;

            LoadOptionData();

    }

    private void NewQueryRadio_Checked(object sender, RoutedEventArgs e)
    {
        QueryComboBox.Visibility = Visibility.Collapsed;
        QueryNameTextBox.Visibility = Visibility.Visible;

        QueryStringTextBox.IsEnabled = true;
        QueryConditionTextBox.IsEnabled = true;

        QueryDifficultySlider.IsEnabled = true;

        ClearQueryFields();
    }

    private void AddDatabaseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddDatabaseWindow(this);

        if (dialog.ShowDialog() == true)
        {
            var newDb = new Database
            {
                Name = dialog.DatabaseName,
            };

            _appDbContext.Databases.Add(newDb);
            _appDbContext.SaveChanges();

            DatabaseComboBox.ItemsSource = GetDatabasesWithAll();
            DatabaseComboBox.SelectedItem = newDb;
        }
    }

    private void CloseAccountMenu_Click(object sender, RoutedEventArgs e)
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        this.Close();
    }

    private void ChangePasswordMenu_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ChangePasswordWindow();
        if (dialog.ShowDialog() == true)
        {
            MessageBox.Show("Пароль обновлён. Войдите заново.", "Аккаунт", MessageBoxButton.OK, MessageBoxImage.Information);
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }


    private void CloseAppMenu_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void ExportAppMenu_Click(object sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        var options = db.Options
            .Include(o => o.IdQueryNavigation)
            .Include(o => o.IdLocationNavigation)
            .ToList();
        var exportWindow = new ExportWindow(options);
        exportWindow.ShowDialog();
    }

    private WorkingState ReadWorkingStateFromUI()
    {
        var ws = new WorkingState
        {
            OptionId = _selectedOption?.IdOption,
            LocationId = (int?)LocationComboBox.SelectedValue,
            QueryId = QueryComboBox.SelectedValue as int?,
            IsNewQueryMode = NewQueryRadio.IsChecked == true,
            Difficulty = (int?)QueryDifficultySlider.Value,
            IdDatabase = DatabaseComboBox.SelectedValue as int?
        };

        if (TimeLimitTimePicker.Value is DateTime dt)
            ws.TimeLimit = (int)(dt - DateTime.Today).TotalSeconds;
        else
            ws.TimeLimit = 0;

        if (ws.IsNewQueryMode)
        {
            ws.NewQueryName = QueryNameTextBox.Text;
            ws.NewQueryText = QueryStringTextBox.Text;
            ws.NewQueryCondition = QueryConditionTextBox.Text;
        }

        return ws;
    }


    private (bool ok, string message) Validate(WorkingState ws)
    {
        if (ws.OptionId is null) return (false, "Не выбран вариант.");
        if (ws.LocationId is null) return (false, "Не выбрана локация.");
        if (ws.TimeLimit <= 0)
            return (false, "Не установлено время.");

        if (ws.Difficulty is null || ws.Difficulty < 1 || ws.Difficulty > 10)
            return (false, "Сложность должна быть от 1 до 10.");

        if (!ws.IsNewQueryMode)
        {
            // режим выбора существующего запроса
            if (ws.IdDatabase is null || ws.IdDatabase == -1)
                return (false, "Выберите базу данных для запроса.");

            if (ws.QueryId is null)
                return (false, "Выберите запрос из списка.");

            var query = _appDbContext.Queries
                .Include(q => q.IdDatabaseNavigation)
                .FirstOrDefault(q => q.IdQuery == ws.QueryId);

            if (query == null)
                return (false, "Запрос не найден.");

            if (query.IdDatabaseNavigation?.IdDatabase != ws.IdDatabase)
                return (false, "Выбранный запрос не соответствует выбранной базе данных.");
        }
        else
        {
            // режим нового запроса
            if (ws.IdDatabase is null || ws.IdDatabase == -1)
                return (false, "Для нового запроса выберите конкретную базу данных.");

            if (string.IsNullOrWhiteSpace(ws.NewQueryName))
                return (false, "Укажите название нового запроса.");
            if (string.IsNullOrWhiteSpace(ws.NewQueryText))
                return (false, "Запрос не должен быть пустым.");
            if (string.IsNullOrWhiteSpace(ws.NewQueryCondition))
                return (false, "Условие запроса не должно быть пустым.");
            if (ws.NewQueryName!.Length > 50) return (false, "Название запроса превышает 50 символов.");
            if (ws.NewQueryText!.Length > 200) return (false, "Текст запроса превышает 200 символов.");
            if (ws.NewQueryCondition!.Length > 150) return (false, "Условие запроса превышает 150 символов.");
        }

        return (true, "OK");
    }


    private void OnSaveVariant(object sender, ExecutedRoutedEventArgs e)
    {
        var ws = ReadWorkingStateFromUI();
        var (ok, msg) = Validate(ws);
        if (!ok)
        {
            MessageBox.Show(msg, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var option = _appDbContext.Options
            .Include(o => o.IdQueryNavigation)
            .FirstOrDefault(o => o.IdOption == ws.OptionId);
        if (option == null)
        {
            MessageBox.Show("Текущий вариант не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        option.IdLocation = ws.LocationId!.Value;
        option.TimeLimit = ws.TimeLimit;

        if (!ws.IsNewQueryMode)
        {
            option.IdQuery = ws.QueryId!.Value;
            _appDbContext.SaveChanges();
            MessageBox.Show("Вариант сохранён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            var newQuery = new Query
            {
                Name = CryptoManager.Encrypt(ws.NewQueryName!),
                QueryString = CryptoManager.Encrypt(ws.NewQueryText!),
                Condition = CryptoManager.Encrypt(ws.NewQueryCondition!),
                Difficulty = Convert.ToInt32(ws.Difficulty!.Value),
                IdDatabase = ws.IdDatabase!.Value
            };

            _appDbContext.Queries.Add(newQuery);
            _appDbContext.SaveChanges();

            option.IdQuery = newQuery.IdQuery;
            _appDbContext.SaveChanges();

            MessageBox.Show("Новый запрос добавлен и вариант сохранён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

            _selectedOption = option;
            LoadOptionData();
            SelectedQueryRadio.IsChecked = true;
        }
    }



    private void SaveOptionMenu_Click(object sender, RoutedEventArgs e)
    {
        OnSaveVariant(sender, null);
    }
}
