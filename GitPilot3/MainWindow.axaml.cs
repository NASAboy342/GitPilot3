using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using GitPilot3.Models;
using GitPilot3.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit;

namespace GitPilot3;

public partial class MainWindow : Window
{
    private readonly IFolderPicker _folderPicker;
    private readonly IGitRepositoryService _gitRepositoryService;
    public GitRepository CurrentRepository { get; set; } = new GitRepository();
    private readonly int _commitMessageRowHeight = 30;

    public MainWindow()
    {
        InitializeComponent();
        _folderPicker = new FolderPicker(this); // Pass 'this' window as parent
        _gitRepositoryService = new GitRepositoryService();
        SetupWindow();
    }

    private void SetupWindow()
    {
        // Set the window icon and initial state
        this.WindowState = WindowState.Maximized;

        // You can add initial repository loading logic here
        LoadDefaultRepository();
    }

    private void LoadDefaultRepository()
    {
        // Placeholder for loading the current directory as a git repository
        // This will be expanded when implementing actual Git functionality
    }

    // Event handlers for menu items and toolbar buttons
    private void OnCloneRepository(object sender, RoutedEventArgs e)
    {
        // TODO: Implement clone repository dialog
    }

    private async void OnOpenRepository(object sender, RoutedEventArgs e)
    {
        var repositoryPath = await _folderPicker.ShowDialogAsync();
        if (string.IsNullOrEmpty(repositoryPath))
            return;
        CurrentRepository = await _gitRepositoryService.LoadRepositoryAsync(repositoryPath);
        CurrentRepository.RemoteBranches = await _gitRepositoryService.GetRemoteBranchesAsync(repositoryPath);
        CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(repositoryPath);
        CurrentRepository.Commits = await _gitRepositoryService.GetCommitsAsync(repositoryPath);
        UpdateCurrentRepositoryDisplay();
    }

    private void UpdateCurrentRepositoryDisplay()
    {
        RepoNameTextBlock.Text = CurrentRepository.Name;
        RepoPathTextBlock.Text = CurrentRepository.Path;
        UpdateLocalBranchesTreeView();
        UpdateRemoteBranchesTreeView();
        UpdateCurrentBranchNameDisplay();
        UpdateCommitsInfoView();
    }

    private void UpdateCommitsInfoView()
    {
        UpdateCommitsMessageView();
        UpdateCommitsGraphView();
        UpdateCommitsDateView();
        UpdateCommitsBranchView();
    }

    private void UpdateCommitsBranchView()
    {
        var commitsBranchStackPanel = this.FindControl<StackPanel>("BranchNamesHeaderStackPanel");
        if (commitsBranchStackPanel == null)
            return;
        commitsBranchStackPanel.Children.Clear();
        commitsBranchStackPanel.Children.Add(GetGraphHeaderItem("Branches"));
        foreach (var commit in CurrentRepository.Commits)
        {
            var commitBranch = GetCommitBranchByName(commit.BranchName);
            var commitBranchItem = new Border
            {
                Height = _commitMessageRowHeight,
                Child = new Border
                {
                    Height = _commitMessageRowHeight - 5,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(5, 0, 5, 0),
                    CornerRadius = new Avalonia.CornerRadius(4),
                    Background = new SolidColorBrush(commitBranch.ToAvaloniaColor()),
                    Child = new TextBlock
                    {
                        Text = commit.BranchName,
                        FontSize = 12,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Margin = new Avalonia.Thickness(5, 0, 0, 0)
                    }
                }
            };
            commitsBranchStackPanel.Children.Add(commitBranchItem);
        }
    }

    private GitBranch GetCommitBranchByName(string branchName)
    {
        return CurrentRepository.LocalBranches.FirstOrDefault(b => b.Name == branchName)
            ?? CurrentRepository.RemoteBranches.FirstOrDefault(b => b.Name == branchName)
            ?? new GitBranch();
    }

    private void UpdateCommitsDateView()
    {
        var commitsDateStackPanel = this.FindControl<StackPanel>("CommitDateHeaderStackPanel");
        if (commitsDateStackPanel == null)
            return;
        commitsDateStackPanel.Children.Clear();
        commitsDateStackPanel.Children.Add(GetGraphHeaderItem("Dates"));
        foreach (var commit in CurrentRepository.Commits)
        {
            var commitDateItem = new Border
            {
                Height = _commitMessageRowHeight,
                Child = new TextBlock
                {
                    Text = commit.AuthorDate.ToString("yyyy-MM-dd"),
                    FontSize = 12,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(5, 0, 0, 0)
                }
            };
            commitsDateStackPanel.Children.Add(commitDateItem);
        }
    }

    private void UpdateCommitsGraphView()
    {
    }

    private void UpdateCommitsMessageView()
    {
        var commitsMessageStackPanel = this.FindControl<StackPanel>("CommitMessageHeaderStackPanel");
        if (commitsMessageStackPanel == null)
            return;
        commitsMessageStackPanel.Children.Clear();
        commitsMessageStackPanel.Children.Add(GetGraphHeaderItem("Commit Messages"));
        foreach (var commit in CurrentRepository.Commits)
        {
            var commitMessageItem = new Border
            {
                Height = _commitMessageRowHeight,
                Child = new TextBlock
                {
                    Text = commit.Message + (commit.IsWorkInProgress ? $"   ✏️{commit.ChangedFilesCount}" : ""),
                    FontSize = 12,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(5, 0, 0, 0)
                },
                Classes = { "commitMessageBorder" }

            };
            commitMessageItem.Tapped += async (sender, e) =>
            {
                await ShowCommitDetails(commit);
            };
            commitsMessageStackPanel.Children.Add(commitMessageItem);
        }
    }

    private async Task ShowCommitDetails(GitCommit commit)
    {
        CurrentRepository.CommitDetail = await _gitRepositoryService.GetCommitDetailsAsync(CurrentRepository.Path, commit);
        UpdateFileChangesView();
    }

    private void UpdateFileChangesView()
    {
        UpdateCommitDetailsHeaderView();
        UpdateCommitDetailMessagePanel();
        UpdateFileChangesHeaderView();
        UpdateFileChangesStackPanel();
    }

    private void UpdateFileChangesStackPanel()
    {
        var fileChangesStackPanel = this.FindControl<StackPanel>("FileChangesStackPanel");
        if (fileChangesStackPanel == null)
            return;

        fileChangesStackPanel.Children.Clear();

        foreach (var fileChange in CurrentRepository.CommitDetail.FilesChanged)
        {
            var fileChangeItem = new Border
            {
                Height = 30,
                Classes = { "fileChangeBorder" },
                Child = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,

                    Children =
                    {
                        new TextBlock
                        {
                            Text = GetChangeTypeSymbol(fileChange.ChangeType),
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = fileChange.FileName,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = $"+{fileChange.Additions}",
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Foreground = Brushes.Green,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = $"-{fileChange.Deletions}",
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Foreground = Brushes.OrangeRed,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        }
                    }
                }
            };
            fileChangeItem.Tapped += async (sender, e) =>
            {
                await ShowFileChangeDetail(fileChange);
            };
            fileChangesStackPanel.Children.Add(fileChangeItem);
        }
    }

    private async Task ShowFileChangeDetail(GitCommitFileChange fileChange)
    {
        var fileContentScrollViewer = this.FindControl<ScrollViewer>("FileContentScrollViewer");
        var commitGraphScrollViewer = this.FindControl<ScrollViewer>("CommitGraphScrollViewer");

        if (fileContentScrollViewer == null)
            return;

        var stackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
        };

        stackPanel.Children.Add(GetFileContentHeader(fileChange, fileContentScrollViewer, commitGraphScrollViewer));

        stackPanel.Children.Add(GetFileContentView(fileChange));
        fileContentScrollViewer.Content = stackPanel;
        fileContentScrollViewer.IsVisible = true;

        if (commitGraphScrollViewer != null)
        {
            commitGraphScrollViewer.IsVisible = false;
        }
    }

    private Control GetFileContentView(GitCommitFileChange fileChange)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Margin = new Avalonia.Thickness(5)
        };

        var lines = fileChange.DiffContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var lineColor = Brushes.White;
            if (line.StartsWith("+"))
            {
                lineColor = Brushes.LightGreen;
            }
            else if (line.StartsWith("-"))
            {
                lineColor = Brushes.IndianRed;
            } 
            stackPanel.Children.Add(new TextBlock
            {
                Text = line,
                FontFamily = new FontFamily("Consolas, 'Courier New', monospace"),
                FontSize = 12,
                Foreground = lineColor
            });
        }

        return stackPanel;
    }

    private Control GetFileContentHeader(GitCommitFileChange fileChange, ScrollViewer fileContentScrollViewer, ScrollViewer? commitGraphScrollViewer)
    {
        var headerStackPanel = new DockPanel
        {
            Height = 30,
            Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
        };

        var fileNameTextBlock = new TextBlock
        {
            Text = fileChange.FileName,
            FontSize = 16,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Avalonia.Thickness(5, 0, 0, 0)
        };
        DockPanel.SetDock(fileNameTextBlock, Dock.Left);

        var backButton = new Button
        {
            Content = "✖︎",
            Padding = new Avalonia.Thickness(5, 0, 5, 0),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 5, 0)
        };
        DockPanel.SetDock(backButton, Dock.Right);

        backButton.Click += (sender, e) =>
        {
            fileContentScrollViewer.IsVisible = false;
            if (commitGraphScrollViewer != null)
            {
                commitGraphScrollViewer.IsVisible = true;
            }
        };

        headerStackPanel.Children.Add(fileNameTextBlock);
        headerStackPanel.Children.Add(backButton);

        return headerStackPanel;
    }

    private string GetChangeTypeSymbol(string changeType)
    {
        switch (changeType)
        {
            case "Added":
                return "➕";
            case "Modified":
                return "✏️";
            case "Deleted":
                return "❌";
            case "Renamed":
                return "🔄";
            case "Copied":
                return "📋";
            default:
                return "❓";
        }
    }

    private void UpdateFileChangesHeaderView()
    {
        var fileChangesHeader = this.FindControl<Border>("FileChangesHeader");
        if (fileChangesHeader == null)
            return;

        var profileStackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };

        profileStackPanel.Children.Add(GetProfileIcon());
        profileStackPanel.Children.Add(GetProfileInfo());
        fileChangesHeader.Child = profileStackPanel;
    }

    private Control GetProfileInfo()
    {
        var profileInfoStackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Margin = new Avalonia.Thickness(10, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        profileInfoStackPanel.Children.Add(new TextBlock
        {
            Text = CurrentRepository.CommitDetail.Commit.AuthorName,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = Brushes.White,
            FontSize = 14,
        });

        profileInfoStackPanel.Children.Add(new TextBlock
        {
            Text = "authored: " + CurrentRepository.CommitDetail.Commit.AuthorDate.ToString("MM/dd/yyyy @ HH:mm tt"),
            Foreground = Brushes.Gray,
        });

        return profileInfoStackPanel;
    }

    private Control GetProfileIcon()
    {
        var profileIconBlock = new Border
        {
            Width = 30,
            Height = 30,
            Background = Brushes.DodgerBlue,
            CornerRadius = new Avalonia.CornerRadius(15),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        var profileLetter = CurrentRepository.CommitDetail.Commit.AuthorName;
        profileLetter = string.IsNullOrEmpty(profileLetter) ? "?" : profileLetter.Substring(0, 1).ToUpper();
        profileIconBlock.Child = new TextBlock
        {
            Text = profileLetter,
            Foreground = Brushes.White,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            FontSize = 16,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        return profileIconBlock;
    }

    private void UpdateCommitDetailMessagePanel()
    {
        var commitDetailMessagePanel = this.FindControl<StackPanel>("CommitDetailMessagePanel");
        if (commitDetailMessagePanel == null)
            return;
        commitDetailMessagePanel.Children.Clear();
        commitDetailMessagePanel.Children.Add(new TextBlock
        {
            FontSize = 16,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Text = CurrentRepository.CommitDetail.Commit.Message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });
        commitDetailMessagePanel.Children.Add(new TextBlock
        {
            FontSize = 12,
            Foreground = Brushes.Gray,
            Text = CurrentRepository.CommitDetail.Commit.Description,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        });

    }

    private void UpdateCommitDetailsHeaderView()
    {
        var commitDetailsHeader = this.FindControl<TextBlock>("CommitDetailsHeader");
        if (commitDetailsHeader == null)
            return;

        var commitSha = CurrentRepository.CommitDetail?.Commit.Sha ?? "";
        commitSha = commitSha.Length > 7 ? commitSha.Substring(0, 7) : commitSha;

        commitDetailsHeader.Text = "commit: " + commitSha;
    }

    private Border? GetGraphHeaderItem(string headerName)
    {
        return new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
            Height = 25,
            Child = new TextBlock
            {
                Text = headerName,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5, 0, 0, 0)
            }
        };
    }

    private void UpdateCurrentBranchNameDisplay()
    {
        var currentBranch = CurrentRepository.LocalBranches.FirstOrDefault(b => b.IsCurrent);
        BranchCurrentTextBlock.Text = currentBranch != null ? $"Branch: {currentBranch.Name}" : "Branch:";
    }

    private void UpdateRemoteBranchesTreeView()
    {
        var remoteBranchesTreeViewItem = this.FindControl<TreeViewItem>("RemoteBranchesTreeViewItem");
        if (remoteBranchesTreeViewItem == null)
            return;
        remoteBranchesTreeViewItem.Items.Clear();
        foreach (var branch in CurrentRepository.RemoteBranches)
        {
            var newItem = new TreeViewItem
            {
                Header = branch.Name,
                IsSelected = false
            };

            newItem.DoubleTapped += async (sender, e) =>
            {
                await OnRemoteBranchDoubleClicked(branch.Name);
                e.Handled = true;
            };

            remoteBranchesTreeViewItem.Items.Add(newItem);
        }
    }

    private async Task OnRemoteBranchDoubleClicked(string name)
    {
        await _gitRepositoryService.CheckoutBranchAsync(CurrentRepository, name);
        CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
        UpdateLocalBranchesTreeView();
        UpdateCurrentBranchNameDisplay();
    }

    private void UpdateLocalBranchesTreeView()
    {
        var localBranchesTreeViewItem = this.FindControl<TreeViewItem>("LocalBranchesTreeViewItem");
        if (localBranchesTreeViewItem == null)
            return;
        localBranchesTreeViewItem.Items.Clear();
        foreach (var branch in CurrentRepository.LocalBranches)
        {
            var newItem = new TreeViewItem
            {
                Header = branch.Name,
                IsSelected = false
            };

            newItem.DoubleTapped += async (sender, e) =>
            {
                await OnLocalBranchDoubleClicked(branch.Name);
                e.Handled = true;
            };

            if (branch.IsCurrent)
            {
                newItem.Background = new SolidColorBrush(Color.FromRgb(81, 81, 81));
            }

            localBranchesTreeViewItem.Items.Add(newItem);
        }
    }

    private async Task OnLocalBranchDoubleClicked(string name)
    {
        await _gitRepositoryService.CheckoutBranchAsync(CurrentRepository, name);
        CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
        UpdateLocalBranchesTreeView();
        UpdateCurrentBranchNameDisplay();
    }

    private static StackPanel AddGreenDot()
    {
        var stackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal
        };
        stackPanel.Children.Add(new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = Brushes.Green
        });
        return stackPanel;
    }

    private void OnFetch(object sender, RoutedEventArgs e)
    {
        // TODO: Implement fetch functionality
    }

    private void OnPull(object sender, RoutedEventArgs e)
    {
        // TODO: Implement pull functionality
    }

    private void OnPush(object sender, RoutedEventArgs e)
    {
        // TODO: Implement push functionality
    }

    private void OnCreateBranch(object sender, RoutedEventArgs e)
    {
        // TODO: Implement create branch dialog
    }

    private void OnRefresh(object sender, RoutedEventArgs e)
    {
        // TODO: Implement refresh repository state
    }
}