using DrawTest.Class;
using DrawTest.Entity;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace DrawTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer;
        private List<Player> players;
        private DrawLots drawLots;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var positions = drawLots.Draw();
            drawResultCtrl.ItemsSource = positions;
        }

        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            if(timer.IsEnabled)
            {
                return;
            }
            if(!int.TryParse(PlayerCountTextBox.Text.Trim(),out int playerCount))
            {
                throw new ArgumentException("Player count must be an integer number.");
            }
            if (!int.TryParse(SeedCountTextBox.Text.Trim(), out int seedCount))
            {
                throw new ArgumentException("Seed count must be an integer number.");
            }
            if (!int.TryParse(DelegationCountTextBox.Text.Trim(), out int delegationCount))
            {
                throw new ArgumentException("Delegation count must be an integer number.");
            }
            players = InitPlayers(playerCount, seedCount, delegationCount);
            drawLots = new DrawLots(players);
            timer.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private List<Player> InitPlayers(
            int playersCount,
            int seedCount,
            int delegationCount)
        {
            if (seedCount > playersCount)
                throw new Exception("Seed count can not be larger than player count.");
            if (delegationCount > playersCount)
                throw new Exception("Delegation count can not be larger than player count.");
            var playerList = new List<Player>(playersCount);
            var delegations = new string[] { "北京", "上海", "天津",
                "重庆", "河北", "山西", "辽宁", "吉林", "黑龙江", "江苏",
                "浙江", "安徽", "福建", "江西", "山东", "河南", "湖北",
                "湖南", "广东", "海南", "四川", "贵州", "云南", "陕西",
                "甘肃", "青海", "台湾", "内蒙古", "广西", "西藏", "宁夏",
                "新疆", "香港", "澳门" };
            for (int i = 1; i < playersCount + 1; i++)
            {
                playerList.Add(new Player
                {
                    Name = $"PLAYER {i}",
                    Id = i
                });
            }
            var drawProvider = new DrawProvider<Player>();
            drawProvider.Shuffle(playerList);
            for (int i = 0; i < seedCount; i++)
            {
                playerList[i].Seed = i + 1;
            }
            drawProvider.Shuffle(playerList);
            var delegationsInfo = new Dictionary<int, int>();
            var random = new Random();
            var restDelegationsCount = delegationCount;
            var restPlayersCount = playersCount;
            int delegationId;
            for (int i = 0; i < delegationCount - 1; i++)
            {
                delegationId = random.Next(0, 33);
                while (delegationsInfo.ContainsKey(delegationId))
                {
                    delegationId = random.Next(0, 33);
                }
                var currentDelegatePlayersCount = random.Next(0, restPlayersCount / restDelegationsCount * 2);
                restPlayersCount -= currentDelegatePlayersCount;
                restDelegationsCount--;
                delegationsInfo.Add(delegationId, currentDelegatePlayersCount);
            }
            delegationId = random.Next(0, 33);
            while (delegationsInfo.ContainsKey(delegationId))
            {
                delegationId = random.Next(0, 33);
            }
            delegationsInfo.Add(delegationId, restPlayersCount);

            var prefix = 0;
            foreach (var delegationInfo in delegationsInfo)
            {
                var delegationName = delegations[delegationInfo.Key];
                for (int i = prefix + 0; i < prefix + delegationInfo.Value; i++)
                {
                    playerList[i].DelegationName = delegationName;
                }
                prefix += delegationInfo.Value;
            }
            return playerList;
        }
    }
}
