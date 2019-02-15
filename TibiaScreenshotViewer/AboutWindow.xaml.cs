using System;
using System.Reflection;
using System.Windows;
using System.Deployment.Application;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace TibiaScreenshotViewer
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            var version = ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : Assembly.GetEntryAssembly().GetName().Version.ToString();        

            InitializeComponent();

            VersionTextBlock.Inlines.Clear();
            VersionTextBlock.Text = "";

            var gitHubRepoHyperlink = new Hyperlink() { NavigateUri = new Uri("https://github.com/Br-ian/tibia-screenshot-viewer") };
            gitHubRepoHyperlink.Inlines.Add("Tibia Screenshot Viewer\r\n");
            gitHubRepoHyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            VersionTextBlock.Inlines.Add(gitHubRepoHyperlink);

            VersionTextBlock.Inlines.Add($"Version {version}\r\n" +
                                         $"Copyright (c) 2019\r\n" +
                                         $"Author: Brian / ");

            var gitHubHyperlink = new Hyperlink() { NavigateUri = new Uri("https://github.com/Br-ian") };
            gitHubHyperlink.Inlines.Add("Br-ian");
            gitHubHyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            VersionTextBlock.Inlines.Add(gitHubHyperlink);

            VersionTextBlock.Inlines.Add(" / ");

            var tibiaHyperlink = new Hyperlink() { NavigateUri = new Uri("https://www.tibia.com/community/?subtopic=characters&name=Oven+Schotel") };
            tibiaHyperlink.Inlines.Add("Oven Schotel");
            tibiaHyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            VersionTextBlock.Inlines.Add(tibiaHyperlink);

            VersionTextBlock.Inlines.Add(" / ");

            var redditHyperlink = new Hyperlink() { NavigateUri = new Uri("https://www.reddit.com/u/eveisdying") };
            redditHyperlink.Inlines.Add("/u/eveisdying");
            redditHyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            VersionTextBlock.Inlines.Add(redditHyperlink);

            VersionTextBlock.Inlines.Add(" / ");

            var discordHyperlink = new Hyperlink() { NavigateUri = new Uri("https://discord.gg/xaYms42") };
            discordHyperlink.Inlines.Add("Brian#9549");
            discordHyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            VersionTextBlock.Inlines.Add(discordHyperlink);

            VersionTextBlock.Inlines.Add("\r\nDonations? Send them to 'Oven Schotel' in-game");

            OkButton.Focus();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
