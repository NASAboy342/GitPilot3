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
using System.Collections.Generic;
using LibGit2Sharp;
using GitPilot3.UserControlles;
using Avalonia.Controls.Documents;

namespace GitPilot3;

public partial class MainWindow : Window
{
    private readonly IFolderPicker _folderPicker;
    private readonly IGitRepositoryService _gitRepositoryService;
    public GitRepository CurrentRepository { get; set; } = new GitRepository();
    private readonly int _commitMessageRowHeight = 30;
    private readonly IUserProfileService _userProfileService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppStageService _appStageService;
    private readonly ErrorMessageHandler _errorMessageHandler;

    public MainWindow(IUserProfileService userProfileService,
    IGitRepositoryService gitRepositoryService,
    IServiceProvider serviceProvider,
    IAppStageService appStageService,
    ErrorMessageHandler errorMessageHandler)
    {
        _userProfileService = userProfileService;
        _gitRepositoryService = gitRepositoryService;
        _serviceProvider = serviceProvider;
        _appStageService = appStageService;
        _errorMessageHandler = errorMessageHandler;
        _folderPicker = new FolderPicker(this);
        InitializeComponent();
        SetupWindow();
        _errorMessageHandler.ErrorMessageShown += (s, e) => AddErrorCard(e);
        _errorMessageHandler.SuccessMessageShown += (s, e) => AddSuccessCard(e);
        StartScheduleRefreshCurrentRepository();
    }

    private async Task StartScheduleRefreshCurrentRepository()
    {
        try
        {
            if (CurrentRepository == null || string.IsNullOrEmpty(CurrentRepository.Path))
            {
                await Task.Delay(TimeSpan.FromSeconds(20));
                StartScheduleRefreshCurrentRepository();
                return;
            }
            await SyncRepository();
        }
        catch (Exception ex)
        {
            _errorMessageHandler.ShowErrorMessage("Failed to refresh repository: " + ex.Message);
        }
        await Task.Delay(TimeSpan.FromSeconds(10));
        StartScheduleRefreshCurrentRepository();
    }

    private async Task SyncRepository()
    {
        if (CurrentRepository == null || string.IsNullOrEmpty(CurrentRepository.Path))
            throw new InvalidOperationException("No repository is currently loaded.");
        var localBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
        UpdateLocalBranches(localBranches);
        var remoteBranches = await _gitRepositoryService.GetRemoteBranchesAsync(CurrentRepository.Path);
        UpdateRemoteBranches(remoteBranches);
        CurrentRepository.Commits = await _gitRepositoryService.GetCommitsAsync(CurrentRepository.Path);
        UpdateCurrentRepositoryDisplay();
        if (CurrentRepository.CommitDetail != null && !string.IsNullOrEmpty(CurrentRepository.CommitDetail.Commit.Sha))
        {
            CurrentRepository.CommitDetail = await _gitRepositoryService.GetCommitDetailsAsync(CurrentRepository.Path, CurrentRepository.CommitDetail.Commit);
            UpdateFileChangesView();
        }
        _appStageService.SaveCurrentRepository(CurrentRepository);
    }

    private void UpdateRemoteBranches(List<GitBranch> remoteBranches)
    {
        foreach (var updatedBranch in remoteBranches)
        {
            var existingBranch = CurrentRepository.RemoteBranches.FirstOrDefault(b => b.Name == updatedBranch.Name);
            if (existingBranch != null)
            {
                existingBranch.InComming = updatedBranch.InComming;
                existingBranch.OutGoing = updatedBranch.OutGoing;
            }
            else
            {
                CurrentRepository.RemoteBranches.Add(updatedBranch);
            }
        }
        foreach (var existingBranch in CurrentRepository.RemoteBranches.ToList())
        {
            if (!remoteBranches.Any(b => b.Name == existingBranch.Name))
            {
                CurrentRepository.RemoteBranches.Remove(existingBranch);
            }
        }
    }

    private void UpdateLocalBranches(List<GitBranch> newLocalBranchesData)
    {
        foreach (var updatedBranch in newLocalBranchesData)
        {
            var existingBranch = CurrentRepository.LocalBranches.FirstOrDefault(b => b.Name == updatedBranch.Name);
            if (existingBranch != null)
            {
                existingBranch.InComming = updatedBranch.InComming;
                existingBranch.OutGoing = updatedBranch.OutGoing;
            }
            else
            {
                CurrentRepository.LocalBranches.Add(updatedBranch);
            }
        }
        foreach (var existingBranch in CurrentRepository.LocalBranches.ToList())
        {
            if (!newLocalBranchesData.Any(b => b.Name == existingBranch.Name))
            {
                CurrentRepository.LocalBranches.Remove(existingBranch);
            }
        }
    }

    private void SetupWindow()
    {
        this.WindowState = WindowState.Maximized;

        LoadProfileInfoDisplay();
        LoadLastOpentedRepository();
    }

    private void AddSuccessCard(string message)
    {
        var errorStackPanel = this.FindControl<StackPanel>("ErrorStackPanel");
        if (errorStackPanel == null)
            return;

        var successCard = new SuccessCard(message);
        successCard.CloseSuccessCardClicked += (s, e) =>
        {
            errorStackPanel.Children.Remove(successCard);
        };
        errorStackPanel.Children.Add(successCard);
    }

    private void AddErrorCard(string message)
    {
        var errorStackPanel = this.FindControl<StackPanel>("ErrorStackPanel");
        if (errorStackPanel == null)
            return;

        var errorCard = new ErrorCard(message);
        errorCard.CloseErrorCardClicked += (s, e) =>
        {
            errorStackPanel.Children.Remove(errorCard);
        };
        errorStackPanel.Children.Add(errorCard);
    }
    private void LoadLastOpentedRepository()
    {
        try
        {
            var lastRepository = _appStageService.GetCurrentRepository();
            if (lastRepository == null)
                return;
            CurrentRepository = lastRepository;
            UpdateCurrentRepositoryDisplay();
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
        }
    }

    private async Task LoadProfileInfoDisplay()
    {
        try
        {
            var profileButton = this.FindControl<Button>("ProfileButton");
            if (profileButton == null)
                return;
            var currentProfile = await _userProfileService.GetCurrentUserProfileAsync();
            if (currentProfile == null)
                return;
            var stackPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 10
            };
            var profileIcon = new Border
            {
                Width = 24,
                Height = 24,
                Background = new Avalonia.Media.SolidColorBrush(currentProfile.ToAvaloniaColor()),
                CornerRadius = new Avalonia.CornerRadius(12),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            var profileIconLetter = new TextBlock
            {
                Text = currentProfile.Username.Length > 0 ? currentProfile.Username[0].ToString().ToUpper() : "?",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = Avalonia.Media.Brushes.White
            };
            profileIcon.Child = profileIconLetter;
            stackPanel.Children.Add(profileIcon);
            var username = new TextBlock
            {
                Text = currentProfile.Username,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            stackPanel.Children.Add(username);
            profileButton.Content = stackPanel;
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
        }
    }

    // Event handlers for menu items and toolbar buttons
    private void OnCloneRepository(object sender, RoutedEventArgs e)
    {
        // TODO: Implement clone repository dialog
    }

    private async void OnOpenRepository(object sender, RoutedEventArgs e)
    {
        try
        {
            var repositoryPath = await _folderPicker.ShowDialogAsync();
            if (string.IsNullOrEmpty(repositoryPath))
                return;
            CurrentRepository = await _gitRepositoryService.LoadRepositoryAsync(repositoryPath);
            CurrentRepository.RemoteBranches = await _gitRepositoryService.GetRemoteBranchesAsync(repositoryPath);
            CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(repositoryPath);
            CurrentRepository.Commits = await _gitRepositoryService.GetCommitsAsync(repositoryPath);
            UpdateCurrentRepositoryDisplay();
            _appStageService.SaveCurrentRepository(CurrentRepository);
            AddSuccessCard($"Repository: {CurrentRepository.Name} loaded successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
        }
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
                    Background = string.IsNullOrEmpty(commit.BranchName) ? new SolidColorBrush(Colors.Transparent) : new SolidColorBrush(commitBranch.ToAvaloniaColor()),
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
        UpdateStagedFileChangesStackPanel();
        SetStagedFileChangesVisibility(CurrentRepository.CommitDetail.Commit.IsWorkInProgress);
    }

    private void UpdateStagedFileChangesStackPanel()
    {
        if (CurrentRepository.CommitDetail == null || !CurrentRepository.CommitDetail.Commit.IsWorkInProgress)
            return;

        var stagedFileChangesStackPanel = this.FindControl<StackPanel>("StagedFileChangesStackPanel");
        if (stagedFileChangesStackPanel == null)
            return;

        stagedFileChangesStackPanel.Children.Clear();

        var stagedHeader = new TextBlock
        {
            Text = "Staged Changes",
            FontSize = 14,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(5, 10, 0, 5)
        };
        stagedFileChangesStackPanel.Children.Add(stagedHeader);

        foreach (var fileChange in CurrentRepository.CommitDetail.FilesChanged.Where(f => f.IsStaged))
        {
            var fileChangeItem = new DockPanel
            {
                Height = 30,
                Classes = { "fileChangeBorder" },
            };

            var fileInfo = new StackPanel
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
            };
            DockPanel.SetDock(fileInfo, Dock.Left);
            fileInfo.Tapped += async (sender, e) =>
            {
                await ShowFileChangeDetail(fileChange);
            };

            var unstageFileButton = new Button
            {
                Content = "−",
                Foreground = Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Height = 20,
                Width = 20,
                Padding = new Avalonia.Thickness(5, 0, 5, 0)
            };

            unstageFileButton.Click += async (sender, e) =>
            {
                await UnstageFileChange(fileChange);
            };
            DockPanel.SetDock(unstageFileButton, Dock.Right);

            fileChangeItem.Children.Add(unstageFileButton);
            fileChangeItem.Children.Add(fileInfo);
            stagedFileChangesStackPanel.Children.Add(fileChangeItem);
        }
    }

    private void SetStagedFileChangesVisibility(bool visible)
    {
        var fileChangesSplitter = this.FindControl<GridSplitter>("FileChangesSplitter");
        var stagedFileChangesScrollViewer = this.FindControl<ScrollViewer>("StagedFileChangesScrollViewer");
        var commitInputScrollViewer = this.FindControl<StackPanel>("CommitInputScrollViewer");

        if (stagedFileChangesScrollViewer != null && fileChangesSplitter != null && commitInputScrollViewer != null)
        {
            // fileChangesSplitter.IsVisible = visible;
            stagedFileChangesScrollViewer.IsVisible = visible;
            commitInputScrollViewer.IsVisible = visible;
        }
    }

    private async Task UnstageFileChange(GitCommitFileChange fileChange)
    {
        if (CurrentRepository.CommitDetail.Commit.IsWorkInProgress)
        {
            await _gitRepositoryService.UnStageFilesAsync(CurrentRepository.Path, new List<string> { fileChange.FilePath });
            await ShowCommitDetails(CurrentRepository.CommitDetail.Commit);
        }
    }

    private void UpdateFileChangesStackPanel()
    {
        var fileChangesStackPanel = this.FindControl<StackPanel>("FileChangesStackPanel");
        if (fileChangesStackPanel == null)
            return;

        fileChangesStackPanel.Children.Clear();

        var stagedHeader = new TextBlock
        {
            Text = "Changes",
            FontSize = 14,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(5, 10, 0, 5)
        };
        fileChangesStackPanel.Children.Add(stagedHeader);

        var fileChanges = CurrentRepository.CommitDetail.FilesChanged.Where(f => !CurrentRepository.CommitDetail.Commit.IsWorkInProgress || !f.IsStaged).ToList();

        foreach (var fileChange in fileChanges)
        {
            var fileChangeItem = new DockPanel
            {
                Height = 30,
            };

            var fileInfo = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Classes = { "fileChangeBorder" },
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
            };
            DockPanel.SetDock(fileInfo, Dock.Left);
            fileInfo.Tapped += async (sender, e) =>
            {
                await ShowFileChangeDetail(fileChange);
            };
            fileChangeItem.Children.Add(fileInfo);

            if (CurrentRepository.CommitDetail.Commit.IsWorkInProgress)
            {
                var stagedFileButton = new Button
                {
                    Content = "+",
                    Foreground = Brushes.White,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Height = 20,
                    Width = 20,
                    Padding = new Avalonia.Thickness(5, 0, 5, 0)
                };
                DockPanel.SetDock(stagedFileButton, Dock.Right);
                stagedFileButton.Click += async (sender, e) =>
                {
                    await StageFilesAsync(new List<string> { fileChange.FilePath });
                };
                fileChangeItem.Children.Add(stagedFileButton);
            }

            fileChangesStackPanel.Children.Add(fileChangeItem);
        }
    }

    private async Task StageFilesAsync(List<string> unstageFilePaths)
    {
        if (CurrentRepository.CommitDetail.Commit.IsWorkInProgress)
        {
            await _gitRepositoryService.StageFilesAsync(CurrentRepository.Path, unstageFilePaths);
            await ShowCommitDetails(CurrentRepository.CommitDetail.Commit);
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
        try
        {
            var currentUserProfile = await _userProfileService.GetCurrentUserProfileAsync();
            await _gitRepositoryService.FetchAsync(CurrentRepository.Path, currentUserProfile);
            await SyncRepository();
            await _gitRepositoryService.CreateNewLocalBranchFromRemoteAsync(CurrentRepository.Path, name);
            var splitedName = name.Split('/').ToList();
            splitedName.RemoveAt(0);
            var localBranchName = string.Join('/', splitedName);
            await _gitRepositoryService.CheckoutBranchAsync(CurrentRepository, localBranchName);
            CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
            UpdateLocalBranchesTreeView();
            UpdateCurrentBranchNameDisplay();
            AddSuccessCard($"Created and switched to branch: {localBranchName} successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }
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
                IsSelected = false
            };
            var textBlock = new TextBlock
            {
                Inlines = {
                    new Run{ Text = branch.Name },
                    new Run{ Text = branch.InComming > 0 ? $"  ⇃{branch.InComming}" : "", Foreground = Brushes.Green, FontSize = 12 },
                    new Run{ Text = branch.OutGoing > 0 ? $"  ↾{branch.OutGoing}" : "", Foreground = Brushes.Red, FontSize = 12 }
                },
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };

            newItem.Header = textBlock;

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
        try
        {
            await _gitRepositoryService.CheckoutBranchAsync(CurrentRepository, name);
            CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
            UpdateLocalBranchesTreeView();
            UpdateCurrentBranchNameDisplay();
            AddSuccessCard($"Switched to branch: {name} successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }

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

    private async void OnFetch(object sender, RoutedEventArgs e)
    {
        try
        {
            var currentUserProfile = await _userProfileService.GetCurrentUserProfileAsync();
            await _gitRepositoryService.FetchAsync(CurrentRepository.Path, currentUserProfile);
            await SyncRepository();
            AddSuccessCard("Fetched latest changes from remote repository successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }
    }

    private async void OnPull(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(CurrentRepository.Name))
                throw new Exception("No repository is currently loaded.");
            await _gitRepositoryService.CheckoutBranchAsync(CurrentRepository, CurrentRepository.LocalBranches.FirstOrDefault(b => b.IsCurrent)?.Name ?? "");
            var currentUserProfile = await _userProfileService.GetCurrentUserProfileAsync();
            await _gitRepositoryService.FetchAsync(CurrentRepository.Path, currentUserProfile);
            await _gitRepositoryService.PullAsync(currentUserProfile, CurrentRepository);
            await SyncRepository();
            AddSuccessCard("Pulled latest changes from remote repository successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }
    }

    private async void OnPush(object sender, RoutedEventArgs e)
    {
        try
        {
            var userProfile = await _userProfileService.GetCurrentUserProfileAsync();
            await _gitRepositoryService.PushAsync(CurrentRepository.Path, CurrentRepository, userProfile);
            await SyncRepository();
            AddSuccessCard("Changes pushed to remote repository successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }
    }

    private void OnCreateBranch(object sender, RoutedEventArgs e)
    {
        // TODO: Implement create branch dialog
    }

    private void OnRefresh(object sender, RoutedEventArgs e)
    {
        // TODO: Implement refresh repository state
    }
    private async void OnCommitButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (CurrentRepository.CommitDetail != null && CurrentRepository.CommitDetail.Commit.IsWorkInProgress)
            {
                var commitMessageTextBox = this.FindControl<TextBox>("CommitMessageTextBox");
                var commitDescriptionTextBox = this.FindControl<TextBox>("CommitDescriptionTextBox");
                if (commitMessageTextBox == null || commitDescriptionTextBox == null)
                {
                    return;
                }
                await _gitRepositoryService.ValidateIfCommitIsPossibleAsync(CurrentRepository.Path, CurrentRepository);
                var currentUserProfile = await _userProfileService.GetCurrentUserProfileAsync();
                var commitRequest = await _gitRepositoryService.GetCommitRequestAsync(CurrentRepository.Path, CurrentRepository, commitMessageTextBox.Text, commitDescriptionTextBox.Text, currentUserProfile);
                await _gitRepositoryService.CommitAsync(CurrentRepository.Path, CurrentRepository, commitRequest);
                CurrentRepository.Commits = await _gitRepositoryService.GetCommitsAsync(CurrentRepository.Path);
                CurrentRepository.RemoteBranches = await _gitRepositoryService.GetRemoteBranchesAsync(CurrentRepository.Path);
                CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
                UpdateCurrentRepositoryDisplay();
                await ShowCommitDetails(CurrentRepository.CommitDetail.Commit);
                AddSuccessCard("Changes committed successfully.");
            }
            else
            {
                AddErrorCard("No changes to commit.");
            }
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
        }
    }
    private void OnOpenProfile(object sender, RoutedEventArgs e)
    {
        var profileWindow = _serviceProvider.GetService(typeof(ProfileManagementWindow)) as ProfileManagementWindow;
        if (profileWindow != null)
        {
            profileWindow.ShowDialog(this);
            profileWindow.Closed += async (s, args) =>
            {
                await LoadProfileInfoDisplay();
            };
        }
    }
}