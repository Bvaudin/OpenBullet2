﻿using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Utils;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for MultiRunJobOptionsDialog.xaml
    /// </summary>
    public partial class MultiRunJobOptionsDialog : Page
    {
        private readonly Action<JobOptions> onAccept;
        private readonly MultiRunJobOptionsViewModel vm;

        public MultiRunJobOptionsDialog(MultiRunJobOptions options = null, Action<JobOptions> onAccept = null)
        {
            this.onAccept = onAccept;
            vm = new MultiRunJobOptionsViewModel(options);
            DataContext = vm;

            vm.StartConditionModeChanged += mode => startConditionTabControl.SelectedIndex = (int)mode;
            
            InitializeComponent();

            startConditionTabControl.SelectedIndex = (int)vm.StartConditionMode;
        }

        private void AddGroupProxySource(object sender, RoutedEventArgs e) => vm.AddGroupProxySource();
        private void AddFileProxySource(object sender, RoutedEventArgs e) => vm.AddFileProxySource();
        private void AddRemoteProxySource(object sender, RoutedEventArgs e) => vm.AddRemoteProxySource();

        private void RemoveProxySource(object sender, RoutedEventArgs e)
            => vm.RemoveProxySource((ProxySourceOptionsViewModel)(sender as Button).Tag);

        public void SelectConfig(ConfigViewModel config) => vm.SelectConfig(config);

        private void SelectConfig(object sender, RoutedEventArgs e)
            => new MainDialog(new SelectConfigDialog(this), "Select a config").ShowDialog();

        private void Accept(object sender, RoutedEventArgs e)
        {
            onAccept?.Invoke(vm.Options);
            ((MainDialog)Parent).Close();
        }
    }

    public class MultiRunJobOptionsViewModel : ViewModelBase
    {
        private readonly ConfigService configService;
        private readonly JobFactoryService jobFactory;
        private readonly IProxyGroupRepository proxyGroupRepo;
        public MultiRunJobOptions Options { get; init; }

        #region Start Condition
        public event Action<StartConditionMode> StartConditionModeChanged;

        public StartConditionMode StartConditionMode
        {
            get => Options.StartCondition switch
            {
                RelativeTimeStartCondition => StartConditionMode.Relative,
                AbsoluteTimeStartCondition => StartConditionMode.Absolute,
                _ => throw new NotImplementedException()
            };
            set
            {
                Options.StartCondition = value switch
                {
                    StartConditionMode.Relative => new RelativeTimeStartCondition(),
                    StartConditionMode.Absolute => new AbsoluteTimeStartCondition(),
                    _ => throw new NotImplementedException()
                };

                OnPropertyChanged();
                OnPropertyChanged(nameof(StartInMode));
                OnPropertyChanged(nameof(StartAtMode));
                StartConditionModeChanged?.Invoke(StartConditionMode);
            }
        }

        public bool StartInMode
        {
            get => StartConditionMode is StartConditionMode.Relative;
            set
            {
                if (value)
                {
                    StartConditionMode = StartConditionMode.Relative;
                }

                OnPropertyChanged();
            }
        }

        public bool StartAtMode
        {
            get => StartConditionMode is StartConditionMode.Absolute;
            set
            {
                if (value)
                {
                    StartConditionMode = StartConditionMode.Absolute;
                }

                OnPropertyChanged();
            }
        }

        public DateTime StartAtTime
        {
            get => Options.StartCondition is AbsoluteTimeStartCondition abs ? abs.StartAt : DateTime.Now;
            set
            {
                if (Options.StartCondition is AbsoluteTimeStartCondition abs)
                {
                    abs.StartAt = value;
                }

                OnPropertyChanged();
            }
        }

        public TimeSpan StartIn
        {
            get => Options.StartCondition is RelativeTimeStartCondition rel ? rel.StartAfter : TimeSpan.Zero;
            set
            {
                if (Options.StartCondition is RelativeTimeStartCondition rel)
                {
                    rel.StartAfter = value;
                }

                OnPropertyChanged();
            }
        }
        #endregion

        #region Config and Proxy options
        private BitmapImage configIcon;
        public BitmapImage ConfigIcon
        {
            get => configIcon;
            private set
            {
                configIcon = value;
                OnPropertyChanged();
            }
        }

        private string configNameAndAuthor;
        public string ConfigNameAndAuthor
        {
            get => configNameAndAuthor;
            set
            {
                configNameAndAuthor = value;
                OnPropertyChanged();
            }
        }

        public void SelectConfig(ConfigViewModel vm)
        {
            Options.ConfigId = vm.Config.Id;
            SetConfigData();
        }

        private void SetConfigData()
        {
            var config = configService.Configs.FirstOrDefault(c => c.Id == Options.ConfigId);

            if (config is not null)
            {
                ConfigIcon = Images.Base64ToBitmapImage(config.Metadata.Base64Image);
                ConfigNameAndAuthor = $"{config.Metadata.Name} by {config.Metadata.Author}";
            }
        }

        public int Bots
        {
            get => Options.Bots;
            set
            {
                Options.Bots = value;
                OnPropertyChanged();
            }
        }

        public int BotLimit => jobFactory.BotLimit;

        public int Skip
        {
            get => Options.Skip;
            set
            {
                Options.Skip = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<JobProxyMode> ProxyModes => Enum.GetValues(typeof(JobProxyMode)).Cast<JobProxyMode>();

        public JobProxyMode ProxyMode
        {
            get => Options.ProxyMode;
            set
            {
                Options.ProxyMode = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<NoValidProxyBehaviour> NoValidProxyBehaviours => Enum.GetValues(typeof(NoValidProxyBehaviour)).Cast<NoValidProxyBehaviour>();

        public NoValidProxyBehaviour NoValidProxyBehaviour
        {
            get => Options.NoValidProxyBehaviour;
            set
            {
                Options.NoValidProxyBehaviour = value;
                OnPropertyChanged();
            }
        }

        public bool ShuffleProxies
        {
            get => Options.ShuffleProxies;
            set
            {
                Options.ShuffleProxies = value;
                OnPropertyChanged();
            }
        }

        public bool MarkAsToCheckOnAbort
        {
            get => Options.MarkAsToCheckOnAbort;
            set
            {
                Options.MarkAsToCheckOnAbort = value;
                OnPropertyChanged();
            }
        }

        public bool NeverBanProxies
        {
            get => Options.NeverBanProxies;
            set
            {
                Options.NeverBanProxies = value;
                OnPropertyChanged();
            }
        }

        public bool ConcurrentProxyMode
        {
            get => Options.ConcurrentProxyMode;
            set
            {
                Options.ConcurrentProxyMode = value;
                OnPropertyChanged();
            }
        }

        public int PeriodicReloadIntervalSeconds
        {
            get => Options.PeriodicReloadIntervalSeconds;
            set
            {
                Options.PeriodicReloadIntervalSeconds = value;
                OnPropertyChanged();
            }
        }

        public int ProxyBanTimeSeconds
        {
            get => Options.ProxyBanTimeSeconds;
            set
            {
                Options.ProxyBanTimeSeconds = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public MultiRunJobOptionsViewModel(MultiRunJobOptions options)
        {
            Options = options ?? new MultiRunJobOptions();
            configService = SP.GetService<ConfigService>();
            jobFactory = SP.GetService<JobFactoryService>();
            proxyGroupRepo = SP.GetService<IProxyGroupRepository>();

            SetConfigData();

            proxyGroups = proxyGroupRepo.GetAll().ToList();
            PopulateProxySources();
        }

        #region Proxy Sources
        private readonly IEnumerable<ProxyGroupEntity> proxyGroups;
        public IEnumerable<string> ProxyGroupNames => new string[] { "All" }.Concat(proxyGroups.Select(g => g.Name));
        public IEnumerable<ProxyType> ProxyTypes => Enum.GetValues(typeof(ProxyType)).Cast<ProxyType>();

        private ObservableCollection<ProxySourceOptionsViewModel> proxySourcesCollection;
        public ObservableCollection<ProxySourceOptionsViewModel> ProxySourcesCollection
        {
            get => proxySourcesCollection;
            set
            {
                proxySourcesCollection = value;
                OnPropertyChanged();
            }
        }

        public void AddGroupProxySource()
        {
            var options = new GroupProxySourceOptions();
            Options.ProxySources.Add(options);
            var vm = new GroupProxySourceOptionsViewModel(options, proxyGroups);
            ProxySourcesCollection.Add(vm);
        }

        public void AddFileProxySource()
        {
            var options = new FileProxySourceOptions();
            Options.ProxySources.Add(options);
            var vm = new FileProxySourceOptionsViewModel(options);
            ProxySourcesCollection.Add(vm);
        }

        public void AddRemoteProxySource()
        {
            var options = new RemoteProxySourceOptions();
            Options.ProxySources.Add(options);
            var vm = new RemoteProxySourceOptionsViewModel(options);
            ProxySourcesCollection.Add(vm);
        }

        public void RemoveProxySource(ProxySourceOptionsViewModel vm)
        {
            ProxySourcesCollection.Remove(vm);
            Options.ProxySources.Remove(vm.Options);
        }

        private void PopulateProxySources()
        {
            ProxySourcesCollection = new ObservableCollection<ProxySourceOptionsViewModel>();

            foreach (var source in Options.ProxySources)
            {
                switch (source)
                {
                    case GroupProxySourceOptions group:
                        ProxySourcesCollection.Add(new GroupProxySourceOptionsViewModel(group, proxyGroups));
                        break;

                    case FileProxySourceOptions file:
                        ProxySourcesCollection.Add(new FileProxySourceOptionsViewModel(file));
                        break;

                    case RemoteProxySourceOptions remote:
                        ProxySourcesCollection.Add(new RemoteProxySourceOptionsViewModel(remote));
                        break;
                }
            }
        }
        #endregion
    }

    public enum StartConditionMode
    {
        Relative,
        Absolute
    }

    #region Other ViewModels
    public class ProxySourceOptionsViewModel : ViewModelBase
    {
        public ProxySourceOptions Options { get; init; }

        public ProxySourceOptionsViewModel(ProxySourceOptions options)
        {
            Options = options;
        }
    }

    public class GroupProxySourceOptionsViewModel : ProxySourceOptionsViewModel
    {
        private GroupProxySourceOptions GroupOptions => Options as GroupProxySourceOptions;

        private readonly IEnumerable<ProxyGroupEntity> proxyGroups;

        public string GroupName
        {
            get => GroupOptions.GroupId == -1 ? "All" : proxyGroups.First(g => g.Id == GroupOptions.GroupId).Name;
            set
            {
                GroupOptions.GroupId = value == "All" ? -1 : proxyGroups.First(g => g.Name == value).Id;
                OnPropertyChanged();
            }
        }

        public GroupProxySourceOptionsViewModel(GroupProxySourceOptions options,
            IEnumerable<ProxyGroupEntity> proxyGroups) : base(options)
        {
            this.proxyGroups = proxyGroups;
        }
    }

    public class FileProxySourceOptionsViewModel : ProxySourceOptionsViewModel
    {
        private FileProxySourceOptions FileOptions => Options as FileProxySourceOptions;

        public string FileName
        {
            get => FileOptions.FileName;
            set
            {
                FileOptions.FileName = value;
                OnPropertyChanged();
            }
        }

        public ProxyType DefaultType
        {
            get => FileOptions.DefaultType;
            set
            {
                FileOptions.DefaultType = value;
                OnPropertyChanged();
            }
        }

        public FileProxySourceOptionsViewModel(FileProxySourceOptions options) : base(options)
        {

        }
    }

    public class RemoteProxySourceOptionsViewModel : ProxySourceOptionsViewModel
    {
        private RemoteProxySourceOptions RemoteOptions => Options as RemoteProxySourceOptions;

        public string Url
        {
            get => RemoteOptions.Url;
            set
            {
                RemoteOptions.Url = value;
                OnPropertyChanged();
            }
        }

        public ProxyType DefaultType
        {
            get => RemoteOptions.DefaultType;
            set
            {
                RemoteOptions.DefaultType = value;
                OnPropertyChanged();
            }
        }

        public RemoteProxySourceOptionsViewModel(RemoteProxySourceOptions options) : base(options)
        {

        }
    }
    #endregion
}
